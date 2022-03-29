using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterStateManager : StateManager
{
    [Header("References")]
    public Animator animator;
    public new Rigidbody rigidbody;

    [Header("Controller Values")]
    public float vertical;
    public float horizontal;
    public bool lockon;
    
    public override void Init()
    {
        animator = GetComponentInChildren<Animator>();
        rigidbody = GetComponentInChildren<Rigidbody>();
    }

    public void PlayAnimation(string animation)
    {
        animator.CrossFade(animation, 0.2f);
    }
}
