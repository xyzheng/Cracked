using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	InputManager im;
	public GameObject gbmObj;
	GameBoardManager gbm;
	//prefabs
	public GameObject pausePanel;
	public GameObject player;
	private Player playerScript;
	bool handledPlayerJump;
    bool handledPlayerLeap;
    bool leapMode;
    public GameObject entrance;
	public GameObject exit;
    //ui
    public Text levelText;
	public GameObject backtrack;
	private BackTrack btScript;
	public GameObject forwardtrack;
	private ForwardTrack ftScript;
    public GameObject jumpIcon;
    private Jump jumpScript;
    public GameObject leapIcon;
    private Leap leapScript;
    public GameObject pushIcon;
    private Push pushScript;
	//States
	enum GameState { TITLE, PAUSE, LOAD, PEEK, PLAY }
	GameState state;
	GameState priorState;
	enum LoadState { PLAYER, NEXT, BACK, FORWARD }
	LoadState lstate;
	private int level;
	private const int LEVEL_START = 0;

	// audio variables
	public static AudioSource[] aSrc;
	public static AudioClip crack;
	public static AudioClip hole;
	public static AudioClip fall;
	public static AudioClip jump;
	//public static AudioSource music;

	//sliders and toggles
	private Slider soundSlider;
	private Slider musicSlider;
	private Toggle soundToggle;
	private Toggle musicToggle;

    // rock push var
    bool rockPushed;

	//called before all Start()
	void Awake()
	{
		level = LEVEL_START;
		state = GameState.TITLE;
		im = new InputManager();
		gbmObj = (GameObject)Instantiate(gbmObj, new Vector3(0, 0, 0), Quaternion.identity);
		gbm = gbmObj.GetComponent<GameBoardManager>();
		//player
		player = (GameObject)Instantiate(player, new Vector3(0, 0, -2), Quaternion.identity);
		playerScript = player.GetComponent<Player>();
		handledPlayerJump = false;
        handledPlayerLeap = false;
        leapMode = false;
        // set up audio variables
        // 0: crack, 1: hole, 2: fall, 3: jump
        aSrc = GetComponents<AudioSource>();
		crack = aSrc[0].clip;
		hole = aSrc[1].clip;
		fall = aSrc[2].clip;
		jump = aSrc[3].clip;
        //soundSlider = GameObject.Find("Sound Slider").GetComponent<Slider>();
        rockPushed = false;
	}

	// Main update loop
	void Update () {
		if (state == GameState.TITLE)
		{
            //player lands on start
            if (!playerScript.duringMove) gbm.steppedOn((int)gbm.getStart().x, (int)gbm.getStart().y);
			//instantiate exit to right of goal/entrance to left
			Vector3 entrancePos = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
			Vector3 exitPos = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
			entrance = (GameObject)Instantiate(entrance, entrancePos, Quaternion.identity);
			exit = (GameObject)Instantiate(exit, exitPos, Quaternion.identity);
			//basic ui
			levelText.text = "Floor\n" + level.ToString();
			backtrack = (GameObject)Instantiate(backtrack, new Vector3(entrancePos.x, exitPos.y + 1, 0), Quaternion.identity);
			btScript = backtrack.GetComponent<BackTrack>();
			btScript.makeTransparent();
			forwardtrack = (GameObject)Instantiate(forwardtrack, new Vector3(exitPos.x, exitPos.y + 1, 0), Quaternion.identity);
			ftScript = forwardtrack.GetComponent<ForwardTrack>();
			ftScript.makeTransparent();
            jumpIcon = (GameObject)Instantiate(jumpIcon, new Vector3(entrancePos.x, exitPos.y, 0), Quaternion.identity);
            jumpScript = jumpIcon.GetComponent<Jump>();
            leapIcon = (GameObject)Instantiate(leapIcon, new Vector3(entrancePos.x, exitPos.y - 1, 0), Quaternion.identity);
            leapScript = leapIcon.GetComponent<Leap>();
            pushIcon = (GameObject)Instantiate(pushIcon, new Vector3(entrancePos.x, exitPos.y - 2, 0), Quaternion.identity);
            pushScript = pushIcon.GetComponent<Push>();
			state = GameState.PLAY;
			//update priorstate
			priorState = GameState.TITLE;
		}
		else if (state == GameState.PAUSE)
		{
            handleUnpause();
			//update prior state
			priorState = GameState.PAUSE;
		}
		else if (state == GameState.PLAY)
		{
			//check if done with peek
			if (priorState == GameState.PEEK)
			{
				gbm.unpeek();
				priorState = GameState.PLAY;
			}
			else
			{
				//synch animations
				if (!playerScript.isBusy())
				{
                    //stop pushing
                    if (rockPushed) { pushScript.stopShake(); }
                    //synch animations
                    if (!playerScript.duringMove) gbm.steppedOn((int)playerScript.getPosition().x, gbm.getCurrentHeight() - (int)playerScript.getPosition().y - 1);
					//handle jump
					if (playerScript.didJump() && !handledPlayerJump)
					{
						handleJump();
					}
				}
				//check ui
				if (gbm.backTrackPossible()) { btScript.makeFullColor(); }
				else { btScript.makeTransparent(); }
				if (gbm.forwardTrackPossible()) { ftScript.makeFullColor(); }
				else { ftScript.makeTransparent(); }
				//check input
				handlePlayInput();
				//update priorstate
				priorState = GameState.PLAY;
			}
		}
		else if (state == GameState.LOAD)
		{
			//wait until player done with animation
			if (lstate == LoadState.PLAYER && !playerScript.isBusy()){
				lstate = LoadState.NEXT;
                gbm.clearedCurrentBoard();
            }
            else if (lstate != LoadState.PLAYER && gbm.busy())
            {
                playerScript.hopInPlace();
            }
            else if (lstate != LoadState.PLAYER && !gbm.busy())
            {
                playerScript.forceUnfade();
                //wait until gbm done with zoom
                if (lstate == LoadState.NEXT) { loadNextFloor(); }
                else if (lstate == LoadState.BACK) { loadBacktrack(); }
                else if (lstate == LoadState.FORWARD) { loadForwardTrack(); }
                state = GameState.PLAY;
            }
			//update priorstate
			priorState = GameState.LOAD;
		}
		else if (state == GameState.PEEK)
		{
			//exit peeking
			if (Input.GetKeyUp(im.getPeekKey()))
			{
				levelText.text = "Floor\n" + level.ToString();
				playerScript.unfadePlayer();
                gbm.unpeek();
				state = GameState.PLAY;
			}
			//update priorstate
			priorState = GameState.PEEK;
		}
	}

	public void handleClearedFloor()
	{
		state = GameState.LOAD;
		lstate = LoadState.PLAYER;
		playerScript.moveRight();
	}
	public void loadNextFloor()
	{
		level += 1;
		//reset player stuff
		playerScript.setPosition(new Vector2(gbm.getStart().x, gbm.getCurrentHeight() - gbm.getStart().y - 1));
		playerScript.notJump();
        playerScript.notLeap();
		handledPlayerJump = false;
        handledPlayerLeap = false;
        rockPushed = false;
        //reset entrance/exit
        entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
		exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
		//ui stuff
		levelText.text = "Floor\n" + level.ToString();
		btScript.setPosition(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		ftScript.setPosition(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
        leapScript.makeFullColor();
        leapScript.untoggle();
        pushScript.stopShake();
        pushScript.makeFullColor();
        jumpScript.makeFullColor();
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
			if (!gbm.nextIsDamagedAt(x, y))  gbm.damageFutureBoard(x, y);
			//up from playerPos
			x = playerBoardPosX;
			y = playerBoardPosY - 1;
			gbm.damageCurrentBoard(x, y);
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//down
			x = playerBoardPosX;
			y = playerBoardPosY + 1;
			gbm.damageCurrentBoard(x, y);
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//left 
			x = playerBoardPosX - 1;
			y = playerBoardPosY;
			gbm.damageCurrentBoard(x, y);
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//right
			x = playerBoardPosX + 1;
			y = playerBoardPosY;
			gbm.damageCurrentBoard(x, y);
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//top left
			x = playerBoardPosX - 1;
			y = playerBoardPosY - 1;
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//top right
			x = playerBoardPosX + 1;
			y = playerBoardPosY - 1;
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//bot left
			x = playerBoardPosX - 1;
			y = playerBoardPosY + 1;
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//bot right
			x = playerBoardPosX + 1;
			y = playerBoardPosY + 1;
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//2 to the up
			x = playerBoardPosX;
			y = playerBoardPosY - 2;
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//2 to the left
			x = playerBoardPosX - 2;
			y = playerBoardPosY;
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//2 to the right
			x = playerBoardPosX + 2;
			y = playerBoardPosY;
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//2 to the bottom
			x = playerBoardPosX;
			y = playerBoardPosY + 2;
			if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
		}
	}
    public void handleLeap()        // healthyness isn't checked here because it is checked outside the function
    {                               // we only call handleLeap if our destination is healthy (uncracked floor)
        handledPlayerLeap = true;
        leapScript.startJump();
        //int playerBoardPosX = (int)playerScript.getPosition().x;        //get player pos
        //int playerBoardPosY = gbm.getCurrentHeight() - (int)playerScript.getPosition().y - 1;
        //int x = playerBoardPosX;            //at player pos
        //int y = playerBoardPosY;
        //if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
        //x = playerBoardPosX;              //up from player pos
        //y = playerBoardPosY - 1;
        //if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
        //x = playerBoardPosX;              //down
        //y = playerBoardPosY + 1;
        //if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
        //x = playerBoardPosX - 1;          //left
        //y = playerBoardPosY;
        //if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
        //x = playerBoardPosX + 1;          //right
        //y = playerBoardPosY;
        //if (!gbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
    }
    public void toggleLeapMode()
    {
        if (!handledPlayerLeap)
        {
            leapMode = !leapMode;
            leapScript.toggle();
        }
    }
    public void handleBacktrack()
	{
		if (gbm.backtrack())
		{
			state = GameState.LOAD;
			lstate = LoadState.BACK;
			gbm.goUp();
            playerScript.fadePlayer();
		}
	}
	public void loadBacktrack()
	{
		//reset player
		playerScript.setPosition(new Vector2(gbm.getStart().x, gbm.getCurrentHeight() - gbm.getStart().y - 1));
		playerScript.notJump();
        playerScript.notLeap();
        handledPlayerJump = false;
        handledPlayerLeap = false;
        leapMode = false;
        rockPushed = false;
        //clear prior keys list
        //im.clearPriorKeys();
        //change floor
        level -= 1;
		//reset entrance/exit
		entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
		exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
		//ui stuff
		levelText.text = "Floor\n" + level.ToString();
		btScript.setPosition(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		ftScript.setPosition(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
        leapScript.makeFullColor();
        leapScript.untoggle();
        pushScript.stopShake();
        pushScript.makeFullColor();
        jumpScript.makeFullColor();
	}
	public void handleForwardTrack()
	{
		if (gbm.forwardTrack())
		{
			state = GameState.LOAD;
			lstate = LoadState.FORWARD;
            //gbm.goDown
		}
	}
	public void loadForwardTrack()
	{
		//reset player
		playerScript.setPosition(new Vector2(gbm.getStart().x, gbm.getCurrentHeight() - gbm.getStart().y - 1));
		playerScript.notJump();
		handledPlayerJump = false;
        playerScript.notLeap();
        handledPlayerJump = false;
        handledPlayerLeap = false;
        leapMode = false;
        rockPushed = false;
        //clear prior keys list
        //im.clearPriorKeys();
        //change floor
        level += 1;
		//reset entrance/exit
		entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
		exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
		//ui stuff
		levelText.text = "Floor\n" + level.ToString();
		btScript.setPosition(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		ftScript.setPosition(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
        leapScript.makeFullColor();
        leapScript.untoggle();
        pushScript.stopShake();
        pushScript.makeFullColor();
        jumpScript.makeFullColor();
	}
	public void handlePeek()
	{
        levelText.text = "Next\nFloor";
		state = GameState.PEEK;
		gbm.peek();
        playerScript.fadePlayer();
	}

	//keyboard input
	public void handlePlayInput()
	{
		if (Input.GetKeyUp(im.getMoveUpKey()) && !playerScript.isBusy()) {
            //player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //if backtracked accept changes
            gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);	
		    //if not leap ...
            if (!leapMode) {
                //if up valid
                if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)) {
                    //player
                    player.GetComponent<Player>().moveUp();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //sound
				    aSrc[0].PlayOneShot(crack, 1.0f);
                }
                else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)
                    && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                    && !gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2))
                {
                    //rock push
                    pushScript.startShake();
                    rockPushed = true;
                    gbm.pushRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1, (int)playerBoardPosition.x, (int)playerBoardPosition.y - 2);
                    //player
                    player.GetComponent<Player>().moveUp();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //sound
                    aSrc[0].PlayOneShot(crack, 1.0f);
                }
                //else playerScript.hopInPlace();        //invalid move
                else playerScript.moveUpSmall();
            }
            else {
                if (gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2))  //check if up is valid & not blocked by rocks
                {
                    player.GetComponent<Player>().moveUp2();        //move player 2 tiles
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);       // play walking sound
                    handleLeap();
                }
                else
                {
                    //can't leap
                    leapScript.toggle(); 
                    //playerScript.hopInPlace();
                    playerScript.moveUpSmall();
                }
                leapMode = false;
            }
        }
		else if (Input.GetKeyUp(im.getMoveDownKey()) && !playerScript.isBusy())
		{
            //player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //if backtracked accept changes
            gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);			
            if (!leapMode)
            {
                if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1))          //check if move is valid
                {
                    player.GetComponent<Player>().moveDown();               //move player
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    //im.addToPriorKeys(im.getMoveDownKey());                //add to prior keys list
                }
                else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)
                    && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                    && !gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2))
                {
                    rockPushed = true;                // can move a block down

                    pushScript.startShake();
                    gbm.pushRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1, (int)playerBoardPosition.x, (int)playerBoardPosition.y + 2);
                    player.GetComponent<Player>().moveDown();                // move player
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                }
                //else playerScript.hopInPlace();     //do the growing animation, hopping
                else playerScript.moveDownSmall();
            }
            else        // Leap mode on
            {
                if (gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2))
                {
                    player.GetComponent<Player>().moveDown2();               //move player 2 tiles
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    handleLeap();
                }
                else
                {
                    //can't leap
                    leapScript.toggle();
                    //playerScript.hopInPlace();     //do the growing animation, hopping
                    playerScript.moveDownSmall();
                }
                leapMode = false;
            }
        }
		else if (Input.GetKeyUp(im.getMoveLeftKey()) && !playerScript.isBusy())
		{
		    //player's y is flipped
		    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //if backtracked accept changes
            gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);			
            if (!leapMode)
            {
                if (gbm.canMoveTo((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y))          //check if move is valid
                {
                    player.GetComponent<Player>().moveLeft();               //move player
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    //im.addToPriorKeys(im.getMoveLeftKey());                //add to prior keys list
                }
                else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)
                    && gbm.currentIsHealthyAt((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                    && !gbm.hasRock((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y))
                {
                    rockPushed = true;                // can move a block left

                    pushScript.startShake();
                    gbm.pushRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y, (int)playerBoardPosition.x - 2, (int)playerBoardPosition.y);
                    player.GetComponent<Player>().moveLeft();                // move player

                    gbm.hasRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y);

                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound

                    gbm.hasRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y);
                }
                //else playerScript.hopInPlace();     //do the growing animation, hopping
                else playerScript.moveLeftSmall();
            }
            else        // Leap mode on
            {
                if (gbm.currentIsHealthyAt((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y))          //check if move is valid
                {
                    player.GetComponent<Player>().moveLeft2();               //move player 2 tiles
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    handleLeap();
                }
                else
                {

                    //can't leap
                    leapScript.toggle();
                    //playerScript.hopInPlace();     //do the growing animation, hopping
                    playerScript.moveLeftSmall();
                }
                leapMode = false;
            }
        }
		else if (Input.GetKeyUp(im.getMoveRightKey()) && !playerScript.isBusy())
		{
			//check if move valid - player's y is flipped
			Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //if backtracked accept changes
            gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);			
            //check if at goal      //only move to next level when you press right and are at goal
            if (gbm.getGoal() == playerBoardPosition) handleClearedFloor();
            if (!leapMode)
            {
                if (gbm.canMoveTo((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y))         //check if move valid
                {
                    player.GetComponent<Player>().moveRight();                //move player
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    //im.addToPriorKeys(im.getMoveRightKey());                //add to prior keys list
                }
                else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)
                    && gbm.currentIsHealthyAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                    && !gbm.hasRock((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y))
                {
                    rockPushed = true;                // can move a block right
                    pushScript.startShake();
                    gbm.pushRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y, (int)playerBoardPosition.x + 2, (int)playerBoardPosition.y);
                    player.GetComponent<Player>().moveRight();                // move player
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                }
                //else playerScript.hopInPlace();      //do the growing animation, hopping
                else playerScript.moveRightSmall();
            }
            else        // Leap mode on
            {
                if (gbm.currentIsHealthyAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y))         //check if move valid
                {
                    player.GetComponent<Player>().moveRight2();                //move player
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    handleLeap();
                }
                else
                {
                    //can't leap
                    leapScript.toggle();
                    //playerScript.hopInPlace();
                    playerScript.moveRightSmall();
                }
                leapMode = false;
            }
        }
		else if (!playerScript.didJump() && Input.GetKeyUp(im.getJumpKey()) && !playerScript.isBusy())
		{
			//check if move valid - player's y is flipped
			Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
			if (gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y))
			{
				//if backtracked accept changes
                gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                jumpScript.startJump();
				playerScript.jump();
				//add to prior keys list
				//im.addToPriorKeys(im.getJumpKey());
			}
		}
		else if (Input.GetKeyUp(im.getResetBoardKey())) { resetBoard(); }
		else if (Input.GetKeyUp(im.getPauseKey()) && state == GameState.PLAY) { handlePause(); }
		else if (Input.GetKeyUp(im.getBacktrackKey())) { handleBacktrack(); }
		else if (Input.GetKeyUp(im.getForwardTrackKey())) { handleForwardTrack(); } 
		else if (Input.GetKeyUp(im.getPeekKey())) { handlePeek(); }
        else if (Input.GetKeyUp(im.getDebugKey())) { debug(); }
        else if (Input.GetKeyUp(im.getToggleLeapKey()) && !playerScript.isBusy()) { toggleLeapMode(); }
        // rockPushed = false; // uncomment for unlimited pushing
    }

    //reset
    public void resetBoard()
	{
		//reset player position
		playerScript.setPosition(new Vector2(gbm.getStart().x, gbm.getCurrentHeight() - gbm.getStart().y - 1));
		//reset player jumped
		playerScript.notJump();
		handledPlayerJump = false;
        handledPlayerLeap = false;
        leapMode = false;
        rockPushed = false;
        gbm.resetBoard(level == LEVEL_START);
		//player lands on start
		gbm.steppedOn((int)gbm.getStart().x, (int)gbm.getStart().y);
		//clear prior keys list
		//im.clearPriorKeys();
		//reset entrance/exit
		entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
		exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
        leapScript.makeFullColor();
        leapScript.untoggle();
        pushScript.stopShake();
        pushScript.makeFullColor();
        jumpScript.makeFullColor();
	}
	public void resetGame()
	{
		level = LEVEL_START;
		levelText.text = "Floor\n" + level.ToString();
		im = new InputManager();
		gbm.clear();
		//player lands on start
		gbm.steppedOn((int)gbm.getStart().x, (int)gbm.getStart().y);
		//player
		playerScript.reset((int)gbm.getStart().x, (int)(gbm.getCurrentHeight() - gbm.getStart().y - 1));
		handledPlayerJump = false;
        handledPlayerLeap = false;
        leapMode = false;
        rockPushed = false;
        //reset entrance/exit
        entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
		exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
        leapScript.makeFullColor();
        leapScript.untoggle();
        pushScript.stopShake();
        pushScript.makeFullColor();
        jumpScript.makeFullColor();
	}

    /* SOUND */
    //sound slider
    public void updateSoundSlider () {
		soundSlider = GameObject.Find("Sound Slider").GetComponent<Slider>();
		for (int i=0; i<aSrc.Length; i++) {
			aSrc[i].volume = soundSlider.value;
		}
	}
	//sound toggle
	public void toggleSound () {
		soundToggle = GameObject.Find("Sound Toggle").GetComponent<Toggle>();
		soundSlider = GameObject.Find("Sound Slider").GetComponent<Slider>();
		for (int i=0; i<aSrc.Length; i++) {
			if (!soundToggle.isOn) {
				aSrc[i].volume = 0f;
			}
			else {
				aSrc[i].volume = soundSlider.value;
			}
		}
	}
	//music slider
	public void updateMusicSlider () {
		musicSlider = GameObject.Find("Music Slider").GetComponent<Slider>();
		//	music.volume = musicSlider.value;
	}
	public void toggleMusic () {
		musicToggle = GameObject.Find("Music Toggle").GetComponent<Toggle>();
		musicSlider = GameObject.Find("Music Slider").GetComponent<Slider>();
		if (!musicToggle.isOn) {
			//music.volume = 0f;
		}
		else {
			//	music.volume = musicSlider.value;
		}
	}

    /* PAUSE PANEL */
    public void handlePause()
    {
        pausePanel.SetActive(true);
        state = GameState.PAUSE;
    }

    public void handleUnpause()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            pausePanel.SetActive(false);
            state = GameState.PLAY;
        }
    }
	public void continueButton () {
		pausePanel.SetActive(false);
		state = GameState.PLAY;
	}
	public void mainMenuButton () {
		Application.LoadLevel("MainMenu");
	}

    public void debug()
    {
        resetGame();
        gbm.bbm.nextPlaceRockAt(2, 3);
        gbm.bbm.damageNextBoard(2, 1);
        gbm.bbm.damageNextBoard(2, 1);
    }
}
