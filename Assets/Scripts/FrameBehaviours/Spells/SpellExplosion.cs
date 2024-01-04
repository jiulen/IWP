using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellExplosion : SpellFrameBehaviour
{
    [SerializeField] Collider2D explosionCollider;

    [SerializeField] string explosionAnim;

    public Transform position0, position1, position2;

    public PlayerExplosion playerExplosion;

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
            case 6:
                explosionCollider.enabled = true;
                break;
            case 7:
                explosionCollider.enabled = false;
                break;
            case 35: //end
                EndAnimation();
                break;
        }
    }

    protected override void HitPlayer(PlayerController playerController)
    {
        explosionCollider.enabled = false;

        knockbackDirection = playerController.transform.position - transform.position;
        if (knockbackDirection.sqrMagnitude == 0)
        {
            knockbackDirection = Vector2.up;
        }
        else
        {
            knockbackDirection.Normalize();
        }

        playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, stunDuration);
        GiveMeter(owner, playerController);

        playerExplosion.EndAnimation();
    }
}
