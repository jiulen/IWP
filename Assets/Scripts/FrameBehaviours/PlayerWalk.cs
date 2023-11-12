using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalk : FrameBehaviour
{
    public bool goLeft = false;
    public bool forwards = true;

    [SerializeField] float walkForce;
    [SerializeField] string walkForwardAnim, walkBackwordAnim;

    public override void SetAnimation()
    {
        currentAnimName = forwards ? walkForwardAnim : walkBackwordAnim;
        AnimatorChangeAnimation(currentAnimName);
    }

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                Vector3 walkDir = Vector3.right * (goLeft ? -1 : 1);
                rb.AddForce(walkDir * walkForce, ForceMode2D.Impulse);
                break;
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                break;
        }

        AnimatorSetFrame();
    }
}
