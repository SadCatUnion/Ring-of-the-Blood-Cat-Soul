using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateManager : StateManager
{
    [Header("References")]
    public Animator animator;
    public new Rigidbody rigidbody;

    [Header("Controller Values")]
    public float vertical;
    public float horizontal;
    
    public override void Init()
    {
        animator = GetComponentInChildren<Animator>();
        rigidbody = GetComponentInChildren<Rigidbody>();
    }
}
