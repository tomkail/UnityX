// A state machine inspired by Prime31's StateKit (https://github.com/prime31/StateKit/tree/master/Assets/StateKit)
// Instructions
// States should be added where the state machine is created, and an initial state is passed directly into the construtor.
// The generic argument is the type of the user - ie, IdleState<Player>, or JumpState<Player>.

// How does it work?
// A state is always active.
// States are not created and destroyed when changed.
// All states are kept in memory and the active state is stored as the currentState and flagged as "active". For more info on this, see State.cs.
// The state machine passes the owner of the machine (the "context") to the states and initializes them when added.
// States are stored in a dictionary using their type as the key, meaning that two states of the same type can not be added (ie two idle states)

// EXAMPLES
// Creating a state machine
//	stateMachine = new StateMachine<TranslationGameTranslationModel>(this, new IdleState());
//	stateMachine.AddState(new JumpState());

// Creating a state
//	public class TranslationGameControllerSubstitutingState : State<TranslationGameTranslationModel> {}

// Switching state
// 	public override void UpdateTransitions () {
//		if(Input.GetKeyDown(Keycode.State))
//			machine.ChangeState<JumpState>();
//	}

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityX.StateMachine {
	/// <summary>
	/// State machine.
	/// </summary>
	public sealed class StateMachine<T> {
		/// <summary>
		/// The context of the state machine.
		/// </summary>
		public T context { get; private set; }
		
		public delegate void OnStateChangedEvent(System.Type lastStateType, System.Type newStateType);
		public event OnStateChangedEvent OnStateChanged;
		
		/// <summary>
		/// Gets the current state.
		/// </summary>
		/// <value>The current state.</value>
		public State<T> currentState { get; private set; }
		
		/// <summary>
		/// The previous state.
		/// </summary>
		public State<T> previousState { get; private set; }
		
		private Dictionary<System.Type, State<T>> states = new Dictionary<System.Type, State<T>>();

		// Create a state machine without a default state (not recommended).
		public StateMachine(T context) {
			this.context = context;
		}

		public StateMachine(T context, State<T> initialState) : this (context) {
			// setup our initial state
			AddState( initialState );
			currentState = initialState;
			EnterState();
		}

		/// <summary>
		/// Adds a state to the machine
		/// </summary>
		public void AddState(State<T> state) {
			DebugX.Assert(state != null, "New state is null");
			states[state.GetType()] = state;
			state.SetMachine(this);
		}
		
		/// <summary>
		/// ticks the state machine with the provided delta time
		/// </summary>
		public void Update(float deltaTime) {
			currentState.elapsedTimeInState += deltaTime;
			currentState.UpdateTransitions();
			currentState.Update(deltaTime);
		}
		
		/// <summary>
		/// changes the current state
		/// </summary>
		public R ChangeState<R>() where R : State<T> {
			var newType = typeof(R);
//			Don't change if we're in the new state already
			if(currentState != null && currentState.GetType() == newType )
				return currentState as R;
			
			if(!ContainsState<R>()) {
				DebugX.LogError(this, "State " + newType.Name + " does not exist on "+context+". Did you forget to add it by calling addState? Current state will remain active.");
				return null;
			}

			// Exit the old state, if it exists.
			if( currentState != null )
				ExitState();
			
				
			// swap states and call begin
			previousState = currentState;
			currentState = states[newType];
			EnterState();
	//		Debug.Log (DebugX.LogString(this, "Transitioned from "+previousState.ToString()+" to "+_currentState.ToString()));
			if(OnStateChanged != null)
				OnStateChanged(previousState == null ? null : previousState.GetType(), currentState.GetType());
			
			// Run the new state.
			currentState.UpdateTransitions();
			
			return currentState as R;
		}
		
		/// <summary>
		/// Clears up the state machine
		/// </summary>
		public void Die () {
			if( currentState != null )
				ExitState();
		}
		
		/// <summary>
		/// Checks to see if a state of this type exists in the machine.
		/// </summary>
		public bool ContainsState<R>() {
			return states.ContainsKey(typeof(R));
		}
		
		/// <summary>
		/// Gets a state from the state machine from a type.
		/// </summary>
		public R GetState<R>() where R : State<T> {
			if(ContainsState<R>()) {
				return states[typeof(R)] as R;
			}
			
			return null;
		}
		
		/// <summary>
		/// Gets all the types in the state machine that inherit from a type.
		/// </summary>
		public List<State<R>> GetStatesInheriting<R>() {
			List<State<R>> validStates = new List<State<R>>();
			foreach (Type stateType in states.Keys) {
				if(stateType.IsSubclassOf(typeof(R))) {
					validStates.Add ((states[stateType] as State<R>));
				}
			}
			return validStates;
		}

		public bool IsInState<R>() where R : State<T> {
			return ContainsState<R>() && currentState is R;
		}

		private void EnterState () {
			currentState.active = true;
			currentState.Enter();
		}

		private void ExitState () {
			currentState.Exit();
			currentState.active = false;
			currentState.elapsedTimeInState = 0;
		}
	}
}