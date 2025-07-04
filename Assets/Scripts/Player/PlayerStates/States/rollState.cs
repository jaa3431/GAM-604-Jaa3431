using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// First Playable: This state is use to roll the dice for the player to move around based on the value of the roll
/// The player can also cancel their roll and move back to the deciding state if they prefer not to roll
/// </summary>

public class rollState : playerStateBase, IConfirm, ICancel
{
    //These are the booleans that will be use to change to a specifc state
    //rollDice is to change the current state to move state
    //rollCancel is to change back the current state to deciding state
    private bool rollDice;
    private bool rollCancel;
    
    //These are the variables that will be use to store the selected card from the deciding state
    private int minimumRoll;
    private int maximumRoll;
    private int manaCost;

    //The player controller is required to provide the roll value and decrease in mana for the player to move around
    private playerController controller;

    //The effect component is needed to prevent the player from going back to the deciding state
    private currentEffects effects;

    //The controls are use to either confirm rolling the dice or cancelling the roll to choose a different card
    private boardControls controls;
    public boardControls Controls
    {
        get { return controls; }
        set { controls = value; }
    }

    //This event occurs once the player has roll
    public event EventHandler rollEvent;
    public event EventHandler rollCancelEvent;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private AudioClip cancelSound;
    private soundManager soundManager;


    public override void EnterState(playerStateManager player)
    {
        //The booleans must stay false to ensure that the player cannot change instantly when enterting the roll state
        rollCancel = false;
        rollDice = false;

        //the controls enable confirm and cancel and will need the interfaces to provide the events
        controls = GetComponent<boardControls>();
        controller = GetComponent<playerController>();
        effects = GetComponent<currentEffects>();
        soundManager = Singleton<soundManager>.Instance;
        Controls.confirmPressed += ConfirmingChoice;
        Controls.confirmPressed += ConfirmSound;
        if (!effects.Confused)
        {
            Controls.cancelPressed += Cancel;
            Controls.cancelPressed += DeclineSound;

        }
    }

    public override void UpdateState(playerStateManager player)
    {
        //Depending on which boolean is set to true first will change to the specifc state
        //if rollDice turns true before rollCancel then the current state will change to move state
        //otherwise if rollCancel turns true before RollDice then the current state will change to deciding state
        if (rollDice) 
        {
            player.ChangeState(player.MoveState);
        }
        
        if (rollCancel)
        {
            player.ChangeState(player.DecidingState);
        }
    }

    public override void ExitState(playerStateManager player)
    {
        //When exiting this state, the control events must disable any method being listened to
        Controls.confirmPressed -= ConfirmingChoice;
        Controls.confirmPressed -= ConfirmSound;
        Controls.cancelPressed -= Cancel;
        Controls.cancelPressed -= DeclineSound;
    }

    //This method gathers the data from the selected card from the deciding state in order to provide a min and max roll value
    //This also gathers the mana cost to check if the player has enough mana to use the card and decrease equal to or more than 0
    public void CollectValue(int minValue, int maxValue, int Cost)
    {
        minimumRoll = minValue;
        maximumRoll = maxValue;
        manaCost = Cost;
    }

    //This interface method rolls the dice for the player if their mana is above or equal to the mana cost
    public void ConfirmingChoice(object sender, EventArgs e)
    {
        //the roll a dice value between the minimum and maximum roll value
        //This is then multiplied by the roll multiplier & converts the value to an int (rounding down)
        controller.Roll((int)(UnityEngine.Random.Range(minimumRoll, maximumRoll + 1) * controller.GetModel.RollMultiplier));

        //This decreases the mana based on the card's required mana
        controller.ChangeMana(manaCost);

        rollEvent?.Invoke(this, EventArgs.Empty);
        rollDice = true;
    }

    //This interface method returns the player back to the deciding state
    public void Cancel(object sender, EventArgs e)
    {
        rollCancelEvent?.Invoke(this, EventArgs.Empty);
        rollCancel = true;       
    }

    public void ConfirmSound(object sender, EventArgs e)
    {
        soundManager.PlaySound(rollSound);
    }

    public void DeclineSound(object sender, EventArgs e)
    {
        soundManager.PlaySound(cancelSound);
    }
}
