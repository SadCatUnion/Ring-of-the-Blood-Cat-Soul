using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : CharacterStateManager
{
    [Header("Inputs")]
    public float mouseX;
    public float mouseY;
    public float moveAmount;
    public Vector3 rotateDirection;

    public override void Init()
    {
        base.Init();

        State locomotion = new State(
            new List<StateAction>
            {
                new InputManager(this),
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

        SwitchState("locomotion");
    }

    private void FixedUpdate()
    {
        base.FixedTick();
    }
    private void Update()
    {
        base.Tick();
    }
    private void LateUpdate()
    {
        base.LateTick();
    }
}
