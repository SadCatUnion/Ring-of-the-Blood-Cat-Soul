using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputController : MonoBehaviour
{
    [Header("Input Settings")]
    public float movementSmoothingSpeed = 1f;

    private Vector2 rawInputMovement;
    private Vector2 smoothInputMovement;

    public void OnMove(InputAction.CallbackContext context)
    {
        rawInputMovement = context.ReadValue<Vector2>();
    }

    /*public void OnLook(InputAction.CallbackContext context)
    {
        m_Look = context.ReadValue<Vector2>();
    }*/

    void Update()
    {
        CaculateMovementInputSmoothing();
    }

    private void CaculateMovementInputSmoothing()
    {
        Debug.Log(rawInputMovement);
        smoothInputMovement = Vector2.Lerp(smoothInputMovement, rawInputMovement, movementSmoothingSpeed * Time.deltaTime);
    }

    public Vector2 GetInput()
    {
        return smoothInputMovement;
    }
}
