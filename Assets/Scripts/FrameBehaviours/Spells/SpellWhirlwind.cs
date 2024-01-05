using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellWhirlwind : SpellFrameBehaviour
{
    [SerializeField] Collider2D whirlwindCollider;

    [SerializeField] string grabAnim, throwAnim;

    public Transform whirlwindTransform;

    public bool hitPlayer = false;

    //Phases
    //0 is grab, 1 is throw

    bool startPhase_1;

    [SerializeField] int grabKnockbackIncrease, grabStunDuration;
    [SerializeField] float grabKnockbackForce;

    public Vector2 grabKnockbackDirection;

    public override void GoToFrame()
    {
        transform.position = whirlwindTransform.position;

        if (phase == 0)
        {
            switch (frameNum)
            {
                case 0:
                    hitPlayer = false;
                    whirlwindCollider.enabled = true;

                    transform.position = spawnPos;

                    currentAnimName = grabAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 2:
                    whirlwindCollider.enabled = false;
                    break;
                case 15: //end
                    if (hitPlayer)
                    {
                        startPhase_1 = true;
                        phase = 1;
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
                    currentAnimName = throwAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 6:
                    whirlwindCollider.enabled = true;
                    break;
                case 9:
                    whirlwindCollider.enabled = false;
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

        whirlwindCollider.enabled = false;

        if (phase == 0) //pull towards player if grabbing
        {
            playerController.TakeHit(grabKnockbackIncrease, grabKnockbackForce * grabKnockbackDirection, grabStunDuration, true, true);
            GiveMeter(owner, playerController);
        }
        else if (phase == 1) //throw away from player
        {
            playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, stunDuration, true);
            GiveMeter(owner, playerController);
        }
    }
}
