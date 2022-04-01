using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationBehaviour : MonoBehaviour
{
    [Header("Component References")]
    public Animator animator;

    private int playerMovementAnimationID;

    public void SetupBehaviour()
    {
        SetupAnimationIDs();
    }

    private void SetupAnimationIDs()
    {
        playerMovementAnimationID = Animator.StringToHash("Movement");
    }

    public void UpdateMovementAnimation(float movementBlendValue)
    {
        animator.SetFloat(playerMovementAnimationID, movementBlendValue);
    }
}
