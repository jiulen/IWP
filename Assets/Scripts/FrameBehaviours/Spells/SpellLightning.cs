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
                lightningCollider.enabled = false;
                transform.position = spawnPos;

                currentAnimName = lightningAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 20:
                lightningCollider.enabled = true;
                
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 30, groundLayerMask);
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
                break;
            case 23:
                lightningCollider.enabled = false;
                break;
            case 85: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (sr != null)
        {
            sr.size = new Vector2(1, srSizeY);
        }
    }

    protected override void HitPlayer(PlayerController playerController)
    {
        lightningCollider.enabled = false;

        playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, stunDuration);
        GiveMeter(owner, playerController);
    }
}
