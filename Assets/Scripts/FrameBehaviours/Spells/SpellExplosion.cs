using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellExplosion : SpellFrameBehaviour
{
    Collider2D[] explosionColliders;

    [SerializeField] string explosionAnim;

    protected override void Awake()
    {
        base.Awake();

        explosionColliders = GetComponents<Collider2D>();
    }

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                foreach (var collider in explosionColliders)
                {
                    collider.enabled = false;
                }

                transform.position = spawnPos;

                currentAnimName = explosionAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 3:
                explosionColliders[0].enabled = true;
                break;
            case 4:
                explosionColliders[0].enabled = false;
                break;
            case 15:
                explosionColliders[1].enabled = true;
                break;
            case 16:
                explosionColliders[1].enabled = false;
                break;
            case 27:
                explosionColliders[2].enabled = true;
                break;
            case 28:
                explosionColliders[2].enabled = false;
                break;
            case 39:
                explosionColliders[3].enabled = true;
                break;
            case 40:
                explosionColliders[3].enabled = false;
                break;
            case 71: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }

    protected override void HitPlayer(PlayerController playerController)
    {
        foreach (var collider in explosionColliders)
        {
            collider.enabled = false;
        }

        playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, stunDuration);
        GiveMeter(owner, playerController);
    }
}
