using System;
using UnityEngine;
using Cinemachine;

public class PlayerController : Singleton<PlayerController>
{
    [Header("Component References")]
    public Animator animator;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float turnSpeed = 1f;

    public float jumpSpeed = 3f;
    public float jumpHeight = 10f;
    private float targetJumpHeight = 0f;
    public float fallSpeed = 3f;

    private Camera mainCamera;
    private CinemachineFreeLook cinemachineFreeLook;

    private bool isOnGound;
    public bool IsOnGound
    {
        get { return isOnGound; }
    }

    private bool isInAir;
    public bool IsInAir
    {
        get { return isInAir; }
    }

    private bool isJump;
    public bool IsJump
    {
        get { return isJump; }
        set { isJump = value; }
    }

    private bool isFall;
    public bool IsFall
    {
        get { return isFall; }
        set { isFall = value; }
    }

    private bool isOnLanding;
    public bool IsOnLanding
    {
        get { return isOnLanding; }
        set { isOnLanding = value; }
    }
    
    void Start()
    {
        SetCamera();
    }

    private void SetCamera()
    {
        mainCamera = CameraManager.Instance.GetMainCamera();
        cinemachineFreeLook = CameraManager.Instance.GetVCamera();
    }

    public Vector3 TransformRawXYInput()
    {
        switch (CameraManager.Instance.GetCameraMode())
        {
            case CameraManager.ECameraMode.LOCK_FREE:
                //get the right-facing direction of the referenceTransform
                var right = mainCamera.transform.right;
                //get the forward direction relative to referenceTransform Right
                var forward = Quaternion.AngleAxis(-90f, Vector3.up) * right;
                var rawXYInput = InputController.Instance.GetRawXYInput();
                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                return (rawXYInput.x * right) + (rawXYInput.y * forward);
            case CameraManager.ECameraMode.LOCK_ON:
                return new Vector3();
            default:
                return new Vector3();
        }
    }

    public void Move(Vector3 xyInput)
    {
        var displacement = xyInput * moveSpeed * Time.deltaTime;
        transform.position = transform.position + displacement;
    }

    public void Turn(Vector3 targetDirection)
    {
        var targetForward = Vector3.RotateTowards(transform.forward, targetDirection, turnSpeed * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(targetForward);
    }
    public void TrySetTriggerJump()
    {
        // todo check can jump
        if (true)
        {
            isJump = true;
            animator.SetBool("IsJump", true);
        }
        animator.SetTrigger("Jump");
    }
    public void TryResetTriggerJump()
    {
        animator.ResetTrigger("Jump");
    }

    public void OnJumpEnter()
    {
        targetJumpHeight = transform.position.y + jumpHeight;
    }

    public void OnJumpUpdate()
    {
        var displacement = Vector3.up * jumpSpeed * Time.deltaTime;
        transform.position = transform.position + displacement;
    }

    public void TriggerFall()
    {
        isFall = true;
        animator.SetBool("IsFall", true);
        animator.SetTrigger("Fall");
    }

    public void OnFallUpdate()
    {
        var displacement = Vector3.down * fallSpeed * Time.deltaTime;
        transform.position = transform.position + displacement;
        Debug.Log("OnFallUpdate");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Land"))
        {
            isOnGound = true;
            if (isFall)
            {
                Debug.Log("Landing");
                isOnLanding = true;
                animator.SetBool("IsOnLanding", true);
                animator.SetTrigger("Landing");
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ceiling"))
        {
            if (isJump)
            {
                TriggerFall();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Land"))
        {
            isOnGound = false;
            animator.ResetTrigger("Landing");
        }
    }
}