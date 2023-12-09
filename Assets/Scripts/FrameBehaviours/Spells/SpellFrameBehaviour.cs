using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpellFrameBehaviour : FrameBehaviour
{
    public string spellName;
    public bool activeSpell = false;
    public Vector3 spawnPos;

    public int phase = 0;
    public Vector2 targetDir = Vector3.zero;

    [SerializeField] protected int knockbackIncrease = 0;
    [SerializeField] protected float knockbackForce = 0;
    public Vector2 knockbackDirection = Vector2.right;
    [SerializeField] protected int stunDuration;

    public int ownerNum = -1;
    public PlayerController owner;

    protected override void Awake()
    {
        base.Awake();

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

            if (playerController.playerNum != ownerNum)
            {
                HitPlayer(playerController);
            }
        }
    }

    protected virtual void HitPlayer(PlayerController playerController)
    {

    }
    
    protected virtual void GiveMeter(PlayerController attacker, PlayerController defender)
    {
        attacker.AddMeter(stunDuration * 0.001f);
        defender.AddMeter(stunDuration * 0.0005f);
    }
}
