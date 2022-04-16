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
        //get the right-facing direction of the referenceTransform
        var right = mainCamera.transform.right;
        right.y = 0;
        //get the forward direction relative to referenceTransform Right
        var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
        // determine the direction the player will face based on input and the referenceTransform's right and forward directions
        movementDirection = (newMovementDirection.x * right) + (newMovementDirection.z * forward);
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
            Vector3 direction = movementDirection;
            direction.y = 0f;
            Vector3 desiredForward = Vector3.RotateTowards(rb.transform.forward, direction.normalized, turnSpeed * Time.deltaTime, .1f);
            rb.MoveRotation(Quaternion.LookRotation(desiredForward));
        }
    }
}
