using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLightning : SpellFrameBehaviour
{
    [SerializeField] SpriteRenderer boltRenderer, hitRenderer;
    [SerializeField] Collider2D lightningCollider;

    [SerializeField] string lightningAnim;

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
                lightningCollider.enabled = true;

                transform.position = spawnPos;
                RaycastHit2D hit = Physics2D.Raycast(spawnPos, Vector2.down, 30, groundLayerMask);
                float hitDistance;
                if (hit)
                {
                    hitDistance = hit.distance;
                    hitRenderer.gameObject.SetActive(true);
                }
                else
                {
                    hitDistance = 30;
                    hitRenderer.gameObject.SetActive(false);
                }

                srSizeY = hitDistance;
                boltRenderer.size = new Vector2(1, srSizeY);
                boltRenderer.transform.localPosition = new Vector3(0, -hitDistance / 2, 0);
                hitRenderer.transform.localPosition = new Vector3(0, -hitDistance, 0);

                lightningCollider.offset = new Vector2(0, -hitDistance / 2);
                lightningCollider.GetComponent<BoxCollider2D>().size = new Vector2(0.15f, hitDistance);

                currentAnimName = lightningAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 17:
                lightningCollider.enabled = false;
                break;
            case 29: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }

    protected override void HitPlayer(PlayerController playerController)
    {
        lightningCollider.enabled = false;

        playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, stunDuration);
        GiveMeter(owner, playerController);
    }
}
