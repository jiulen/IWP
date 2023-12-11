using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLightning : SpellFrameBehaviour
{
    [SerializeField] SpriteRenderer boltRenderer, hitRenderer;
    [SerializeField] Collider2D lightingCollider;

    [SerializeField] string explosionAnim;

    protected override void Awake()
    {
        base.Awake();

        sr = boltRenderer;
    }

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                lightingCollider.enabled = false;

                transform.position = spawnPos;

                currentAnimName = explosionAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 17:
                lightingCollider.enabled = true;
                break;
            case 29:
                lightingCollider.enabled = false;
                break;
            case 83: //end
                EndAnimation();
                break;
        }
    }

    protected override void HitPlayer(PlayerController playerController)
    {
        lightingCollider.enabled = false;

        playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, stunDuration);
        GiveMeter(owner, playerController);
    }
}
