using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlock: PlayerFrameBehaviour
{
    public SpellBlock spellBlock;

    [SerializeField] string blockAnim; //can use same anim as wait

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                //reset spellBlock to start
                spellBlock.activeSpell = true;
                spellBlock.enabledBehaviour = true;
                spellBlock.lastFrame = false;
                spellBlock.frameNum = 0;
                spellBlock.phase = 0;

                currentAnimName = blockAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 59: //end
                ShrinkSpellBlock();

                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }

    public void ShrinkSpellBlock()
    {
        spellBlock.phase = 1;
        spellBlock.startPhase_1 = true;
    }
}
