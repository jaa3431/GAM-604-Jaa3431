using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// The space effect occurs when the player has ended their movement
/// The space have their own behaiour and apply unique state changes for the player
/// </summary>

public class spaceManager : Singleton<spaceManager>
{
    public static spaceManager instance;
    private luckySpace lucky;
    [SerializeField] private eventSpace events;

    private GameObject player;
    private playerStateManager state;

    private minigameManager minigameManager;

    [Header ("User Interface")]
    [SerializeField] private TMP_Text eventText;

    [Header("Sound Effects")]
    private soundManager soundManager;
    [SerializeField] private AudioClip[] spaceClips;

    private musicManager musicManager;

    private void Start()
    {
        soundManager = Singleton<soundManager>.Instance;
        lucky = GetComponent<luckySpace>();
        musicManager = Singleton<musicManager>.Instance;
        minigameManager = Singleton<minigameManager>.Instance;
    }

    //this method is only used when the player has ended their turn
    //the parameters are the current player and the current type they landed on when their movement has ended
    public void ActivateEffect(GameObject currentPlayer, spaceEnum type)
    {
        //this reference the player's state manager and checks if the reference occurred
        player = currentPlayer;
        state = player.GetComponent<playerStateManager>();

        //the behaviour occurs depending on the player's space type
        //if the type is blue then add 3 cash to the current cash in the player controller
        //change the player's current state to inactive state to end their turn
        if (type == spaceEnum.Blue)
        {
            playerController controller = player.GetComponent<playerController>();

            controller.ChangeCash(3);
            eventText.SetText(player.name + " gain 3 cash");
            soundManager.PlaySound(spaceClips[(int)spaceEnum.Blue]);
            StartCoroutine(ChangePlayerState(state.InactiveState, 2));
        }

        //if the type is red then subtract 3 cash to the current cash in the player controller
        //change the player's current state to inactive state to end their turn
        else if (type == spaceEnum.Red)
        {
            playerController controller = player.GetComponent<playerController>();

            controller.ChangeCash(-3);
            eventText.SetText(player.name + " lose 3 cash");
            soundManager.PlaySound(spaceClips[(int)spaceEnum.Red]);
            StartCoroutine(ChangePlayerState(state.InactiveState, 2));
        }

        //if the type is card then change their state to picking state
        else if (type == spaceEnum.Card)
        {
            eventText.SetText(player.name + " is obtaining a card of their preferred type");
            soundManager.PlaySound(spaceClips[(int)spaceEnum.Card]);
            StartCoroutine(ChangePlayerState(state.PickingState, 3f));
        }

        //if the type is item then change their state to item state
        else if (type == spaceEnum.Item)
        {
            eventText.SetText(player.name + " can choose to obtain a relic or give someone a omen");
            soundManager.PlaySound(spaceClips[(int)spaceEnum.Item]);
            StartCoroutine(ChangePlayerState(state.ItemState, 3));
        }

        //if the type is lucky then apply 1 of the random 10 outcomes for the player to obtain
        else if (type == spaceEnum.Lucky)
        {
            lucky.beginLucky(player, Random.Range(1, 11));
            soundManager.PlaySound(spaceClips[(int)spaceEnum.Lucky]);
        }

        //if the type is event then currently it does nothing
        else if (type == spaceEnum.Event)
        {
            moveState findEvent = player.GetComponent<moveState>();
            events = findEvent.CurrentSpace.GetComponent<eventSpace>();
            soundManager.PlaySound(spaceClips[(int)spaceEnum.Event]);
            events.ActivateEvent();
        }

        //if the type is market then changes the state to the market state
        else if (type == spaceEnum.Market)
        {
            eventText.SetText(player.name + " is entering the market to gain items and cards");
            soundManager.PlaySound(spaceClips[(int)spaceEnum.Market]);
            StartCoroutine(ChangePlayerState(state.MarketState, 3));
        }

        else if (type == spaceEnum.FruitMachine) 
        {
            eventText.SetText(player.name + " is entering the Fruit Machine, they can confirm with spacebar to pay 20 cash to spin or cancel with backspace to ignore");
            soundManager.PlaySound(spaceClips[(int)spaceEnum.FruitMachine]);
            StartCoroutine(ChangePlayerState(state.SpinState, 3));
        }

        else if (type == spaceEnum.Minigame)
        {
            StartCoroutine(LoadingMinigame());
            StartCoroutine(ChangePlayerState(state.InactiveState, 10));

        }
    }

    IEnumerator ChangePlayerState(playerStateBase changeState, float time)
    {
        yield return new WaitForSeconds(time);
        state.ChangeState(changeState);
    }

    IEnumerator LoadingMinigame()
    {
        int randomInt = Random.Range(0, (int)minigameEnum.Null);
        eventText.SetText(player.name + " is going to play a minigame, are they alone or with a friend?");
        yield return new WaitForSeconds(2);
        if (randomInt == (int)minigameEnum.DoubleOrNothing || randomInt == (int)minigameEnum.TicTacStash) 
        {
            eventText.SetText("Outcome: Single Player");
        }
        else if(randomInt == (int)minigameEnum.XIII)
        {
            eventText.SetText("Outcome: Multi Player");
        }
        yield return new WaitForSeconds(4);
        minigameManager.Minigame(randomInt);
        musicManager.MinigameMusic();

    }
}
