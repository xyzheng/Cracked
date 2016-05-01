using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    public Text currentLevelText;
    public Text nextLevelText;
	public GameObject btIcon;
	private BackTrack btScript;
	public GameObject ftIcon;
	private ForwardTrack ftScript;
    public GameObject jumpIcon;
    private Jump jumpScript;
    public GameObject leapIcon;
    private Leap leapScript;
    public GameObject pushIcon;
    private Push pushScript;
    public GameObject eye;
    public GameObject peekKey;
    public GameObject peekName;
    private Eye eyeScript;
	private Fade fadeScript;
	//States
    enum GameState { TITLE, PAUSE, LOAD, PEEK, PLAY_ENDLESS, PLAY_ARCADE }
	GameState state;
	GameState priorState;
    enum ArcadeState { IDLE, JUMP, LEAP, PUSH, EWP, TRE }
    ArcadeState astate = ArcadeState.IDLE;
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
		fadeScript = GetComponent<Fade>();
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
			currentLevelText.text = "Floor\n" + level.ToString();
            nextLevelText.text = "Floor " + (level + 1).ToString();
			btIcon = (GameObject)Instantiate(btIcon, new Vector3(entrancePos.x + 2, exitPos.y + 1, 0), Quaternion.identity);
			btScript = btIcon.GetComponent<BackTrack>();
			btScript.makeTransparent();
			ftIcon = (GameObject)Instantiate(ftIcon, new Vector3(exitPos.x - 2, exitPos.y + 1, 0), Quaternion.identity);
			ftScript = ftIcon.GetComponent<ForwardTrack>();
			ftScript.makeTransparent();
            jumpIcon = (GameObject)Instantiate(jumpIcon, new Vector3(entrancePos.x - 0.2f, exitPos.y + 0.0f, 0), Quaternion.identity);
            jumpScript = jumpIcon.GetComponent<Jump>();
            leapIcon = (GameObject)Instantiate(leapIcon, new Vector3(entrancePos.x - 0.2f, exitPos.y - 1.0f, 0), Quaternion.identity);
            leapScript = leapIcon.GetComponent<Leap>();
            pushIcon = (GameObject)Instantiate(pushIcon, new Vector3(entrancePos.x - 0.2f, exitPos.y - 2.0f, 0), Quaternion.identity);
            pushScript = pushIcon.GetComponent<Push>();
            eyeScript = eye.GetComponent<Eye>();
			state = GameState.PLAY_ENDLESS;
			//update priorstate
			priorState = GameState.TITLE;
		}
		else if (state == GameState.PAUSE)
		{
			if (fadeScript.fadingPanel.alpha == 1) {
				SceneManager.LoadScene ("MainMenu");
			}
            handleUnpause();
			//update prior state
			priorState = GameState.PAUSE;
		}
		else if (state == GameState.PLAY_ENDLESS)
		{
            //remove this with main menu stuff
			if (Input.GetKeyUp(KeyCode.T))
            {
                level = 0;
                state = GameState.PLAY_ARCADE;
                astate = ArcadeState.JUMP;
                gbm.loadJumpLevel(level);
                priorState = GameState.PLAY_ENDLESS;
                jumpIcon.SetActive(true);
                leapIcon.SetActive(false);
                pushIcon.SetActive(false);
                eye.SetActive(false);
                nextLevelText.text = "";
                btIcon.SetActive(false);
                ftIcon.SetActive(false);
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
                currentLevelText.text = "Floor\n" + level.ToString();
                Debug.Log("Loading Jump levels");
            }
            if (Input.GetKeyUp(KeyCode.Y))
            {
                level = 0;
                state = GameState.PLAY_ARCADE;
                astate = ArcadeState.LEAP;
                gbm.loadLeapLevel(level);
                priorState = GameState.PLAY_ENDLESS;
                jumpIcon.SetActive(false);
                leapIcon.SetActive(true);
                leapScript.makeFullColor();
                pushIcon.SetActive(false);
                eye.SetActive(false);
                nextLevelText.text = "";
                btIcon.SetActive(false);
                ftIcon.SetActive(false);
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
                currentLevelText.text = "Floor\n" + level.ToString();
                Debug.Log("Loading Leap levels");
            }
            if (Input.GetKeyUp(KeyCode.U))
            {
                level = 0;
                state = GameState.PLAY_ARCADE;
                astate = ArcadeState.PUSH;
                gbm.loadPushLevel(level);
                priorState = GameState.PLAY_ENDLESS;
                jumpIcon.SetActive(false);
                leapIcon.SetActive(false);
                pushIcon.SetActive(true);
                eye.SetActive(false);
                nextLevelText.text = "";
                btIcon.SetActive(false);
                ftIcon.SetActive(false);
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
                currentLevelText.text = "Floor\n" + level.ToString();
                Debug.Log("Loading Push levels");
            }

			//check if done with peek
			if (priorState == GameState.PEEK)
			{
				gbm.unpeek();
				priorState = GameState.PLAY_ENDLESS;
			}
			else
			{
				//synch animations
				if (!playerScript.isBusy())
				{
                    //stop pushing
                    if (rockPushed) { pushScript.stopShake(); }
                    //synch animations
                    if (!playerScript.duringMove)       // The tile gets cracked once the player steps on it (and is done hopping)
                        gbm.steppedOn((int)playerScript.getPosition().x, gbm.getCurrentHeight() - (int)playerScript.getPosition().y - 1);
					//handle jump
					if (playerScript.didJump() && !handledPlayerJump) { 
                        handleJump();
                        //update icons
                    }
                    handleIcons();
                    // There should be no red tiles highlighted when the player is idle (or "is not busy")
                }
				//check ui
				if (gbm.backTrackPossible()) { btScript.makeFullColor(); }
				else { btScript.makeTransparent(); }
				if (gbm.forwardTrackPossible()) { ftScript.makeFullColor(); }
				else { ftScript.makeTransparent(); }
				//check input
				handlePlayInput();
				//update priorstate
				priorState = GameState.PLAY_ENDLESS;
			}
		}
        else if (state == GameState.PLAY_ARCADE)
        {
            peekKey.SetActive(false);
            peekName.SetActive(false);
            if (astate == ArcadeState.JUMP)
            {
                rockPushed = true;
                //synch animations
                if (!playerScript.isBusy())
                {
                    //synch animations
                    if (!playerScript.duringMove)       // The tile gets cracked once the player steps on it (and is done hopping)
                        gbm.steppedOn((int)playerScript.getPosition().x, gbm.getCurrentHeight() - (int)playerScript.getPosition().y - 1);
                    //handle jump
                    if (playerScript.didJump() && !handledPlayerJump) { handleJump(); }
                    handleIcons();
                }
                priorState = GameState.PLAY_ARCADE;
                //input
                if (Input.GetKeyUp(im.getMoveUpKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //if up valid
                    if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1))
                    {
                        //player
                        player.GetComponent<Player>().moveUp();
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        //sound
                        aSrc[0].PlayOneShot(crack, 1.0f);
                    }
                    else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)
                        //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                        && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                        && !gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                        && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2))
                    {
                        //rock push
                        pushScript.startShake();
                        rockPushed = true;
                        gbm.pushRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1, (int)playerBoardPosition.x, (int)playerBoardPosition.y - 2);
                        //player
                        player.GetComponent<Player>().moveUp();
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        //sound
                        aSrc[0].PlayOneShot(crack, 1.0f);
                    }
                    //else playerScript.hopInPlace();        //invalid move
                    else
                    {
                        playerScript.moveUpSmall();
                        if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)) pushScript.makeRed();
                        if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)
                        && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1))
                        { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1); }
                    }

                }
                else if (Input.GetKeyUp(im.getMoveDownKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1))          //check if move is valid
                    {
                        player.GetComponent<Player>().moveDown();               //move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                        //im.addToPriorKeys(im.getMoveDownKey());                //add to prior keys list
                    }
                    else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)
                        //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                        && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                        && !gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                        && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2))
                    {
                        rockPushed = true;                // can move a block down

                        pushScript.startShake();
                        gbm.pushRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1, (int)playerBoardPosition.x, (int)playerBoardPosition.y + 2);
                        player.GetComponent<Player>().moveDown();                // move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                    }
                    //else playerScript.hopInPlace();     //do the growing animation, hopping
                    else
                    {
                        playerScript.moveDownSmall();
                        if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)) pushScript.makeRed();
                        if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)
                        && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1))
                        { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1); }
                    }
                }
                else if (Input.GetKeyUp(im.getMoveLeftKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    if (gbm.canMoveTo((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y))          //check if move is valid
                    {
                        player.GetComponent<Player>().moveLeft();               //move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                        //im.addToPriorKeys(im.getMoveLeftKey());                //add to prior keys list
                    }
                    else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)
                        //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                        && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                        && !gbm.hasRock((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                        && gbm.canMoveTo((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y))
                    {
                        rockPushed = true;                // can move a block left

                        pushScript.startShake();
                        gbm.pushRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y, (int)playerBoardPosition.x - 2, (int)playerBoardPosition.y);
                        player.GetComponent<Player>().moveLeft();                // move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                    }
                    //else playerScript.hopInPlace();     //do the growing animation, hopping
                    else
                    {
                        playerScript.moveLeftSmall();
                        if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)) pushScript.makeRed();
                        if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)
                        && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y))
                        { gbm.setRedTile((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y); }
                    }
                }
                else if (Input.GetKeyUp(im.getMoveRightKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //check if move valid - player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //check if at goal      //only move to next level when you press right and are at goal
                    if (gbm.getGoal() == playerBoardPosition)
                    {
                        level += 1;
                        if (!gbm.loadJumpLevel(level))
                        {
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
                            currentLevelText.text = "Floor\n" + level.ToString();
                            btScript.setPosition(gbm.getStart().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                            ftScript.setPosition(gbm.getGoal().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                            jumpScript.makeFullColor();
                            //update icons
                            handleIcons();
                        }
                        else
                        {
                            //DONE WITH JUMP LEVELS
                            Debug.Log("Done with Jump Levels");
                        }
                    }
                    if (gbm.canMoveTo((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y))         //check if move valid
                    {
                        player.GetComponent<Player>().moveRight();                //move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                        //im.addToPriorKeys(im.getMoveRightKey());                //add to prior keys list
                    }
                    else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)
                        //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                        && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                        && !gbm.hasRock((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                        && gbm.canMoveTo((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y))
                    {
                        rockPushed = true;                // can move a block right
                        pushScript.startShake();
                        gbm.pushRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y, (int)playerBoardPosition.x + 2, (int)playerBoardPosition.y);
                        player.GetComponent<Player>().moveRight();                // move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                    }
                    //else playerScript.hopInPlace();      //do the growing animation, hopping
                    else
                    {
                        playerScript.moveRightSmall();
                        if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)) pushScript.makeRed();
                        if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)
                        && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y))
                        { gbm.setRedTile((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y); }
                    }
                }
                else if (!playerScript.didJump() && Input.GetKeyUp(im.getJumpKey()) && !playerScript.isBusy() && !leapMode)
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
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
                    else
                    {
                        playerScript.hopInPlace();
                        jumpScript.makeRed();
                        gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    }
                }
                else if (Input.GetKeyUp(im.getResetBoardKey()))
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    gbm.loadJumpLevel(level);
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
                    currentLevelText.text = "Floor\n" + level.ToString();
                    btScript.setPosition(gbm.getStart().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                    ftScript.setPosition(gbm.getGoal().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                    jumpScript.makeFullColor();
                    //update icons
                    handleIcons();
                }
                else if (Input.GetKeyUp(im.getPauseKey()) && state == GameState.PLAY_ARCADE) { handlePause(); }
            }
            else if (astate == ArcadeState.LEAP)
            {
                rockPushed = true;
                //synch animations
                if (!playerScript.isBusy())
                {
                    //synch animations
                    if (!playerScript.duringMove)       // The tile gets cracked once the player steps on it (and is done hopping)
                        gbm.steppedOn((int)playerScript.getPosition().x, gbm.getCurrentHeight() - (int)playerScript.getPosition().y - 1);
                    //handle jump
                    if (playerScript.didJump() && !handledPlayerJump) { handleJump(); }
                    handleIcons();
                }
                priorState = GameState.PLAY_ARCADE;
                if (Input.GetKeyUp(im.getMoveUpKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //if not leap ...
                    if (!leapMode)
                    {
                        //if up valid
                        if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1))
                        {
                            //player
                            player.GetComponent<Player>().moveUp();
                            //update icons
                            handleIcons();
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            //sound
                            aSrc[0].PlayOneShot(crack, 1.0f);
                        }
                        else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)
                            //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                            && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                            && !gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                            && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2))
                        {
                            //rock push
                            pushScript.startShake();
                            rockPushed = true;
                            gbm.pushRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1, (int)playerBoardPosition.x, (int)playerBoardPosition.y - 2);
                            //player
                            player.GetComponent<Player>().moveUp();
                            //update icons
                            handleIcons();
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            //sound
                            aSrc[0].PlayOneShot(crack, 1.0f);
                        }
                        //else playerScript.hopInPlace();        //invalid move
                        else
                        {
                            playerScript.moveUpSmall();
                            if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)) pushScript.makeRed();
                            if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)
                            && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1))
                            { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1); }
                        }
                    }
                    else
                    {
                        if (gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                            && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)) // check if move is valid
                        {
                            Debug.Log("LEAP1");
                            player.GetComponent<Player>().moveUp2();        //move player two spaces up
                            //update icons
                            handleIcons();
                            Debug.Log("LEAP2");
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            aSrc[0].PlayOneShot(crack, 1.0f);       // play walking sound
                            handleLeap();
                            Debug.Log("LEAP3");
                        }
                        else
                        {
                            //can't leap
                            leapScript.toggle();
                            leapScript.makeRed();
                            //playerScript.hopInPlace();
                            playerScript.moveUpSmall();
                            if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                            && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2))
                            { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2); }
                        }
                        leapMode = false;
                    }
                }
                else if (Input.GetKeyUp(im.getMoveDownKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    if (!leapMode)
                    {
                        if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1))          //check if move is valid
                        {
                            player.GetComponent<Player>().moveDown();               //move player
                            //update icons
                            handleIcons();
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                            //im.addToPriorKeys(im.getMoveDownKey());                //add to prior keys list
                        }
                        else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)
                            //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                            && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                            && !gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                            && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2))
                        {
                            rockPushed = true;                // can move a block down

                            pushScript.startShake();
                            gbm.pushRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1, (int)playerBoardPosition.x, (int)playerBoardPosition.y + 2);
                            player.GetComponent<Player>().moveDown();                // move player
                            //update icons
                            handleIcons();
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                        }
                        //else playerScript.hopInPlace();     //do the growing animation, hopping
                        else
                        {
                            playerScript.moveDownSmall();
                            if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)) pushScript.makeRed();
                            if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)
                            && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1))
                            { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1); }
                        }
                    }
                    else        // Leap mode on
                    {
                        if (gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                            && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2))
                        {
                            player.GetComponent<Player>().moveDown2();               //move player two spaces down
                            //update icons
                            handleIcons();
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                            handleLeap();
                        }
                        else
                        {
                            //can't leap
                            leapScript.toggle();
                            leapScript.makeRed();
                            //playerScript.hopInPlace();     //do the growing animation, hopping
                            playerScript.moveDownSmall();
                            if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                            && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2))
                            { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2); }
                        }
                        leapMode = false;
                    }
                }
                else if (Input.GetKeyUp(im.getMoveLeftKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    if (!leapMode)
                    {
                        if (gbm.canMoveTo((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y))          //check if move is valid
                        {
                            player.GetComponent<Player>().moveLeft();               //move player
                            //update icons
                            handleIcons();
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                            //im.addToPriorKeys(im.getMoveLeftKey());                //add to prior keys list
                        }
                        else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)
                            //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                            && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                            && !gbm.hasRock((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                            && gbm.canMoveTo((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y))
                        {
                            rockPushed = true;                // can move a block left

                            pushScript.startShake();
                            gbm.pushRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y, (int)playerBoardPosition.x - 2, (int)playerBoardPosition.y);
                            player.GetComponent<Player>().moveLeft();                // move player
                            //update icons
                            handleIcons();
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                        }
                        //else playerScript.hopInPlace();     //do the growing animation, hopping
                        else
                        {
                            playerScript.moveLeftSmall();
                            if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)) pushScript.makeRed();
                            if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)
                            && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y))
                            { gbm.setRedTile((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y); }
                        }
                    }
                    else        // Leap mode on
                    {
                        if (gbm.currentIsHealthyAt((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                            && gbm.canMoveTo((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y))          //check if move is valid
                        {
                            player.GetComponent<Player>().moveLeft2();               //move player two spaces left
                            //update icons
                            handleIcons();
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                            handleLeap();
                        }
                        else
                        {
                            //can't leap
                            leapScript.toggle();
                            leapScript.makeRed();
                            //playerScript.hopInPlace();     //do the growing animation, hopping
                            playerScript.moveLeftSmall();
                            if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                            && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y))
                            { gbm.setRedTile((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y); }
                        }
                        leapMode = false;
                    }
                }
                else if (Input.GetKeyUp(im.getMoveRightKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //check if move valid - player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //check if at goal      //only move to next level when you press right and are at goal
                    if (gbm.getGoal() == playerBoardPosition)
                    {
                        level += 1;
                        gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                        if (!gbm.loadLeapLevel(level))
                        {
                            playerScript.setPosition(new Vector2(gbm.getStart().x, gbm.getCurrentHeight() - gbm.getStart().y - 1));
                            playerScript.notJump();
                            playerScript.notLeap();
                            handledPlayerJump = false;
                            handledPlayerLeap = false;
                            leapMode = false;
                            //reset entrance/exit
                            entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
                            exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
                            //ui stuff
                            currentLevelText.text = "Floor\n" + level.ToString();
                            btScript.setPosition(gbm.getStart().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                            ftScript.setPosition(gbm.getGoal().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                            leapScript.makeFullColor();
                            leapScript.untoggle();
                            //update icons
                            handleIcons();
                        }
                        else
                        {
                            Debug.Log("Done with Leap levels");
                        }
                    }
                    if (!leapMode)
                    {
                        if (gbm.canMoveTo((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y))         //check if move valid
                        {
                            player.GetComponent<Player>().moveRight();                //move player
                            //update icons
                            handleIcons();
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                            //im.addToPriorKeys(im.getMoveRightKey());                //add to prior keys list
                        }
                        else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)
                            //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                            && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                            && !gbm.hasRock((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                            && gbm.canMoveTo((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y))
                        {
                            rockPushed = true;                // can move a block right
                            pushScript.startShake();
                            gbm.pushRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y, (int)playerBoardPosition.x + 2, (int)playerBoardPosition.y);
                            player.GetComponent<Player>().moveRight();                // move player
                            //update icons
                            handleIcons();
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                        }
                        //else playerScript.hopInPlace();      //do the growing animation, hopping
                        else
                        {
                            playerScript.moveRightSmall();
                            if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)) pushScript.makeRed();
                            if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)
                            && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y))
                            { gbm.setRedTile((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y); }
                        }
                    }
                    else        // Leap mode on
                    {
                        if (gbm.currentIsHealthyAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                            && gbm.canMoveTo((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y))         //check if move valid
                        {
                            player.GetComponent<Player>().moveRight2();                //move player two spaces right
                            //update icons
                            handleIcons();
                            if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                            aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                            handleLeap();
                        }
                        else
                        {
                            //can't leap
                            leapScript.toggle();
                            leapScript.makeRed();
                            //playerScript.hopInPlace();
                            playerScript.moveRightSmall();
                            if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                            && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y))
                            { gbm.setRedTile((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y); }
                        }
                        leapMode = false;
                    }
                }
                else if (Input.GetKeyUp(im.getResetBoardKey()))
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    gbm.loadLeapLevel(level);
                    playerScript.setPosition(new Vector2(gbm.getStart().x, gbm.getCurrentHeight() - gbm.getStart().y - 1)); 
                    //player lands on start
                    gbm.steppedOn((int)gbm.getStart().x, (int)gbm.getStart().y);
                    playerScript.notJump();
                    playerScript.notLeap();
                    handledPlayerJump = false;
                    handledPlayerLeap = false;
                    leapMode = false;
                    //reset entrance/exit
                    entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
                    exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
                    //ui stuff
                    currentLevelText.text = "Floor\n" + level.ToString();
                    btScript.setPosition(gbm.getStart().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                    ftScript.setPosition(gbm.getGoal().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                    leapScript.makeFullColor();
                    leapScript.untoggle();
                    //update icons
                    handleIcons();
                }
                else if (Input.GetKeyUp(im.getPauseKey()) && state == GameState.PLAY_ARCADE) { handlePause(); }
                else if (Input.GetKeyUp(im.getToggleLeapKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();
                    toggleLeapMode();
                }
            }
            else if (astate == ArcadeState.PUSH)
            {
                //stop pushing
                if (rockPushed) { pushScript.stopShake(); }
                //synch animations
                if (!playerScript.isBusy())
                {
                    //synch animations
                    if (!playerScript.duringMove)       // The tile gets cracked once the player steps on it (and is done hopping)
                        gbm.steppedOn((int)playerScript.getPosition().x, gbm.getCurrentHeight() - (int)playerScript.getPosition().y - 1);
                    //handle jump
                    if (playerScript.didJump() && !handledPlayerJump) { handleJump(); }
                    handleIcons();
                }
                priorState = GameState.PLAY_ARCADE;
                if (Input.GetKeyUp(im.getMoveUpKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);

                    //if up valid
                    if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1))
                    {
                        //player
                        player.GetComponent<Player>().moveUp();
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        //sound
                        aSrc[0].PlayOneShot(crack, 1.0f);
                    }
                    else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)
                        //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                        && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                        && !gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                        && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2))
                    {
                        //rock push
                        pushScript.startShake();
                        rockPushed = true;
                        gbm.pushRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1, (int)playerBoardPosition.x, (int)playerBoardPosition.y - 2);
                        //player
                        player.GetComponent<Player>().moveUp();
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        //sound
                        aSrc[0].PlayOneShot(crack, 1.0f);
                    }
                    //else playerScript.hopInPlace();        //invalid move
                    else
                    {
                        playerScript.moveUpSmall();
                        if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)) pushScript.makeRed();
                        if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)
                        && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1))
                        { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1); }
                    }
                }
                else if (Input.GetKeyUp(im.getMoveDownKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1))          //check if move is valid
                    {
                        player.GetComponent<Player>().moveDown();               //move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                        //im.addToPriorKeys(im.getMoveDownKey());                //add to prior keys list
                    }
                    else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)
                        //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                        && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                        && !gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                        && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2))
                    {
                        rockPushed = true;                // can move a block down

                        pushScript.startShake();
                        gbm.pushRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1, (int)playerBoardPosition.x, (int)playerBoardPosition.y + 2);
                        player.GetComponent<Player>().moveDown();                // move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                    }
                    //else playerScript.hopInPlace();     //do the growing animation, hopping
                    else
                    {
                        playerScript.moveDownSmall();
                        if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)) pushScript.makeRed();
                        if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)
                        && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1))
                        { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1); }
                    }
                }
                else if (Input.GetKeyUp(im.getMoveLeftKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    if (gbm.canMoveTo((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y))          //check if move is valid
                    {
                        player.GetComponent<Player>().moveLeft();               //move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                        //im.addToPriorKeys(im.getMoveLeftKey());                //add to prior keys list
                    }
                    else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)
                        //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                        && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                        && !gbm.hasRock((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                        && gbm.canMoveTo((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y))
                    {
                        rockPushed = true;                // can move a block left

                        pushScript.startShake();
                        gbm.pushRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y, (int)playerBoardPosition.x - 2, (int)playerBoardPosition.y);
                        player.GetComponent<Player>().moveLeft();                // move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                    }
                    //else playerScript.hopInPlace();     //do the growing animation, hopping
                    else
                    {
                        playerScript.moveLeftSmall();
                        if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)) pushScript.makeRed();
                        if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)
                        && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y))
                        { gbm.setRedTile((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y); }
                    }
                }
                else if (Input.GetKeyUp(im.getMoveRightKey()) && !playerScript.isBusy())
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    //check if move valid - player's y is flipped
                    Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                    //if backtracked accept changes
                    gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //check if at goal      //only move to next level when you press right and are at goal
                    if (gbm.getGoal() == playerBoardPosition)
                    {
                        level += 1;
                        if (!gbm.loadPushLevel(level))
                        {
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
                            currentLevelText.text = "Floor\n" + level.ToString();
                            btScript.setPosition(gbm.getStart().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                            ftScript.setPosition(gbm.getGoal().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                            pushScript.makeFullColor();
                            //update icons
                            handleIcons();
                        }
                        else
                        {
                            Debug.Log("Cleared Push Levels");
                        }
                    }
                    if (gbm.canMoveTo((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y))         //check if move valid
                    {
                        player.GetComponent<Player>().moveRight();                //move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                        //im.addToPriorKeys(im.getMoveRightKey());                //add to prior keys list
                    }
                    else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)
                        //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                        && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                        && !gbm.hasRock((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                        && gbm.canMoveTo((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y))
                    {
                        rockPushed = true;                // can move a block right
                        pushScript.startShake();
                        gbm.pushRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y, (int)playerBoardPosition.x + 2, (int)playerBoardPosition.y);
                        player.GetComponent<Player>().moveRight();                // move player
                        //update icons
                        handleIcons();
                        if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                        aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                    }
                    //else playerScript.hopInPlace();      //do the growing animation, hopping
                    else
                    {
                        playerScript.moveRightSmall();
                        if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)) pushScript.makeRed();
                        if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)
                        && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y))
                        { gbm.setRedTile((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y); }
                    }
                }
                else if (Input.GetKeyUp(im.getResetBoardKey()))
                {
                    gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
                    gbm.loadPushLevel(level);
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
                    currentLevelText.text = "Floor\n" + level.ToString();
                    btScript.setPosition(gbm.getStart().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                    ftScript.setPosition(gbm.getGoal().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
                    pushScript.makeFullColor();
                    //update icons
                    handleIcons();
                }
                else if (Input.GetKeyUp(im.getPauseKey()) && state == GameState.PLAY_ARCADE) { handlePause(); }
            }
            else if (astate == ArcadeState.EWP)
            {
            }
            else if (astate == ArcadeState.TRE)
            {
            }
        }
        else if (state == GameState.LOAD)
        {
            //wait until player done with animation
            if (lstate == LoadState.PLAYER && !playerScript.isBusy())
            {
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
                state = GameState.PLAY_ENDLESS;
            }
            //update priorstate
            priorState = GameState.LOAD;
        }
        else if (state == GameState.PEEK)
        {
            //exit peeking
            if (Input.GetKeyUp(im.getPeekKey()))
            {
                currentLevelText.text = "Floor\n" + level.ToString();
                nextLevelText.text = "Floor: " + (level + 1).ToString();
                playerScript.unfadePlayer();
                gbm.unpeek();
                eyeScript.unblink();
                state = GameState.PLAY_ENDLESS;
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
		currentLevelText.text = "Floor\n" + level.ToString();
        nextLevelText.text = "Floor: " + (level + 1).ToString();
		btScript.setPosition(gbm.getStart().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		ftScript.setPosition(gbm.getGoal().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
        leapScript.makeFullColor();
        leapScript.untoggle();
        pushScript.stopShake();
        pushScript.makeFullColor();
        jumpScript.makeFullColor();
        //update icons
        handleIcons();
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
    }
    public void toggleLeapMode()
    {
        if (!handledPlayerLeap) { 
            leapMode = !leapMode;
            if (leapMode) handleLeapTileColors((int)playerScript.getPosition().x, gbm.getCurrentHeight() - (int)playerScript.getPosition().y - 1);
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
		currentLevelText.text = "Floor\n" + level.ToString();
        nextLevelText.text = "Floor: " + (level + 1).ToString();
		btScript.setPosition(gbm.getStart().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		ftScript.setPosition(gbm.getGoal().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
        leapScript.makeFullColor();
        leapScript.untoggle();
        pushScript.stopShake();
        pushScript.makeFullColor();
        jumpScript.makeFullColor();
        //update icons
        handleIcons();
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
		currentLevelText.text = "Floor\n" + level.ToString();
        nextLevelText.text = "Floor: " + (level + 1).ToString();
		btScript.setPosition(gbm.getStart().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		ftScript.setPosition(gbm.getGoal().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
        leapScript.makeFullColor();
        leapScript.untoggle();
        pushScript.stopShake();
        pushScript.makeFullColor();
        jumpScript.makeFullColor();
        //update icons
        handleIcons();
	}
	public void handlePeek()
	{
        currentLevelText.text = "Next\nFloor";
        nextLevelText.text = "";
		state = GameState.PEEK;
		gbm.peek();
        eyeScript.blink();
        playerScript.fadePlayer();
	}
    public void unColorIcons()
    {
        if (!pushScript.isFaded()) { pushScript.unColor(); }
        if (!leapScript.isFaded()) { leapScript.unColor(); }
        if (!jumpScript.isFaded()) { jumpScript.unColor(); }
    }
    public void handleIcons()
    {
        Vector2 pos = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
        if (gbm.bbm.currentIsDamagedAt((int)pos.x, (int)pos.y))         
        {
            if (!pushScript.isFaded()) { pushScript.unColor(); }
            if (!jumpScript.isFaded()) { jumpScript.unColor(); }
        }
        else
        {
            if (!pushScript.isFaded()) { pushScript.makeGreen(); }
            if (!jumpScript.isFaded()) { jumpScript.makeGreen(); }
        }
        if (!leapScript.isFaded())
        { 
            if (!gbm.bbm.currentHasRockAt((int)pos.x + 2, (int)pos.y) && gbm.bbm.currentIsHealthyAt((int)pos.x + 2, (int)pos.y))
            {
                leapScript.makeGreen();
                return;
            }
            if (!gbm.bbm.currentHasRockAt((int)pos.x - 2, (int)pos.y) && gbm.bbm.currentIsHealthyAt((int)pos.x - 2, (int)pos.y))
            {
                leapScript.makeGreen();
                return;
            }
            if (!gbm.bbm.currentHasRockAt((int)pos.x, (int)pos.y + 2) && gbm.bbm.currentIsHealthyAt((int)pos.x, (int)pos.y + 2))
            {
                leapScript.makeGreen();
                return;
            }
            if (!gbm.bbm.currentHasRockAt((int)pos.x, (int)pos.y - 2) && gbm.bbm.currentIsHealthyAt((int)pos.x, (int)pos.y - 2))
            {
                leapScript.makeGreen();
                return;
            }
            leapScript.unColor();
        }
    }
    public void handleLeapTileColors(int x, int y)
    {
        if (gbm.bbm.currentIsValidAt(x + 2, y)) gbm.setLeapTile1(x + 2, y);
        if (gbm.bbm.currentIsValidAt(x - 2, y)) gbm.setLeapTile2(x - 2, y);
        if (gbm.bbm.currentIsValidAt(x, y + 2)) gbm.setLeapTile3(x, y + 2);
        if (gbm.bbm.currentIsValidAt(x, y - 2)) gbm.setLeapTile4(x, y - 2);
    }
    //keyboard input
    public void handlePlayInput()
	{
        if (Input.GetKeyUp(im.getMoveUpKey()) && !playerScript.isBusy())
        {
            gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
            //player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //if backtracked accept changes
            gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
            //if not leap ...
            if (!leapMode)
            {
                //if up valid
                if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1))
                {
                    //player
                    player.GetComponent<Player>().moveUp();
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //sound
                    aSrc[0].PlayOneShot(crack, 1.0f);
                }
                else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)
                    //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                    && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                    && !gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                    && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2))
                {
                    //rock push
                    pushScript.startShake();
                    rockPushed = true;
                    gbm.pushRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1, (int)playerBoardPosition.x, (int)playerBoardPosition.y - 2);
                    //player
                    player.GetComponent<Player>().moveUp();
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //sound
                    aSrc[0].PlayOneShot(crack, 1.0f);
                }
                //else playerScript.hopInPlace();        //invalid move
                else {
                    playerScript.moveUpSmall();
                    if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)) pushScript.makeRed();
                    if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)
                    && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1))
                        { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1); }
                }
            }
            else {
                if (gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                    && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)) // check if move is valid
                {
                    player.GetComponent<Player>().moveUp2();        //move player two spaces up
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);       // play walking sound
                    handleLeap();
                }
                else
                {
                    //can't leap
                    leapScript.toggle();
                    leapScript.makeRed();
                    //playerScript.hopInPlace();
                    playerScript.moveUpSmall();
                    if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2)
                    && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2))
                        { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y - 2); }
                }
                leapMode = false;
            }
        }
        else if (Input.GetKeyUp(im.getMoveDownKey()) && !playerScript.isBusy())
        {
            gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
            //player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //if backtracked accept changes
            gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
            if (!leapMode)
            {
                if (gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1))          //check if move is valid
                {
                    player.GetComponent<Player>().moveDown();               //move player
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    //im.addToPriorKeys(im.getMoveDownKey());                //add to prior keys list
                }
                else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)
                    //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                    && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                    && !gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                    && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2))
                {
                    rockPushed = true;                // can move a block down

                    pushScript.startShake();
                    gbm.pushRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1, (int)playerBoardPosition.x, (int)playerBoardPosition.y + 2);
                    player.GetComponent<Player>().moveDown();                // move player
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                }
                //else playerScript.hopInPlace();     //do the growing animation, hopping
                else {
                    playerScript.moveDownSmall();
                    if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)) pushScript.makeRed();
                    if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)
                    && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1))
                        { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1); }
                }
            }
            else        // Leap mode on
            {
                if (gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                    && gbm.canMoveTo((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2))
                {
                    player.GetComponent<Player>().moveDown2();               //move player two spaces down
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    handleLeap();
                }
                else
                {
                    //can't leap
                    leapScript.toggle();
                    leapScript.makeRed();
                    //playerScript.hopInPlace();     //do the growing animation, hopping
                    playerScript.moveDownSmall();
                    if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2)
                    && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2))
                        { gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y + 2); }
                }
                leapMode = false;
            }
        }
        else if (Input.GetKeyUp(im.getMoveLeftKey()) && !playerScript.isBusy())
        {
            gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
            //player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //if backtracked accept changes
            gbm.moveWhileBacktrack((int)playerBoardPosition.x, (int)playerBoardPosition.y);
            if (!leapMode)
            {
                if (gbm.canMoveTo((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y))          //check if move is valid
                {
                    player.GetComponent<Player>().moveLeft();               //move player
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    //im.addToPriorKeys(im.getMoveLeftKey());                //add to prior keys list
                }
                else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)
                    //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                    && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                    && !gbm.hasRock((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                    && gbm.canMoveTo((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y))
                {
                    rockPushed = true;                // can move a block left

                    pushScript.startShake();
                    gbm.pushRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y, (int)playerBoardPosition.x - 2, (int)playerBoardPosition.y);
                    player.GetComponent<Player>().moveLeft();                // move player
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                }
                //else playerScript.hopInPlace();     //do the growing animation, hopping
                else {
                    playerScript.moveLeftSmall();
                    if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)) pushScript.makeRed();
                    if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)
                    && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y))
                        { gbm.setRedTile((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y); }
                }
            }
            else        // Leap mode on
            {
                if (gbm.currentIsHealthyAt((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                    && gbm.canMoveTo((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y))          //check if move is valid
                {
                    player.GetComponent<Player>().moveLeft2();               //move player two spaces left
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    handleLeap();
                }
                else
                {
                    //can't leap
                    leapScript.toggle();
                    leapScript.makeRed();
                    //playerScript.hopInPlace();     //do the growing animation, hopping
                    playerScript.moveLeftSmall();
                    if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y)
                    && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y))
                        { gbm.setRedTile((int)playerBoardPosition.x - 2, (int)playerBoardPosition.y); }
                }
                leapMode = false;
            }
        }
        else if (Input.GetKeyUp(im.getMoveRightKey()) && !playerScript.isBusy())
        {
            gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
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
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    //im.addToPriorKeys(im.getMoveRightKey());                //add to prior keys list
                }
                else if (!rockPushed && gbm.hasRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)
                    //&& gbm.currentIsHealthyAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                    && gbm.currentIsHealthyAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)
                    && !gbm.hasRock((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                    && gbm.canMoveTo((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y))
                {
                    rockPushed = true;                // can move a block right
                    pushScript.startShake();
                    gbm.pushRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y, (int)playerBoardPosition.x + 2, (int)playerBoardPosition.y);
                    player.GetComponent<Player>().moveRight();                // move player
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);                // play walking sound
                }
                //else playerScript.hopInPlace();      //do the growing animation, hopping
                else {
                    playerScript.moveRightSmall();
                    if (!pushScript.isFaded() && gbm.hasRock((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)) pushScript.makeRed();
                    if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)
                    && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y))
                        { gbm.setRedTile((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y); }
                }
            }
            else        // Leap mode on
            {
                if (gbm.currentIsHealthyAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                    && gbm.canMoveTo((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y))         //check if move valid
                {
                    player.GetComponent<Player>().moveRight2();                //move player two spaces right
                    //update icons
                    handleIcons();
                    if (!playerScript.duringMove) gbm.steppedOffOf((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    aSrc[0].PlayOneShot(crack, 1.0f);               // play walking sound
                    handleLeap();
                }
                else
                {
                    //can't leap
                    leapScript.toggle();
                    leapScript.makeRed();
                    //playerScript.hopInPlace();
                    playerScript.moveRightSmall();
                    if (gbm.bbm.currentIsValidAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y)
                    && !gbm.bbm.currentIsDestroyedAt((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y))
                        { gbm.setRedTile((int)playerBoardPosition.x + 2, (int)playerBoardPosition.y); }
                }
                leapMode = false;
            }
        }
        else if (!playerScript.didJump() && Input.GetKeyUp(im.getJumpKey()) && !playerScript.isBusy() && !leapMode)
        {
            gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
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
            else
            {
                playerScript.hopInPlace();
                jumpScript.makeRed();
                gbm.setRedTile((int)playerBoardPosition.x, (int)playerBoardPosition.y);
            }
        }
        else if (Input.GetKeyUp(im.getResetBoardKey()))
        {
            gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
            resetBoard();
        }
        else if (Input.GetKeyUp(im.getPauseKey()) && state == GameState.PLAY_ENDLESS) { handlePause(); }
        else if (Input.GetKeyUp(im.getBacktrackKey()))
        {
            gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
            handleBacktrack();
        }
        else if (Input.GetKeyUp(im.getForwardTrackKey()))
        {
            gbm.clearAllTileColors();     // Remove all red tiles when another key is pressed
            handleForwardTrack();
        }
        else if (Input.GetKeyUp(im.getPeekKey())) { handlePeek(); }
        else if (Input.GetKeyUp(im.getToggleLeapKey()) && !playerScript.isBusy())
        {
            gbm.clearAllTileColors();
            toggleLeapMode();
        }
        // rockPushed = false; // uncomment for unlimited pushing
    }

    //reset
    public void resetBoard()
	{
		//reset player position
		playerScript.setPosition(new Vector2(gbm.getStart().x, gbm.getCurrentHeight() - gbm.getStart().y - 1));
		//reset player jumped
		playerScript.notJump();
        playerScript.notLeap();
        handledPlayerJump = false;
        handledPlayerLeap = false;
        leapMode = false;
        rockPushed = false;
        gbm.resetBoard(level != LEVEL_START);
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
        //update icons
        gbm.clearAllTileColors();
        handleIcons();
    }
	public void resetGame()
	{ 
		level = LEVEL_START;
		currentLevelText.text = "Floor\n" + level.ToString();
        nextLevelText.text = "Floor " + (level + 1).ToString();
		im = new InputManager();
		gbm.clear();
		//player lands on start
		gbm.steppedOn((int)gbm.getStart().x, (int)gbm.getStart().y);
		//player
		playerScript.reset((int)gbm.getStart().x, (int)(gbm.getCurrentHeight() - gbm.getStart().y - 1));
        playerScript.notJump();
        playerScript.notLeap();
        handledPlayerJump = false;
        handledPlayerLeap = false;
        leapMode = false;
        rockPushed = false;
        //reset entrance/exit
        entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
		exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
        leapIcon.SetActive(true);
        leapScript.makeFullColor();
        leapScript.untoggle();
        pushIcon.SetActive(true);
        pushScript.stopShake();
        pushScript.makeFullColor();
        jumpIcon.SetActive(true);
        jumpScript.makeFullColor();
        eye.SetActive(true);
        peekName.SetActive(true);
        peekKey.SetActive(true);
        btIcon.SetActive(true);
        ftIcon.SetActive(true);
        //update icons
        gbm.clearAllTileColors();
        handleIcons();
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
            state = GameState.PLAY_ENDLESS;
        }
    }
	public void continueButton () {
		pausePanel.SetActive(false);
		state = GameState.PLAY_ENDLESS;
	}
	public void mainMenuButton () {
		StartCoroutine(fadeScript.gameFadeToBlack());
	}

    public void debug()
    {
        resetGame();
        gbm.tiles[2][2].GetComponent<Renderer>().material.color = Color.red;
    }
}
