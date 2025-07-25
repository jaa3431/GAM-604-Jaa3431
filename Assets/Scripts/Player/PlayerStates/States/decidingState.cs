using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// First Playable: The Deciding State is to have the player select a movement card to choose for rolling
/// </summary>

public class decidingState : playerStateBase, IDecideDown, IDecideUp, IDecideRight, IDecideLeft, IConfirm, IUseAbility, IReveal, IMenu

{
    //This controls will be use for providing inputs of deciding the movement card
    private boardControls controls;
    public boardControls Controls
    {
        get { return controls; }
        set { controls = value; }
    }

    //This will provide the cards to collect from the deck in the child of the player object
    private movementDeckPile movementDeck;
    public movementDeckPile MovementDeck
    {
        get { return movementDeck; }
    }

    private statusDeckPile statusDeck;
    public statusDeckPile StatusDeck
    {
        get { return statusDeck; }
    }

    //The controller is use to call the one use ability
    private playerController controller;

    //this will be use to send to the roll state what card was selected to roll
    private GameObject selectedCard;
    private movementCard moveCard;
    private statusCard statCard;

    //These values will be converted to the roll state to roll the minimum and maximum value along with using suitable amount of mana
    private int minRoll;
    private int maxRoll;
    private int manaCost;
    [SerializeField] private int lowestManaCost;

    //This boolean checks when the player has selected a movement card and can move onto the roll state
    private bool hasSelected;
    private bool isTargeting;

    //This boolean checks when the player has selected the ability and can use their one use ability
    private bool usingAbility;
    private bool unableMove;

    //This boolean checks when the player wants to view their deck
    private bool viewingResource;
    private bool isViewing;

    //This is to check if the player is confused
    private currentEffects effects;

    //This boolean checks if the player wants to end the game and leave to the main menu
    private bool leaveGame;

    [Header("Scene Management")]
    [SerializeField] private sceneEnum scene;
    private sceneManager sceneManager;

    [Header("User Interface")]
    //This is to add UI to the cards and add description of the card.
    [SerializeField] private GameObject decidingDisplay;
    [SerializeField] private GameObject statusDisplay;
    [SerializeField] private TMP_Text[] manaCostText = new TMP_Text [4];
    [SerializeField] private TMP_Text[] cardNameText = new TMP_Text[4];
    [SerializeField] private TMP_Text[] cardDescriptionText = new TMP_Text[4];
    [SerializeField] private TMP_Text eventText;

    ///The encapsulation is required for the lucky gambler, This can possibly expand to use to identify the outcome of the one use ability.
    public TMP_Text EventText
    {
        get { return eventText; }
        set { eventText.SetText(value.ToString()); }
    }

    [Header("Sound Effects")]
    [SerializeField] private AudioClip choiceSound;
    [SerializeField] private AudioClip abilitySound;
    [SerializeField] private AudioClip confirmSound;
    [SerializeField] private AudioClip declineSound;
    private soundManager soundManager;

