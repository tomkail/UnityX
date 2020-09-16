// Copyright Cape Guy Ltd. 2015. http://capeguy.co.uk.
// Provided under the terms of the MIT license -
// http://opensource.org/licenses/MIT. Cape Guy accepts
// no responsibility for any damages, financial or otherwise,
// incurred as a result of using this code.
//
// For more information see https://capeguy.co.uk/2016/01/a-for-all/.

//#define REQUIRE_STRUCT // If you undefine this, you must also undefine ASTAR_REQUIRES_STRUCT_ELEMENT in AStar_Tests.cs

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Utils.Algorithms {

	public class AStar<GraphElement>
	#if REQUIRE_STRUCT
		where GraphElement : struct
	#endif
	{
		
		public class PathfinderSolution {
			public IList<GraphElement> solution;
			public float totalCost;

			public PathfinderSolution (IList<GraphElement> solution, float totalCost) {
				this.solution = solution;
				this.totalCost = totalCost;
			}
		}
		/// <summary>
		/// Data which defines a single search space in which we can perform AStar searches (defines the actual graph programatically).
		/// Rather than giving all the data up front, we use callback methods to enquire about the graph as we need. That allows for
		/// lazy evaluation of the graph space by the user/algorithm.
		/// </summary>
		public struct LazyGraph
		{
			public bool GetIsValid ()
			{
				bool isValid = getLowestCostEstimateForMovementBetweenElementsFunc != null;
				isValid = isValid && getElementsConnectedToElementFunc != null;
				isValid = isValid && getActualCostForMovementBetweenElementsFunc != null;
				return isValid;
			}
			public int numGraphEntriesEstimate;

			// Called repeatedly during AStar calculation to get an estimate of the lowest possible cost of movement between two elements.
			// Note: The second element will always be the current target element.
			public Func<GraphElement, GraphElement, float> getLowestCostEstimateForMovementBetweenElementsFunc;

			// Called repeatedly during AStar calculation to get the elements which are connected to the given elements.
			// Note: IList must not be modified after it is given to the algorithm.
			public Func<GraphElement, IEnumerable<GraphElement>> getElementsConnectedToElementFunc;
			// Sets the size of the list of elements connected to a GraphElement up front, which can reduce memory as lists resize
			public int numElementsConnectedToElementEstimate;
			
			// Gets the actual cost of moving from the first element to the second element. This will only be called for pairs of
			// elements returned by getElementsConnectedToElementFunc.
			public Func<GraphElement, GraphElement, float> getActualCostForMovementBetweenElementsFunc;
		}

		/// <summary>
		/// When running CalculateAsync, what's the maximum duration that it should run per frame, in milliseconds?
		/// </summary>
		public float asyncMillisecsBudget = 4.0f;


		/// <summary>
		/// When running CalculateAsync, what's the minimum number of ticks (node queries) it should run before
		/// checking whether we've gone over our milliseconds budget? This is so that calculations with a large
		/// number of nodes but a small cost-per-node isn't encumbered by the cost of the budget calculation.
		/// </summary>
		public int minTicksPerAsyncFrame = 10;

		/// <summary>
		/// Is AStar in the middle of calculating a route asynchronously after calling CalculateAsync? If you
		/// want to interrupt it and start a new calculation, call StopCalculateAsync().
		/// </summary>
		/// <value></value>
		public bool calculatingAsync {
			get {
				return _activeAsyncCoroutine != null;
			}
		}

		/// <summary>
		/// Create an AStar searcher for the given graph.
		///
		/// For more information see https://capeguy.co.uk/2016/01/a-for-all/.
		/// </summary>
		/// <param name="graph">The graph we will be performing searches on.</param>
		public AStar (LazyGraph graph)
		{
			Assert.IsTrue (graph.GetIsValid ());
			_graph = graph;
		}

		public void ClearCache(bool releaseMemory = false) {
			Assert.IsFalse(calculatingAsync);
			_sortedCandidateGraphEntries.Clear();
			if (!releaseMemory) {
				foreach (var graphEntry in _graphEntries.Values) {
					graphEntry.Clear();
					_graphEntryCache.Add(graphEntry);
				}
			}
			_graphEntries.Clear();
			_targetEntry = null;
			_startEntry = null;
		}
		

		/// <summary>
		/// Calculates the shortest route through the graph to the target using the given LazyGraph callbacks.
		/// </summary>
		/// <param name="startingElement">Starting element.</param>
		/// <param name="allowPartialSolution">If true, then the closest to complete solution will be used, even if the solution
		/// doesn't actually reacth the target position.</param>
		public PathfinderSolution Calculate (GraphElement startElement, GraphElement targetElement, bool allowPartialSolution = false, float maxAcceptableRouteLength = float.MaxValue)
		{
			// Special case for if we are starting our search at the target element.
			if (startElement.Equals (targetElement)) {
				return new PathfinderSolution(new List<GraphElement> { targetElement }, 0);
			}

			SetupForCalculate(startElement, targetElement, maxAcceptableRouteLength);
		
			while (true) {
				var complete = TickCalculation();
				if( complete ) break;
			}

			var solutionList = ExtractSolution(targetElement, allowPartialSolution);
			Assert.IsTrue (solutionList.solution == null || startElement.Equals (solutionList.solution[0]));

			// Reset the working data.
			_sortedCandidateGraphEntries.Clear();
			
			return solutionList;
		}

		/// <summary>
		/// Asynchronously calculates the shortest route through the graph to the target using the given LazyGraph callbacks.
		/// This call starts the calculation. In order to process it asynchronously, you need to tick the IEnumerator, for
		/// example:
		/// 
		///    StartCoroutine(CalculateAsync(...));
		/// 
		/// When complete, the completeCallback will be called with the result route.
		/// </summary>
		/// <param name="startingElement">Starting element.</param>
		/// <param name="targetElement">Target element.</param>
		/// <param name="completeCallback">Callback to be called with the result when the calculation completes.</param>
		/// <param name="allowPartialSolution">If true, then the closest to complete solution will be used, even if the solution
		/// doesn't actually reacth the target position.</param>
		public IEnumerator CalculateAsync(GraphElement startElement, GraphElement targetElement, Action<PathfinderSolution> completeCallback, bool allowPartialSolution = false, float maxAcceptableRouteLength = float.MaxValue)
		{
			Assert.AreNotEqual(completeCallback, null);

			// Special case for if we are starting our search at the target element.
			if (startElement.Equals (targetElement)) {
				completeCallback(new PathfinderSolution(new List<GraphElement> { targetElement }, 0));
				return null;
			}

			SetupForCalculate(startElement, targetElement, maxAcceptableRouteLength);

			_activeAsyncCoroutine = CalculateAsyncCoroutine(startElement, targetElement, completeCallback, allowPartialSolution);
			return _activeAsyncCoroutine;
		}

		/// <summary>
		/// Cancels an asynchronous calculation that was started with CalculateAsync. This is necessary
		/// if it's in the middle of a calculation and you want to force it to re-run.
		/// </summary>
		public void StopCalculateAsync()
		{
			Assert.IsTrue(_activeAsyncCoroutine != null);
			_activeAsyncCoroutine = null;
			_sortedCandidateGraphEntries.Clear();
		}

		#region private

		void SetupForCalculate(GraphElement startElement, GraphElement targetElement, float maxAcceptableRouteLength)
		{
			#if !REQUIRE_STRUCT
			Assert.IsTrue (startElement != null);
			Assert.IsTrue (targetElement != null);
			#endif
			Assert.IsTrue (0.0f < maxAcceptableRouteLength);
			Assert.IsTrue (_sortedCandidateGraphEntries != null && _sortedCandidateGraphEntries.Count == 0);
			
			if(_graphEntries == null) {
				_graphEntries = new Dictionary<GraphElement, GraphEntry>(graph.numGraphEntriesEstimate);
			}
			Assert.IsFalse(calculatingAsync);

			// Clear all the cached data if the search has changed...
			{
				Assert.IsTrue (_graphEntries != null);
				bool hasSearchChanged = false;
				if (_maxAcceptableRouteLength != maxAcceptableRouteLength) {
					_maxAcceptableRouteLength = maxAcceptableRouteLength;
					hasSearchChanged = true;
				}

				if (_startEntry != null && !startElement.Equals(_startEntry.graphElement)) {
					hasSearchChanged = true;
				}

				// Set the target entry...
				if (_targetEntry != null && !targetElement.Equals(_targetEntry.graphElement)) {
					hasSearchChanged = true;
				}

				if (hasSearchChanged) {
					_startEntry = null;
					_targetEntry = null;
					_graphEntries.Clear();
				}
			}

			if (_targetEntry == null) {
				_targetEntry = CreateGraphEntryFor (targetElement);
			}
			Assert.IsTrue (_targetEntry.graphElement.Equals(targetElement));

			// Initialise the search graph with the starting element...
			AddStartGraphEntry (startElement);
		}

		private IEnumerator CalculateAsyncCoroutine(GraphElement startElement, GraphElement targetElement, Action<PathfinderSolution> completeCallback, bool allowPartialSolution)
		{
			var ticksPerSecond = System.Diagnostics.Stopwatch.Frequency;
			double ticksPerMillisecond = ticksPerSecond / 1000.0;

			var stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();

			while(true) {

				bool complete = false;
				int minTicks = minTicksPerAsyncFrame;
				if( minTicks < 1 ) minTicks = 1;

				for(int i=0; i<minTicks && !complete; i++)
					complete = TickCalculation();

				if( complete ) 
					break;
				
				var ticks = stopwatch.ElapsedTicks;
				float millisecs = (float)(ticks / ticksPerMillisecond);
				if( millisecs > asyncMillisecsBudget ) {
					stopwatch.Reset();
					yield return null;
					stopwatch.Start();
				}
			}
			
			var solutionList = ExtractSolution(targetElement, allowPartialSolution);
			Assert.IsTrue (solutionList == null || startElement.Equals (solutionList.solution[0]));

			StopCalculateAsync();

			completeCallback(solutionList);
		}

		private void AddStartGraphEntry (GraphElement startingElement)
		{
			Assert.IsTrue (_targetEntry != null);
			if (_startEntry == null || !_startEntry.graphElement.Equals(startingElement)) {
				_startEntry = GetEntryForElement (startingElement);
				if (_startEntry == null) {
					_startEntry = CreateGraphEntryFor (startingElement);
				}
			}
			AddSortedCandidate (_startEntry);
		}

		private bool TickCalculation()
		{
			if (_sortedCandidateGraphEntries.Count == 0)
			{
				// We're run out of candidates and haven't found the solution.
				// There is no route across the graph. :-(.
				return true;
			}
			else
			{
				GraphEntry currTestEntry = GetCurrBestTestEntry();

				if (currTestEntry.Equals(targetElement))
				{
					// We have a full best solution to the target.
					return true;
				}
				else
				{
					ProcessBestEntry(currTestEntry);
				}
			}

			return false;
		}

		private void ProcessBestEntry (GraphEntry bestEntry)
		{
			// First we should remove the entry from the candidate array
			Assert.IsTrue (_sortedCandidateGraphEntries [0] == bestEntry);
			_sortedCandidateGraphEntries.RemoveAt (0);
			GraphElement bestElement = bestEntry.graphElement;
			#if !REQUIRE_STRUCT
			Assert.IsTrue (bestElement != null);
			#endif

			// Now we should add/update all the connected entries.
			if (bestEntry.connectedElements != null) {
				foreach (GraphElement currConnectedElement in bestEntry.connectedElements)
				{
					float connectionCost = _graph.getActualCostForMovementBetweenElementsFunc(bestElement, currConnectedElement);
					float totalCostToConnection = bestEntry.bestCostToHere + connectionCost;
					GraphEntry connectedEntry = GetEntryForElement(currConnectedElement);
					if (connectedEntry == _startEntry)
					{
						// This is the start entry so just ignore it as the best route never returns to the start point!
					} else if (connectedEntry == _targetEntry) {
						// We've reached the target entry, add it as a sorted candidate, we're only finished if this route becomes the best possible.
						if (connectedEntry.bestCostVia == null || totalCostToConnection < connectedEntry.bestCostToHere) {
							connectedEntry.SetBestCost(bestEntry, totalCostToConnection);
							AddSortedCandidate(connectedEntry);
						}
					}
					else if (connectedEntry != null && connectedEntry.bestCostVia != null)
					{
						// We've already visited this entry so see if this is the optimum route.
						if (totalCostToConnection < connectedEntry.bestCostToHere)
						{
							// This route is shorter than the one we've previously found so replace it.
							RemoveSortedCandidate(connectedEntry);
							connectedEntry.SetBestCost(bestEntry, totalCostToConnection);
							AddSortedCandidate(connectedEntry);
						}
					}
					else
					{
						// This is the first time we've seen this entry so just add it
						Assert.IsTrue (connectedEntry == null || connectedEntry.bestCostVia == null);
						if (connectedEntry == null)
						{
							connectedEntry = CreateGraphEntryFor(currConnectedElement);
						}
						connectedEntry.SetBestCost(bestEntry, totalCostToConnection);
						AddSortedCandidate(connectedEntry);
					}
				}
			}
		}

		private GraphEntry CreateGraphEntryFor (GraphElement graphElement)
		{
			Assert.IsTrue (!_graphEntries.ContainsKey(graphElement)); // We should only be creating a new element if there isn't one already.
			// If the _targetEntry hasn't been setup yet then we must be building it now. Pass null (which evaluates to zero estimated cost as we ARE the target).
			GraphEntry newEntry;
			if (_graphEntryCache.Count == 0) {
				newEntry = new GraphEntry (this);
			} else {
				newEntry = _graphEntryCache.Last();
				_graphEntryCache.RemoveAt(_graphEntryCache.Count - 1);
			}
			newEntry.Setup(graphElement);
			_graphEntries [graphElement] = newEntry;
			return newEntry;
		}

		private GraphEntry GetCurrBestTestEntry ()
		{
			Assert.IsTrue (_sortedCandidateGraphEntries.Count > 0);
			return _sortedCandidateGraphEntries [0];
		}

		/// <summary>
		/// Extracts a list of GraphElements representing the shortest route from start to finish.
		/// Returns null if no such route was found.
		/// </summary>
		/// <returns>The solution.</returns>
		/// <param name="targetElement">Target element.</param>
		private PathfinderSolution ExtractSolution (GraphElement targetElement, bool allowPartialSolution)
		{
			List<GraphElement> solutionList = null;
			float totalCost = 0;

			GraphEntry currEntry = GetEntryForElement (targetElement);
			bool hasCompleteSolution = currEntry != null && currEntry.bestCostVia != null;
			if (!hasCompleteSolution && allowPartialSolution) {
				// Get a list of all the entries which we did reach in our search.
				var viableEntries = _graphEntries.Values.Where (reachedFilterEntry => (reachedFilterEntry.bestCostVia != null || reachedFilterEntry == _startEntry));
				if (!(viableEntries.Count() == 0)) {
					// Filter the list down to the entries which got the closest possible distance away from the target...
					var orderedViableEntries = viableEntries.OrderBy (distFromSolutionEntry => distFromSolutionEntry.bestPossibleCostToTarget);
					float bestPossibleRemainingCost = orderedViableEntries.First ().bestPossibleCostToTarget;
					viableEntries = viableEntries.Where (filderRemaininCostEntry => filderRemaininCostEntry.bestPossibleCostToTarget == bestPossibleRemainingCost);
					Assert.IsFalse (viableEntries.Count() == 0);

					// Order the remaining viable entries by the total cost and pick the smallest...
					orderedViableEntries = viableEntries.OrderBy (totalCostOrderEntry => totalCostOrderEntry.currentBestPossibleTotalCost);
					Assert.IsFalse (orderedViableEntries.Count() == 0);

					// Set the curr entry to be the best possible end entry...
					currEntry = orderedViableEntries.First ();
				}
			}

			
			// Now we know where the path ends, trace the route back to the start entry.
			if (currEntry != null && (currEntry.bestCostVia != null || currEntry == _startEntry))
			{
				solutionList = new List<GraphElement> ();
				while (currEntry != null)
				{
					solutionList.Add (currEntry.graphElement);
					totalCost += currEntry.bestCostToHere;
					currEntry = currEntry.bestCostVia;
				}
				solutionList.Reverse ();
			}

			return new PathfinderSolution(solutionList, totalCost);
		}

		/// <summary>
		/// Tries to get the associated entry for the given graph element, returns null if no such element exists.
		/// </summary>
		/// <returns>The entry for element.</returns>
		/// <param name="graphElement">Graph element.</param>
		private GraphEntry GetEntryForElement (GraphElement graphElement)
		{
			GraphEntry graphEntry;
			_graphEntries.TryGetValue (graphElement, out graphEntry);
			return graphEntry;
		}

		// Helper for AddSortedCandidate which orders the candidates, putting the least cost first.
		private class CandidateComparer : IComparer<GraphEntry>
		{
			public int Compare (GraphEntry lhs, GraphEntry rhs)
			{
				// Sort firstly by best possible total cost...
				if (lhs.currentBestPossibleTotalCost < rhs.currentBestPossibleTotalCost)
				{
					return -1;
				}
				else if (lhs.currentBestPossibleTotalCost == rhs.currentBestPossibleTotalCost)
				{
					// Within equal best possible total costs, sort the ones we have gone furthest with first.
					if (lhs.bestCostToHere > rhs.bestCostToHere) {
						return -1;
					} else if (lhs.bestCostToHere < rhs.bestCostToHere) {
						return 1;
					} else {
						return 0;
					}
				}
				else
				{
					return 1;
				}
			}
		}

		private void AddSortedCandidate (GraphEntry newCandidateEntry)
		{
			// Only actually add if if it is possible that it will produce a better solution than our existing best solution, and that it's better than the worst cost we're interested in.
			float currBestSolutionCost = _targetEntry.currentBestPossibleTotalCost;
			if (newCandidateEntry.currentBestPossibleTotalCost <= _maxAcceptableRouteLength && (_targetEntry.bestCostVia == null || newCandidateEntry.currentBestPossibleTotalCost < currBestSolutionCost))
			{
				int findIndex = _sortedCandidateGraphEntries.BinarySearch (newCandidateEntry, _candidateComparer);
				if (findIndex < 0) {
					// We didn't find it so insert it at the bitwise compliment of findIndex (see https://msdn.microsoft.com/en-us/library/w4e7fxsh(v=vs.110).aspx)
					_sortedCandidateGraphEntries.Insert (~findIndex, newCandidateEntry);
				} else {
					_sortedCandidateGraphEntries.Insert(findIndex, newCandidateEntry);
				}
			}
		}
		static readonly CandidateComparer _candidateComparer = new CandidateComparer();

		private void RemoveSortedCandidate (GraphEntry candidateEntry)
		{
			Assert.IsTrue (candidateEntry != null);
			CandidateComparer comparer = new CandidateComparer ();
			int findIndex = _sortedCandidateGraphEntries.BinarySearch (candidateEntry, comparer);
			if (findIndex >= 0)
			{
				// Find the lower bound...
				int lowerBound = findIndex;
				while (lowerBound > 0) {
					if (comparer.Compare (_sortedCandidateGraphEntries[lowerBound - 1], _sortedCandidateGraphEntries[findIndex]) == 0) {
						--lowerBound;
					} else {
						break;
					}
				}

				// Find the upper bound...
				int upperBound = findIndex;
				while (upperBound < (_sortedCandidateGraphEntries.Count - 1)) {
					if (comparer.Compare (_sortedCandidateGraphEntries[upperBound + 1], _sortedCandidateGraphEntries[findIndex]) == 0) {
						++upperBound;
					} else {
						break;
					}
				}

				// Loop over all possible candidates until we find the one we're looking for.
				for (int currTestIndex = lowerBound; currTestIndex <= upperBound; ++currTestIndex) {
					Assert.IsTrue (comparer.Compare (_sortedCandidateGraphEntries[currTestIndex], _sortedCandidateGraphEntries[findIndex]) == 0);
					if (_sortedCandidateGraphEntries[findIndex] == candidateEntry) {
						_sortedCandidateGraphEntries.RemoveAt(findIndex);
						break;
					}
				}
			}
		}

		private class GraphEntry
		{
			public GraphEntry (AStar<GraphElement> aStar)
			{
				Assert.IsNotNull(aStar);
				_aStar = aStar;
			}

			public void Setup(GraphElement graphElement) {
				_graphElement = graphElement;
				IEnumerable<GraphElement> connectedElements = graph.getElementsConnectedToElementFunc (graphElement);
				if (connectedElements != null) {
					if (_connectedElements == null) {
						_connectedElements = new List<GraphElement>(graph.numElementsConnectedToElementEstimate);
					}
					_connectedElements.AddRange(connectedElements);
				}
			}

			public void Clear() {
				_graphElement = default(GraphElement);
				_connectedElements.Clear();

				_bestCostToHere = 0.0f;
				_bestCostVia = null;

				_cachedBestPossibleCostToTarget = 0;
				_hasCachedBestPossibleCostToTarget = false;
			}

			public void SetBestCost (GraphEntry viaEntry, float totalCostToHere)
			{
				Assert.IsTrue (totalCostToHere < this.bestCostToHere || bestCostVia == null);
				Assert.IsTrue (totalCostToHere >= 0.0f);
				Assert.IsTrue (viaEntry != null);
				Assert.IsTrue (totalCostToHere >= viaEntry.bestCostToHere); // We can't have a smaller cost than our via entry.

				this._bestCostToHere = totalCostToHere;
				this._bestCostVia = viaEntry;
			}

			public float currentBestPossibleTotalCost { get { return bestCostToHere + bestPossibleCostToTarget; } }

			public float bestCostToHere { get { return _bestCostToHere; } }
			public GraphEntry bestCostVia { get { return _bestCostVia; } }

			public GraphElement graphElement { get { return _graphElement; } }
			
			bool _hasCachedBestPossibleCostToTarget;
			float _cachedBestPossibleCostToTarget;
			public float bestPossibleCostToTarget {
				get {
					if(!_hasCachedBestPossibleCostToTarget) {
						GraphElement targetElement = _aStar.targetElement;
						#if REQUIRE_STRUCT
						_cachedBestPossibleCostToTarget = graph.getLowestCostEstimateForMovementBetweenElementsFunc (graphElement, targetElement);
						#else
						_cachedBestPossibleCostToTarget = targetElement != null ? graph.getLowestCostEstimateForMovementBetweenElementsFunc (graphElement, targetElement) : 0.0f;
						#endif
						_hasCachedBestPossibleCostToTarget = true;
					}
					return _cachedBestPossibleCostToTarget;
				}
			}
			public IEnumerable<GraphElement> connectedElements { get { return _connectedElements; } }

			#region private
			private LazyGraph graph { get { return _aStar.graph; } }

			// Can only be set on initialisation.
			private GraphElement _graphElement;
			private List<GraphElement> _connectedElements;

			private AStar<GraphElement> _aStar;

			// Can be updated (via SetBestCost
			public float _bestCostToHere = 0.0f;
			public GraphEntry _bestCostVia = null;
			#endregion
		}

		internal LazyGraph graph { get { return _graph; } }
		internal GraphElement targetElement { get { return _targetEntry.graphElement; } }
		internal GraphElement startElement { get { return _startEntry.graphElement; } }

		private LazyGraph _graph;

		// Contains the current candidates in sorted order (best first).  Once processed, a candidate should be removed from the candidate list.
		private List<GraphEntry> _sortedCandidateGraphEntries = new List<GraphEntry>();

		// Contains all the graph entries, indexed by GraphElement.
		private Dictionary<GraphElement, GraphEntry> _graphEntries;
		private IList<GraphEntry> _graphEntryCache = new List<GraphEntry>();

		// The current target element.
		GraphEntry _targetEntry;

		// The current start element.
		GraphEntry _startEntry;

		IEnumerator _activeAsyncCoroutine;

		float _maxAcceptableRouteLength = float.MaxValue;
	}

	#endregion

}