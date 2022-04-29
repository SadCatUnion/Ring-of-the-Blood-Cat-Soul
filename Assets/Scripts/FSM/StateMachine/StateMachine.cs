using System.Collections.Generic;
using UnityEngine;
namespace FSM
{
    public class StateMachine<TOwnId, TStateId, TEvent> : StateBase<TStateId>, ITriggerable<TEvent>, IStateMachine<TStateId>, IActionable<TEvent>
    {
        private class StateBundle
        {
            public StateBase<TStateId> state;
            public List<TransitionBase<TStateId>> transitions;
            public Dictionary<TEvent, List<TransitionBase<TStateId>>> triggerToTransitions;
            public void AddTransition(TransitionBase<TStateId> transition)
            {
                transitions = transitions ?? new List<TransitionBase<TStateId>>();
                transitions.Add(transition);
            }
            public void AddTriggerTransition(TEvent trigger, TransitionBase<TStateId> transition)
            {
                triggerToTransitions = triggerToTransitions ?? new Dictionary<TEvent, List<TransitionBase<TStateId>>>();
                List<TransitionBase<TStateId>> transitionsOfTrigger;
                if (!triggerToTransitions.TryGetValue(trigger, out transitionsOfTrigger))
                {
                    transitionsOfTrigger = new List<TransitionBase<TStateId>>();
                    transitionsOfTrigger.Add(transition);
                }
                triggerToTransitions.Add(trigger, transitionsOfTrigger);
            }
        }

        private static readonly List<TransitionBase<TStateId>> noTransitions = new List<TransitionBase<TStateId>>(0);
        private static readonly Dictionary<TEvent, List<TransitionBase<TStateId>>> noTriggerTransitions = new Dictionary<TEvent, List<TransitionBase<TStateId>>>(0);
        private (TStateId state, bool hasState) startState = (default, false);
        private (TStateId state, bool isPending) pendingState = (default, false);

        private Dictionary<TStateId, StateBundle> nameToStateBundle = new Dictionary<TStateId, StateBundle>();
        private StateBase<TStateId> activeState = null;
        private List<TransitionBase<TStateId>> activeTransitions = noTransitions;
        private Dictionary<TEvent, List<TransitionBase<TStateId>>> activeTriggerTransitions = noTriggerTransitions;
        private List<TransitionBase<TStateId>> transitionsFromAny = new List<TransitionBase<TStateId>>();
        private Dictionary<TEvent, List<TransitionBase<TStateId>>> triggerTransitionsFromAny = new Dictionary<TEvent, List<TransitionBase<TStateId>>>();

        public StateBase<TStateId> ActiveState
        {
            get
            {
                EnsureIsInitializedFor("Trying to get the active state");
                return activeState;
            }
        }
        public TStateId ActiveStateName => activeState.name;
        private bool IsRootFSM => fsm == null;
        public StateMachine(bool needsExitTime = true) : base(needsExitTime)
        {
        }
        private void EnsureIsInitializedFor(string context)
        {
            if (activeState == null)
                throw new FSM.Exceptions.StateMachineNotInitializedException(context);
        }