    public override void EnterState(playerStateManager player)
    {
        
        //The hasSelected & usingAbility boolean stays false when entering the state to be capable of returning to the state
        //selectedCard becomes null to prevent a card being chosen despite not being the 1 of 3 cards drawn from the deck
        hasSelected = false;
        isTargeting = false;
        usingAbility = false;
        unableMove = false;
        viewingResource = false;
        isViewing = false;
        leaveGame = false;
        selectedCard = null;
        moveCard = null;
        lowestManaCost = 99;
        
        //Each controls adds the event for each key to enable the correct input on choosing a specifc card
        controls = GetComponent<boardControls>();
        Controls.upPressed += DecidingUp;
        Controls.downPressed += DecidingDown;
        Controls.leftPressed += DecidingLeft;
        Controls.rightPressed += DecidingRight;
        Controls.confirmPressed += ConfirmingChoice;
        Controls.useAbilityPressed += UsingAbility;
        Controls.revealPressed += Reveal;

        //This reference the movement deck pile inside of the child of the player object
        movementDeck = GetComponentInChildren<movementDeckPile>();
        
        //This reference the staus deck pile isnide of the child of the player object
        statusDeck = GetComponentInChildren<statusDeckPile>();

        //This reference the player controller to use to invoke the one use ability
        controller = GetComponent<playerController>();

        effects = GetComponent<currentEffects>();

        //This reference the singleton of the scene manager
        sceneManager = Singleton<sceneManager>.Instance;
        Controls.cancelPressed += MainMenu;

        //This reference the singleton of the sound manager
        soundManager = Singleton<soundManager>.Instance;
        Controls.upPressed += ChoosingSound;
        Controls.downPressed += ChoosingSound;
        Controls.leftPressed += ChoosingSound;
        Controls.rightPressed += ChoosingSound;
        Controls.useAbilityPressed += ChoosingSound;

        //This display the UI for the player to choose a card
        decidingDisplay.SetActive(true);
        eventText.SetText(this.gameObject.name + "'s turn. Choose a Movement or Status Card");

        //This conditional statement checks if the previous state was start state to draw cards
        //If the previous state is not the start state then the cards stay the same
        if (player.PreviousState == player.StartState)
        {
            movementDeck.DrawCards();
            statusDeck.DrawCard();

            //If the player is confused then the player will chose a random card
            if(!unableMove && effects.Confused)
            {
                do
                {
                    int randomInt = UnityEngine.Random.Range(0, movementDeck.SelectedCards.Length);
                    selectedCard = movementDeck.SelectedCards[randomInt];
                    moveCard = selectedCard.GetComponent<movementCard>();
                }
                while (moveCard.ManaCost > controller.GetModel.CurrentMana);


                eventText.SetText("A Random Movement Card was chosen: " + moveCard.MoveCard.cardName + " .Roll the Dice by pressing Space");
                hasSelected = true;

            }
        }

        //The purpose of this condition is to check if the player is confused after using a status card
        else if(player.PreviousState == player.TargetState && effects.Confused)
        {
            do
            {
                int randomInt = UnityEngine.Random.Range(0, movementDeck.SelectedCards.Length);
                selectedCard = movementDeck.SelectedCards[randomInt];
                moveCard = selectedCard.GetComponent<movementCard>();
            }
            while (moveCard.ManaCost > controller.GetModel.CurrentMana);

            eventText.SetText("A Random Movement Card was chosen: " + moveCard.MoveCard.cardName + " Roll the Dice by pressing Space");
            hasSelected = true;
        }

        //This for loop checks which card has the lowest maan cost which will be use to check if the player can use the card
        for (int i = 0; i < movementDeck.SelectedCards.Length; i++)
        {
            movementCard movecard = movementDeck.SelectedCards[i].GetComponent<movementCard>();
            manaCostText[i].SetText(movecard.MoveCard.manaCost.ToString());
            cardNameText[i].SetText(movecard.MoveCard.cardName);
            cardDescriptionText[i].SetText(movecard.MoveCard.cardDescription);
            if (movecard.ManaCost < lowestManaCost)
            {
                lowestManaCost = movecard.ManaCost;
            }
        }

        if (statusDeck.SelectedCard != null) 
        {
            statusCard statcard = statusDeck.SelectedCard.GetComponent<statusCard>();
            statusDisplay.SetActive(true);
            manaCostText[3].SetText(statcard.StatusCard.manaCost.ToString());
            cardNameText[3].SetText(statcard.StatusCard.cardName);
            cardDescriptionText[3].SetText(statcard.StatusCard.cardDescription);

            if (statcard.ManaCost < lowestManaCost)
            {
                lowestManaCost = statcard.ManaCost;
            }
        }
        else
        {
            statusDisplay.SetActive(false);
        }

        if (controller.GetModel.CurrentMana < lowestManaCost)
        {
            unableMove = true;
            eventText.SetText("Unable to Move due to low amount of Mana");

        }

        //This prevents the player to start their turn
        if (effects.Stunned) 
        {
            unableMove = true;
            eventText.SetText("Player is Stunned");
        }

    }

    public override void UpdateState(playerStateManager player)
    {
        //Once the player selects the card they want the player changes to the roll state & applies the minimum, maximum roll and mana cost from the selected card

        if (hasSelected)
        {
            //This is to collect the reference of the card that was selected and provide the data of the roll values and mana cost
            minRoll = moveCard.RollMinimumValue;
            maxRoll = moveCard.RollMaximumValue;
            manaCost = moveCard.ManaCost;
            moveCard.ApplyAdditionalEffect();

            //This is to change the state to roll state
            player.ChangeState(player.RollState);
        }

        if (isTargeting) 
        {
            player.ChangeState(player.TargetState);
        }

        if (isViewing) 
        { 
            player.ChangeState(player.ViewingState);
        }

        if (unableMove)
        {
            player.ChangeState(player.InactiveState);
        }
        
    }

    public override void ExitState(playerStateManager player)
    {
        decidingDisplay.SetActive(false);

        //Before exiting the deciding state, the state must reference the rolll state to have the roll state collect the suitable values
        if (hasSelected) 
        {
            rollState Rolling = player.RollState.GetComponent<rollState>();
            Rolling.CollectValue(minRoll, maxRoll, manaCost);
        }

        if (isTargeting) 
        {
            eventText.SetText("Status Card Selected: " + statCard.StatusCard.cardName + " Choose someone to be affected");
            targetState Targeting = player.TargetState.GetComponent<targetState>();
            Targeting.CollectStatusCard(selectedCard);
        }

        //the inputs will need to be disabled once the state has been changed
        Controls.upPressed -= DecidingUp;
        Controls.downPressed -= DecidingDown;
        Controls.leftPressed -= DecidingLeft;
        Controls.rightPressed -= DecidingRight;
        Controls.confirmPressed -= ConfirmingChoice;
        Controls.useAbilityPressed -= UsingAbility;
        Controls.useAbilityPressed -= ChoosingSound;
        Controls.upPressed -= ChoosingSound;
        Controls.downPressed -= ChoosingSound;
        Controls.leftPressed -= ChoosingSound;
        Controls.rightPressed -= ChoosingSound;
        Controls.revealPressed -= Reveal;
        Controls.cancelPressed -= MainMenu;
    }

