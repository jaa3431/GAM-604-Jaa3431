using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

/// <summary>
/// First Playable: The move state is to move around the spaces by providing a path and move around based on the order of the path
/// The first and last objects in the path must be MultiPathChoice prefabs, the rest are spaces
/// The method to check if the player has reach the destination requires a raycast to hit the targeted space
/// Once the raycast hits the correct target the object can then move onto the next target space in the path
/// The move state carries on until the player has reach to the last object in the path (which should be a MultiPathChoice) or movement becomes 0
/// TODO: The player can also change when colliding with the opponent on the same space, hence will cause a battle to begin
/// </summary>

public class moveState : playerStateBase
{
    //The movement is use to check the amount of move value it has left before ending the player's turn
    private int movement;

    //This is the movement speed along with the ray that is use to detect the player on reaching to the targeted space
    private float speed = 2f;
    private Ray detectRay;
    private Vector3 rayPosition;

    //These booleans are use to change to a specific state
    //changeDirection is used to change the current state to choosing state
    private bool changeDirection;
    private bool beginCombat;

    //The player controller is use to collect the roll value
    //There is a coroutine which when started will allow the player to move around the board until eaching a multi path choice or movement becomes 0
    private playerController controller;
    private Coroutine movePlayer;
    private Coroutine lookPlayer;

    //The direction enum is use to provide the restriction of choosing a certain path
    private directionEnum currentDirection;

    //The current path object provides the path order which is use to provide the order for the player to move around
    //the integer is referencing the list of objects, which the current value being the current space and +1 will consider the targeted space in that list
    private GameObject currentPath;

    //the current space is the object that the raycast is detected last as the targeted object
    //the space enum checks which type of space the player is currently landed on
    private GameObject currentSpace;
    public GameObject CurrentSpace
    {
        get { return currentSpace; } 
    }

    private pathOrder pathOrder;
    private spaceEnum currentSpaceType;

    //The effect manager applies the space effect that the player is currently on when movement becomes 0
    private spaceManager spaceManager;

    //The music manager applies changing the music from board map music to battle music
    private musicManager musicManager;

    //the target space is the next space after the currentSpace from the currentPath list
    private GameObject targetSpace;

    //A vector 3 is used to move & look the player around the board
    private Vector3 spacePosition;
    private Vector3 spaceRotation;

    [SerializeField] private GameObject opponentDetected;
    private bool invincibleBattle;

    public event EventHandler combatEngage;

    public event EventHandler endTurnEvent;

    [Header("User Interface")]
    [SerializeField] private TMP_Text eventText;

    public event EventHandler beginMoveEvent;
    public event EventHandler beginItemMoveEvent;

    [Header("Sound Effect")]
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip combatSound;
    private soundManager soundManager;

    [Header("Animation")]
    stateAnimation stateAnimation;

    public override void EnterState(playerStateManager player)
    {
        //This checks if the previous state was the choosing state in which resets the current space integer
        //This means that a new path is used for the player to follow around the map
        if(player.PreviousState == player.ChoosingState)
        {
            controller.CurrentSpaceInt = 0;
        }

        //This collects the infromation from the controller based on which path the player is currently on
        controller = GetComponent<playerController>();
        currentPath = controller.Path;

        //The current path provides the direction and the list of spaces in order for the player to follow
        //INFO: The direction is based on when the player chose the direction from the chosenState
        //This doesn't mean that they will always move in the direction
        pathOrder = currentPath.GetComponent<pathOrder>();
        currentDirection = pathOrder.Direction;

        //The currentSpace is the current integer of the currentSpaceInteger
        //Whilst targetSpace is the current integer +1 of the currentSpaceInteger
        currentSpace = pathOrder.SpaceOrder[controller.CurrentSpaceInt];
        targetSpace = pathOrder.SpaceOrder[controller.CurrentSpaceInt + 1];

        //This is to call the space effects towards ending the turn with a space effect occurring
        spaceManager = Singleton<spaceManager>.Instance;

        //This is to call the music manager by using a singleton
        musicManager = Singleton<musicManager>.Instance;

        //This is to call the sound manager using a singleton
        soundManager = Singleton<soundManager>.Instance;

        //This is to check if the previous state was the roll state
        //If the previous state was the roll state then the movement becomes the roll value of the controller
        //otherwise the movement's integer stays as it is
        if (player.PreviousState == player.RollState)
        {
            beginMoveEvent?.Invoke(this, EventArgs.Empty);
            beginItemMoveEvent?.Invoke(this, EventArgs.Empty);
            movement = controller.GetModel.RollValue;
        }

        eventText.SetText(movement.ToString());
        
        //These are the booleans that require to become false when entering this state to prevent instantly movinhg to the next state
        changeDirection = false;
        beginCombat = false;

        opponentDetected = null;

        
        combatEngage += CombatSound;
        combatEngage += musicManager.BattleMusic;
        combatEngage += AttackCombat;

        //The coroutine will start moving the player around the board
        movePlayer = StartCoroutine(Moving());
        lookPlayer = StartCoroutine(Looking());

        stateAnimation = GetComponentInChildren<stateAnimation>();
        endTurnEvent += stateAnimation.StopWalking;
    }

