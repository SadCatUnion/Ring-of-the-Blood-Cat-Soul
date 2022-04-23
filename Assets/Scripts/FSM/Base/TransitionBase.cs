namespace FSM
{
    public class TransitionBase<TStateId>
    {
        public TStateId from;
        public TStateId to;
        public bool forceInstantly;
        public IStateMachine<TStateId> fsm;
        public TransitionBase(TStateId from, TStateId to, bool forceInstantly = false)
        {
            this.from = from;
            this.to = to;
            this.forceInstantly = forceInstantly;
        }
        public virtual void Init() {}
        public virtual void OnEnter() {}
        public virtual bool ShouldTransition()
        {
            return true;
        }
    }
    public class TransitionBase : TransitionBase<string>
    {
        public TransitionBase(string from, string to, bool forceInstantly = false) : base(from, to, forceInstantly)
        {
        }
    }
}