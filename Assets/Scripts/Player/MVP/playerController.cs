using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
/// <summary>
/// This is the player controller (presenter) that provides the logic for the model and changes to the view
/// This collect the character data and applies changes to the model.
/// </summary>
public class playerController : MonoBehaviour
{
    //This indicates the player
    [SerializeField] private int player;
    public int Player
    {
        get { return player; }
    }

    [SerializeField] private int character;
    [SerializeField] private characterData[] characters;

    //this provide encapsulation of the player model, character data and the current path
    //this allows other classes to reference these to return the data
    //However the choosing state can set a new current path for the controller to collect and provide
    private playerModel playerModel;
    private playerView playerView;
    [SerializeField] private characterData data;

    //This is to provide the current path for the move state to use
    [SerializeField] private GameObject currentPath;
    private int currentSpaceInt = 1;

    //These are the events to activate the passive and one use ability
    public event EventHandler oneUseEvent;

    //These are the events to activate the effects that occur
    public event EventHandler effectStartEvent;
    public event EventHandler effectEndEvent;

    public playerModel GetModel { get { return playerModel; } }
    public characterData GetData {  get { return data; } }

    public GameObject Path { get { return currentPath; } set { currentPath = value; } }
    public int CurrentSpaceInt
    {
        get { return currentSpaceInt; }
        set { currentSpaceInt = value; }
    }

    [Header("User Interface")]
    [SerializeField] private TMP_Text eventText;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip gainCashSound;
    [SerializeField] private AudioClip loseCashSound;
    [SerializeField] private AudioClip thrustSound;
    [SerializeField] private AudioClip guardSound;
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private AudioClip decreaseSound;
    [SerializeField] private AudioClip gameOverSound;
    private soundManager soundManager;

    [Header("Animation")]
    private stateAnimation animator;
    public event EventHandler takeDamageEvent;
    public event EventHandler endDamageEvent;
    public event EventHandler isDefeatedEvent;

    void Awake()
    {
        SelectedData characterData = characterSystem.Retrieve();
        if (characterData != null)
        {
            if (player == 1)
            {
                character = characterData.playerOne;
            }
            else if (player == 2)
            {
                character = characterData.playerTwo;
            }

            data = characters[character];
        }
        else
        {
            sceneManager scene = Singleton<sceneManager>.Instance;
            scene.ChangeScene(sceneEnum.MainMenu);
        }

        playerModel = new playerModel(data);

        //this collects the path list for the player to start on
        pathOrder startingSpace = currentPath.GetComponent<pathOrder>();

        //this creates a new player model based on the character the player has chosen
        transform.position = new Vector3(startingSpace.SpaceOrder[currentSpaceInt].transform.position.x, 2f, startingSpace.SpaceOrder[currentSpaceInt].transform.position.z);

        //this collects the view for providing the interface of the statistics
        playerView = GetComponent<playerView>();

        Instantiate(GetData.characterObject, this.transform);
    }

    private void Start()
    {
        soundManager = Singleton<soundManager>.Instance;
        animator = GetComponentInChildren<stateAnimation>();
        takeDamageEvent += animator.DamageAnimation;
        endDamageEvent += animator.EndDamageAnimation;
        isDefeatedEvent += animator.DeadAnimation;

    }

    public void ChangeStats()
    {
        allPaths paths = allPaths.instance;

        if (player == 1)
        {
            playerOneData playerData = GetComponentInChildren<playerOneData>();
            GetModel.CurrentHealth = playerData.healthCurrent;
            GetModel.MaxHealth = playerData.healthMax;
            GetModel.CurrentMana = playerData.manaCurrent;
            GetModel.MaxMana = playerData.manaMax;
            GetModel.CurrentCash = playerData.cashCurrent;
            GetModel.AbilityUsed = playerData.usedAbility;
            currentPath = paths.Paths[playerData.currentPath];
            currentSpaceInt = playerData.spaceInt;
            GetModel.ThrustMultiplier = playerData.storedThrust;
            GetModel.GuardMultiplier = playerData.storedGuard;
            GetModel.RollMultiplier = playerData.storedRoll;
            playerData.ControllerComplete = true;
        }
        else if (player == 2) 
        { 
            playerTwoData playerData = GetComponentInChildren<playerTwoData>();
            GetModel.CurrentHealth = playerData.healthCurrent;
            GetModel.MaxHealth = playerData.healthMax;
            GetModel.CurrentMana = playerData.manaCurrent;
            GetModel.MaxMana = playerData.manaMax;
            GetModel.CurrentCash = playerData.cashCurrent;
            GetModel.AbilityUsed = playerData.usedAbility;
            currentPath = paths.Paths[playerData.currentPath];
            currentSpaceInt = playerData.spaceInt;
            GetModel.ThrustMultiplier = playerData.storedThrust;
            GetModel.GuardMultiplier = playerData.storedGuard;
            GetModel.RollMultiplier = playerData.storedRoll;
            playerData.ControllerComplete = true;
        }


        //this collects the path list for the player to start on
        pathOrder startingSpace = currentPath.GetComponent<pathOrder>();

        //this creates a new player model based on the character the player has chosen
        transform.position = new Vector3(startingSpace.SpaceOrder[currentSpaceInt].transform.position.x, 2f, startingSpace.SpaceOrder[currentSpaceInt].transform.position.z);

        //This displays the new data to the UI
        playerView.DisplayUI();
    }

