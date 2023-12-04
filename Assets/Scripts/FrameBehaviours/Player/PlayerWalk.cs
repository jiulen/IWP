using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalk : PlayerFrameBehaviour
{
    public bool goLeft = false;
    public bool forwards = true;
    public bool firstHalf = true; //check if walk cycle is first or second half

    [SerializeField] float walkForce;
    [SerializeField] string walkForwardAnim_1, walkForwardAnim_2, walkBackwordAnim_1, walkBackwordAnim_2;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                if (forwards)
                {
                    currentAnimName = firstHalf ? walkForwardAnim_1 : walkForwardAnim_2;
                }
                else
                {
                    currentAnimName = firstHalf ? walkBackwordAnim_1 : walkBackwordAnim_2;
                }
                AnimatorChangeAnimation(currentAnimName);

                Vector2 walkDir = Vector2.right * (goLeft ? -1 : 1);
                rb.AddForce(walkDir * walkForce, ForceMode2D.Impulse);
                break;
            case 17: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
