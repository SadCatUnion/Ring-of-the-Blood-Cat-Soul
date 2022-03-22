using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : CharacterStateManager
{
    public override void Init()
    {
        base.Init();

        State locomotion = new State(
            new List<StateAction>
            {

            },
            new List<StateAction>
            {

            },
            new List<StateAction>
            {

            }
        );
        State attack = new State(
            new List<StateAction>
            {

            },
            new List<StateAction>
            {

            },
            new List<StateAction>
            {

            }
        );

        AddState("locomotion", locomotion);
        AddState("attack", attack);
    }
}
