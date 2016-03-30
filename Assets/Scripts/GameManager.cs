using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    InputManager im;
    public GameObject gbmObj;
    GameBoardManager gbm;
    //prefabs
    public GameObject player;
    Player playerScript;
    bool handledPlayerJump;
    public GameObject entrance;
    public GameObject exit;
    public Text levelText;
    //GameStates
    enum GameState { TITLE, PAUSE, PLAY }
    GameState state;

    int level;
    int LEVEL_START = 0;

    // audio variables
    public static AudioSource[] aSrc;
    public static AudioClip crack;
    public static AudioClip hole;
    public static AudioClip fall;
    public static AudioClip jump;

    //called before all Start()
    void Awake()
    {
        level = LEVEL_START;
        state = GameState.TITLE; //this should be title ..
        im = new InputManager();
        gbmObj = (GameObject)Instantiate(gbmObj, new Vector3(0, 0, 0), Quaternion.identity);
        gbm = gbmObj.GetComponent<GameBoardManager>();
        //player
        player = (GameObject)Instantiate(player, new Vector3(0, 0, -2), Quaternion.identity);
        playerScript = player.GetComponent<Player>();
        handledPlayerJump = false;
        // set up audio variables
        // 0: crack, 1: hole, 2: fall, 3: jump
        aSrc = GetComponents<AudioSource>();
        crack = aSrc[0].clip;
        hole = aSrc[1].clip;
        fall = aSrc[2].clip;
        jump = aSrc[3].clip;
    }

	// Main update loop
	void Update () {
        if (state == GameState.TITLE)
        {
            //player lands on start
            gbm.steppedOn((int)gbm.getStart().x, (int)gbm.getStart().y);
            //instantiate exit to right of goal/entrance to left
            entrance = (GameObject)Instantiate(entrance, new Vector3(gbm.getStart().x -1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0), Quaternion.identity);
            exit = (GameObject)Instantiate(exit, new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0), Quaternion.identity);
            state = GameState.PLAY;
        }
        else if (state == GameState.PAUSE)
        {
        }
        else if (state == GameState.PLAY)
        {
            //levelText
            levelText.text = "Floors descended: " + level.ToString();
            
            //synch animations
            if (!playerScript.isBusy())
            {
                //synch animations
                gbm.steppedOn((int)playerScript.getPosition().x, gbm.getCurrentHeight() - (int)playerScript.getPosition().y - 1);
                //handle jump
                if (playerScript.didJump() && !handledPlayerJump)
                {
                    handleJump();
                }
            }
            //check input
            handleInput();
            //check reach goal
            if (gbm.getGoal() == new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1) && !playerScript.isBusy()) { handleClearedFloor(); }
        }
	}
    public void handleClearedFloor()
    {
        level += 1;
    //clear priorKeys
        //im.clearPriorKeys();
    //gbm
        gbm.clearedCurrentBoard(); 
    //reset player stuff
        playerScript.setPosition(new Vector2(gbm.getStart().x, gbm.getCurrentHeight() - gbm.getStart().y - 1));
        playerScript.notJump();
        handledPlayerJump = false;
        //reset entrance/exit
        entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
        exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
        state = GameState.PLAY;

    }
    public void handleJump()
    {
        handledPlayerJump = true;
        //get player pos
        int playerBoardPosX = (int) playerScript.getPosition().x;
        int playerBoardPosY = gbm.getCurrentHeight() - (int)playerScript.getPosition().y - 1;
        //check if on solid ground
        if (gbm.currentIsHealthyAt(playerBoardPosX, playerBoardPosY)) {
            //player jump
            //at player pos
            int x = playerBoardPosX;
            int y = playerBoardPosY;
            gbm.damageCurrentBoard(x, y);
            gbm.damageFutureBoard(x, y);
            //up from playerPos
            x = playerBoardPosX;
            y = playerBoardPosY - 1;
            gbm.damageCurrentBoard(x, y);
            gbm.damageFutureBoard(x, y);
            //down
            x = playerBoardPosX;
            y = playerBoardPosY + 1;
            gbm.damageCurrentBoard(x, y);
            gbm.damageFutureBoard(x, y);
            //left 
            x = playerBoardPosX - 1;
            y = playerBoardPosY;
            gbm.damageCurrentBoard(x, y);
            gbm.damageFutureBoard(x, y);
            //right
            x = playerBoardPosX + 1;
            y = playerBoardPosY;
            gbm.damageCurrentBoard(x, y);
            gbm.damageFutureBoard(x, y);
            //top left
            x = playerBoardPosX - 1;
            y = playerBoardPosY - 1;
            gbm.damageFutureBoard(x, y);
            //top right
            x = playerBoardPosX + 1;
            y = playerBoardPosY - 1;
            gbm.damageFutureBoard(x, y);
            //bot left
            x = playerBoardPosX - 1;
            y = playerBoardPosY + 1;
            gbm.damageFutureBoard(x, y);
            //bot right
            x = playerBoardPosX + 1;
            y = playerBoardPosY + 1;
            gbm.damageFutureBoard(x, y);
            //2 to the up
            x = playerBoardPosX;
            y = playerBoardPosY - 2;
            gbm.damageFutureBoard(x, y);
            //2 to the left
            x = playerBoardPosX - 2;
            y = playerBoardPosY;
            gbm.damageFutureBoard(x, y);
            //2 to the right
            x = playerBoardPosX + 2;
            y = playerBoardPosY;
            gbm.damageFutureBoard(x, y);
            //2 to the bottom
            x = playerBoardPosX;
            y = playerBoardPosY + 2;
            gbm.damageFutureBoard(x, y);
        }
    }
    public void handleBacktrack()
    {
        if (gbm.backtrack())
        {
            //reset player
            playerScript.setPosition(new Vector2(gbm.getStart().x, gbm.getCurrentHeight() - gbm.getStart().y - 1));
            playerScript.notJump();
            handledPlayerJump = false;
            //clear prior keys list
            //im.clearPriorKeys();
            //change floor
            level -= 1;
            //reset entrance/exit
            entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
            exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
        }
    }
    public void handleForwardTrack()
    {
        if (gbm.forwardTrack())
        {
            //reset player
            playerScript.setPosition(new Vector2(gbm.getStart().x, gbm.getCurrentHeight() - gbm.getStart().y - 1));
            playerScript.notJump();
            handledPlayerJump = false;
            //clear prior keys list
            //im.clearPriorKeys();
            //change floor
            level += 1;
            //reset entrance/exit
            entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
            exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
        }
    }
    //keyboard input
    public void handleInput()
    {
        if (Input.GetKeyUp(im.getMoveUpKey()) && !playerScript.isBusy()) {
            //if backtracked accept changes
            gbm.moveWhileBacktrack();
            //player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //check if up is valid & not blocked by rocks
            if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1))
            {
                //move player
                player.GetComponent<Player>().moveUp();
                gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                // play walking sound
                aSrc[0].PlayOneShot(crack, 1.0f);
                //add to prior keys list
                //im.addToPriorKeys(im.getMoveUpKey());
            }
            else
            {
                //do the growing animation, hopping
                playerScript.hopInPlace();
            }
        }
        else if (Input.GetKeyUp(im.getMoveDownKey()) && !playerScript.isBusy())
        {
            //if backtracked accept changes
            gbm.moveWhileBacktrack();
            //player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //check if move is valid
            if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1))
            {
                //move player
                player.GetComponent<Player>().moveDown();
                gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                // play walking sound
                aSrc[0].PlayOneShot(crack, 1.0f);
                //add to prior keys list
                //im.addToPriorKeys(im.getMoveDownKey());
            }
            else
            {
                //do the growing animation, hopping
                playerScript.hopInPlace();
            }
        }
        else if (Input.GetKeyUp(im.getMoveLeftKey()) && !playerScript.isBusy())
        {
            //if backtracked accept changes
            gbm.moveWhileBacktrack();
            //player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //check if move is valid
            if (gbm.canMoveTo((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y))
            {
                //move player
                player.GetComponent<Player>().moveLeft();
                gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                // play walking sound
                aSrc[0].PlayOneShot(crack, 1.0f);
                //add to prior keys list
                //im.addToPriorKeys(im.getMoveLeftKey());
            }
            else
            {
                //do the growing animation, hopping
                playerScript.hopInPlace();
            }
        }
        else if (Input.GetKeyUp(im.getMoveRightKey()) && !playerScript.isBusy())
        {
            //if backtracked accept changes
            gbm.moveWhileBacktrack();
            //check if move valid - player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //check if move valid
            if (gbm.canMoveTo((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y))
            {
                //move player
                player.GetComponent<Player>().moveRight();
                gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                // play walking sound
                aSrc[0].PlayOneShot(crack, 1.0f);
                //add to prior keys list
                //im.addToPriorKeys(im.getMoveRightKey());
            }
            else
            {
                //do the growing animation, hopping
                playerScript.hopInPlace();
            }
        }
        else if (!playerScript.didJump() && Input.GetKeyUp(im.getJumpKey()) && !playerScript.isBusy())
        {
            //check if move valid - player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            if (gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y))
            {
                //if backtracked accept changes
                gbm.moveWhileBacktrack();
                playerScript.jump();
                //add to prior keys list
                //im.addToPriorKeys(im.getJumpKey());
            }
        }
        else if (Input.GetKeyUp(im.getResetBoardKey())) { resetBoard(); }
        else if (Input.GetKeyUp(im.getPauseKey()))
        {
            //pause the game
            //currentState = GameState.PAUSE;
        }
        else if (Input.GetKeyUp(im.getBacktrackKey())) { handleBacktrack(); }
        else if (Input.GetKeyUp(im.getForwardTrackKey())) { handleForwardTrack(); } 
        else if (Input.GetKeyUp(im.getResetGameKey())){ resetGame(); }
    }

    //reset
    public void resetBoard()
    {
        //reset player position
        playerScript.setPosition(new Vector2(gbm.getStart().x, gbm.getCurrentHeight() - gbm.getStart().y - 1));
        //reset player jumped
        playerScript.notJump();
        handledPlayerJump = false;
        gbm.resetBoard(level == LEVEL_START);
        //player lands on start
        gbm.steppedOn((int)gbm.getStart().x, (int)gbm.getStart().y);
        //clear prior keys list
        //im.clearPriorKeys();
        //reset entrance/exit
        entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
        exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
    }
    public void resetGame()
    {
        level = LEVEL_START;
        im = new InputManager();
        gbm.clear();
        //player lands on start
        gbm.steppedOn((int)gbm.getStart().x, (int)gbm.getStart().y);
        //player
        playerScript.reset((int)gbm.getStart().x, (int)(gbm.getCurrentHeight() - gbm.getStart().y - 1));
        handledPlayerJump = false;
        //reset entrance/exit
        entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
        exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
    }
}
