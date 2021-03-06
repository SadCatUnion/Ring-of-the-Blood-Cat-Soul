namespace FSM
{
    public interface ITriggerable<TEvent>
    {
        void Trigger(TEvent trigger);
    }
    public interface ITriggerable : ITriggerable<string>
    {
    }
}