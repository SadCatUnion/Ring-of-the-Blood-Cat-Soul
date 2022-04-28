using System;
namespace FSM
{
    public static class StateMachineShortcuts
    {
        public static void AddState<TOwnId, TStateId, TEvent>(
            this StateMachine<TOwnId, TStateId, TEvent> fsm,
            TStateId name,
            Action<State<TStateId, TEvent>> onEnter = null,
            Action<State<TStateId, TEvent>> onFocus = null,
            Action<State<TStateId, TEvent>> onExit = null,
            Func<State<TStateId, TEvent>, bool> canExit = null,
            bool needsExitTime = false)
        {
            if (onEnter == null && onFocus == null && onExit == null && canExit == null)
            {
                fsm.AddState(name, new StateBase<TStateId>(needsExitTime));
                return;
            }
            fsm.AddState(name, new State<TStateId, TEvent>(onEnter, onFocus, onExit, canExit, needsExitTime));
        }

        private static TransitionBase<TStateId> CreateOptimizedTransition<TStateId>(
            TStateId from,
            TStateId to,
            Func<Transition<TStateId>, bool> condition = null,
            bool forceInstantly = false)
        {
            if (condition == null)
                return new TransitionBase<TStateId>(from, to, forceInstantly);
            return new Transition<TStateId>(from, to, condition, forceInstantly);
        }

        public static void AddTransition<TOwnId, TStateId, TEvent>(
            this StateMachine<TOwnId, TStateId, TEvent> fsm,
            TStateId from,
            TStateId to,
            Func<Transition<TStateId>, bool> condition = null,
            bool forceInstantly = false)
        {
            fsm.AddTransition(CreateOptimizedTransition<TStateId>(from, to, condition, forceInstantly));
        }

        public static void AddTransitionFromAny<TOwnId, TStateId, TEvent>(
            this StateMachine<TOwnId, TStateId, TEvent> fsm,
            TStateId to,
            Func<Transition<TStateId>, bool> condition = null,
            bool forceInstantly = false)
        {
            fsm.AddTransitionFromAny(CreateOptimizedTransition<TStateId>(default, to, condition, forceInstantly));
        }

        public static void AddTriggerTransition<TOwnId, TStateId, TEvent>(
            this StateMachine<TOwnId, TStateId, TEvent> fsm,
            TEvent trigger,
            TStateId from,
            TStateId to,
            Func<Transition<TStateId>, bool> condition = null,
            bool forceInstantly = false)
        {
            fsm.AddTriggerTransition(trigger, CreateOptimizedTransition<TStateId>(from, to, condition, forceInstantly));
        }

        public static void AddTriggerTransitionFromAny<TOwnId, TStateId, TEvent>(
            this StateMachine<TOwnId, TStateId, TEvent> fsm,
            TEvent trigger,
            TStateId to,
            Func<Transition<TStateId>, bool> condition = null,
            bool forceInstantly = false)
        {
            fsm.AddTriggerTransitionFromAny(trigger, CreateOptimizedTransition<TStateId>(default, to, condition, forceInstantly));
        }
    }
}