using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellIcicle : SpellFrameBehaviour
{
    [SerializeField] float icicleSpeed;
    [SerializeField] Collider2D icicleCollider;

    //Phases
    //0 is start, 1 is repeatable, 2 is hit

    bool startPhase_1, startPhase_2;

    [SerializeField] string startAnim, repeatableAnim, hitAnim;

    public override void GoToFrame()
    {
        if (phase == 0)
        {
            switch (frameNum)
            {
                case 0:
                    icicleCollider.enabled = false;

                    transform.position = spawnPos;

                    currentAnimName = startAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 17: //end start
                    rb.velocity = targetDir * icicleSpeed;

                    startPhase_1 = true;
                    phase = 1;
                    break;
            }
        }
        else if (phase == 1)
        {
            if (startPhase_1)
            {
                icicleCollider.enabled = true;

                frameNum = 0;

                currentAnimName = repeatableAnim;
                AnimatorChangeAnimation(currentAnimName);

                startPhase_1 = false;
            }

            if (frameNum > 59)
            {
                frameNum %= 60;
            }
        }
        else if (phase == 2)
        {
            if (startPhase_2)
            {
                frameNum = 0;

                startPhase_2 = false;
            }

            switch (frameNum)
            {
                case 0:
                    currentAnimName = hitAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 41: //end
                    EndAnimation();
                    break;
            }
        }

        AnimatorSetFrame();
    }

    protected override void HitPlayer(PlayerController playerController)
    {
        rb.velocity = Vector2.zero;
        icicleCollider.enabled = false;

        startPhase_2 = true;
        phase = 2;

        playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, 6);
    }
}