    //These are using the interfaces of deciding Up, Down, Left, Right & Confirm in order for the state
    // Up, Left & Right provide movement cards and select unique cards
    // TODO Next Stage - Down provides the status effect
    // Once the player has confirm their choice the player moves onto the roll state (unless the selected card is still empty)
    
    //For each move card selected must empty statCard and turn using ability to false to make sure that only the move card is used
    public void DecidingUp(object sender, EventArgs e)
    {
        ChoosingMovement(1);
    }

    //For status card selected must empty moveCard and turn using ability to false to make sure that only the status card is used
    public void DecidingDown(object sender, EventArgs e)
    {
        if(statusDeck.SelectedCard != null)
        {
            selectedCard = statusDeck.SelectedCard;
            eventText.SetText(cardNameText[3].text + " " + cardDescriptionText[3].text);
            statCard = selectedCard.GetComponent<statusCard>();
            moveCard = null;
            usingAbility = false;
            viewingResource = false;
            leaveGame = false;
        }
        else
        {
            eventText.SetText("You've used a status effect");

        }
    }

    public void DecidingLeft(object sender, EventArgs e)
    {
        ChoosingMovement(0);
    }

    public void DecidingRight(object sender, EventArgs e)
    {
        ChoosingMovement(2);
    }

    private void ChoosingMovement(int card)
    {
        selectedCard = movementDeck.SelectedCards[card];
        eventText.SetText(cardNameText[card].text + " " + cardDescriptionText[card].text);
        moveCard = selectedCard.GetComponent<movementCard>();
        statCard = null;
        usingAbility = false;
        viewingResource = false;
        leaveGame = false;
    }

    public void ConfirmingChoice(object sender, EventArgs e)
    {
        //If there is a GameObject inside of confirm choice then the player has successfully selected
        if (selectedCard != null)
        {
            if (moveCard != null)
            {
                if (controller.GetModel.CurrentMana >= moveCard.ManaCost)
                {
                    hasSelected = true;
                    soundManager.PlaySound(confirmSound);
                    eventText.SetText("Movement Card Selected: " + moveCard.MoveCard.cardName + " Roll the Dice by pressing Space or Go Back by Pressing Cancel");
                }
                else
                {
                    soundManager.PlaySound(declineSound);
                    eventText.SetText("You don't have enough mana to use that move card");
                }
            }
            else if (statCard != null) 
            {
                if (controller.GetModel.CurrentMana >= statCard.ManaCost)
                {
                    isTargeting = true;
                    statusDeck.SelectedCard = null;
                    soundManager.PlaySound(confirmSound);
                }
                else
                {
                    soundManager.PlaySound(declineSound);
                    eventText.SetText("You don't have enough mana to use that status card");
                }
            }
        }

        
        //otherwise check is the player is using an ability
        else if(usingAbility)
        {
            //If the player is using an ability then invoke the one use ability from the controller
            if (controller.GetModel.AbilityUsed)
            {
                soundManager.PlaySound(abilitySound);
                controller.ActivateOneUse();
                controller.GetModel.AbilityUsed = false;
            }
            //otherwise apply an error to inform that the player hasn't chosen a card yet
            else
            {
                soundManager.PlaySound(declineSound);
                eventText.SetText("You already used your one use ability");
            }

        }

        else if (viewingResource)
        {
            isViewing = true;
        }

        else if (leaveGame)
        {
            sceneManager.ChangeScene(scene);
        }

        else
        {
            eventText.SetText("You chosen nothing... Choose One with WASD or Q for One Use Ability");
            soundManager.PlaySound(declineSound);
        }
    }

    //This makes the player choose to use their one use ability
    //This needs to empty both cards to prevent changing state
    public void UsingAbility(object sender, EventArgs e)
    {
        eventText.SetText("One Use Ability: " + controller.GetData.oneUseDescription);
        selectedCard = null;
        moveCard = null;
        statCard = null;
        usingAbility = true;
        viewingResource = false;
        leaveGame = false;
    }

    public void Reveal(object sender, EventArgs e)
    {
        eventText.SetText("Reavling Your Deck");
        viewingResource = true;
        selectedCard = null;
        moveCard = null;
        statCard = null;
        usingAbility = false;
        leaveGame = false;
    }

    public void MainMenu(object sender, EventArgs e)
    {
        eventText.SetText("Return to Main Menu. WARNING: Save files & Autosave haven't been created yet, you won't be able to load the game again to this point");
        viewingResource = false;
        selectedCard = null;
        moveCard = null;
        statCard = null;
        usingAbility = false;
        leaveGame = true;
    }

    public void ChoosingSound(object sender, EventArgs e) 
    {
        soundManager.PlaySound(choiceSound);
    }
}
