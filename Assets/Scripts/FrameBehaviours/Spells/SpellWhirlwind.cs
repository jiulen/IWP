using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellWhirlwind : SpellFrameBehaviour
{
    public Transform playerCenter;

    [SerializeField] Collider2D whirlwindCollider;

    [SerializeField] string whirlwindAnim;

    public override void GoToFrame()
    {
        transform.position = playerCenter.position;

        switch (frameNum)
        {
            case 0:
                whirlwindCollider.enabled = true;

                currentAnimName = whirlwindAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 17:
                whirlwindCollider.enabled = false;
                break;
            case 47: //end
                EndAnimation();
                break;
        }
    }

    protected override void HitPlayer(PlayerController playerController)
    {
        whirlwindCollider.enabled = false;

        knockbackDirection = transform.position - playerController.transform.position;
        if (knockbackDirection.sqrMagnitude != 0)
        {
            knockbackDirection.Normalize();
        }

        playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, stunDuration);
        GiveMeter(owner, playerController);
    }
}
