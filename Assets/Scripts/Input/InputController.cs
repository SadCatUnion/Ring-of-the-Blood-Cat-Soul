using UnityEngine;
using UnityEngine.InputSystem;
using FSM;
public class InputController : Singleton<InputController>
{
    [Header("Component References")]
    public Animator animator;

    private Vector2 rawXYInput;

    public void OnMove(InputAction.CallbackContext context)
    {
        rawXYInput = context.ReadValue<Vector2>();
        // animator.SetFloat("XInput", rawXYInput.normalized.x);
        // animator.SetFloat("YInput", rawXYInput.normalized.y);
    }

    /*public void OnLook(InputAction.CallbackContext context)
    {
        m_Look = context.ReadValue<Vector2>();
    }*/

    public void OnEvade(InputAction.CallbackContext context)
    {
        // if (context.ReadValueAsButton())
        // {
        //     animator.SetTrigger("Evade");
        // }
        // else
        // {
        //     animator.ResetTrigger("Evade");
        // }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            PlayerController.Instance.TrySetTriggerJump();
        }
        else
        {
            PlayerController.Instance.TryResetTriggerJump();
        }
    }

    public Vector2 GetRawXYInput()
    {
        return rawXYInput;
    }    
}