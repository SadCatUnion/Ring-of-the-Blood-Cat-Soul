using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovementBehaviour : MonoBehaviour
{
    [Header("Component References")]
    public Rigidbody rb;

    [Header("Movement Settings")]
    public float movementSpeed = 3f;
    public float turnSpeed = 0.1f;

    private Camera mainCamera;
    private CinemachineFreeLook cinemachineFreeLook;
    private Vector3 movementDirection;
    
    public void SetupBehaviour()
    {
        SetMainCamera();
    }

    private void SetMainCamera()
    {
        mainCamera = CameraManager.Instance.GetMainCamera();
        cinemachineFreeLook = CameraManager.Instance.GetVCamera();
    }

    private void FixedUpdate()
    {
        Turn();
        Move();
    }

    public void UpdateMovementData(Vector3 newMovementDirection)
    {
        movementDirection = newMovementDirection;
    }

    private void Move()
    {
        Vector3 movement = movementDirection * movementSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);
    }

    private void Turn()
    {
        if (movementDirection.sqrMagnitude > 0.01f)
        {
            Vector3 forward = Quaternion.Euler(0f, cinemachineFreeLook.m_XAxis.Value, 0f) * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();

            Quaternion targetRotation;

            if (Mathf.Approximately(Vector3.Dot(movementDirection, Vector3.forward), -1f))
            {
                targetRotation = Quaternion.LookRotation(-forward);
            }
            else
            {
                Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, movementDirection);
                targetRotation = Quaternion.LookRotation(cameraToInputOffset * forward);
            }


            Quaternion rotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed);
            //Quaternion rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(movementDirection), turnSpeed);
            rb.MoveRotation(rotation);
        }
    }
}
