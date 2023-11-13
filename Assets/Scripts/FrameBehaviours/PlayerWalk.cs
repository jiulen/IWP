using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalk : FrameBehaviour
{
    public bool goLeft = false;
    public bool forwards = true;

    [SerializeField] float walkForce;
    [SerializeField] string walkForwardAnim, walkBackwordAnim;

    private void Awake()
    {
        
    }

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = forwards ? walkForwardAnim : walkBackwordAnim;
                AnimatorChangeAnimation(currentAnimName);

                Vector2 walkDir = Vector2.right * (goLeft ? -1 : 1);
                rb.AddForce(walkDir * walkForce, ForceMode2D.Impulse);
                break;
            case 36: //end
                rb.velocity = Vector2.zero;
                break;
        }

        AnimatorSetFrame();
    }
}
