using System;
namespace FSM
{
    public class State<TStateId, TEvent> : ActionState<TStateId, TEvent>
    {
        private Action<State<TStateId, TEvent>> onEnter;
        private Action<State<TStateId, TEvent>> onFocus;
        private Action<State<TStateId, TEvent>> onExit;
        private Func<State<TStateId, TEvent>, bool> canExit;
        public ITimer timer;
        public State(
            Action<State<TStateId, TEvent>> onEnter = null,
            Action<State<TStateId, TEvent>> onFocus = null,
            Action<State<TStateId, TEvent>> onExit = null,
            Func<State<TStateId, TEvent>, bool> canExit = null,
            bool needsExitTime = false) : base(needsExitTime: needsExitTime)
        {
            this.onEnter = onEnter;
            this.onFocus = onFocus;
            this.onExit = onExit;
            this.canExit = canExit;
            this.timer = new Timer();
        }
        public override void OnEnter()
        {
            timer.Reset();
            onEnter?.Invoke(this);
        }
        public override void OnFocus()
        {
            onFocus?.Invoke(this);
        }
        public override void OnExit()
        {
            onExit?.Invoke(this);
        }
        public override void OnExitRequest()
        {
            if (!needsExitTime || canExit != null && canExit(this))
            {
                fsm.StateCanExit();
            }
        }
    }
    public class State<TStateId> : State<TStateId, string>
    {
        public State(
            Action<State<TStateId, string>> onEnter = null,
            Action<State<TStateId, string>> onFocus = null,
            Action<State<TStateId, string>> onExit = null,
            Func<State<TStateId, string>, bool> canExit = null,
            bool needsExitTime = false) : base(onEnter: onEnter, onFocus: onFocus, onExit: onExit, canExit: canExit, needsExitTime: needsExitTime)
        {
        }
    }
    public class State : State<string, string>
    {
        public State(
            Action<State<string, string>> onEnter = null,
            Action<State<string, string>> onFocus = null,
            Action<State<string, string>> onExit = null,
            Func<State<string, string>, bool> canExit = null,
            bool needsExitTime = false) : base(onEnter: onEnter, onFocus: onFocus, onExit: onExit, canExit: canExit, needsExitTime: needsExitTime)
        {
        }
    }
}
