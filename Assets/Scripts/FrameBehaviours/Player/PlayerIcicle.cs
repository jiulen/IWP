using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerIcicle : PlayerFrameBehaviour
{
    public bool goLeft = false;

    [SerializeField] Transform icicleSpawnPoint;

    [SerializeField] string attackAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 18: //create icicle
                /*GameObject icicleObj = PhotonNetwork.InstantiateRoomObject("Icicle", icicleSpawnPoint.position, Quaternion.identity);
                if (goLeft)
                {
                    icicleObj.transform.localScale = new Vector3(-1, 1, 1);
                }

                ShooterGameManager.Instance.spells.Add(icicleObj.GetComponent<SpellFrameBehaviour>());*/
                break;
            case 23: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
