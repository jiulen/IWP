using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBurst : PlayerFrameBehaviour
{
    [SerializeField] Transform playerCenter;
    [SerializeField] string burstAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = burstAnim;
                AnimatorChangeAnimation(currentAnimName);

                rb.velocity = Vector2.zero;
                rb.gravityScale = 0;

                GameObject burstObj = ShooterGameManager.Instance.GetPooledSpell("Burst");

                SpellBurst spellBurst = burstObj.GetComponent<SpellBurst>();
                spellBurst.spawnPos = playerCenter.position;
                spellBurst.ownerNum = playerController.playerNum;
                spellBurst.owner = playerController;

                burstObj.transform.rotation = Quaternion.identity;
                break;
            case 5: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }

    public override void EndAnimation()
    {
        base.EndAnimation();

        rb.gravityScale = 1;
    }
}
