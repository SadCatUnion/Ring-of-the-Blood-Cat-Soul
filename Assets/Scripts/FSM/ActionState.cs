using System;
using System.Collections.Generic;
namespace FSM
{
    public class ActionState<TStateId, TEvent> : StateBase<TStateId>, IActionable<TEvent>
    {
        private Dictionary<TEvent, Delegate> actionsByEvent;
        public ActionState(bool needsExitTime) : base(needsExitTime: needsExitTime)
        {
        }
        private void AddGenericAction(TEvent trigger, Delegate action)
        {
            actionsByEvent = actionsByEvent ?? new Dictionary<TEvent, Delegate>();
            actionsByEvent.Add(trigger, action);
        }

        private TTarget TryGetAndCastAction<TTarget>(TEvent trigger) where TTarget : Delegate
        {
            Delegate action = null;
            actionsByEvent?.TryGetValue(trigger, out action);
            if (action is null)
            {
                return null;
            }
            TTarget target = action as TTarget;
            if (target is null)
            {

            }
            return target;
        }
        public ActionState<TStateId, TEvent> AddAction(TEvent trigger, Action action)
        {
            AddGenericAction(trigger, action);
            return this;
        }
        public ActionState<TStateId, TEvent> AddAction<TData>(TEvent trigger, Action<TData> action)
        {
            AddGenericAction(trigger, action);
            return this;
        }
        public void OnAction(TEvent trigger)
            => TryGetAndCastAction<Action>(trigger)?.Invoke();
        public void OnAction<TData>(TEvent trigger, TData data)
            => TryGetAndCastAction<Action<TData>>(trigger)?.Invoke(data);
    }
    public class ActionState : ActionState<string, string>
    {
        public ActionState(bool needsExitTime) : base(needsExitTime: needsExitTime)
        {
        }
    }
}