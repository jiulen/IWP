using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellIcicle : SpellFrameBehaviour
{
    [SerializeField] float icicleSpeed;
    [SerializeField] Collider2D icicleCollider;

    public PlayerIcicle playerIcicle;

    public Transform position0, position1, position2;

    bool hitPlayer = false;

    //Phases
    //0 is start and damaging, 1 is hit

    bool startPhase_1;

    [SerializeField] string startAnim, hitAnim;

    [SerializeField] int finalKnockbackIncrease, finalStunDuration;
    [SerializeField] float finalKnockbackForce;

    public override void GoToFrame()
    {
        if (phase == 0)
        {
            if (frameNum < 2)
            {
                transform.position = position0.position;
            }
            else if (frameNum < 6)
            {
                transform.position = position1.position;
            }
            else
            {
                transform.position = position2.position;
            }

            switch (frameNum)
            {
                case 0:
                    hitPlayer = false;
                    icicleCollider.enabled = false;

                    transform.position = spawnPos;

                    currentAnimName = startAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 6:
                    icicleCollider.enabled = true;
                    break;
                case 9:
                    icicleCollider.enabled = true;
                    break;
                case 12:
                    icicleCollider.enabled = true;
                    break;
                case 15:
                    icicleCollider.enabled = true;
                    break;
                case 18:
                    icicleCollider.enabled = true;
                    break;
                case 20: //end start
                    if (hitPlayer)
                    {
                        startPhase_1 = true;
                        phase = 1;
                        playerIcicle.EndAnimation();
                    }
                    else
                    {
                        EndAnimation();
                    }

                    break;
            }
        }
        else if (phase == 1)
        {
            if (startPhase_1)
            {
                frameNum = 0;

                startPhase_1 = false;
            }

            switch (frameNum)
            {
                case 0:
                    currentAnimName = hitAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 20: //end
                    EndAnimation();
                    break;
            }
        }

        AnimatorSetFrame();
    }

    protected override void HitPlayer(PlayerController playerController)
    {
        hitPlayer = true;
        icicleCollider.enabled = false;

        if (frameNum < 18)
        {
            playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, stunDuration);
        }
        else
        {
            Vector2 finalKnockbackDirection = knockbackDirection + Vector2.up * 0.5f;
            finalKnockbackDirection.Normalize();
            playerController.TakeHit(finalKnockbackIncrease, finalKnockbackForce * finalKnockbackDirection, finalStunDuration);
        }
        GiveMeter(owner, playerController);
    }

    public override void EndAnimation()
    {
        base.EndAnimation();

        playerIcicle.icicleEnded = true;
    }
}