    public void StoreStats(object sender, EventArgs e)
    {
        allPaths paths = allPaths.instance;

        if (player == 1)
        {
            playerOneData playerData = GetComponentInChildren<playerOneData>();
            playerData.healthCurrent = GetModel.CurrentHealth;
            playerData.healthMax = GetModel.MaxHealth;
            playerData.manaCurrent = GetModel.CurrentMana;
            playerData.manaMax = GetModel.MaxMana;
            playerData.cashCurrent = GetModel.CurrentCash;
            playerData.usedAbility = GetModel.AbilityUsed;
            playerData.spaceInt = currentSpaceInt;
            playerData.storedThrust = GetModel.ThrustMultiplier;
            playerData.storedGuard = GetModel.GuardMultiplier;
            playerData.storedRoll = GetModel.RollMultiplier;
            for (int i = 0; i < paths.Paths.Length; i++)
            {
                if (paths.Paths[i].gameObject == currentPath.gameObject)
                {
                    playerData.currentPath = i;
                }
            }
        }
        else if (player == 2)
        {
            playerTwoData playerData = GetComponentInChildren<playerTwoData>();
            playerData.healthCurrent = GetModel.CurrentHealth;
            playerData.healthMax = GetModel.MaxHealth;
            playerData.manaCurrent = GetModel.CurrentMana;
            playerData.manaMax = GetModel.MaxMana;
            playerData.cashCurrent = GetModel.CurrentCash;
            playerData.usedAbility = GetModel.AbilityUsed;
            playerData.storedThrust = GetModel.ThrustMultiplier;
            playerData.storedGuard = GetModel.GuardMultiplier;
            playerData.storedRoll = GetModel.RollMultiplier;
            playerData.spaceInt = currentSpaceInt;
            for (int i = 0; i < paths.Paths.Length; i++)
            {
                if (paths.Paths[i].gameObject == currentPath.gameObject)
                {
                    playerData.currentPath = i;

                }
            }
        }
    }
    //This is to reset the multipliers from the effects of their previous turn
    //Thus also regains the mana for the player to use cards
    public void ResetStats(object sender, EventArgs e)
    {
        playerModel.CurrentMana = playerModel.MaxMana;
        playerView.ManaUI();
        ChangeThrust(1);
        ChangeGuard(1);
        ChangeRoll(1);
    }

    //ChangeCash is a method that when landing on a blue or red space will change the current cash to certain value
    public void ChangeCash(int value)
    {

        if (playerModel.CurrentCash + value < 0)
        {
            playerModel.CurrentCash = 0;
            soundManager.PlaySound(loseCashSound);
        }

        else
        {

            playerModel.CurrentCash += value;
            if (value > 0) 
            { 
                soundManager.PlaySound(gainCashSound);
            }
            else if(value < 0)
            {
                soundManager.PlaySound(loseCashSound);
            }
        }

        playerView.CashUI();
    }

    public void ChangeMana(int cost)
    {
        playerModel.CurrentMana -= cost;
        playerView.ManaUI();
    }

    //Roll is a mathod that subtracts the mana based on mana cost (parameter is roll cost) and the value of the dice (parameter is value)
    public void Roll(int value) 
    { 
        currentEffects shockEffect = GetComponent<currentEffects>();

        playerModel.RollValue = value;
        if (shockEffect.Shocked) 
        {
            ChangeHealth(-value);
        }
    }

    public void ChangeHealth(int value) 
    {
        //This gets the current buff component to see if the player is invincble from taking any damage

        currentBuffs buffs = GetComponent<currentBuffs>();
        //If the player is invincle & would take and damage this turn
        //Then turn the value to 0 to prevent any damage from occurring
        if(buffs.IsInvincible && value < 0)
        {
            value = 0;
        }

        //If the current health being subtracted from the value is less than 0, the player is defeated
        if (playerModel.CurrentHealth + value <= 0)
        {
            playerModel.IsAlive = false;
            playerModel.CurrentHealth = 0;
            soundManager.PlaySound(gameOverSound);
            eventText.SetText(this.gameObject.name + " is Defeated");
            StartCoroutine(TakingDamage());
            StartCoroutine(GameOver());
        }
        //Else if the current health being added from the the value is greater than the max health, the current health will maximise to only the maximum health
        else if (playerModel.CurrentHealth + value > playerModel.MaxHealth)
        {
            playerModel.CurrentHealth = playerModel.MaxHealth;
            soundManager.PlaySound(healSound);
        }
        //otherwise the value adds (or subtract if the value is negative) to the new current health
        else
        {
            playerModel.CurrentHealth += value;
            if (value > 0) 
            { 
                soundManager.PlaySound(healSound);
            }
            else if(value < 0)
            {
                soundManager.PlaySound(damageSound);
                StartCoroutine(TakingDamage());
            }
        }

        playerView.HealthUI();
    }

