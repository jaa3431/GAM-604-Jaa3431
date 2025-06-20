using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
/// <summary>
/// This is the current effects to check which effects are currently true
/// If an effect is true, then this will add an event to either the start or end effect depending on the effect
/// Some effects might need to have their boolean encapsulated in order to get the boolean value for other components
/// </summary>

public class currentEffects : MonoBehaviour
{
    private playerController controller;
    private currentBuffs buffs;

    //Each effect has a bool to check if the effect is active on the player
    //Each effect also has a int for the cooldown that decrements either at the start or the end of their turn
    //Some effect also have a getter that other scripts can use to get the value of the boolean
    [SerializeField] private bool isBurned;
    private int burnCooldown;

    [SerializeField] private bool isSlowed;
    private int slowCooldown;

    [SerializeField] private bool isShocked;
    private int shockCooldown;
    public bool Shocked
    {
        get { return isShocked; }
    }

    [SerializeField] private bool isExposed;
    private int exposeCooldown;

    [SerializeField] private bool isBleeding;
    private int bleedCooldown;

    [SerializeField] private bool isPoisoned;
    private int poisonCooldown;

    [SerializeField] private bool isBlistered;
    private int blisterCooldown;

    [SerializeField] private bool isUnstabled;
    private int unstableCooldown;

    [SerializeField] private bool isStunned;
    private int stunCooldown;
    public bool Stunned
    {
        get { return isStunned; }
    }

    [SerializeField] private bool isFeared;
    private int fearCooldown;

    [SerializeField] private bool isConfused;
    private int confusedCooldown;
    public bool Confused
    {
        get { return isConfused; }
    }

    [SerializeField] private bool isBlind;
    private int blindCooldown;
    public bool Blind
    {
        get { return isBlind; }
    }

    [Header("Sound Effect")]
    [SerializeField] private AudioClip debuffSound;
    [SerializeField] private AudioClip[] effectSounds = new AudioClip[12];
    private soundManager soundManager;

    private void Start()
    {
        controller = GetComponent<playerController>();
        buffs = GetComponent<currentBuffs>();
        soundManager = Singleton<soundManager>.Instance;

    }

    public void SaveEffects(object sender, EventArgs e)
    {
        if (controller.Player == 1)
        {
            playerOneData playerData = GetComponentInChildren<playerOneData>();
            playerData.storedEffects[0] = burnCooldown;
            playerData.storedEffects[1] = shockCooldown;
            playerData.storedEffects[2] = exposeCooldown;
            playerData.storedEffects[3] = bleedCooldown;
            playerData.storedEffects[4] = poisonCooldown;
            playerData.storedEffects[5] = blisterCooldown;
            playerData.storedEffects[6] = unstableCooldown;
            playerData.storedEffects[7] = slowCooldown;
            playerData.storedEffects[8] = confusedCooldown;
            playerData.storedEffects[9] = fearCooldown;
            playerData.storedEffects[10] = stunCooldown;
            playerData.storedEffects[11] = blindCooldown;
        }
        else if (controller.Player == 2)
        {
            playerTwoData playerData = GetComponentInChildren<playerTwoData>();
            playerData.storedEffects[0] = burnCooldown;
            playerData.storedEffects[1] = shockCooldown;
            playerData.storedEffects[2] = exposeCooldown;
            playerData.storedEffects[3] = bleedCooldown;
            playerData.storedEffects[4] = poisonCooldown;
            playerData.storedEffects[5] = blisterCooldown;
            playerData.storedEffects[6] = unstableCooldown;
            playerData.storedEffects[7] = slowCooldown;
            playerData.storedEffects[8] = confusedCooldown;
            playerData.storedEffects[9] = fearCooldown;
            playerData.storedEffects[10] = stunCooldown;
            playerData.storedEffects[11] = blindCooldown;
        }

    }

