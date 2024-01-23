using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCancelBlock : PlayerFrameBehaviour
{
    [SerializeField]
    PlayerBlock playerBlock;

    [SerializeField] string cancelBlockAnim; //can use same anim as wait

    // Start is called before the first frame update
    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = cancelBlockAnim;
                AnimatorChangeAnimation(currentAnimName);

                playerBlock.ShrinkSpellBlock();
                //run frame 0 of spellBlock shrinking
                playerBlock.spellBlock.GoToFrame();
                break;
            case 5: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
