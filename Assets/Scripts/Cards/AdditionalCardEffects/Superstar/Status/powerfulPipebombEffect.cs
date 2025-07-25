using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerfulPipebombEffect : MonoBehaviour
{
    private statusCard statusCard;
    private Transform playerTransform;
    private GameObject player;


    ///Unlike the other additional effects, this only needs to be added into the status card's event
    void Awake()
    {
        statusCard = GetComponentInParent<statusCard>();
        statusCard.additionalEvent += PowerfulPipebomb;
        //The transform is used to locate the player since the additional effects for status cards must only apply to the player
        playerTransform = this.transform.parent.parent.parent;
        player = playerTransform.gameObject;
    }


    // Powerful Pipebomb Increases Thrust by 10% this turn
    public void PowerfulPipebomb(object sender, EventArgs e)
    {
        playerController controller = player.GetComponent<playerController>();
        controller.ChangeThrust(controller.GetModel.ThrustMultiplier + 0.1f);
    }

    void OnDisable()
    {
        statusCard.additionalEvent -= PowerfulPipebomb;
    }
}