    //When the current player has chosen this player
    //This method requires the type and cooldown of the status card that is effecting this player
    public void AddEffect(effectEnum type, int cooldown)
    {


        if(!buffs.IsHealthy)
        {
            //This is the procedure of how to apply status effects
            //If the effect has a cooldown more than 0 then increase the cooldown
            if (type == effectEnum.Burned)
            {
                isBurned = true;
                if (burnCooldown > 0)
                {
                    burnCooldown += cooldown;
                }
                else
                {
                    burnCooldown = cooldown;
                    controller.effectEndEvent += BurnPlayer;
                    controller.DisplayEffect((int)type, true);
                }

            }

            else if (type == effectEnum.Slowed)
            {
                isSlowed = true;
                if (slowCooldown > 0)
                {
                    slowCooldown += cooldown;
                }
                else
                {
                    slowCooldown = cooldown;
                    controller.effectStartEvent += SlowPlayer;
                    controller.DisplayEffect((int)type, true);

                }

            }

            else if (type == effectEnum.Shocked)
            {
                isShocked = true;
                if (shockCooldown > 0)
                {
                    shockCooldown += cooldown;
                }
                else
                {
                    shockCooldown = cooldown;
                    controller.effectEndEvent += ShockPlayer;
                    controller.DisplayEffect((int)type, true);

                }


            }

            else if (type == effectEnum.Exposed)
            {
                isExposed = true;
                if (exposeCooldown > 0)
                {
                    exposeCooldown += cooldown;
                }
                else
                {
                    exposeCooldown = cooldown;
                    controller.effectStartEvent += ExposePlayer;
                    controller.DisplayEffect((int)type, true);
                }

            }

            else if (type == effectEnum.Bleeding)
            {
                isBleeding = true;
                if (bleedCooldown > 0)
                {
                    bleedCooldown += cooldown;
                }
                else
                {
                    bleedCooldown = cooldown;
                    controller.effectEndEvent += BleedPlayer;
                    controller.DisplayEffect((int)type, true);

                }


            }

            else if (type == effectEnum.Poison)
            {
                isPoisoned = true;
                if (poisonCooldown > 0)
                {
                    poisonCooldown += cooldown;
                }
                else
                {
                    poisonCooldown = cooldown;
                    controller.effectEndEvent += PoisonPlayer;
                    controller.DisplayEffect((int)type, true);

                }



            }

            else if (type == effectEnum.Blistered)
            {
                isBlistered = true;
                if (blisterCooldown > 0)
                {
                    blisterCooldown += cooldown;
                }
                else
                {
                    blisterCooldown = cooldown;
                    controller.effectEndEvent += BlisterPlayer;
                    controller.DisplayEffect((int)type, true);
                }



            }

            else if (type == effectEnum.Unstabled)
            {
                isUnstabled = true;
                if (unstableCooldown > 0)
                {
                    unstableCooldown += cooldown;
                }
                else
                {
                    unstableCooldown = cooldown;
                    controller.effectEndEvent += UnstablePlayer;
                    controller.DisplayEffect((int)type, true);
                }



            }

            else if (type == effectEnum.Stunned)
            {
                isStunned = true;
                if (stunCooldown > 0)
                {
                    stunCooldown += cooldown;
                }
                else
                {
                    stunCooldown = cooldown;
                    controller.effectEndEvent += StunPlayer;
                    controller.DisplayEffect((int)type, true);

                }


            }

            else if (type == effectEnum.Feared)
            {
                isFeared = true;
                if (fearCooldown > 0)
                {
                    fearCooldown += cooldown;
                }
                else
                {
                    fearCooldown = cooldown;
                    controller.effectStartEvent += FearPlayer;
                    controller.DisplayEffect((int)type, true);
                }


            }

            else if (type == effectEnum.Confused)
            {
                isConfused = true;
                if (confusedCooldown > 0)
                {
                    confusedCooldown += cooldown;
                }
                else
                {
                    confusedCooldown = cooldown;
                    controller.effectEndEvent += ConfusePlayer;
                    controller.DisplayEffect((int)type, true);
                }


            }

            else if (type == effectEnum.Blind)
            {
                isBlind = true;
                if (blindCooldown > 0)
                {
                    blindCooldown += cooldown;
                }
                else
                {
                    blindCooldown = cooldown;
                    controller.effectEndEvent += BlindPlayer;
                    controller.DisplayEffect((int)type, true);
                }


            }

        }
    }