    //For any Multiplier Changes the procedure for the parameter must be:
    //Collecting the Multiplier from the controller's model
    //Then Add/Substract/Divide/Multiply the multiplier
    //That paremeter float value is now the new multiplier
    public void ChangeThrust(float value)
    {
        if (value < 0) 
        {
            GetModel.ThrustMultiplier = 0;
            soundManager.PlaySound(decreaseSound);
        }
        else
        {
            if (value < GetModel.ThrustMultiplier) 
            {
                soundManager.PlaySound(decreaseSound);
            }
            else if(value > GetModel.ThrustMultiplier)
            {
                soundManager.PlaySound(thrustSound);
            }
            GetModel.ThrustMultiplier = value;
        }
        playerView.ThrustUI();
    }

    public void ChangeGuard(float value)
    {
        if (value < 0)
        {
            GetModel.GuardMultiplier = 0;
            soundManager.PlaySound(decreaseSound);
        }
        else
        {
            if (value < GetModel.GuardMultiplier)
            {
                soundManager.PlaySound(decreaseSound);
            }
            else if (value > GetModel.GuardMultiplier)
            {
                soundManager.PlaySound(guardSound);
            }
            GetModel.GuardMultiplier = value;
        }
        playerView.GuardUI();
    }

    public void ChangeRoll(float value)
    {
        if (value < 0)
        {
            GetModel.RollMultiplier = 0;
            soundManager.PlaySound(decreaseSound);
        }
        else
        {
            if (value < GetModel.RollMultiplier)
            {
                soundManager.PlaySound(decreaseSound);
            }
            else if (value > GetModel.RollMultiplier)
            {
                soundManager.PlaySound(rollSound);
            }
            GetModel.RollMultiplier = value;
        }
        playerView.RollUI();
    }

    public void IncrementDeck(deckTypeEnum deck)
    {
        if (deck == deckTypeEnum.Offence) 
        {
            GetModel.OffenceCards++;
            playerView.OffenceUI();
        }

        else if(deck == deckTypeEnum.Defence)
        {
            GetModel.DefenceCards++;
            playerView.DefenceUI();
        }

        else if(deck == deckTypeEnum.Movement)
        {
            GetModel.MovementCards++;
            playerView.MovementUI();
        }

        else if(deck == deckTypeEnum.Status)
        {
            GetModel.StatusCards++;
            playerView.StatusUI();
        }

        else if(deck == deckTypeEnum.Item)
        {
            GetModel.ItemPile++;
            playerView.ItemUI();
        }
    }

    //This is activated when the player wants to use their one use ability from the player's specifc character.
    public void ActivateOneUse()
    {
        oneUseEvent?.Invoke(this, EventArgs.Empty);
        GetModel.AbilityUsed = false;
        playerView.OneUseAbilityUI();
    }

    public void ActivateStartEffects(object sender, EventArgs e)
    {
        effectStartEvent?.Invoke(this, EventArgs.Empty);
    }

    public void ActivateEndEffect()
    {
        effectEndEvent?.Invoke(this, EventArgs.Empty);
    }

    public void DisplayEffect(int enumInt, bool display)
    {
        playerView.EffectUI(enumInt, display);
    }

    public void DisplayBuff(int enumInt, bool display)
    {
        playerView.BuffUI(enumInt, display);
    }

    public void DisplayAbility(Sprite icon, Color colour)
    {
        playerView.AbilityUI(icon, colour);
    }

    IEnumerator TakingDamage()
    {
        takeDamageEvent?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(2);
        if (GetModel.IsAlive)
        {
            endDamageEvent?.Invoke(this, EventArgs.Empty);
        }
        else 
        { 
            isDefeatedEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2);
        if (player == 1)
        {
            eventText.SetText("Player 1 has been defeated, Player 2 Wins!");
        }
        else if (player == 2) 
        {
            eventText.SetText("Player 2 has been defeated, Player 1 Wins!");

        }
        yield return new WaitForSeconds(3);
        sceneManager sceneManager = Singleton<sceneManager>.Instance;
        sceneManager.ChangeScene(sceneEnum.Victory);
    }

    private void OnDisable()
    {
        takeDamageEvent -= animator.DamageAnimation;
        endDamageEvent -= animator.EndDamageAnimation;
        isDefeatedEvent -= animator.DeadAnimation;
    }
}
