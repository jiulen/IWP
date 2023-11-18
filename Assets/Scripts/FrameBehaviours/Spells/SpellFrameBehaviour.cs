using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellFrameBehaviour : FrameBehaviour
{
    public string spellName;
    public bool activeSpell = false;
    public Vector3 spawnPos;

    public int phase = 0;
    public Vector3 moveDir = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
}