        public void StateCanExit()
        {
            if (pendingState.isPending)
            {
                ChangeState(pendingState.state);
                pendingState = (default, false);
            }
            fsm?.StateCanExit();
        }
        public override void OnExitRequest()
        {
            if (activeState.needsExitTime)
            {
                activeState.OnExitRequest();
                return;
            }
            fsm?.StateCanExit();
        }
        public void ChangeState(TStateId name)
        {
            activeState?.OnExit();
            StateBundle bundle;
            if (!nameToStateBundle.TryGetValue(name, out bundle) || bundle.state == null)
            {
                throw new FSM.Exceptions.StateNotFoundException<TStateId>(name, "Switching states");
            }
            activeTransitions = bundle.transitions ?? noTransitions;
            activeTriggerTransitions = bundle.triggerToTransitions ?? noTriggerTransitions;
            activeState = bundle.state;
            activeState.OnEnter();
            foreach (var transition in activeTransitions)
            {
                transition.OnEnter();
            }
            foreach (var transitions in activeTriggerTransitions.Values)
            {
                foreach (var transition in transitions)
                {
                    transition.OnEnter();
                }
            }
        }
        public void RequestStateChange(TStateId name, bool forceInstantly = false)
        {
            if (!activeState.needsExitTime || forceInstantly)
            {
                ChangeState(name);
            }
            else
            {
                pendingState = (name, true);
                activeState.OnExitRequest();
            }
        }
        public bool TryTransition(TransitionBase<TStateId> transition)
        {
            if (!transition.ShouldTransition())
                return false;
            RequestStateChange(transition.to, transition.forceInstantly);
            return true;
        }
        public void SetStartState(TStateId name)
        {
            startState = (name, true);
        }
        public override void Init()
        {
            if (!IsRootFSM) return;
            OnEnter();
        }
        public override void OnEnter()
        {
            if (!startState.hasState)
            {

            }
            Debug.Log(startState.state);
            ChangeState(startState.state);
            
            foreach (var transition in transitionsFromAny)
            {
                transition.OnEnter();
            }
            foreach (var transitions in triggerTransitionsFromAny.Values)
            {
                foreach (var transition in transitions)
                {
                    transition.OnEnter();
                }
            }
        }
        public override void OnFocus()
        {
            EnsureIsInitializedFor("Running OnFocus");
            foreach (var transition in transitionsFromAny)
            {
                if (EqualityComparer<TStateId>.Default.Equals(transition.to, activeState.name))
                    continue;
                if (TryTransition(transition))
                    break;
            }
            foreach (var transition in activeTransitions)
            {
                if (TryTransition(transition))
                    break;
            }
            activeState.OnFocus();
        }
        public override void OnExit()
        {
            if (activeState != null)
            {
                activeState.OnExit();
                activeState = null;
            }
        }
        private StateBundle GetOrCreateStateBundle(TStateId name)
        {
            StateBundle bundle;
            if (!nameToStateBundle.TryGetValue(name, out bundle))
            {
                bundle = new StateBundle();
                nameToStateBundle.Add(name, bundle);
            }
            return bundle;
        }
        public void AddState(TStateId name, StateBase<TStateId> state)
        {
            state.fsm = this;
            state.name = name;
            state.Init();
            var bundle = GetOrCreateStateBundle(name);
            bundle.state = state;
            if (nameToStateBundle.Count == 1 || !startState.hasState)
            {
                SetStartState(name);
            }
        }
        private void InitTransition(TransitionBase<TStateId> transition)
        {
            transition.fsm = this;
            transition.Init();
        }
        public void AddTransition(TransitionBase<TStateId> transition)
        {
            InitTransition(transition);
            var bundle = GetOrCreateStateBundle(transition.from);
            bundle.AddTransition(transition);
        }
        public void AddTransitionFromAny(TransitionBase<TStateId> transition)
        {
            InitTransition(transition);
            transitionsFromAny.Add(transition);
        }
        public void AddTriggerTransition(TEvent trigger, TransitionBase<TStateId> transition)
        {
            InitTransition(transition);
            var bundle = GetOrCreateStateBundle(transition.from);
            bundle.AddTriggerTransition(trigger, transition);
        }
        public void AddTriggerTransitionFromAny(TEvent trigger, TransitionBase<TStateId> transition)
        {
            InitTransition(transition);
            List<TransitionBase<TStateId>> transitionsOfTrigger;
            if (!triggerTransitionsFromAny.TryGetValue(trigger, out transitionsOfTrigger))
            {
                transitionsOfTrigger = new List<TransitionBase<TStateId>>();
                triggerTransitionsFromAny.Add(trigger, transitionsOfTrigger);
            }
            transitionsOfTrigger.Add(transition);
        }
        public bool TryTrigger(TEvent trigger)
        {
            EnsureIsInitializedFor("Checking all trigger transitions of the active state");
            List<TransitionBase<TStateId>> triggerTransitions;
            if (triggerTransitionsFromAny.TryGetValue(trigger, out triggerTransitions))
            {
                foreach (var transition in triggerTransitions)
                {
                    if (EqualityComparer<TStateId>.Default.Equals(transition.to, activeState.name))
                        continue;
                    if (TryTransition(transition))
                        return true;
                }
            }
            if (activeTriggerTransitions.TryGetValue(trigger, out triggerTransitions))
            {
                foreach (var transition in triggerTransitions)
                {
                    if (TryTransition(transition))
                        return true;
                }
            }
            return false;
        }
        public void Trigger(TEvent trigger)
        {
            if (TryTrigger(trigger))
                return;
            (activeState as ITriggerable<TEvent>)?.Trigger(trigger);
        }
        public void TriggerLocally(TEvent trigger)
        {
            TryTrigger(trigger);
        }
        public StateBase<TStateId> GetState(TStateId name)
        {
            StateBundle bundle;
            if (!nameToStateBundle.TryGetValue(name, out bundle) || bundle.state == null)
            {
                throw new FSM.Exceptions.StateNotFoundException<TStateId>(name, "Getting a state");
            }
            return bundle.state;
        }
        public void OnAction(TEvent trigger)
        {
            EnsureIsInitializedFor("Running OnAction of the active state");
            (activeState as IActionable<TEvent>)?.OnAction(trigger);
        }
        public void OnAction<TData>(TEvent trigger, TData data)
        {
            EnsureIsInitializedFor("Running OnAction of the active state");
            (activeState as IActionable<TEvent>)?.OnAction<TData>(trigger, data);
        }
        public StateMachine<string, string, string> this[TStateId name]
        {
            get
            {
                var state = GetState(name);
                var subFsm = state as StateMachine<string, string, string>;
                if (subFsm == null)
                {

                }
                return subFsm;
            }
        }
    }
    public class StateMachine<TStateId, TEvent> : StateMachine<TStateId, TStateId, TEvent>
    {
        public StateMachine(bool needsExitTime = true) : base(needsExitTime)
        {
        }
    }
    public class StateMachine<TStateId> : StateMachine<TStateId, TStateId, string>
    {
        public StateMachine(bool needsExitTime = true) : base(needsExitTime)
        {
        }
    }
    public class StateMachine : StateMachine<string, string, string>
    {
        public StateMachine(bool needsExitTime = true) : base(needsExitTime)
        {
        }
    }
}