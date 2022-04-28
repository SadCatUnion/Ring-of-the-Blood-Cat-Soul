using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputController : MonoBehaviour
{
    [Header("Component References")]
    public Animator animator;
    private Vector2 rawInputMovement;
    private bool isSpaceButtonDown;

    public void OnMove(InputAction.CallbackContext context)
    {
        rawInputMovement = context.ReadValue<Vector2>();
        animator.SetFloat("XInput", rawInputMovement.normalized.x);
        animator.SetFloat("YInput", rawInputMovement.normalized.y);
    }

    /*public void OnLook(InputAction.CallbackContext context)
    {
        m_Look = context.ReadValue<Vector2>();
    }*/

    public void OnEvade(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            // animator.SetTrigger("Evade");
            animator.CrossFade("Evade", 0.2f);
        } 
        else
        {
            // animator.ResetTrigger("Evade");
        }
    }

    public Vector2 GetInput()
    {
        return rawInputMovement;
    }
}