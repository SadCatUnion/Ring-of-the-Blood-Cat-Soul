using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateManager : MonoBehaviour
{
    Dictionary<string, State> states = new Dictionary<string, State>();
    State currentState;
    private void Start()
    {
        Init();
    }
    public abstract void Init();
    public void FixedTick()
    {
        if (currentState != null)
            currentState.FixedTick();
    }
    public void Tick()
    {
        if (currentState != null)
            currentState.Tick();
    }
    public void LateTick()
    {
        if (currentState != null)
            currentState.LateTick();
    }

    protected void AddState(string key, State state)
    {
        states.Add(key, state);
    }
    State GetState(string key)
    {
        states.TryGetValue(key, out State value);
        return value;
    }
    public void SwitchState(string targetKey)
    {
        if (currentState != null)
        {
            // on exit
        }

        State targetState = GetState(targetKey);
        // on enter

        currentState = targetState;
    }
}
