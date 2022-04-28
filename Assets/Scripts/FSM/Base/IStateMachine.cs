namespace FSM
{
    public interface IStateMachine<TStateId>
    {
        void StateCanExit();
        void RequestStateChange(TStateId name, bool forceInstantly = false);
        StateBase<TStateId> ActiveState { get; }
        TStateId ActiveStateName { get; }
    }
    public interface IStateMachine : IStateMachine<string>
    {
    }
}