using UnityEngine;
using System.Collections;

public class GameBoardManager : MonoBehaviour
{
    //BoardManager
    protected BoardManager bbm;
    //gameobjects
    public GameObject tile;
    public GameObject rock;
    public GameObject[][] tiles;
    public GameObject[][] rocks;

	public Rock rockManager;

    // constructor
    void Start()
    {
        bbm = new BoardManager();
        //gameobjects
		rockManager = rock.GetComponent<Rock>();
        tiles = new GameObject[bbm.getCurrentWidth()][];
        for (int i = 0; i < bbm.getCurrentWidth(); i++) { tiles[i] = new GameObject[bbm.getCurrentHeight()]; }
        rocks = new GameObject[bbm.getCurrentWidth()][];
        for (int i = 0; i < bbm.getCurrentWidth(); i++) { rocks[i] = new GameObject[bbm.getCurrentHeight()]; }
        //draw tiles; draw rocks
        drawTiles();
        drawRocks();
    }
    //getters
    public int getCurrentWidth() { return bbm.getCurrentWidth(); }
    public int getCurrentHeight() { return bbm.getCurrentHeight(); }
    public bool currentIsHealthyAt(int x, int y) { return bbm.currentIsHealthyAt(x, y); }
    public bool canMoveTo(int x, int y)
    {
        return bbm.currentIsValidAt(x, y) && !bbm.currentIsDestroyedAt(x, y) && !bbm.currentHasRockAt(x, y);
    }
    public void steppedOn(int boardX, int boardY)
    {
        //check to affect cracked 'tiles' of current board
        tiles[boardX][boardY].GetComponent<Tile>().steppedOnTile();
    }
    public void steppedOffOf(int boardX, int boardY)
    {
        if (bbm.currentIsDamagedAt(boardX, boardY))
        {
            //break the floor
            tiles[boardX][boardY].GetComponent<Tile>().breakTile();
            bbm.damageCurrentBoard(boardX, boardY);
            //check rocks
            dropRocks(boardX, boardY);
        }
        else if (!bbm.nextIsDamagedAt(boardX, boardY))
        {
            //did not step off a damaged tile
            bbm.damageNextBoard(boardX, boardY);
        }
    }
    public Vector2 getStart() { return bbm.getStart(); }
    public Vector2 getGoal() { return bbm.getGoal(); }
    //setters
    public void moveWhileBacktrack()
    {
        //if backtracked accept changes
        if (bbm.didBackTrack())
        {
            bbm.clearForwardBoards();
            bbm.clearForwardBoards();
        }
    }
    public bool backtrack()
    {
        if (bbm.backTrackPossible())
        {
            //backtrack a level
            bbm.backTrack();
            //draw objects
            clearTiles();
            drawTiles();
            clearRocks();
            drawRocks();
            handleRocks();
            return true;
        }
        return false;
    }
    public bool forwardTrack()
    {
        if (bbm.didBackTrack() && bbm.forwardTrackPossible())
        {
            //forwardtrack a level
            bbm.forwardTrack();
            //drawobjects
            clearTiles();
            drawTiles();
            clearRocks();
            drawRocks();
            handleRocks();
            return true;
        }
        return false;
    }
    public void clearedCurrentBoard()
    {
        //tiles
        clearTiles();

        //rocks
        clearRocks();
        //place a rock at the exit
        bbm.nextPlaceRockAt((int)bbm.getGoal().x, (int)bbm.getGoal().y);
        //bbm.clearedCurrentBoard(); //update rock
        bbm.clearedCurrentBoard();
        drawRocks();
        drawTiles();
        handleRocks(); //check to remove edge case
    }
    public void damageCurrentBoard(int x, int y)
    {
        if (bbm.damageCurrentBoard(x, y))
        {
            //change the tile sprite or animate ...
            if (bbm.currentIsDamagedAt(x, y))
            {
                //damaged
                tiles[x][y].GetComponent<Tile>().crackTile();
            }
            else
            {
                //hole
                tiles[x][y].GetComponent<Tile>().breakTile();
                //drop rocks
                dropRocks(x, y);
            }
        }
    }
    public void damageFutureBoard(int x, int y)
    {
        if (bbm.currentIsValidAt(x,y)) {
            tiles[x][y].GetComponent<Tile>().steppedOnTile();
        }
        bbm.damageNextBoard(x, y);
        //check rocks
        if (bbm.nextIsDestroyedAt(x, y)) { bbm.nextRemoveRockAt(x, y); }
    }
    //rocks 
    public void dropRocks(int x, int y)
    {
//		float deltaPosition = 1.0f / movingFrames;
		int direction = 0;
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
				direction = 1;
                rX = x - 1;
                rY = y;
            }
            else if (i == 2)
            {
                //right of
				direction = 2;
                rX = x + 1;
                rY = y;
            }
            else if (i == 3)
            {
                //above
				direction = 3;
                rX = x;
                rY = y - 1;
            }
            else if (i == 4)
            {
                //below
				direction = 4;
                rX = x;
                rY = y + 1;
            }
            //main rock drop
            if (bbm.currentIsValidAt(x, y) && bbm.currentIsDestroyedAt(x, y)
                && bbm.currentHasRockAt(rX, rY)
                && (!bbm.currentIsValidAt(rX, rY) || rocks[rX][rY] != null))
            {
				
				if (direction == 1) {
					//rockManager.moveRockWest(rocks[rX][rY], rocks[rX][rY].transform.position.x, rocks[rX][rY].transform.position.y);
					//rockManager.currentState = Rock.rockMovement.MOVING_LEFT;
					StartCoroutine(rockManager.moveAndScaleRock(rocks[rX][rY], new Vector3 (rocks[rX][rY].transform.position.x + 1.0f, rocks[rX][rY].transform.position.y, rocks[rX][rY].transform.position.z), 0.5f));
				}
				if (direction == 2) {
					//rockManager.moveRockEast(rocks[rX][rY], rocks[rX][rY].transform.position.x, rocks[rX][rY].transform.position.y);
					//rockManager.currentState = Rock.rockMovement.MOVING_RIGHT;
					StartCoroutine(rockManager.moveAndScaleRock(rocks[rX][rY], new Vector3 (rocks[rX][rY].transform.position.x - 1.0f, rocks[rX][rY].transform.position.y, rocks[rX][rY].transform.position.z), 0.5f));
				}
				if (direction == 3) {
					//rockManager.moveRockNorth(rocks[rX][rY], rocks[rX][rY].transform.position.x, rocks[rX][rY].transform.position.y);
					//rockManager.currentState = Rock.rockMovement.MOVING_UP;
					StartCoroutine(rockManager.moveAndScaleRock(rocks[rX][rY], new Vector3 (rocks[rX][rY].transform.position.x, rocks[rX][rY].transform.position.y - 1.0f, rocks[rX][rY].transform.position.z), 0.5f));
				}
				if (direction == 4) {
					//moveRock(rocks[rX][rY], rocks[rX][rY].transform.position.x, rocks[rX][rY].transform.position.y);
					//rockManager.currentState = Rock.rockMovement.MOVING_DOWN;
					StartCoroutine(rockManager.moveAndScaleRock(rocks[rX][rY], new Vector3 (rocks[rX][rY].transform.position.x, rocks[rX][rY].transform.position.y + 1.0f, rocks[rX][rY].transform.position.z), 0.5f));
				}

				//Destroy(rocks[rX][rY]);
                rocks[rX][rY] = null;
                bbm.currentRemoveAt(rX, rY);
                //check if next board has a rock on hole's position
                if (bbm.nextHasRockAt(x, y))
                {
                    //next has rock, damage the floor
                    bbm.damageNextBoard(x, y);
                    //if floor is destroyed ... remove the rock
                    if (bbm.nextIsDestroyedAt(x, y)) { bbm.nextRemoveRockAt(x, y); }
                }
                else if (!bbm.nextIsDestroyedAt(x, y)) { bbm.nextPlaceRockAt(x, y); } //next board has no rock, place it there unless it has a hole
            }
        }
    }
    public void handleRocks()
    {
        //check rocks
        for (int i = 0; i < bbm.getCurrentWidth(); i++)
        {
            for (int j = 0; j < bbm.getCurrentHeight(); j++)
            {
                dropRocks(i, j);
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
    //drawstuff
    public void drawTiles()
    {
        //add to tiles list and instantiate them - board is flipped
        for (int i = 0; i < bbm.getCurrentWidth(); i++)
        {
            for (int j = 0; j < bbm.getCurrentHeight(); j++)
            {
                //flip y
                int y = bbm.getCurrentHeight() - j - 1;
                if (bbm.currentIsHealthyAt(i, j)) { tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, 0), Quaternion.identity); }
                else if (bbm.currentIsDamagedAt(i, j))
                {
                    tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, 0), Quaternion.identity);
                    Tile script = tiles[i][j].GetComponent<Tile>();
                    script.crackTile();
                }
                else if (bbm.currentIsDestroyedAt(i, j))
                {
                    tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, 0), Quaternion.identity);
                    Tile script = tiles[i][j].GetComponent<Tile>();
                    script.crackTile();
                    script.breakTile();
                }
            }
        }
    }
    public void drawRocks()
    {
        //add to tiles list and instantiate them - board is flipped
        for (int i = 0; i < bbm.getCurrentWidth(); i++)
        {
            for (int j = 0; j < bbm.getCurrentHeight(); j++)
            {
                //remove rock if it exists && flip y
                if (bbm.currentHasRockAt(i, j)) { rocks[i][j] = (GameObject)Instantiate(rock, new Vector3(i, bbm.getCurrentHeight() - j - 1, -1), Quaternion.identity); }
            }
        }
    }

    //reset
    public void resetBoard(bool placeRockAtExit)
    {
        //redraw
        clearTiles();
        //reset rocks
        clearRocks();
        //place a rock at the exit
        if (placeRockAtExit) { bbm.currentPlaceRockAt((int)bbm.getGoal().x, (int)bbm.getGoal().y); }
        bbm.reset(); //update board
        //draw
        drawRocks(); 
        drawTiles();
        handleRocks();
    }
    public void clear()
    {
        clearTiles();
        clearRocks();
        bbm = new BoardManager();
        drawTiles();
        drawRocks();
    }
}



