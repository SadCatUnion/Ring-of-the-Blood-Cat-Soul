using System;
namespace FSM
{
    public class Transition<TStateId> : TransitionBase<TStateId>
    {
        public Func<Transition<TStateId>, bool> condition;
        public Transition(TStateId from, TStateId to, Func<Transition<TStateId>, bool> condition = null, bool forceInstantly = false) : base(from, to, forceInstantly)
        {
            this.condition = condition;
        }
        public override bool ShouldTransition()
        {
            if (condition == null)
            {
                return true;
            }
            return condition(this);
        }
    }
    public class Transition : Transition<string>
    {
        public Transition(string from, string to, Func<Transition<string>, bool> condition = null, bool forceInstantly = false) : base(from, to, condition, forceInstantly)
        {
        }
    }
}