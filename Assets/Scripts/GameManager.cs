using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    InputManager im;
    TileBoardManager tbm;
    RockBoardManager rbm;
    //prefabs
    public GameObject player;
    Player playerScript;
    bool handledPlayerJump;
    public GameObject rock;
    public GameObject tile;
    public GameObject damagedTile;
    public GameObject destroyedTile;
    public Text levelText;
    GameObject[][] tiles; //to keep track of tiles
    GameObject[][] rocks;
    //GameStates
    enum GameState { TITLE, PAUSE, PLAY }
    GameState state;

    int level;
    int LEVEL_START = 99;

    //called before all Start()
    void Awake()
    {
        level = LEVEL_START;
        state = GameState.PLAY; //this should be title ..
        im = new InputManager();
        //tiles
        tbm = new TileBoardManager();
        //set up tile gameobjects
        tiles = new GameObject[tbm.getCurrentWidth()][];
        for (int i = 0; i < tbm.getCurrentWidth(); i++){ tiles[i] = new GameObject[tbm.getCurrentHeight()]; }
        //rocks
        rbm = new RockBoardManager();
        //set up rock gameobjects
        rocks = new GameObject[rbm.getCurrentWidth()][];
        for (int i = 0; i < rbm.getCurrentWidth(); i++) { rocks[i] = new GameObject[tbm.getCurrentHeight()]; }
        //player
        player = (GameObject)Instantiate(player, new Vector3(0, 0, -1), Quaternion.identity);
        playerScript = player.GetComponent<Player>();
        handledPlayerJump = false;
        //draw board
        drawTiles();
    }

	// Main update loop
	void Update () {
        if (state == GameState.TITLE)
        {
        }
        else if (state == GameState.PAUSE)
        {
        }
        else if (state == GameState.PLAY)
        {
            //levelText
            levelText.text = "Level: " + level.ToString();
            //check input
            handleInput();
            //handle jump
            if (playerScript.didJump() && playerScript.didLand() && !handledPlayerJump)
            {
                handleJump();
            }
            //check reach goal
                //get player position - have to swap y
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, tbm.getCurrentHeight() - playerScript.getPosition().y - 1);
                //cleared floor
            if (tbm.getGoal() == playerBoardPosition && !playerScript.isBusy()) { handleClearedFloor(); }
        }
	}
    public void handleClearedFloor()
    {
        level -= 1;
    //clear priorKeys
        im.clearPriorKeys();
    //reset tiles
        clearTiles();
        tbm.clearedCurrentBoard(); //update boardmanager
        drawTiles();
    //reset player stuff
        playerScript.setPosition(new Vector2(tbm.getStart().x, tbm.getCurrentHeight() - tbm.getStart().y - 1));
        playerScript.notJump();
        handledPlayerJump = false;
    //reset rocks
        clearRocks();
    //place a rock at the exit
        rbm.nextPlaceAt((int)tbm.getGoal().x, (int)tbm.getGoal().y);
        rbm.clearedCurrentBoard(); //update rockmanager
        drawRocks();
        handleRocks(); //check to remove edge case

    }
    public void handleJump()
    {
        handledPlayerJump = true;
        //get player pos
        int playerBoardPosX = (int) playerScript.getPosition().x;
        int playerBoardPosY = tbm.getCurrentHeight() - (int)playerScript.getPosition().y - 1;
        //check if on solid ground
        if (tbm.currentIsHealthyAt(playerBoardPosX, playerBoardPosY)) {
            //player jump
            //playerScript.jump();

            //at player pos
            int x = playerBoardPosX;
            int y = playerBoardPosY;
            damageCurrentBoard(x, y);
            damageFutureBoard(x, y);
            //up from playerPos
            x = playerBoardPosX;
            y = playerBoardPosY - 1;
            damageCurrentBoard(x, y);
            damageFutureBoard(x, y);
            //down
            x = playerBoardPosX;
            y = playerBoardPosY + 1;
            damageCurrentBoard(x, y);
            damageFutureBoard(x, y);
            //left 
            x = playerBoardPosX - 1;
            y = playerBoardPosY;
            damageCurrentBoard(x, y);
            damageFutureBoard(x, y);
            //right
            x = playerBoardPosX + 1;
            y = playerBoardPosY;
            damageCurrentBoard(x,y);
            damageFutureBoard(x, y);
            //top left
            x = playerBoardPosX - 1;
            y = playerBoardPosY - 1;
            damageFutureBoard(x, y);
            //top right
            x = playerBoardPosX + 1;
            y = playerBoardPosY - 1;
            damageFutureBoard(x, y);
            //bot left
            x = playerBoardPosX - 1;
            y = playerBoardPosY + 1;
            damageFutureBoard(x, y);
            //bot right
            x = playerBoardPosX + 1;
            y = playerBoardPosY + 1;
            damageFutureBoard(x, y);
            //2 to the up
            x = playerBoardPosX;
            y = playerBoardPosY - 2;
            damageFutureBoard(x, y);
            //2 to the left
            x = playerBoardPosX - 2;
            y = playerBoardPosY;
            damageFutureBoard(x, y);
            //2 to the right
            x = playerBoardPosX + 2;
            y = playerBoardPosY;
            damageFutureBoard(x, y);
            //2 to the bottom
            x = playerBoardPosX;
            y = playerBoardPosY + 2;
            damageFutureBoard(x, y);
        }
    }
    //rockfall function
    public void dropRocks(int x, int y)
    {
        int rX = 0; //x position of rock
        int rY = 0; //y position of rock
        for (int i = 0; i < 5; i++)
        {
            if (i == 0)
            {
                //at position
                rX = x;
                rY = y;
            }
            else if (i == 1)
            {
                //left of
                rX = x - 1;
                rY = y;
            }
            else if (i == 2)
            {
                //right of
                rX = x + 1;
                rY = y;
            }
            else if (i == 3)
            {
                //above
                rX = x;
                rY = y - 1;
            }
            else if (i == 4)
            {
                //below
                rX = x;
                rY = y + 1;
            }
            //main rock drop
            //if (tbm.currentIsValidAt(x, y)) Debug.Log("A_x: " + x + " _ y: " + y);
            //if (tbm.currentIsDestroyedAt(x, y)) Debug.Log("B_x: " + x + " _ y: " + y);
            //if (rbm.currentHasRockAt(rX, rY)) Debug.Log("C_x: " + rX + " _ y: " + rY);
            //if (!rbm.currentIsValidAt(rX, rY) || rocks[rX][rY] != null) Debug.Log("D_x: " + rX + " _ y: " + rY);
            //check position
            if (tbm.currentIsValidAt(x, y) && tbm.currentIsDestroyedAt(x, y)
                && rbm.currentHasRockAt(rX, rY)
                && (!rbm.currentIsValidAt(rX, rY) || rocks[rX][rY] != null))
            {
                //Debug.Log("D_x: " + x + " _ y: " + y + "R_:" + rX + " _ y: " + rY);
                //remove rock on current / play animation
                Destroy(rocks[rX][rY]);
                rocks[rX][rY] = null;
                rbm.currentRemoveAt(rX, rY);
                //check if next board has a rock on hole's position
                if (rbm.nextHasRockAt(x, y))
                {
                    //next has rock, damage the floor
                    tbm.damageNextBoard(x, y);
                    //if floor is destroyed ... remove the rock
                    if (tbm.nextIsDestroyedAt(x, y)) { rbm.nextRemoveAt(x, y); }
                }
                else if (!tbm.nextIsDestroyedAt(x, y)) { rbm.nextPlaceAt(x, y); } //next board has no rock, place it there unless it has a hole
            }
        }
    }
    //calls droprocks on all tiles
    public void handleRocks()
    {
        //check rocks
        for (int i = 0; i < rbm.getCurrentWidth(); i++)
        {
            for (int j = 0; j < rbm.getCurrentHeight(); j++)
            {
                dropRocks(i, j);
            }
        }
    }
    public void handleBacktrack()
    {
        if (tbm.backTrackPossible())
        {
            //backtrack a level
            clearTiles();
            tbm.backTrack();
            drawTiles();
            //update rocks
            clearRocks();
            rbm.backTrack();
            drawRocks();
            //reset player
            playerScript.setPosition(new Vector2(tbm.getStart().x, tbm.getCurrentHeight() - tbm.getStart().y - 1));
            playerScript.notJump();
            handledPlayerJump = false;
            //clear prior keys list
            im.clearPriorKeys();
            handleRocks();
            //change floor
            level += 1;
        }
    }
    public void handleForwardTrack()
    {
        if (tbm.didBackTrack() && tbm.forwardTrackPossible())
        {
            //backtrack a level
            clearTiles();
            tbm.forwardTrack();
            drawTiles();
            //update rocks
            clearRocks();
            rbm.forwardTrack();
            drawRocks();
            //reset player
            playerScript.setPosition(new Vector2(tbm.getStart().x, tbm.getCurrentHeight() - tbm.getStart().y - 1));
            playerScript.notJump();
            handledPlayerJump = false;
            //clear prior keys list
            im.clearPriorKeys();
            handleRocks();
            //change floor
            level -= 1;
        }
    }
    //keyboard input
    public void handleInput()
    {
        if (Input.GetKeyUp(im.getMoveUpKey()) && !playerScript.isBusy()) {
            //if backtracked accept changes
            if (tbm.didBackTrack())
            {
                tbm.clearForwardBoards();
                rbm.clearForwardBoards();
            }
            //player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, tbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //check if up is valid & not blocked by rocks
            if (tbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1)
                && !tbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y - 1) 
                && rocks[(int)playerBoardPosition.x][(int)playerBoardPosition.y - 1] == null)
            {
                //move player
                player.GetComponent<Player>().moveUp();
                //check to affect cracked 'tiles' of current board
                if (tbm.currentIsDamagedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y))
                {
                    //break the floor
                    tbm.damageCurrentBoard((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //reload that tile - call its animation(?)
                    Destroy(tiles[(int)playerBoardPosition.x][(int)playerBoardPosition.y]);
                    //have to reverse y
                    tiles[(int)playerBoardPosition.x][(int)playerBoardPosition.y] = (GameObject)Instantiate(destroyedTile, new Vector3((int)playerBoardPosition.x, (int)playerScript.getPosition().y - 1, 0), Quaternion.identity); 
                    //check rocks
                    dropRocks((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                }
                else if (!tbm.nextIsDamagedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)){
                    //did not step off a damaged tile
                    tbm.damageNextBoard((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                }
                //add to prior keys list
                im.addToPriorKeys(im.getMoveUpKey());
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
            if (tbm.didBackTrack())
            {
                tbm.clearForwardBoards();
                rbm.clearForwardBoards();
            }
            //player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, tbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //check if move is valid
            if (tbm.currentIsValidAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1)
                && !tbm.currentIsDestroyedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y + 1) 
                && rocks[(int)playerBoardPosition.x][(int)playerBoardPosition.y + 1] == null)
            {
                //move player
                player.GetComponent<Player>().moveDown();
                //check to affect cracked 'tiles' of current board
                if (tbm.currentIsDamagedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y))
                {
                    //break the floor
                    tbm.damageCurrentBoard((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //reload that tile - call its animation(?)
                    Destroy(tiles[(int)playerBoardPosition.x][(int)playerBoardPosition.y]);
                    //have to reverse y ... again
                    tiles[(int)playerBoardPosition.x][(int)playerBoardPosition.y] = (GameObject)Instantiate(destroyedTile, new Vector3((int)playerBoardPosition.x, (int)playerScript.getPosition().y + 1, 0), Quaternion.identity);
                    //check rocks
                    dropRocks((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                }
                else if (!tbm.nextIsDamagedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)) {
                    //did not step off a damaged tile
                    tbm.damageNextBoard((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                }
                //add to prior keys list
                im.addToPriorKeys(im.getMoveDownKey());
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
            if (tbm.didBackTrack())
            {
                tbm.clearForwardBoards();
                rbm.clearForwardBoards();
            }
            //player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, tbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //check if move is valid
            if (tbm.currentIsValidAt((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y)
                && !tbm.currentIsDestroyedAt((int)playerBoardPosition.x - 1, (int)playerBoardPosition.y) 
                && rocks[(int)playerBoardPosition.x - 1][(int)playerBoardPosition.y] == null)
            {
                //move player
                player.GetComponent<Player>().moveLeft();
                //check to affect cracked 'tiles' of current board
                if (tbm.currentIsDamagedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y))
                {
                    //break the floor
                    tbm.damageCurrentBoard((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //reload that tile - call its animation(?)
                    Destroy(tiles[(int)playerBoardPosition.x][(int)playerBoardPosition.y]);
                    //have to reverse y ... again
                    tiles[(int)playerBoardPosition.x][(int)playerBoardPosition.y] = (GameObject)Instantiate(destroyedTile, new Vector3((int)playerBoardPosition.x, (int)playerScript.getPosition().y, 0), Quaternion.identity);
                    //check rocks
                    dropRocks((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                }
                else if (!tbm.nextIsDamagedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)) { 
                    //did not step off a damaged tile
                    tbm.damageNextBoard((int)playerBoardPosition.x, (int)playerBoardPosition.y); 
                }
                //add to prior keys list
                im.addToPriorKeys(im.getMoveLeftKey());
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
            if (tbm.didBackTrack())
            {
                tbm.clearForwardBoards();
                rbm.clearForwardBoards();
            }
            //check if move valid - player's y is flipped
            Vector2 playerBoardPosition = new Vector2(playerScript.getPosition().x, tbm.getCurrentHeight() - playerScript.getPosition().y - 1);
            //check if move valid
            if (tbm.currentIsValidAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y) 
                && !tbm.currentIsDestroyedAt((int)playerBoardPosition.x + 1, (int)playerBoardPosition.y)
                && rocks[(int)playerBoardPosition.x + 1][(int)playerBoardPosition.y] == null)
            {
                //move player
                player.GetComponent<Player>().moveRight();
                //check to affect cracked 'tiles' of current board
                if (tbm.currentIsDamagedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y))
                {
                    //break the floor
                    tbm.damageCurrentBoard((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                    //reload that tile - call its animation(?)
                    Destroy(tiles[(int)playerBoardPosition.x][(int)playerBoardPosition.y]);
                    //have to reverse y ... again
                    tiles[(int)playerBoardPosition.x][(int)playerBoardPosition.y] = (GameObject)Instantiate(destroyedTile, new Vector3((int)playerBoardPosition.x, (int)playerScript.getPosition().y, 0), Quaternion.identity);
                    //check rocks
                    dropRocks((int)playerBoardPosition.x, (int)playerBoardPosition.y);
                }
                else if (!tbm.nextIsDamagedAt((int)playerBoardPosition.x, (int)playerBoardPosition.y)) { 
                    //did not step off a damaged tile
                    tbm.damageNextBoard((int)playerBoardPosition.x, (int)playerBoardPosition.y); 
                }
                //add to prior keys list
                im.addToPriorKeys(im.getMoveRightKey());
            }
            else
            {
                //do the growing animation, hopping
                playerScript.hopInPlace();
            }
        }
        else if (!playerScript.didJump() && Input.GetKeyUp(im.getJumpKey()) && !playerScript.isBusy())
        {
            //if backtracked accept changes
            if (tbm.didBackTrack())
            {
                tbm.clearForwardBoards();
                rbm.clearForwardBoards();
            }
            playerScript.jump();
            //handleJump();
            //add to prior keys list
            im.addToPriorKeys(im.getJumpKey());
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

    public void damageCurrentBoard(int x, int y)
    {
        if (tbm.damageCurrentBoard(x, y))
        {
            //change the tile sprite or animate ...
            Destroy(tiles[x][y]);
            if (tbm.currentIsDamagedAt(x, y)) { 
                //draw a damaged tile
                tiles[x][y] = (GameObject)Instantiate(damagedTile, new Vector3(x, tbm.getCurrentHeight() - y - 1, 0), Quaternion.identity); 
            }
            else {
                //draw a destroyed tile
                tiles[x][y] = (GameObject)Instantiate(destroyedTile, new Vector3(x, tbm.getCurrentHeight() - y - 1, 0), Quaternion.identity);
                //drop rocks
                dropRocks(x, y);
            }
        }
    }
    public void damageFutureBoard(int x, int y)
    {
        tbm.damageNextBoard(x, y);
        //check rocks
        if (tbm.nextIsDestroyedAt(x, y)) { rbm.nextRemoveAt(x, y); }
    }

    //drawstuff
    public void drawTiles()
    {
        //add to tiles list and instantiate them - board is flipped
        for (int i = 0; i < tbm.getCurrentWidth(); i++)
        {
            for (int j = 0; j < tbm.getCurrentHeight(); j++)
            {
                //flip y
                int y = tbm.getCurrentHeight() - j - 1;
                if (tbm.currentIsHealthyAt(i, j)) { tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, 0), Quaternion.identity); }
                else if (tbm.currentIsDamagedAt(i, j)) { tiles[i][j] = (GameObject)Instantiate(damagedTile, new Vector3(i, y, 0), Quaternion.identity); }
                else if (tbm.currentIsDestroyedAt(i, j)) { tiles[i][j] = (GameObject)Instantiate(destroyedTile, new Vector3(i, y, 0), Quaternion.identity); }
            }
        }
    }
    public void drawRocks()
    {
        //add to tiles list and instantiate them - board is flipped
        for (int i = 0; i < rbm.getCurrentWidth(); i++) {
            for (int j = 0; j < rbm.getCurrentHeight(); j++) {
                //remove rock if it exists && flip y
                if (rbm.currentHasRockAt(i, j)) { rocks[i][j] = (GameObject)Instantiate(rock, new Vector3(i, rbm.getCurrentHeight() - j - 1, -1), Quaternion.identity);  }
            }
        }
    }

    //clearstuff
    public void clearTiles()
    {
        //destroy all instantiated objects
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                Destroy(tiles[i][j]);
                tiles[i][j] = null;
            }
        }
    }
    public void clearRocks()
    {
        //destroy all instantiated objects
        for (int i = 0; i < rocks.Length; i++)
        {
            for (int j = 0; j < rocks[i].Length; j++)
            {
                Destroy(rocks[i][j]);
                rocks[i][j] = null;
            }
        }
    }

    //reset
    public void resetBoard()
    {
        //reset player position
        playerScript.setPosition(new Vector2(tbm.getStart().x, tbm.getCurrentHeight() - tbm.getStart().y - 1));
        //reset player jumped
        playerScript.notJump();
        handledPlayerJump = false;
        //redraw
        clearTiles();
        tbm.reset(); //reset board to original
        drawTiles();
        //reset rocks
        clearRocks();
        //place a rock at the exit
        if (level != LEVEL_START) { rbm.currentPlaceAt((int)tbm.getGoal().x, (int)tbm.getGoal().y); }
        rbm.reset(); //update rockmanager
        drawRocks();
        handleRocks();
        //clear prior keys list
        im.clearPriorKeys();
    }
    public void resetGame()
    {
        level = LEVEL_START;
        im = new InputManager();
        //tiles
        clearTiles();
        tbm = new TileBoardManager();
        //rocks
        clearRocks();
        rbm = new RockBoardManager();
        //player
        playerScript.reset((int)tbm.getStart().x, (int)(tbm.getCurrentHeight() - tbm.getStart().y - 1));
        playerScript.notJump();
        handledPlayerJump = false;
        //draw board
        drawTiles();
    }
}