    public override void UpdateState(playerStateManager player)
    {

        //When changeDirection becomes true, the choosing state will need to collect the multi path object the player is currently on
        //along with the current direction the player went, this will then change the state to choosing state.
        if (changeDirection) 
        {
            choosingState choosing = player.ChoosingState.GetComponent<choosingState>();
            choosing.CollectCurrentPath(pathOrder.SpaceOrder[controller.CurrentSpaceInt], currentDirection);
            player.ChangeState(player.ChoosingState);
        }

        if (beginCombat) 
        { 
            player.ChangeState(player.AttackState);
        }


    }

    //the raycast requires physics to provide the position of the ray, the rotation
    private void FixedUpdate()
    {
        rayPosition = new Vector3(transform.position.x,  transform.position.y - 0.5f, transform.position.z);

        detectRay = new Ray(rayPosition, -transform.up);
    }

    //when exiting this state, the method should stop the coroutine to prevent the player moving
    public override void ExitState(playerStateManager player) 
    {        
        combatEngage -= musicManager.BattleMusic;
        combatEngage -= CombatSound;
        combatEngage -= AttackCombat;
        endTurnEvent -= stateAnimation.StopWalking;
        StopCoroutine(movePlayer);
        StopCoroutine(lookPlayer);
    }

    //this method provide a integer parameter of the next space to change the current targted space to the new current space & provide a new target to follow
    void ChangeTarget(int nextSpace)
    {
        //the new currentSpace is the currentSpaceInt which has been incremented from the coroutine
        currentSpace = pathOrder.SpaceOrder[controller.CurrentSpaceInt];

        //This changes the space type based on the new current space
        spaceBehaviour type = pathOrder.SpaceOrder[controller.CurrentSpaceInt].transform.GetComponent<spaceBehaviour>();
        currentSpaceType = type.SpaceType;

        //A new target space is created for the player to follow
        targetSpace = pathOrder.SpaceOrder[controller.CurrentSpaceInt + 1];

        //The movement integer is decremented
        movement--;
        soundManager.PlaySound(moveSound);
        eventText.SetText(movement.ToString());

        if (invincibleBattle)
        {
            invincibleBattle = false;
        }

        CheckCombat();
        
    }

    //This coroutine is used to move the player around the board based on their targeted space
    IEnumerator Moving()
    {
        //This while loop carries on moving the player and detects if the player has reach the space
        while (movement > 0)
        {
            spacePosition = new Vector3(targetSpace.transform.position.x, 2f, targetSpace.transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, spacePosition, speed * Time.deltaTime);

            
            //if the raycast detects an object, then the detected object has to be the targetedSpace object
            if (Physics.Raycast(detectRay, out RaycastHit info, 10f))
            {
                //If the detected object is the target space then increment the current space int
                if (info.collider.gameObject == targetSpace.gameObject)
                {
                    controller.CurrentSpaceInt++;

                    //this then checks if the targeted space was either a regular space or a multi path object
                    //if the object was a multi path (also known as the last object in the path list)
                    //then change the state to choosing state
                    if(controller.CurrentSpaceInt + 1 == pathOrder.SpaceOrder.Count)
                    {
                        changeDirection = true;
                    }
                    //otherwise change the target to the next space from the list
                    else
                    {
                        ChangeTarget(controller.CurrentSpaceInt);
                    }
                    
                }
                               
            }
            yield return null;
        }

        //Once the loop is over (which is when movement reaches 0), apply the space effect based on the type of space the player is currently on
        eventText.SetText("Landed on: " + currentSpaceType + " space");
        endTurnEvent?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(2);
        spaceManager.ActivateEffect(this.gameObject, currentSpaceType);

    }

    //This method uses a sphere collider to check if there's an opponent
    private void OnTriggerEnter(Collider opponent)
    {
        if(opponent.gameObject.tag == "Player")
        {
            opponentDetected = opponent.gameObject;
            inactiveState opponentState = opponent.GetComponent<inactiveState>();
            combatEngage += opponentState.DefendCombat;
        }
    }

    private void OnTriggerExit(Collider opponent)
    {
        if (opponent.gameObject.tag == "Player")
        {
            inactiveState opponentState = opponent.GetComponent<inactiveState>();
            combatEngage -= opponentState.DefendCombat;
            opponentDetected = null;
        }
    }

    IEnumerator Looking()
    {
        while(movement > 0)
        {
            spaceRotation = new Vector3(targetSpace.transform.position.x, 2f, targetSpace.transform.position.z);
            Quaternion lookRotation = Quaternion.LookRotation(spaceRotation - transform.position);
            float time = 0;
            while(time < 1)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
                time += Time.deltaTime;
                yield return null;
            }

            yield return null;
        }

        yield return null;
    }

    void CheckCombat()
    {
        if(opponentDetected != null && !invincibleBattle)
        {

            invincibleBattle = true;
            combatEngage?.Invoke(this, EventArgs.Empty);           
        }
    }
    
    public void AttackCombat(object sender, EventArgs e)
    {
        beginCombat = true;
    }

    public void CombatSound(object sender, EventArgs e)
    {
        soundManager.PlaySound(combatSound);
    }
}
