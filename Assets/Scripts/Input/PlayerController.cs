using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using FSM;

public class PlayerController : MonoBehaviour
{
    [Header("Component References")]
    public Transform playerTransform;
    public Animator animator;

    [Header("Sub Behaviours")]
    public InputController inputController;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float turnSpeed = 1f;

    private StateMachine fsm;
    private Camera mainCamera;
    private CinemachineFreeLook cinemachineFreeLook;
    private Vector3 XYInput;
    private Vector3 targetDirection;
    
    void Start()
    {
        fsm = new StateMachine();
        fsm.AddState("Idle", new State());
        fsm.AddState("Walk", new State());
        fsm.AddState("Run", new State());
        fsm.SetStartState("Idle");

        // fsm.AddTransition(new Transition(
        //     "Idle",
        //     "Walk",
        //     (transition) => Idle2Walk()
        // ));
        // fsm.AddTransition(new Transition(
        //     "Walk",
        //     "Idle",
        //     (transition) => Walk2Idle()
        // ));

        fsm.Init();
        SetMainCamera();
    }

    void Update()
    {
        TransformInput();
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

    public void TransformInput()
    {
        switch (CameraManager.Instance.GetCameraMode())
        {
            case CameraManager.ECameraMode.LOCK_FREE:
                //get the right-facing direction of the referenceTransform
                var right = mainCamera.transform.right;
                //get the forward direction relative to referenceTransform Right
                var forward = Quaternion.AngleAxis(-90f, Vector3.up) * right;
                var input = inputController.GetInput();
                // Debug.Log(input);
                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                XYInput = (input.x * right) + (input.y * forward);
                targetDirection = XYInput.normalized;
                
                break;
            case CameraManager.ECameraMode.LOCK_ON:
                break;
        }
        animator.SetFloat("XYInputLength", XYInput.magnitude);
    }

    private void Move()
    {
        var displacement = XYInput * moveSpeed * Time.deltaTime;
        playerTransform.position = playerTransform.position + displacement;
    }

    private void Turn()
    {
        var targetForward = Vector3.RotateTowards(playerTransform.forward, targetDirection, turnSpeed * Time.deltaTime, 0f);
        playerTransform.rotation = Quaternion.LookRotation(targetForward);
    }
}
