using UnityEngine;
using UnityEngine.InputSystem;
using FSM;
public class InputController : MonoBehaviour
{
    [Header("Component References")]
    public Animator animator;
    public PlayerController playerController;
    private Vector2 rawInputMovement;

    private StateMachine fsm;

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
            animator.SetTrigger("Evade");
        }
        else
        {
            animator.ResetTrigger("Evade");
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            //animator.SetTrigger("Jump");
            playerController.GetFSM().Trigger("Input");
            animator.CrossFade("Air", 0.2f);
        }
        else
        {
            //animator.ResetTrigger("Jump");
        }
    }

    public Vector2 GetInput()
    {
        return rawInputMovement;
    }
}