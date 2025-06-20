using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fruitEffect : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject player;
    private itemBehaviour item;
    private playerController controller;
    [SerializeField] int healthValue;

    // Fruit will increase their health only being picked up
    void Awake()
    {
        playerTransform = this.transform.parent.parent.parent;
        player = playerTransform.gameObject;
        controller = player.GetComponent<playerController>();
        item = GetComponentInParent<itemBehaviour>();
        item.pickupEvent += UponPickup;

    }

    public void UponPickup(object sender, EventArgs e)
    {
        controller.GetModel.MaxHealth += healthValue;
        controller.ChangeHealth(healthValue);
        item.pickupEvent -= UponPickup;
    }

}
