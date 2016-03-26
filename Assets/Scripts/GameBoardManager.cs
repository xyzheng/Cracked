using UnityEngine;
using System.Collections;

public class GameBoardManager : MonoBehaviour
{
    //BoardManagers
    protected TileBoardManager tbm;
    protected RockBoardManager rbm;
    //gameobjects
    public GameObject tile;
    public GameObject rock;
    public GameObject[][] tiles;
    public GameObject[][] rocks;

    // constructor
    void Start()
    {
        tbm = new TileBoardManager();
        rbm = new RockBoardManager();
        //gameobjects
        tiles = new GameObject[tbm.getCurrentWidth()][];
        for (int i = 0; i < tbm.getCurrentWidth(); i++) { tiles[i] = new GameObject[tbm.getCurrentHeight()]; }
        rocks = new GameObject[tbm.getCurrentWidth()][];
        for (int i = 0; i < tbm.getCurrentWidth(); i++) { rocks[i] = new GameObject[tbm.getCurrentHeight()]; }
        //draw tiles; draw rocks
        drawTiles();
        drawRocks();
    }
    //getters
    public int getCurrentWidth() { return tbm.getCurrentWidth(); }
    public int getCurrentHeight() { return tbm.getCurrentHeight(); }
    public bool currentIsHealthyAt(int x, int y) { return tbm.currentIsHealthyAt(x, y); }
    public bool canMoveTo(int x, int y)
    {
        return tbm.currentIsValidAt(x, y) && !tbm.currentIsDestroyedAt(x, y) && !rbm.currentHasRockAt(x, y);
    }
    public void steppedOn(int boardX, int boardY)
    {
        //check to affect cracked 'tiles' of current board
        tiles[boardX][boardY].GetComponent<Tile>().steppedOnTile();
    }
    public void steppedOffOf(int boardX, int boardY)
    {
        if (tbm.currentIsDamagedAt(boardX, boardY))
        {
            //break the floor
            tiles[boardX][boardY].GetComponent<Tile>().breakTile();
            tbm.damageCurrentBoard(boardX, boardY);
            //check rocks
            dropRocks(boardX, boardY);
        }
        else if (!tbm.nextIsDamagedAt(boardX, boardY))
        {
            //did not step off a damaged tile
            tbm.damageNextBoard(boardX, boardY);
        }
    }
    public Vector2 getStart() { return tbm.getStart(); }
    public Vector2 getGoal() { return tbm.getGoal(); }
    //setters
    public void moveWhileBacktrack()
    {
        //if backtracked accept changes
        if (tbm.didBackTrack())
        {
            tbm.clearForwardBoards();
            rbm.clearForwardBoards();
        }
    }
    public bool backtrack()
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
            handleRocks();
            return true;
        }
        return false;
    }
    public bool forwardTrack()
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
            handleRocks();
            return true;
        }
        return false;
    }
    public void clearedCurrentBoard()
    {
        //tiles
        clearTiles();
        tbm.clearedCurrentBoard();
        drawTiles();
        //rocks
        clearRocks();
        //place a rock at the exit
        rbm.nextPlaceAt((int)tbm.getGoal().x, (int)tbm.getGoal().y);
        rbm.clearedCurrentBoard(); //update rockmanager
        drawRocks();
        handleRocks(); //check to remove edge case
    }
    public void damageCurrentBoard(int x, int y)
    {
        if (tbm.damageCurrentBoard(x, y))
        {
            //change the tile sprite or animate ...
            if (tbm.currentIsDamagedAt(x, y))
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
        tbm.damageNextBoard(x, y);
        //check rocks
        if (tbm.nextIsDestroyedAt(x, y)) { rbm.nextRemoveAt(x, y); }
    }
    //rocks 
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
        for (int i = 0; i < tbm.getCurrentWidth(); i++)
        {
            for (int j = 0; j < tbm.getCurrentHeight(); j++)
            {
                //flip y
                int y = tbm.getCurrentHeight() - j - 1;
                if (tbm.currentIsHealthyAt(i, j)) { tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, 0), Quaternion.identity); }
                else if (tbm.currentIsDamagedAt(i, j))
                {
                    tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, 0), Quaternion.identity);
                    Tile script = tiles[i][j].GetComponent<Tile>();
                    script.crackTile();
                }
                else if (tbm.currentIsDestroyedAt(i, j))
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
        for (int i = 0; i < rbm.getCurrentWidth(); i++)
        {
            for (int j = 0; j < rbm.getCurrentHeight(); j++)
            {
                //remove rock if it exists && flip y
                if (rbm.currentHasRockAt(i, j)) { rocks[i][j] = (GameObject)Instantiate(rock, new Vector3(i, rbm.getCurrentHeight() - j - 1, -1), Quaternion.identity); }
            }
        }
    }
    //reset
    public void resetBoard(bool placeRockAtExit)
    {
        //redraw
        clearTiles();
        tbm.reset(); //reset board to original
        drawTiles();
        //reset rocks
        clearRocks();
        //place a rock at the exit
        if (placeRockAtExit) { rbm.currentPlaceAt((int)tbm.getGoal().x, (int)tbm.getGoal().y); }
        rbm.reset(); //update rockmanager
        drawRocks();
        handleRocks();
    }
    public void clear()
    {
        clearTiles();
        clearRocks();
        tbm = new TileBoardManager();
        rbm = new RockBoardManager();
        drawTiles();
        drawRocks();
    }
}