    //If the player is burned, then the player will decrease health at the end of their turn based on the amount of offence cards they have
    public void BurnPlayer(object sender, EventArgs e)
    {
        if (isBurned) 
        {
            offenceDeckPile offenceCards = GetComponentInChildren<offenceDeckPile>();
            controller.ChangeHealth(-offenceCards.OffenceCards.Count);

            burnCooldown--;
            EffectSound((int)effectEnum.Burned);
            if (burnCooldown <= 0) 
            { 
                isBurned = false;
                controller.effectEndEvent -= BurnPlayer;
                controller.DisplayEffect((int)effectEnum.Burned, false);
            }

        }
    }

    //Slow decreases the roll value by 20%
    public void SlowPlayer(object sender, EventArgs e)
    {
        //This checks if hasty is on, if it's not on then decrease roll value by 20%
        if (isSlowed && !buffs.IsHasty) 
        {
            controller.ChangeRoll(controller.GetModel.RollMultiplier - 0.2f);
            slowCooldown--;
            EffectSound((int)effectEnum.Slowed);
            if (slowCooldown <= 0)
            {
                isSlowed = false;
                controller.effectStartEvent -= SlowPlayer;
                controller.DisplayEffect((int)effectEnum.Slowed, false);
            }
        }

        //otherwise check if hasty is active
        //If it is then remove the debuff on player
        else if (buffs.IsHasty)
        {
            isSlowed = false;
            slowCooldown = 0;
            controller.effectStartEvent -= SlowPlayer;
            controller.DisplayEffect((int)effectEnum.Slowed, false);
        }
    }

    //Shock deals damage equal to the roll value
    public void ShockPlayer(object sender, EventArgs e)
    {
        shockCooldown--;
        EffectSound((int)effectEnum.Shocked);
        if (shockCooldown <= 0)
        {
            isShocked = false;
            controller.effectEndEvent -= ShockPlayer;
            controller.DisplayEffect((int)effectEnum.Shocked, false);
        }
    }

    //Expose Decrease Guard by 25%
    public void ExposePlayer(object sender, EventArgs e)
    {
        if (isExposed && !buffs.IsResistant)
        {
            controller.ChangeGuard(controller.GetModel.GuardMultiplier - 0.25f);
            exposeCooldown--;
            EffectSound((int)effectEnum.Exposed);
            if (exposeCooldown <= 0)
            {
                isExposed = false;
                controller.effectStartEvent -= ExposePlayer;
                controller.DisplayEffect((int)effectEnum.Exposed, false);

            }
        }

        //otherwise check if resistant is active
        //If it is then remove the debuff on player
        else if (buffs.IsResistant)
        {
            isExposed = false;
            exposeCooldown = 0;
            controller.effectStartEvent -= ExposePlayer;
            controller.DisplayEffect((int)effectEnum.Exposed, false);
        }
    }

    //Bleed Deals damage at the end of the turn depending on the cooldown (e.g, cooldown is 2 meaning Damage = 2)
    public void BleedPlayer(object sender, EventArgs e)
    {
        if (isBleeding)
        {
            controller.ChangeHealth(-bleedCooldown);
            bleedCooldown--;
            EffectSound((int)effectEnum.Bleeding);
            if (bleedCooldown <= 0)
            {
                isBleeding = false;
                controller.effectEndEvent -= BleedPlayer;
                controller.DisplayEffect((int)effectEnum.Bleeding, false);
            }
        }
    }

