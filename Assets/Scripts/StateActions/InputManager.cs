using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : StateAction
{
    PlayerStateManager playerStateManager;

    // Trigger
    bool RB;
    bool RT;
    bool LB;
    bool LT;
    // Inventory
    bool InventoryInput;
    // Button
    bool XB;
    bool YB;
    bool AB;
    bool BB;
    // Dpad
    bool left;
    bool right;
    bool up;
    bool down;

    public InputManager(PlayerStateManager stateManager)
    {
        playerStateManager = stateManager;
    }

    public override bool Execute()
    {
        playerStateManager.horizontal = Input.GetAxis("Horizontal");
        playerStateManager.vertical = Input.GetAxis("Vertical");

        // Trigger
        RB = Input.GetButton("RB");
        RT = Input.GetButton("RT");
        LB = Input.GetButton("LB");
        LT = Input.GetButton("LT");
        // Inventory
        InventoryInput = Input.GetButton("Inventroy");
        // Button
        XB = Input.GetButton("X");
        YB = Input.GetButton("Y");
        AB = Input.GetButton("A");
        BB = Input.GetButton("B");
        // Dpad
        left = Input.GetButton("Left");
        right = Input.GetButton("Right");
        up = Input.GetButton("Up");
        down = Input.GetButton("Down");

        return false;
    }
}
