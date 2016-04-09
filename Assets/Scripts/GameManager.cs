<<<<<<< HEAD
﻿using UnityEngine;
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
	public GameObject entrance;
	public GameObject exit;
	public Text levelText;
	//basic ui
	public GameObject backtrack;
	private BackTrack btScript;
	public GameObject forwardtrack;
	private ForwardTrack ftScript;
	public GameObject reset;
	public GameObject pause;
	public GameObject eye;
	private Eye eyeScript;
	//States
	enum GameState { TITLE, PAUSE, LOAD, PEEK, PLAY }
	GameState state;
	GameState priorState;
	enum LoadState { PLAYER, NEXT, BACK, FORWARD }
	LoadState lstate;
	int level;
	int LEVEL_START = 0;

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
		// set up audio variables
		// 0: crack, 1: hole, 2: fall, 3: jump
		aSrc = GetComponents<AudioSource>();
		crack = aSrc[0].clip;
		hole = aSrc[1].clip;
		fall = aSrc[2].clip;
		jump = aSrc[3].clip;
		//soundSlider = GameObject.Find("Sound Slider").GetComponent<Slider>();
	}

	// Main update loop
	void Update () {
		if (state == GameState.TITLE)
		{
			//player lands on start
			gbm.steppedOn((int)gbm.getStart().x, (int)gbm.getStart().y);
			//instantiate exit to right of goal/entrance to left
			Vector3 entrancePos = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
			Vector3 exitPos = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
			entrance = (GameObject)Instantiate(entrance, entrancePos, Quaternion.identity);
			exit = (GameObject)Instantiate(exit, exitPos, Quaternion.identity);
			//basic ui
			levelText.text = "Floor\n" + level.ToString();
			reset = (GameObject)Instantiate(reset, new Vector3(entrancePos.x + 1.5f, exitPos.y + 1, 0), Quaternion.identity);
			pause = (GameObject)Instantiate(pause, new Vector3(exitPos.x - 1.5f, exitPos.y + 1, 0), Quaternion.identity);
			backtrack = (GameObject)Instantiate(backtrack, new Vector3(entrancePos.x, exitPos.y + 1, 0), Quaternion.identity);
			btScript = backtrack.GetComponent<BackTrack>();
			btScript.makeTransparent();
			forwardtrack = (GameObject)Instantiate(forwardtrack, new Vector3(exitPos.x, exitPos.y + 1, 0), Quaternion.identity);
			ftScript = forwardtrack.GetComponent<ForwardTrack>();
			ftScript.makeTransparent();
			eye = (GameObject)Instantiate(eye, new Vector3(exitPos.x, exitPos.y - 1, 0), Quaternion.identity);
			eyeScript = eye.GetComponent<Eye>();
			state = GameState.PLAY;
			//update priorstate
			priorState = GameState.TITLE;
		}
		else if (state == GameState.PAUSE)
		{
			//update prior state
			//handlePause();
			handleUnpause();
			priorState = GameState.PAUSE;
		}
		else if (state == GameState.PLAY)
		{
			//check if done with peek
			if (priorState == GameState.PEEK)
			{
				eyeScript.unblink();
				gbm.unPeek();
				priorState = GameState.PLAY;
			}
			else
			{
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
				//check ui
				if (gbm.backTrackPossible()) { btScript.makeFullColor(); }
				else { btScript.makeTransparent(); }
				if (gbm.forwardTrackPossible()) { ftScript.makeFullColor(); }
				else { ftScript.makeTransparent(); }
				//check input
				handlePlayInput();
				//check reach goal
				if (gbm.getGoal() == new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1) && !playerScript.isBusy())
				{
					handleClearedFloor();
				}
				//update priorstate
				priorState = GameState.PLAY;
			}
		}
		else if (state == GameState.LOAD)
		{
			//wait until player done with animation
			if (lstate == LoadState.PLAYER && !playerScript.isBusy()){
				lstate = LoadState.NEXT;
				gbm.zoomTilesOut();
			}
			else if (lstate != LoadState.PLAYER && !gbm.busy()) {//wait until gbm done with zoom
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
			if (!gbm.busy() && !gbm.isPeeking())
			{
				levelText.text = "Next\nFloor";
				playerScript.fadePlayer();
				gbm.peek();
			}
			//exit peeking
			if (Input.GetKeyUp(im.getPeekKey()))
			{
				levelText.text = "Floor\n" + level.ToString();
				playerScript.unfadePlayer();
				gbm.unfadeTiles();
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
		//ui stuff
		levelText.text = "Floor\n" + level.ToString();
		reset.transform.position = new Vector3(gbm.getStart().x + 0.5f, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		pause.transform.position = new Vector3(gbm.getGoal().x - 0.5f, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		btScript.setPosition(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		ftScript.setPosition(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		eyeScript.setPosition(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 2, 0);
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
			if (!gbm.bbm.nextIsDamagedAt(x, y))  gbm.damageFutureBoard(x, y);
			//up from playerPos
			x = playerBoardPosX;
			y = playerBoardPosY - 1;
			gbm.damageCurrentBoard(x, y);
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//down
			x = playerBoardPosX;
			y = playerBoardPosY + 1;
			gbm.damageCurrentBoard(x, y);
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//left 
			x = playerBoardPosX - 1;
			y = playerBoardPosY;
			gbm.damageCurrentBoard(x, y);
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//right
			x = playerBoardPosX + 1;
			y = playerBoardPosY;
			gbm.damageCurrentBoard(x, y);
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//top left
			x = playerBoardPosX - 1;
			y = playerBoardPosY - 1;
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//top right
			x = playerBoardPosX + 1;
			y = playerBoardPosY - 1;
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//bot left
			x = playerBoardPosX - 1;
			y = playerBoardPosY + 1;
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//bot right
			x = playerBoardPosX + 1;
			y = playerBoardPosY + 1;
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//2 to the up
			x = playerBoardPosX;
			y = playerBoardPosY - 2;
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//2 to the left
			x = playerBoardPosX - 2;
			y = playerBoardPosY;
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//2 to the right
			x = playerBoardPosX + 2;
			y = playerBoardPosY;
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
			//2 to the bottom
			x = playerBoardPosX;
			y = playerBoardPosY + 2;
			if (!gbm.bbm.nextIsDamagedAt(x, y)) gbm.damageFutureBoard(x, y);
		}
	}
	public void handleBacktrack()
	{
		if (gbm.backtrack())
		{
			state = GameState.LOAD;
			lstate = LoadState.BACK;
			gbm.zoomTilesIn();
		}
	}
	public void loadBacktrack()
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
		//ui stuff
		levelText.text = "Floor\n" + level.ToString();
		reset.transform.position = new Vector3(gbm.getStart().x + 0.5f, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		pause.transform.position = new Vector3(gbm.getGoal().x - 0.5f, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		btScript.setPosition(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		ftScript.setPosition(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		eyeScript.setPosition(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 2, 0);
	}
	public void handleForwardTrack()
	{
		if (gbm.forwardTrack())
		{
			state = GameState.LOAD;
			lstate = LoadState.FORWARD;
			gbm.zoomTilesOut();
		}
	}
	public void loadForwardTrack()
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
		//ui stuff
		levelText.text = "Floor\n" + level.ToString();
		reset.transform.position = new Vector3(gbm.getStart().x + 0.5f, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		pause.transform.position = new Vector3(gbm.getGoal().x - 0.5f, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		btScript.setPosition(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		ftScript.setPosition(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		eyeScript.setPosition(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 2, 0);
	}
	public void handlePeek()
	{
		state = GameState.PEEK;
		eyeScript.blink();
		gbm.fadeTiles();
	}

	public void handlePause() {
		pausePanel.SetActive(true);
		state = GameState.PAUSE;
	}

	public void handleUnpause() {
		if (Input.GetKeyUp (KeyCode.Escape)) {
			pausePanel.SetActive(false);
			state = GameState.PLAY;
		}
	}

	//keyboard input
	public void handlePlayInput()
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
		else if (Input.GetKeyUp(im.getPauseKey()) && state == GameState.PLAY) {
			handlePause();
			//pause the game
			//currentState = GameState.PAUSE;
		}
		/*
		else if (Input.GetKeyUp (im.getPauseKey()) && state == GameState.PAUSE) {
			handleUnpause();
		}
		*/
		else if (btScript.clicked() || Input.GetKeyUp(im.getBacktrackKey())) { 
			handleBacktrack();
			btScript.unclick();
		}
		else if (Input.GetKeyUp(im.getForwardTrackKey())) { handleForwardTrack(); } 
		else if (Input.GetKeyUp(im.getResetGameKey())){ resetGame(); }
		else if (Input.GetKeyUp(im.getPeekKey())) { handlePeek(); }
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
		level = 0;//LEVEL_START;
		//levelText.text = "Floor\n" + level.ToString();
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

	public void continueButton () {
		pausePanel.SetActive(false);
		state = GameState.PLAY;
	}

	public void mainMenuButton () {
		Application.LoadLevel("MainMenu");
	}

=======
﻿using UnityEngine;
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
	public GameObject entrance;
	public GameObject exit;
	public Text levelText;
	//basic ui
	public GameObject backtrack;
	private BackTrack btScript;
	public GameObject forwardtrack;
	private ForwardTrack ftScript;
	//States
	enum GameState { TITLE, PAUSE, LOAD, PEEK, PLAY }
	GameState state;
	GameState priorState;
	enum LoadState { PLAYER, NEXT, BACK, FORWARD }
	LoadState lstate;
	int level;
	int LEVEL_START = 0;

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
		// set up audio variables
		// 0: crack, 1: hole, 2: fall, 3: jump
		aSrc = GetComponents<AudioSource>();
		crack = aSrc[0].clip;
		hole = aSrc[1].clip;
		fall = aSrc[2].clip;
		jump = aSrc[3].clip;
		//soundSlider = GameObject.Find("Sound Slider").GetComponent<Slider>();
	}

	// Main update loop
	void Update () {
		if (state == GameState.TITLE)
		{
			//player lands on start
			gbm.steppedOn((int)gbm.getStart().x, (int)gbm.getStart().y);
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
			state = GameState.PLAY;
			//update priorstate
			priorState = GameState.TITLE;
		}
		else if (state == GameState.PAUSE)
		{
			//update prior state
			//handlePause();
			handleUnpause();
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
					//synch animations
					gbm.steppedOn((int)playerScript.getPosition().x, gbm.getCurrentHeight() - (int)playerScript.getPosition().y - 1);
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
				//check reach goal
				if (gbm.getGoal() == new Vector2(playerScript.getPosition().x, gbm.getCurrentHeight() - playerScript.getPosition().y - 1) && !playerScript.isBusy())
				{
					handleClearedFloor();
				}
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
		handledPlayerJump = false;
		//reset entrance/exit
		entrance.GetComponent<Transform>().position = new Vector3(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getStart().y - 1, 0);
		exit.GetComponent<Transform>().position = new Vector3(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y - 1, 0);
		//ui stuff
		levelText.text = "Floor\n" + level.ToString();
		btScript.setPosition(gbm.getStart().x - 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
		ftScript.setPosition(gbm.getGoal().x + 1, gbm.getCurrentHeight() - gbm.getGoal().y, 0);
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
		handledPlayerJump = false;
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
	}
	public void handlePeek()
	{
        levelText.text = "Next\nFloor";
		state = GameState.PEEK;
		gbm.peek();
        playerScript.fadePlayer();
	}

	public void handlePause() {
		pausePanel.SetActive(true);
		state = GameState.PAUSE;
	}

	public void handleUnpause() {
		if (Input.GetKeyUp (KeyCode.Escape)) {
			pausePanel.SetActive(false);
			state = GameState.PLAY;
		}
	}

	//keyboard input
	public void handlePlayInput()
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
		else if (Input.GetKeyUp(im.getPauseKey()) && state == GameState.PLAY) {
			handlePause();
			//pause the game
			//currentState = GameState.PAUSE;
		}
		/*
		else if (Input.GetKeyUp (im.getPauseKey()) && state == GameState.PAUSE) {
			handleUnpause();
		}
		*/
		else if (btScript.clicked() || Input.GetKeyUp(im.getBacktrackKey())) { 
			handleBacktrack();
			btScript.unclick();
		}
		else if (Input.GetKeyUp(im.getForwardTrackKey())) { handleForwardTrack(); } 
		else if (Input.GetKeyUp(im.getPeekKey())) { handlePeek(); }
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
		levelText.text = "Floor\n" + level.ToString();
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

	public void continueButton () {
		pausePanel.SetActive(false);
		state = GameState.PLAY;
	}

	public void mainMenuButton () {
		Application.LoadLevel("MainMenu");
	}

>>>>>>> michael
}