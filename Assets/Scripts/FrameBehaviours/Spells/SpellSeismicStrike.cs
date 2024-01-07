using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellSeismicStrike : SpellFrameBehaviour
{
    [SerializeField] Collider2D seismicStrikeCollider;

    [SerializeField] string seismicStrikeAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                seismicStrikeCollider.enabled = false;

                transform.position = spawnPos;

                currentAnimName = seismicStrikeAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 6:
                seismicStrikeCollider.enabled = true;
                break;
            case 9:
                seismicStrikeCollider.enabled = false;
                break;
            case 26: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }

    protected override void HitPlayer(PlayerController playerController)
    {
        seismicStrikeCollider.enabled = false;

        playerController.TakeHit(knockbackIncrease, knockbackForce * knockbackDirection, stunDuration);
        GiveMeter(owner, playerController);
    }
}
