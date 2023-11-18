using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellFrameBehaviour : FrameBehaviour
{
    public string spellName;
    public bool activeSpell = false;

    protected override void Awake()
    {
        base.Awake();

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
}
