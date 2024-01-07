using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLightning : PlayerFrameBehaviour
{
    [SerializeField] float ySpawnPos;
    [SerializeField] string attackAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 19: //create lightning
                GameObject lightningObj = ShooterGameManager.Instance.GetPooledSpell("Lightning");

                SpellLightning spellLightning = lightningObj.GetComponent<SpellLightning>();
                spellLightning.spawnPos = new Vector3(playerController.oppTransform.position.x, ySpawnPos, 0);
                spellLightning.ownerNum = playerController.playerNum;
                spellLightning.owner = playerController;

                lightningObj.transform.rotation = Quaternion.identity;
                break;
            case 32:
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
