using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalk : PlayerFrameBehaviour
{
    public bool goLeft = false;

    [SerializeField] float walkForce;
    [SerializeField] string walkForwardAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                rb.velocity = Vector2.zero;

                currentAnimName = walkForwardAnim;
                AnimatorChangeAnimation(currentAnimName);

                Vector2 walkDir = Vector2.right * (goLeft ? -1 : 1);
                rb.AddForce(walkDir * walkForce, ForceMode2D.Impulse);
                break;
            case 15: //end
                rb.velocity = new Vector2(0, rb.velocity.y);

                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
