namespace UnityX.StateMachine {
	public delegate void OnEnterStateEvent();
	public delegate void OnExitStateEvent();
	
	public interface IState<T> {
		event OnEnterStateEvent OnEnter;
		event OnExitStateEvent OnExit;
		
		StateMachine<T> machine {get;}
		T context {get;}
		bool active {get;}
		void Init();
		void Enter();
		void UpdateTransitions();
		void Update(float deltaTime);
		void Exit();
	}
}