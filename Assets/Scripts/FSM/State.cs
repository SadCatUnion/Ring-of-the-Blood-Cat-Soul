using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    bool forceExit;
    List<StateAction> fixedUpdateActions;
    List<StateAction> updateActions;
    List<StateAction> lateUpdateActions;

    public State(List<StateAction> fixedUpdateActions, List<StateAction> updateActions, List<StateAction> lateUpdateActions)
    {
        this.fixedUpdateActions = fixedUpdateActions;
        this.updateActions = updateActions;
        this.lateUpdateActions = lateUpdateActions;
    }

    public void FixedTick()
    {
        Execute(fixedUpdateActions);
    }
    public void Tick()
    {
        Execute(updateActions);
    }
    public void LateTick()
    {
        Execute(lateUpdateActions);
        forceExit = false;
    }

    public void Execute(List<StateAction> list)
    {
        foreach (var action in list)
        {
            if (forceExit)
                return;
            forceExit = action.Execute();
        }
    }
}