    //If the player is poison, then the player will decrease health at the end of their turn based on the amount of defence cards they have
    public void PoisonPlayer(object sender, EventArgs e)
    {
        if (isPoisoned)
        {
            defenceDeckPile defenceCards = GetComponentInChildren<defenceDeckPile>();
            controller.ChangeHealth(-defenceCards.DefenceCards.Count);


            poisonCooldown--;
            EffectSound((int)effectEnum.Poison);
            if (poisonCooldown <= 0)
            {
                isPoisoned = false;
                controller.effectEndEvent -= PoisonPlayer;
                controller.DisplayEffect((int)effectEnum.Poison, false);
            }

        }
    }

    //If the player is blistered, then the player will decrease health at the end of their turn based on the amount of movement cards they have
    public void BlisterPlayer(object sender, EventArgs e)
    {
        if (isBlistered)
        {
            movementDeckPile movementCards = GetComponentInChildren<movementDeckPile>();
            controller.ChangeHealth(-movementCards.MovementCards.Count);

            EffectSound((int)effectEnum.Blistered);
            blisterCooldown--;
            if (blisterCooldown <= 0)
            {
                isBlistered = false;               
                controller.effectEndEvent -= BlisterPlayer;
                controller.DisplayEffect((int)effectEnum.Blistered, false);
            }

        }
    }

    //If the player is Unstabled, then the player will decrease health at the end of their turn based on the amount of status cards they have
    public void UnstablePlayer(object sender, EventArgs e)
    {
        if (isUnstabled)
        {
            statusDeckPile statusCards = GetComponentInChildren<statusDeckPile>();
            controller.ChangeHealth(-statusCards.StatusCards.Count);

            EffectSound((int)effectEnum.Unstabled);
            unstableCooldown--;
            if (unstableCooldown <= 0)
            {
                isUnstabled = false;
                controller.effectEndEvent -= UnstablePlayer;
                controller.DisplayEffect((int)effectEnum.Unstabled, false);
            }

        }
    }

    //This ends the player's turn
    //In addition this prevents the player to defend
    public void StunPlayer(object sender, EventArgs e)
    {
        stunCooldown--;
        EffectSound((int)effectEnum.Stunned);
        if (stunCooldown <= 0)
        {
            isStunned = false;
            controller.effectEndEvent -= StunPlayer;
            controller.DisplayEffect((int)effectEnum.Stunned, false);
        }
    }

    //Fear Decreases Thrust by 25%
    public void FearPlayer(object sender, EventArgs e)
    {
        if (isFeared && !buffs.IsImpactful)
        {
            controller.ChangeThrust(controller.GetModel.ThrustMultiplier - 0.25f);
            fearCooldown--;
            EffectSound((int)effectEnum.Feared);
            if (fearCooldown <= 0)
            {
                isFeared = false;
                controller.effectStartEvent -= FearPlayer;
                controller.DisplayEffect((int)effectEnum.Feared, false);
            }
        }

        //otherwise check if impactful is active
        //If it is then remove the debuff on player
        else if (buffs.IsImpactful)
        {
            isFeared = false;
            fearCooldown = 0;
            controller.effectStartEvent -= FearPlayer;
            controller.DisplayEffect((int)effectEnum.Feared, false);
        }
    }

    //Confuse randomly selects the player's movement, defence & offence card
    //The player cannot use abilities or status card whilst confused
    public void ConfusePlayer(object sender, EventArgs e)
    {
        confusedCooldown--;
        EffectSound((int)effectEnum.Confused);
        if (confusedCooldown <= 0)
        {
            isConfused = false;
            controller.effectEndEvent -= ConfusePlayer;
            controller.DisplayEffect((int)effectEnum.Confused, false);
        }
    }

    //Blind randomly selects the player's direction
    public void BlindPlayer(object sender, EventArgs e)
    {
        blindCooldown--;
        EffectSound((int)effectEnum.Blind);
        if (blindCooldown <= 0)
        {
            isBlind = false;
            controller.effectEndEvent -= BlindPlayer;
            controller.DisplayEffect((int)effectEnum.Blind, false);
        }
    }

    public void EffectSound(int sound)
    {
        soundManager.PlaySound(effectSounds[sound]);
    }
}
