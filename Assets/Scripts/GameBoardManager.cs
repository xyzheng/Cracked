using UnityEngine;
using System.Collections;

public class GameBoardManager : MonoBehaviour
{
    //BoardManager
   	public BoardManager bbm;
    //gameobjects
    public GameObject tile;
    public GameObject rock;
    public GameObject[][] tiles;
    public GameObject[][] rocks;

	public Rock rockScript;

    //peeking
    private bool peeking;
    private Color color;
    private int fadeFrames = 10;

    ///zooming in and out
    private enum AnimState { IDLE, ZOOM_IN, ZOOM_OUT, FADE_IN, FADE_OUT }
    private AnimState state;
    private bool evenWidth;
    private bool evenHeight;
    private float currentTileWidth;
    private float maxWidth = 5f;
    private float baseWidth = 1f;
    private float minWidth = 1f;
    private int frames = 25;

    // constructor
    void Start()
    {
        bbm = new BoardManager();
        //gameobjects
		rockScript = rock.GetComponent<Rock>();
        tiles = new GameObject[bbm.getCurrentWidth()][];
        for (int i = 0; i < bbm.getCurrentWidth(); i++) { tiles[i] = new GameObject[bbm.getCurrentHeight()]; }
        rocks = new GameObject[bbm.getCurrentWidth()][];
        for (int i = 0; i < bbm.getCurrentWidth(); i++) { rocks[i] = new GameObject[bbm.getCurrentHeight()]; }
        //draw tiles; draw rocks
        drawCurrentTiles();
        drawCurrentRocks();
        //peeking
        peeking = false;
        color = new Vector4(1f, 1f, 1f, 1f);
        //zoom
        state = AnimState.IDLE;
        currentTileWidth = 1;
    }
    void LateUpdate()
    {
        if (state != AnimState.IDLE)
        {
            if (state == AnimState.ZOOM_IN) { zoomIn(); }
            else if (state == AnimState.ZOOM_OUT) { zoomOut(); }
            else if (state == AnimState.FADE_IN) { unfade(); }
            else if (state == AnimState.FADE_OUT) { fade(); }
        }
    }
    //getters
    public int getCurrentWidth() { return bbm.getCurrentWidth(); }
    public int getCurrentHeight() { return bbm.getCurrentHeight(); }
    public bool didBackTrack() { return bbm.didBackTrack(); }
    public bool backTrackPossible() { return bbm.backTrackPossible(); }
    public bool forwardTrackPossible() { return bbm.forwardTrackPossible(); } 
    public bool currentIsHealthyAt(int x, int y) { return bbm.currentIsHealthyAt(x, y); }
    public bool canMoveTo(int x, int y)
    {
        return bbm.currentIsValidAt(x, y) && !bbm.currentIsDestroyedAt(x, y) && !bbm.currentHasRockAt(x, y);
    }
    public void steppedOn(int boardX, int boardY)
    {
        //check to affect cracked 'tiles' of current board
        if (bbm.currentIsDamagedAt(boardX, boardY)) { tiles[boardX][boardY].GetComponent<Tile>().stepCrackTile(); } 
        else if (bbm.currentIsValidAt(boardX, boardY)) { 
            tiles[boardX][boardY].GetComponent<Tile>().steppedOnTile();
            if (!bbm.nextIsDamagedAt(boardX, boardY))
            {
                //did not step off a damaged tile
                bbm.damageNextBoard(boardX, boardY);
            }
        }
    }
    public void steppedOffOf(int boardX, int boardY)
    {
        if (bbm.currentIsDamagedAt(boardX, boardY))
        {
            //break the floor
            tiles[boardX][boardY].GetComponent<Tile>().breakTile();
            bbm.damageCurrentBoard(boardX, boardY);
            // play sound for making a hole
            GameManager.aSrc[1].PlayOneShot(GameManager.hole, 1.0f);
            //check rocks
            dropRocks(boardX, boardY);
        }
        else if (!bbm.nextIsDamagedAt(boardX, boardY))
        {
            //did not step off a damaged tile
            bbm.damageNextBoard(boardX, boardY);
        }
    }
    public bool busy()
    {
        return state != AnimState.IDLE;
    }
    public bool isPeeking() { return peeking; } 
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
            drawCurrentTiles();
            clearRocks();
            drawCurrentRocks();
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
            drawCurrentTiles();
            clearRocks();
            drawCurrentRocks();
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
        drawCurrentRocks();
        drawCurrentTiles();
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
                tiles[x][y].GetComponent<Tile>().stepCrackTile();
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
		//        float deltaPosition = 1.0f / movingFrames;
		int rX = 0; //x position of rock
		int rY = 0; //y position of rock
		for (int i = 0; i < 5; i++)
		{
			if (i == 0)
			{
				//rock is at current tile's position (rock is on top of this tile)
				rX = x;
				rY = y;
			}
			else if (i == 1)
			{
				//rock is below (y - 1) current tile
				rX = x;
				rY = y + 1;
			}
			else if (i == 2)
			{
				//rock to left of current tile
				rX = x - 1;
				rY = y;
			}
			else if (i == 3)
			{
				//rock is above (y + 1) current tile
				rX = x;
				rY = y - 1;
			}
			else if (i == 4)
			{
				//rock is right of current tile
				rX = x + 1;
				rY = y;
			}
			//main rock drop
			if (bbm.currentIsValidAt(x, y) && bbm.currentIsDestroyedAt(x, y)
				&& (bbm.currentIsValidAt(rX, rY) && rocks[rX][rY] != null) && !bbm.nextHasRockAt(x, y)) 
			{
				if (i == 0)
				{
					StartCoroutine(rockScript.scaleRock(rocks[rX][rY], 0.5f));
				}
				if (i == 1)
				{
					StartCoroutine(rockScript.moveAndScaleRock(rocks[rX][rY], new Vector3(rocks[rX][rY].transform.position.x, rocks[rX][rY].transform.position.y + 1.0f, rocks[rX][rY].transform.position.z), 0.5f));
				}
				if (i == 2)
				{
					StartCoroutine(rockScript.moveAndScaleRock(rocks[rX][rY], new Vector3(rocks[rX][rY].transform.position.x + 1.0f, rocks[rX][rY].transform.position.y, rocks[rX][rY].transform.position.z), 0.5f));
				}
				if (i == 3)
				{
					StartCoroutine(rockScript.moveAndScaleRock(rocks[rX][rY], new Vector3(rocks[rX][rY].transform.position.x, rocks[rX][rY].transform.position.y - 1.0f, rocks[rX][rY].transform.position.z), 0.5f));
				}
				if (i == 4)
				{
					StartCoroutine(rockScript.moveAndScaleRock(rocks[rX][rY], new Vector3(rocks[rX][rY].transform.position.x - 1.0f, rocks[rX][rY].transform.position.y, rocks[rX][rY].transform.position.z), 0.5f));
				}
				//Destroy(rocks[rX][rY]);
				rocks[rX][rY] = null;
				bbm.currentRemoveAt(rX, rY);
				//next board has no rock, place it there unless it has a hole
				if (!bbm.nextIsDestroyedAt(x, y)) { bbm.nextPlaceRockAt(x, y); }
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
    private void drawCurrentTiles()
    {
        //add to tiles list and instantiate them - board is flipped
        for (int i = 0; i < bbm.getCurrentWidth(); i++)
        {
            for (int j = 0; j < bbm.getCurrentHeight(); j++)
            {
                //flip y
                int y = bbm.getCurrentHeight() - j - 1;
                if (bbm.currentIsHealthyAt(i, j)) {
                    tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, 0), Quaternion.identity);
                    if (bbm.nextIsDamagedAt(i, j)) { tiles[i][j].GetComponent<Tile>().steppedOnTile(); }
                }
                else if (bbm.currentIsDamagedAt(i, j))
                {
                    tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, 0), Quaternion.identity);
                    Tile script = tiles[i][j].GetComponent<Tile>();
                    script.steppedOnTile();
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
    private void drawNextTiles()
    {
        //add to tiles list and instantiate them - board is flipped
        for (int i = 0; i < bbm.getNextWidth(); i++)
        {
            for (int j = 0; j < bbm.getNextHeight(); j++)
            {
                //flip y
                int y = bbm.getNextHeight() - j - 1;
                if (bbm.nextIsHealthyAt(i, j)) { tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, 0), Quaternion.identity); }
                else if (bbm.nextIsDamagedAt(i, j))
                {
                    tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, 0), Quaternion.identity);
                    Tile script = tiles[i][j].GetComponent<Tile>();
                    script.steppedOnTile();
                    script.crackTile();
                }
                else if (bbm.nextIsDestroyedAt(i, j))
                {
                    tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, 0), Quaternion.identity);
                    Tile script = tiles[i][j].GetComponent<Tile>();
                    script.crackTile();
                    script.breakTile();
                }
            }
        }
    }
    private void drawCurrentRocks()
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
    private void drawNextRocks()
    {
        //add to tiles list and instantiate them - board is flipped
        for (int i = 0; i < bbm.getNextWidth(); i++)
        {
            for (int j = 0; j < bbm.getNextHeight(); j++)
            {
                //remove rock if it exists && flip y
                if (bbm.nextHasRockAt(i, j)) { rocks[i][j] = (GameObject)Instantiate(rock, new Vector3(i, bbm.getCurrentHeight() - j - 1, -1), Quaternion.identity); }
            }
        }
    }
    //zoom
    public void zoomTilesOut()
    {
        state = AnimState.ZOOM_OUT;
        currentTileWidth = baseWidth;
        if (bbm.getCurrentWidth() % 2 == 0) { evenWidth = true; }
        if (bbm.getCurrentHeight() % 2 == 0) { evenHeight = true; }
    }
    private void zoomOut()
    {
        //get center
        int cx = bbm.getCurrentWidth() / 2 + 1;
        int cy = bbm.getCurrentHeight() / 2 + 1;
        //get width
        float deltaSize = (maxWidth - minWidth) / frames;
        currentTileWidth += deltaSize;
        //done
        if (currentTileWidth >= maxWidth)
        {
            state = AnimState.IDLE;
            currentTileWidth = baseWidth;
        }
        float posX = 0;
        float posY = 0;
        float posZ = -2;
        //transform
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                //position
                //if at center, stay where you are; else move by sizeDelta
                if (i + 1 == cx || (evenWidth && i + 1 == cx - 1)) { posX = tiles[i][j].transform.position.x; }
                else { posX = tiles[i][j].transform.position.x - (cx - i - 1) * deltaSize; }
                //if at center, stay where you are; else move by sizeDelta
                if (j + 1 == cy || (evenHeight &&j + 1 == cy - 1)) { posY = tiles[i][j].transform.position.y; }
                else { posY = tiles[i][j].transform.position.y + (cy - j - 1) * deltaSize; }
                //movement
                if (state == AnimState.IDLE) { 
                    tiles[i][j].transform.position = new Vector3(i, bbm.getCurrentHeight() - j - 1, 0);
                    if (rocks[i][j] != null) { rocks[i][j].transform.position = new Vector3(i, bbm.getCurrentHeight() - j - 1, -1); }
                }
                else { 
                    tiles[i][j].transform.position = new Vector3(posX, posY, posZ);
                    if (rocks[i][j] != null) { rocks[i][j].transform.position = new Vector3(posX, posY, -1); }
                }
                //scale
                tiles[i][j].transform.localScale = new Vector3(currentTileWidth, currentTileWidth, transform.localScale.z);
                if (rocks[i][j] != null) { rocks[i][j].transform.localScale = new Vector3(currentTileWidth, currentTileWidth, transform.localScale.z); }
            }
        }
    }
    public void zoomTilesIn()
    {
        state = AnimState.ZOOM_IN;
        currentTileWidth = maxWidth;
        //get center
        int cx = bbm.getCurrentWidth() / 2 + 1;
        int cy = bbm.getCurrentHeight() / 2 + 1;
        //start zoomed out
        float posX = 0;
        float posY = 0;
        float posZ = -2;
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                //make maxwidth
                tiles[i][j].transform.localScale = new Vector3(maxWidth, maxWidth, transform.localScale.z);
                //if at center, stay where you are; else move by maxWidth
                if (i + 1 == cx || (evenWidth && i + 1 == cx - 1)) { posX = tiles[i][j].transform.position.x; }
                else { posX = tiles[i][j].transform.position.x - (cx - i - 1) * maxWidth; }
                //if at center, stay where you are; else move by maxWidth
                if (j + 1 == cy || (evenHeight && j + 1 == cy - 1)) { posY = tiles[i][j].transform.position.y; }
                else { posY = tiles[i][j].transform.position.y + (cy - j - 1) * maxWidth; }
                //move to maxpos
                tiles[i][j].transform.position = new Vector3(posX, posY, posZ);
                if (rocks[i][j] != null) { rocks[i][j].transform.position = new Vector3(posX, posY, -3); }
                if (rocks[i][j] != null) { rocks[i][j].transform.localScale = new Vector3(maxWidth, maxWidth, transform.localScale.z); }
            }
        }
        if (bbm.getCurrentWidth() % 2 == 0) { evenWidth = true; }
        if (bbm.getCurrentHeight() % 2 == 0) { evenHeight = true; }
    }
    private void zoomIn()
    {
        //get center
        int cx = bbm.getCurrentWidth() / 2 + 1;
        int cy = bbm.getCurrentHeight() / 2 + 1;
        //get width
        float deltaSize = (maxWidth - minWidth) / frames;
        currentTileWidth -= deltaSize;
        //done
        if (currentTileWidth <= minWidth)
        {
            state = AnimState.IDLE;
            currentTileWidth = baseWidth;
        }
        float posX = 0;
        float posY = 0;
        float posZ = -2;
        //transform
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                //position
                //if at center, stay where you are; else move by sizeDelta
                if (i + 1 == cx || (evenWidth && i + 1 == cx - 1)) { posX = tiles[i][j].transform.position.x; }
                else { posX = tiles[i][j].transform.position.x + (cx - i - 1) * deltaSize * 3/2; }
                //if at center, stay where you are; else move by sizeDelta
                if (j + 1 == cy || (evenHeight && j + 1 == cy - 1)) { posY = tiles[i][j].transform.position.y; }
                else { posY = tiles[i][j].transform.position.y - (cy - j - 1) * deltaSize * 3/2; }
                //movement
                if (state == AnimState.IDLE) { 
                    tiles[i][j].transform.position = new Vector3(i, bbm.getCurrentHeight() - j - 1, 0);
                    if (rocks[i][j] != null) { rocks[i][j].transform.position = new Vector3(i, bbm.getCurrentHeight() - j - 1, -1); }
                }
                else { 
                    tiles[i][j].transform.position = new Vector3(posX, posY, posZ);
                    if (rocks[i][j] != null) { rocks[i][j].transform.position = new Vector3(posX, posY, -1); }
                }
                //scale
                tiles[i][j].transform.localScale = new Vector3(currentTileWidth, currentTileWidth, transform.localScale.z);
                if (rocks[i][j] != null) { rocks[i][j].transform.localScale = new Vector3(currentTileWidth, currentTileWidth, transform.localScale.z); }
            }
        }
    }
    //peek
    public void peek()
    {
        peeking = true;
        //clear
        clearTiles();
        clearRocks();
        //draw
        drawNextTiles();
        drawNextRocks();
    }
    public void unPeek()
    {
        peeking = false;
        //clear
        clearTiles();
        clearRocks();
        //draw
        drawCurrentTiles();
        drawCurrentRocks();
        handleRocks(); //check to remove edge case
    }
    public void fadeTiles()
    {
        state = AnimState.FADE_OUT;
    }
    private void fade()
    {
        //calc color
        color = new Color(1f, 1f, 1f, color.a - (1f - 0.5f) / fadeFrames);
        //done
        if (color.a <= 0.5)
        {
            state = AnimState.IDLE;
            color = new Vector4(1f, 1f, 1f, 0.5f);
        }
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                tiles[i][j].GetComponent<SpriteRenderer>().color = color;
            }
        }
    }
    public void unfadeTiles()
    {
        state = AnimState.FADE_IN;
    }
    private void unfade()
    {
        //calc color
        color = new Color(1f, 1f, 1f, color.a + (1f - 0.5f) / fadeFrames);
        //done
        if (color.a >= 1f)
        {
            state = AnimState.IDLE;
            color = new Vector4(1f, 1f, 1f, 1f);
        }
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                tiles[i][j].GetComponent<SpriteRenderer>().color = color;
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
        drawCurrentRocks(); 
        drawCurrentTiles();
        handleRocks();
    }
    public void clear()
    {
        clearTiles();
        clearRocks();
        bbm = new BoardManager();
        drawCurrentTiles();
        drawCurrentRocks();
    }
}



