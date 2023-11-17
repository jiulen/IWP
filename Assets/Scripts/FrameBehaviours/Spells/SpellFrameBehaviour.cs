using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellFrameBehaviour : FrameBehaviour
{
    public PlayerController.PlayerActions spellType;

    protected override void Awake()
    {
        base.Awake();

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
}
