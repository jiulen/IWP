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

    [SerializeField] protected LayerMask groundLayerMask;

    protected override void Awake()
    {
        base.Awake();

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected override void Start()
    {
        base.Start();

        CameraController.Instance.TrackTransform(transform);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!PhotonNetwork.IsMasterClient && !ShooterGameManager.Instance.isReplay)
            return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

            if (playerController.playerNum != ownerNum)
            {
                HitPlayer(playerController);
            }
        }

        if (((1 << collision.gameObject.layer) & groundLayerMask) != 0)
        {
            HitGround();
        }
    }

    protected virtual void HitPlayer(PlayerController playerController)
    {

    }

    protected virtual void HitGround()
    {

    }

    protected virtual void GiveMeter(PlayerController attacker, PlayerController defender)
    {
        attacker.AddMeter(stunDuration / 180f);
        defender.AddMeter(stunDuration / 360f);
    }
}
