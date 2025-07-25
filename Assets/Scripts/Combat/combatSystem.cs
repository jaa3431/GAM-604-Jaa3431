using System;
using System.Collections;
using UnityEngine;
using TMPro;
/// <summary>
/// This is the combat system that provides the outcome of the battle
/// This checks the attack and defend values and then calculates the damage
/// If Attack is greater than Defend then the defending player must take damage equal to the difference
/// 
/// </summary>

public class combatSystem : MonoBehaviour
{
    //The combat system will need to reference itself to provide other abilities & additional effects to be used
    public static combatSystem instance;

    //The attacker provides:
    // - Current Player's Turn Object
    // - Player's controller
    // - Attack Value of the Card they chosen
    // - The multiplier of the thrust
    private GameObject attackingPlayer;
    private playerController attackingPlayerController;
    [SerializeField] private int attackValue;
    private float thrustMultiplier;
    private bool attackerReady;
    public bool AttackerIsReady
    {
        get { return attackerReady; }
    }

    //The defender provides:
    // - The player that is on the same space as the current player's turn
    // - Player's controller
    // - Defend Value of the Card they chosen
    // - The multiplier of the guard
    private GameObject defendingPlayer;
    private playerController defendingPlayerController;
    [SerializeField] private int defendValue;
    private float guardMultiplier;
    private bool defenderReady;

    //These events handle to additional card effects that occur in the game.
    public event EventHandler combatComplete;
    public event EventHandler beforeCombatEvent;
    public event EventHandler duringCombatEvent;
    public event EventHandler afterCombatEvent;

    //These are essential for additional effects and abilities
    public GameObject DefendingPlayer
    {
        get { return defendingPlayer; }
    }

    public GameObject AttackingPlayer
    {
        get { return attackingPlayer; }
    }

    public int AttackValue
    {
        get { return attackValue; }
        set { attackValue = value; }
    }
    public int DefendValue
    {
        get { return defendValue; }
        set { defendValue = value; }
    }

    [Header("User Interface")]
    //This is the UI texts that are require to identify the value and outcome of the combat
    [SerializeField] private TMP_Text offenceValue;
    public TMP_Text OffenceValue
    {
        get { return offenceValue; }
        set { offenceValue.SetText(value.ToString());}
    }
    [SerializeField] private TMP_Text defenceValue;
    public TMP_Text DefenceValue
    {
        get { return defenceValue; }
        set { defenceValue.SetText(value.ToString());}
    }

    [SerializeField] private TMP_Text eventText;
    public TMP_Text EventText
    {
        get { return eventText; }
        set { eventText.SetText(value.ToString()); }
    }

    [Header("Sound Effects")]
    private soundManager soundManager;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip defendSound;
    [SerializeField] private AudioClip blockSound;

    [Header("Animator")]
    private stateAnimation attackerAnimator;


    //this is used to make this a singular instance of the component
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

    }

    private void Start()
    {
        soundManager = Singleton<soundManager>.Instance;
    }

    public void AttackerReady(GameObject attacker, int value)
    {
        //This method collect the defender's object along with the controller to collect the guard multiplier
        attackingPlayer = attacker;
        attackerAnimator = attackingPlayer.GetComponentInChildren<stateAnimation>();
        attackingPlayerController = attackingPlayer.GetComponent<playerController>();
        thrustMultiplier = attackingPlayerController.GetModel.ThrustMultiplier;

        //This calculates the defend value in an integer on the value multiplied by the thrust
        attackValue = (int)(value * thrustMultiplier);
        attackerReady = true;
        soundManager.PlaySound(attackSound);
        duringCombatEvent += attackerAnimator.AttackingAnimation;
        afterCombatEvent += attackerAnimator.EndAttackingAnimation;
        CombatReady();

    }

    public void DefenderReady(GameObject defender, int value)
    {
        //This method collect the defender's object along with the controller to collect the guard multiplier
        defendingPlayer = defender;
        defendingPlayerController = defendingPlayer.GetComponent<playerController>();
        guardMultiplier = defendingPlayerController.GetModel.GuardMultiplier;

        //This calculates the defend value in an integer on the value multiplied by the guard
        defendValue = (int)(value * guardMultiplier);
        defenderReady = true;
        soundManager.PlaySound(defendSound);
        CombatReady();
    }

    private void CombatReady()
    {
        if (attackerReady && defenderReady) 
        {
            StartCoroutine(Calculating());
        }
    }

    //This Coroutine provides time for other methods to call and finish their coding to ready the combat
    IEnumerator Calculating()
    {
        eventText.SetText("Combat Begin!");
        yield return new WaitForSeconds(1);
        beforeCombatEvent?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(1);
        offenceValue.SetText("Offence Value: " + attackValue.ToString());
        yield return new WaitForSeconds(1);
        defenceValue.SetText("Defence Value: " + defendValue.ToString());
        yield return new WaitForSeconds(1);
        BattleCalculation();
    }

    public void BattleCalculation()
    {
        //This checks if the attack value is above the defend value
        //If it is then have the defender recieve the difference between the value as damage
        if(attackValue > defendValue)
        {
            eventText.SetText("Defender Recieved " + (-defendValue - -attackValue).ToString() + " Damage");
            defendingPlayerController.ChangeHealth(defendValue - attackValue);
        }

        else
        {
            eventText.SetText("Defender Didn't Recieved Any Damage");
            soundManager.PlaySound(blockSound);
        }

        StartCoroutine(DuringCombat());
    }

    //This Coroutine is used to provide time during battle calculation
    IEnumerator DuringCombat()
    {
        duringCombatEvent?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(3);
        StartCoroutine(BattleFinished());
    }

    //This Coroutine is used to provide time for applying additional effect that occur.
    IEnumerator BattleFinished()
    {
        afterCombatEvent?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(3);
        BattleOver();
    }

    //Once the battle has finished this turns the booleans to false & invokes each character to the correct state
    //The Attacker will return to the moving state & the Defender will return to the exit state
    private void BattleOver()
    {
        attackerReady = false;
        defenderReady = false;
        duringCombatEvent -= attackerAnimator.AttackingAnimation;
        afterCombatEvent -= attackerAnimator.EndAttackingAnimation;
        combatComplete?.Invoke(this, EventArgs.Empty);
    }
}
