using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLingeringSpirit : SpellFrameBehaviour
{
    [SerializeField] int maxHomingFrames;
    int currHomingFrames;
    [SerializeField] float minHomingMultiplier;
    float homingMultiplier;

    public bool startSpell = false;
    public Transform target;

    [SerializeField] float homingForce;
    [SerializeField] float spiritStartSpeed, spiritMaxSpeed;
    [SerializeField] Collider2D spiritCollider;

    //Phases
    //0 is repeatable, 1 is hit

    bool startPhase_1;

    [SerializeField] string repeatableAnim, hitAnim;

    public override void GoToFrame()
    {
        if (phase == 0)
        {
            if (startSpell)
            {
                spiritCollider.enabled = true;

                transform.position = spawnPos;

                currentAnimName = repeatableAnim;
                AnimatorChangeAnimation(currentAnimName);

                rb.velocity = targetDir * spiritMaxSpeed;

                startSpell = false;
            }

            //push projectile toward player
            if (currHomingFrames < maxHomingFrames) ++currHomingFrames;
            homingMultiplier = (maxHomingFrames - currHomingFrames) / (float)maxHomingFrames + minHomingMultiplier;
            if (homingMultiplier > 1) homingMultiplier = 1;

            Vector2 homeDir = (target.position - transform.position).normalized;
            rb.AddForce(homingForce * homingMultiplier * homeDir, ForceMode2D.Impulse);

            //limit speed
            if (rb.velocity.sqrMagnitude > spiritMaxSpeed * spiritMaxSpeed)
            {
                rb.velocity = rb.velocity.normalized * spiritMaxSpeed;
            }

            //rotate missile to face player
            if (rb.velocity.sqrMagnitude > 0)
            {
                Vector3 faceDir = rb.velocity.normalized;
                if (targetDir == Vector2.left) faceDir *= -1;

                transform.right = faceDir;
            }

            if (frameNum > 59)
            {
                frameNum %= 60;
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

                    transform.rotation = Quaternion.identity;
                    break;
                case 35: //end
                    EndAnimation();
                    break;
            }
        }

        AnimatorSetFrame();
    }

    protected override void HitPlayer(PlayerController playerController)
    {
        rb.velocity = Vector2.zero;
        spiritCollider.enabled = false;

        startPhase_1 = true;
        phase = 1;

        knockbackDirection = Vector2.right;

        playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, 6);
    }
}
