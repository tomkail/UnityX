// A state machine inspired by Prime31's StateKit (https://github.com/prime31/StateKit/tree/master/Assets/StateKit).

using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace UnityX.StateMachine {	
	/// <summary>
	/// State.
	/// </summary>
	public abstract class State<T> : IState<T> {
	
		public StateMachine<T> machine { get; private set; }
		public T context { get {return machine.context;}}
		public bool active { get; set; }
		
		/// <summary>
		/// The time spent in this state since becoming active.
		/// </summary>
		[DisableAttribute]
		public float elapsedTimeInState = 0f;
		
		/// <summary>
		/// Occurs when the state machine enters this state.
		/// </summary>
		public event OnEnterStateEvent OnEnter;
		
		/// <summary>
		/// Occurs when the state machine exists this state/
		/// </summary>
		public event OnExitStateEvent OnExit;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="State`1"/> class.
		/// </summary>
		public State () {
		
		}
		
		/// <summary>
		/// Sets the machine and context.
		/// </summary>
		/// <param name="_machine">_machine.</param>
		/// <param name="_context">_context.</param>
		public void SetMachine(StateMachine<T> _machine) {
			machine = _machine;
			Init();
		}
		
		/// <summary>
		/// Called directly after the machine and context are set allowing the state to do any required setup
		/// </summary>
		public virtual void Init() {}

		/// <summary>
		/// When the state is set as the active state
		/// </summary>
		public virtual void Enter() {
			if(OnEnter != null) OnEnter();
		}
		
		/// <summary>
		/// Reason is where transitions should go. Called once directly after Enter, and before every Update.
		/// </summary>
		public virtual void UpdateTransitions() {}
		
		/// <summary>
		/// Update the state, with a specific deltaTime.
		/// </summary>
		/// <param name="deltaTime">Delta time.</param>
		public virtual void Update(float deltaTime) {
			
		}
		
		/// <summary>
		/// When the state is set as inactive, just before entering the new state.
		/// </summary>
		public virtual void Exit() {
			if(OnExit != null) OnExit();
		}
	}
}