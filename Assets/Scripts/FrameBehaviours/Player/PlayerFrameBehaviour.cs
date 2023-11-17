using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFrameBehaviour : FrameBehaviour
{
    PlayerController playerController;

    protected override void Awake()
    {
        base.Awake();

        playerController = GetComponent<PlayerController>();

        sr = playerController.playerSr;
        rb = playerController.rb;
        animator = playerController.animator;
    }
}
