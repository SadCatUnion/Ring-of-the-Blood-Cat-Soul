using System;
namespace FSM
{
    public class HybridStateMachine<TOwnId, TStateId, TEvent> : StateMachine<TOwnId, TStateId, TEvent>
    {
        private Action<HybridStateMachine<TOwnId, TStateId, TEvent>> onEnter;
        private Action<HybridStateMachine<TOwnId, TStateId, TEvent>> onFocus;
        private Action<HybridStateMachine<TOwnId, TStateId, TEvent>> onExit;
        public Timer timer;
        public HybridStateMachine(
            Action<HybridStateMachine<TOwnId, TStateId, TEvent>> onEnter = null,
            Action<HybridStateMachine<TOwnId, TStateId, TEvent>> onFocus = null,
            Action<HybridStateMachine<TOwnId, TStateId, TEvent>> onExit = null,
            bool needsExitTime = false
        ) : base(needsExitTime)
        {
            this.onEnter = onEnter;
            this.onFocus = onFocus;
            this.onExit = onExit;
            this.timer = new Timer();
        }
        public override void OnEnter()
        {
            base.OnEnter();
            timer.Reset();
            onEnter?.Invoke(this);
        }
        public override void OnFocus()
        {
            base.OnFocus();
            onFocus?.Invoke(this);
        }
        public override void OnExit()
        {
            base.OnExit();
            onExit?.Invoke(this);
        }
    }
    public class HybridStateMachine<TStateId, TEvent> : HybridStateMachine<TStateId, TStateId, TEvent>
    {
        public HybridStateMachine(
            Action<HybridStateMachine<TStateId, TStateId, TEvent>> onEnter = null,
            Action<HybridStateMachine<TStateId, TStateId, TEvent>> onFocus = null,
            Action<HybridStateMachine<TStateId, TStateId, TEvent>> onExit = null,
            bool needsExitTime = false
        ) : base(onEnter, onFocus, onExit, needsExitTime)
        {
        }
    }
    public class HybridStateMachine<TStateId> : HybridStateMachine<TStateId, TStateId, string>
    {
        public HybridStateMachine(
            Action<HybridStateMachine<TStateId, TStateId, string>> onEnter = null,
            Action<HybridStateMachine<TStateId, TStateId, string>> onFocus = null,
            Action<HybridStateMachine<TStateId, TStateId, string>> onExit = null,
            bool needsExitTime = false
        ) : base(onEnter, onFocus, onExit, needsExitTime)
        {
        }
    }
    public class HybridStateMachine : HybridStateMachine<string, string, string>
    {
        public HybridStateMachine(
            Action<HybridStateMachine<string, string, string>> onEnter = null,
            Action<HybridStateMachine<string, string, string>> onFocus = null,
            Action<HybridStateMachine<string, string, string>> onExit = null,
            bool needsExitTime = false
        ) : base(onEnter, onFocus, onExit, needsExitTime)
        {
        }
    }
}