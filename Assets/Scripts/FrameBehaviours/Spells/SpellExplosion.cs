using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellExplosion : SpellFrameBehaviour
{
    [SerializeField] Collider2D explosionCollider;

    [SerializeField] string explosionAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                explosionCollider.enabled = false;

                transform.position = spawnPos;

                currentAnimName = explosionAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 17:
                explosionCollider.enabled = true;
                break;
            case 29:
                explosionCollider.enabled = false;
                break;
            case 83: //end
                EndAnimation();
                break;
        }
    }

    protected override void HitPlayer(PlayerController playerController)
    {
        explosionCollider.enabled = false;

        knockbackDirection = playerController.transform.position - transform.position;
        knockbackDirection.Normalize();

        playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, 12);
    }
}
