using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerWhirlwind : PlayerFrameBehaviour
{
    [SerializeField] Transform playerCenter;
    [SerializeField] string attackAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 17: //create whirlwind + end
                GameObject whirlwindObj = ShooterGameManager.Instance.GetPooledSpell("Whirlwind");

                SpellWhirlwind spellWhirlwind = whirlwindObj.GetComponent<SpellWhirlwind>();
                spellWhirlwind.ownerNum = playerController.playerNum;
                spellWhirlwind.owner = playerController;
                spellWhirlwind.playerCenter = playerCenter;

                whirlwindObj.transform.rotation = Quaternion.identity;

                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
