namespace FSM
{
    public class StateBase<TStateId>
    {
        public bool needsExitTime;
        public TStateId name;
        public IStateMachine<TStateId> fsm;
        public StateBase(bool needsExitTime)
        {
            this.needsExitTime = needsExitTime;
        }
        public virtual void Init() {}
        public virtual void OnEnter() {}
        public virtual void OnFocus() {}
        public virtual void OnExit() {}
        public virtual void OnExitRequest() {}
    }
    public class StateBase : StateBase<string>
    {
        public StateBase(bool needsExitTime) : base(needsExitTime)
        {
        }
    }
}