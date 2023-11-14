using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeAnimator : MonoBehaviour
{
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Freeze(bool freeze)
    {
        animator.speed = freeze ? 0 : 1;
    }
}
