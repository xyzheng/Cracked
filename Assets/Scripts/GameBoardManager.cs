using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameBoardManager : MonoBehaviour
{
    //BoardManager
    public BoardManager bbm;
    //gameobjects
    public GameObject tile;
    public GameObject mTile;
    public GameObject rock;
    public GameObject mRock;
    public GameObject[][] tiles;
    public GameObject[][] mTiles;
    public GameObject[][] rocks;
    private float TILE_Z = 0;
    private float ROCK_Z = -1;
    private float N_TILE_Z = 2;
    private float N_ROCK_Z = 1;
    public GameObject[][] mRocks;

    //for synching rock anim
    private List<KeyValuePair<Vector2, Vector2>> movingRocks;

    //minimap
    private Vector2 sAnchor;
    private Vector2 eAnchor;
    private float baseWidth = 1f;
    private float minWidth = 0.25f;

    //peeking
    private bool peeking;
    private Color color;
    private int fadeFrames = 10;

    ///zooming in and out
    private enum State { IDLE, BACKTRACK, FORWARDTRACK, PEEKING, CLEARED_BOARD, UNPEEKING }
    private State state;
    private float currentTileWidth;
    private float nextTileWidth;
    private int frames = 16;

    // constructor
    void Start()
    {
        bbm = new BoardManager();
        //gameobjects
        tiles = new GameObject[bbm.getCurrentWidth()][];
        for (int i = 0; i < bbm.getCurrentWidth(); i++) { tiles[i] = new GameObject[bbm.getCurrentHeight()]; }
        rocks = new GameObject[bbm.getCurrentWidth()][];
        for (int i = 0; i < bbm.getCurrentWidth(); i++) { rocks[i] = new GameObject[bbm.getCurrentHeight()]; }
        mTiles = new GameObject[bbm.getNextWidth()][];
        for (int i = 0; i < bbm.getNextWidth(); i++) { mTiles[i] = new GameObject[bbm.getNextHeight()]; }
        mRocks = new GameObject[bbm.getCurrentWidth()][];
        for (int i = 0; i < bbm.getNextWidth(); i++) { mRocks[i] = new GameObject[bbm.getNextHeight()]; }
        movingRocks = new List<KeyValuePair<Vector2,Vector2>>();
        //draw tiles; draw rocks
        drawTiles();
        drawRocks();
        //peeking
        peeking = false;
        color = new Vector4(1f, 1f, 1f, 1f);
        //zoom
        state = State.IDLE;
        currentTileWidth = baseWidth;
        nextTileWidth = minWidth;
    }
    //Do all procedural animation in lateupdate - conflicts with unity's animator otherwise
    void LateUpdate()
    {
        //rocks
        if (movingRocks.Count != 0)
        {
            for (int i = 0; i < movingRocks.Count; i++)
            {
                if (rocks[(int)movingRocks[i].Key.x][(int)movingRocks[i].Key.y] == null){
                    Debug.Log("Rock at x:" + (int)movingRocks[i].Key.x + " y:" + (int)movingRocks[i].Key.y + "is null");
                }
                else if (!rocks[(int)movingRocks[i].Key.x][(int)movingRocks[i].Key.y].GetComponent<Rock>().isBusy())
                {
                    Vector2 dest = movingRocks[i].Value;
                    //swap
                    rocks[(int)dest.x][(int)dest.y] = rocks[(int)movingRocks[i].Key.x][(int)movingRocks[i].Key.y];
                    rocks[(int)movingRocks[i].Key.x][(int)movingRocks[i].Key.y] = null;
                    handleSingleRock((int)dest.x, (int)dest.y);
                    movingRocks.RemoveAt(i);
                    i--;
                }
            }
        }
        //tiles
        if (state != State.IDLE)
        {
            if (state == State.BACKTRACK) {
                if (moveBoardRight())
                {
                    //backtrack a level
                    bbm.backTrack();
                    //draw objects
                    clearTiles();
                    drawTiles();
                    clearRocks();
                    drawRocks();
                    handleRocks();
                }
            }
            else if (state == State.FORWARDTRACK) {
                if (moveMapLeft())
                {
                    //forwardtrack a level
                    bbm.forwardTrack();
                    //drawobjects
                    clearTiles();
                    drawTiles();
                    clearRocks();
                    drawRocks();
                    handleRocks();
                }
            }
            else if (state == State.CLEARED_BOARD)
            {
                if (moveMapLeft())
                {
                    //clear
                    clearTiles();
                    clearRocks();
                    //place a rock at the exit
                    bbm.nextPlaceRockAt((int)bbm.getGoal().x, (int)bbm.getGoal().y);
                    bbm.clearedCurrentBoard();
                    drawRocks();
                    drawTiles();
                    handleRocks(); //check to remove edge case
                }
            }
            else if (state == State.PEEKING)
            {
                if (moveMapLeft())
                {
                    for (int i = 0; i < mTiles.Length; i++)
                    {
                        for (int j = 0; j < mTiles[i].Length; j++)
                        {
                            mTiles[i][j].transform.position = new Vector3(i, getCurrentHeight() - j - 1, TILE_Z - 2);
                            mTiles[i][j].transform.localScale = new Vector3(baseWidth, baseWidth, transform.localScale.z);
                            if (mRocks[i][j] != null)
                            {
                                mRocks[i][j].transform.position = new Vector3(i, getCurrentHeight() - j - 1, ROCK_Z - 2);
                                mRocks[i][j].transform.localScale = new Vector3(baseWidth, baseWidth, transform.localScale.z);
                            }
                        }
                    }
                }
            }
            else if (state == State.UNPEEKING)
            {
                if (moveMapRight())
                {
                    for (int i = 0; i < mTiles.Length; i++)
                    {
                        for (int j = 0; j < mTiles[i].Length; j++)
                        {
                            mTiles[i][j].transform.position = new Vector3(eAnchor.x + (i * minWidth), eAnchor.y + ((bbm.getNextHeight() - j - 1) * minWidth), N_TILE_Z);
                            mTiles[i][j].transform.localScale = new Vector3(minWidth, minWidth, transform.localScale.z);
                            if (mRocks[i][j] != null)
                            {
                                mRocks[i][j].transform.position = new Vector3(eAnchor.x + (i * minWidth), eAnchor.y + ((bbm.getNextHeight() - j - 1) * minWidth), N_TILE_Z);
                                mRocks[i][j].transform.localScale = new Vector3(minWidth, minWidth, transform.localScale.z);
                            }
                        }
                    }

                }
            }
        }
    }
    //getters
    public int getCurrentWidth() { return bbm.getCurrentWidth(); }
    public int getCurrentHeight() { return bbm.getCurrentHeight(); }
    public bool didBackTrack() { return bbm.didBackTrack(); }
    public bool backTrackPossible() { return bbm.backTrackPossible(); }
    public bool forwardTrackPossible() { return bbm.forwardTrackPossible(); }
    public bool currentIsHealthyAt(int x, int y) { return bbm.currentIsHealthyAt(x, y); }
    public bool nextIsDamagedAt(int x, int y) { return bbm.nextIsDamagedAt(x, y); }
    public bool canMoveTo(int x, int y) { return bbm.currentIsValidAt(x, y) && !bbm.currentIsDestroyedAt(x, y) && !bbm.currentHasRockAt(x, y); }
    public void steppedOn(int x, int y)
    {
        if (!bbm.currentIsDamagedAt(x, y))
        {
            tiles[x][y].GetComponent<Tile>().stepTile();
            if (!bbm.nextIsDamagedAt(x, y)) { 
                bbm.damageNextBoard(x, y);
                updateTile(x, y);
            } 
        } else if (bbm.currentIsDamagedAt(x,y)){
            //shake rocks
            if (bbm.currentHasRockAt(x, y + 1)) {
                //check if moving rock
                if (rocks[x][y+1] == null){
                    for (int i = 0; i < movingRocks.Count; i++)
                    {
                        if (movingRocks[i].Value.x == x && movingRocks[i].Value.y == y+1)
                        {
                            rocks[x][y].GetComponent<Rock>().startShake(); 
                        }
                    }
                } else { rocks[x][y+1].GetComponent<Rock>().startShake(); }
            }
            else if (bbm.currentHasRockAt(x - 1, y))
            {
                if (rocks[x - 1][y] == null)
                {
                    for (int i = 0; i < movingRocks.Count; i++)
                    {
                        if (movingRocks[i].Value.x == x && movingRocks[i].Value.y == y + 1)
                        {
                            rocks[x][y].GetComponent<Rock>().startShake();
                        }
                    }
                }
                else { rocks[x - 1][y].GetComponent<Rock>().startShake(); }
            }
            else if (bbm.currentHasRockAt(x, y - 1))
            {
                if (rocks[x][y - 1] == null)
                {
                    for (int i = 0; i < movingRocks.Count; i++)
                    {
                        if (movingRocks[i].Value.x == x && movingRocks[i].Value.y == y + 1)
                        {
                            rocks[x][y].GetComponent<Rock>().startShake();
                        }
                    }
                }
                else { rocks[x][y - 1].GetComponent<Rock>().startShake(); }
            }
            else if (bbm.currentHasRockAt(x + 1, y))
            {
                if (rocks[x + 1][y] == null)
                {
                    for (int i = 0; i < movingRocks.Count; i++)
                    {
                        if (movingRocks[i].Value.x == x && movingRocks[i].Value.y == y + 1)
                        {
                            rocks[x][y].GetComponent<Rock>().startShake();
                        }
                    }
                }
                else { rocks[x + 1][y].GetComponent<Rock>().startShake(); }
            }
        }
       
    }
    public void steppedOffOf(int x, int y)
    {
        if (bbm.currentIsDamagedAt(x, y))
        {
            //break the floor
            bbm.damageCurrentBoard(x, y);
            // play sound for making a hole
            GameManager.aSrc[1].PlayOneShot(GameManager.hole, 1.0f);
            updateTile(x, y);
            //check rocks
            dropRocks(x, y);
        }
        else if (!bbm.nextIsDamagedAt(x, y))
        {
            //did not step off a damaged tile
            bbm.damageNextBoard(x, y);
            updateTile(x, y);
        }
    }
    public bool busy()
    {
        return state != State.IDLE;
    }
    public bool isPeeking() { return peeking; }
    public Vector2 getStart() { return bbm.getStart(); }
    public Vector2 getGoal() { return bbm.getGoal(); }
    //setters
    public void moveWhileBacktrack(int offOfx, int offOfy)
    {
        //if backtracked accept changes
        if (bbm.didBackTrack())
        {
            bbm.clearForwardBoards();
            steppedOn(offOfx, offOfy);
            clearTiles();
            clearRocks();
            drawTiles();
            drawRocks();
        }
    }
    public bool backtrack()
    {
        if (bbm.backTrackPossible())
        {
            goUp();
            return true;
        }
        return false;
    }
    public bool forwardTrack()
    {
        if (bbm.didBackTrack() && bbm.forwardTrackPossible())
        {
            goDown();
            return true;
        }
        return false;
    }
    public void clearedCurrentBoard()
    {
        state = State.CLEARED_BOARD;
    }
    public void damageCurrentBoard(int x, int y)
    {
        if (bbm.damageCurrentBoard(x, y))
        {
            updateTile(x, y);
            if (bbm.currentIsDestroyedAt(x, y)) { dropRocks(x, y); }
        }
    }
    public void damageFutureBoard(int x, int y)
    {
        bbm.damageNextBoard(x, y);
        updateTile(x,y);
        //check rocks
        if (bbm.nextIsDestroyedAt(x, y)) { bbm.nextRemoveRockAt(x, y); }
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
                //rock is at current tile's position (rock is on top of this tile)
                rX = x;
                rY = y;
            }
            else if (i == 1)
            {
                //rock is below current tile
                rX = x;
                rY = y + 1;
                //Debug.Log("rx:" + rX + " ry:" + rY);
                //Debug.Log(bbm.currentIsValidAt(x, y));
                //Debug.Log(bbm.currentIsDestroyedAt(x, y));
                //Debug.Log(bbm.currentIsValidAt(rX, rY) && rocks[rX][rY] != null);
                //Debug.Log(!bbm.nextHasRockAt(x, y));
                //Debug.Log(bbm.currentIsValidAt(rX, rY) && rocks[rX][rY] != null && !rocks[rX][rY].GetComponent<Rock>().isBusy());
            }
            else if (i == 2)
            {
                //rock to left of current tile
                rX = x - 1;
                rY = y;
            }
            else if (i == 3)
            {
                //rock is above current tile
                rX = x;
                rY = y - 1;
                //Debug.Log("rx:" + rX + " ry:" + rY);
                //Debug.Log(bbm.currentIsValidAt(x, y));
                //Debug.Log(bbm.currentIsDestroyedAt(x, y));
                //Debug.Log(bbm.currentIsValidAt(rX, rY) && rocks[rX][rY] != null);
                //Debug.Log(!bbm.nextHasRockAt(x, y));
                //Debug.Log(bbm.currentIsValidAt(rX, rY) && rocks[rX][rY] != null && !rocks[rX][rY].GetComponent<Rock>().isBusy());
            }
            else if (i == 4)
            {
                //rock is right of current tile
                rX = x + 1;
                rY = y;
            }
            //main rock drop
            if (bbm.currentIsValidAt(x, y) && bbm.currentIsDestroyedAt(x, y)
                && (bbm.currentIsValidAt(rX, rY) && rocks[rX][rY] != null) && !bbm.nextHasRockAt(x, y)
                && (bbm.currentIsValidAt(rX, rY) && !rocks[rX][rY].GetComponent<Rock>().isBusy()))
            {
                if (i == 0)
                {
                    StartCoroutine(rocks[rX][rY].GetComponent<Rock>().scale(0.5f));
                }
                if (i == 1)
                {
                    StartCoroutine(rocks[rX][rY].GetComponent<Rock>().moveAndScale(new Vector3(rocks[rX][rY].transform.position.x, rocks[rX][rY].transform.position.y + 1.0f, rocks[rX][rY].transform.position.z), 0.5f));
                }
                if (i == 2)
                {
                    StartCoroutine(rocks[rX][rY].GetComponent<Rock>().moveAndScale(new Vector3(rocks[rX][rY].transform.position.x + 1.0f, rocks[rX][rY].transform.position.y, rocks[rX][rY].transform.position.z), 0.5f));
                }
                if (i == 3)
                {
                    StartCoroutine(rocks[rX][rY].GetComponent<Rock>().moveAndScale(new Vector3(rocks[rX][rY].transform.position.x, rocks[rX][rY].transform.position.y - 1.0f, rocks[rX][rY].transform.position.z), 0.5f));
                }
                if (i == 4)
                {
                    StartCoroutine(rocks[rX][rY].GetComponent<Rock>().moveAndScale(new Vector3(rocks[rX][rY].transform.position.x - 1.0f, rocks[rX][rY].transform.position.y, rocks[rX][rY].transform.position.z), 0.5f));
                }
                //Destroy(rocks[rX][rY]);
                rocks[rX][rY] = null;
                bbm.currentRemoveAt(rX, rY);
                //next board has no rock, place it there unless it has a hole
                if (!bbm.nextIsDestroyedAt(x, y)) { 
                    bbm.nextPlaceRockAt(x, y);
                    updateRock(x, y);
                }
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
    public void handleSingleRock(int x, int y)      // calls drop rock on all holes adj to a specific rock
    {
        if (bbm.currentIsDestroyedAt(x, y))
        {
            dropRocks(x, y);            // at Position
        }
        if (bbm.currentIsDestroyedAt(x, y + 1))
        {
            dropRocks(x, y + 1);    // below Pos

        }
        if (bbm.currentIsDestroyedAt(x - 1, y)) dropRocks(x - 1, y);    // left of Pos
        if (bbm.currentIsDestroyedAt(x, y - 1))
        {
            dropRocks(x, y - 1);    // above Pos
        }
        if (bbm.currentIsDestroyedAt(x + 1, y)) dropRocks(x + 1, y);    // right of Pos
    }
    //clearstuff
    private void clearTiles()
    {
        clearCurrentTiles();
        clearNextTiles();
    }
    private void clearCurrentTiles()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                Destroy(tiles[i][j]);
                tiles[i][j] = null;
            }
        }
    }
    private void clearNextTiles()
    {
        for (int i = 0; i < mTiles.Length; i++)
        {
            for (int j = 0; j < mTiles[i].Length; j++)
            {
                Destroy(mTiles[i][j]);
                mTiles[i][j] = null;
            }
        }
    }
    private void clearRocks()
    {
        clearCurrentRocks();
        clearNextRocks();
    }
    private void clearCurrentRocks()
    {
        for (int i = 0; i < rocks.Length; i++)
        {
            for (int j = 0; j < rocks[i].Length; j++)
            {
                Destroy(rocks[i][j]);
                rocks[i][j] = null;
            }
        }
    }
    private void clearNextRocks()
    {
        for (int i = 0; i < mRocks.Length; i++)
        {
            for (int j = 0; j < mRocks[i].Length; j++)
            {
                Destroy(mRocks[i][j]);
                mRocks[i][j] = null;
            }
        }
    }
    //drawstuff
    private void drawBoard()
    {
        drawTiles();
        drawRocks();
    }
    private void drawTiles()
    {
        sAnchor = new Vector2(bbm.getStart().x, bbm.getCurrentHeight() - bbm.getStart().y);
        eAnchor = new Vector2(sAnchor.x + bbm.getCurrentWidth() - minWidth, bbm.getCurrentHeight() - (bbm.getCurrentHeight() / 2) - 1.0f);
        drawCurrentTiles();
        drawNextTiles();
    }
    private void drawCurrentTiles()
    {
        //add to tiles list and instantiate them - board is flipped
        for (int i = 0; i < bbm.getCurrentWidth(); i++)
        {
            for (int j = 0; j < bbm.getCurrentHeight(); j++)
            {
                //flip y
                int y = bbm.getCurrentHeight() - j - 1;
                if (bbm.currentIsHealthyAt(i, j))
                {
                    tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, TILE_Z), Quaternion.identity);
                    if (bbm.nextIsDamagedAt(i, j)) { tiles[i][j].GetComponent<Tile>().forceSteppedTile(); }
                }
                else if (bbm.currentIsDamagedAt(i, j))
                {
                    tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, TILE_Z), Quaternion.identity);
                    Tile script = tiles[i][j].GetComponent<Tile>();
                    script.forceCrackedTile();
                }
                else if (bbm.currentIsDestroyedAt(i, j))
                {
                    tiles[i][j] = (GameObject)Instantiate(tile, new Vector3(i, y, TILE_Z), Quaternion.identity);
                    Tile script = tiles[i][j].GetComponent<Tile>();
                    script.forceBrokenTile();
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
                mTiles[i][j] = (GameObject)Instantiate(mTile, new Vector3(eAnchor.x + (i * minWidth), eAnchor.y + ((bbm.getNextHeight() - j - 1) * minWidth), N_TILE_Z), Quaternion.identity);
                if (bbm.nextIsDamagedAt(i, j))
                {
                    Tile script = mTiles[i][j].GetComponent<Tile>();
                    script.crackTile();
                }
                else if (bbm.nextIsDestroyedAt(i, j))
                {
                    Tile script = mTiles[i][j].GetComponent<Tile>();
                    script.crackTile();
                    script.breakTile();
                }
            }
        }
    }
    private void drawRocks()
    {
        sAnchor = new Vector2(bbm.getStart().x, bbm.getCurrentHeight() - bbm.getStart().y);
        eAnchor = new Vector2(sAnchor.x + bbm.getCurrentWidth() - minWidth, bbm.getCurrentHeight() - (bbm.getCurrentHeight()/2) - 1.0f);
        drawCurrentRocks();
        drawNextRocks();
    }
    private void drawCurrentRocks()
    {
        for (int i = 0; i < bbm.getCurrentWidth(); i++)
        {
            for (int j = 0; j < bbm.getCurrentHeight(); j++)
            {
                if (bbm.currentHasRockAt(i, j)) { rocks[i][j] = (GameObject)Instantiate(rock, new Vector3(i, bbm.getCurrentHeight() - j - 1, -1), Quaternion.identity); }
            }
        }
    }
    private void drawNextRocks()
    {
        for (int i = 0; i < bbm.getNextWidth(); i++)
        {
            for (int j = 0; j < bbm.getNextHeight(); j++)
            {
                if (bbm.nextHasRockAt(i, j)) { mRocks[i][j] = (GameObject)Instantiate(mRock, new Vector3(eAnchor.x + (i * minWidth), eAnchor.y + ((bbm.getCurrentHeight() - j - 1) * minWidth), N_ROCK_Z), Quaternion.identity); }
            }
        }
    }
    private void updateTile(int x, int y)
    {
        //current board
        if (bbm.currentIsDamagedAt(x, y)){
            if (bbm.nextIsDamagedAt(x,y)){ tiles[x][y].GetComponent<Tile>().stepCrackTile(); }
            else { tiles[x][y].GetComponent<Tile>().crackTile(); }
        }
        else if (bbm.currentIsDestroyedAt(x, y)) { tiles[x][y].GetComponent<Tile>().breakTile(); }
        //next board
        if (bbm.nextIsDamagedAt(x, y)) {
            tiles[x][y].GetComponent<Tile>().stepTile();
            mTiles[x][y].GetComponent<Tile>().crackTile(); 
        }
        else if (bbm.nextIsDestroyedAt(x, y)) { mTiles[x][y].GetComponent<Tile>().breakTile(); }
    }
    private void updateRock(int x, int y)
    {
        //next
        if (bbm.nextHasRockAt(x,y) && mRocks[x][y] == null){
            mRocks[x][y] = (GameObject)Instantiate(mRock, new Vector3(eAnchor.x + (x * minWidth), eAnchor.y + ((bbm.getCurrentHeight() - y - 1) * minWidth), N_ROCK_Z), Quaternion.identity);
        }
    }

    //zoom
    public void goDown()
    {
        state = State.FORWARDTRACK;
        currentTileWidth = baseWidth;
        nextTileWidth = minWidth;
    }
    private bool moveMapLeft() {
        Vector2 delta = (sAnchor - eAnchor) / frames;
        float x = mTiles[0][0].transform.position.x + delta.x;
        float y = mTiles[0][0].transform.position.y - delta.y;
        //done
        if (nextTileWidth >= baseWidth || x < sAnchor.x || y < sAnchor.y)
        {
            state = State.IDLE;
            nextTileWidth = minWidth;
        }
        else
        {
            float deltaSize = (baseWidth - minWidth)/frames;
            nextTileWidth += deltaSize;
            //transform
            for (int i = 0; i < mTiles.Length; i++)
            {
                for (int j = 0; j < mTiles[i].Length; j++)
                {
                    mTiles[i][j].transform.position = new Vector3(x + (i * nextTileWidth), y - (j * nextTileWidth), TILE_Z - 2);
                    mTiles[i][j].transform.localScale = new Vector3(nextTileWidth, nextTileWidth, transform.localScale.z);
                    if (mRocks[i][j] != null)
                    {
                        mRocks[i][j].transform.position = new Vector3(x + (i * nextTileWidth), y - (j * nextTileWidth), ROCK_Z - 2);
                        mRocks[i][j].transform.localScale = new Vector3(nextTileWidth, nextTileWidth, transform.localScale.z);
                    }
                }
            }
        }
        return state == State.IDLE;
    }
    //private void zoomOut()
    //{
    //    //get center
    //    int cx = bbm.getCurrentWidth() / 2 + 1;
    //    int cy = bbm.getCurrentHeight() / 2 + 1;
    //    //get width
    //    float deltaSize = (maxWidth - minWidth) / frames;
    //    currentTileWidth += deltaSize;
    //    //done
    //    if (currentTileWidth >= maxWidth)
    //    {
    //        state = AnimState.IDLE;
    //        currentTileWidth = baseWidth;
    //    }
    //    float posX = 0;
    //    float posY = 0;
    //    float posZ = -2;
    //    //transform
    //    for (int i = 0; i < tiles.Length; i++)
    //    {
    //        for (int j = 0; j < tiles[i].Length; j++)
    //        {
    //            //position
    //            //if at center, stay where you are; else move by sizeDelta
    //            if (i + 1 == cx || (evenWidth && i + 1 == cx - 1)) { posX = tiles[i][j].transform.position.x; }
    //            else { posX = tiles[i][j].transform.position.x - (cx - i - 1) * deltaSize; }
    //            //if at center, stay where you are; else move by sizeDelta
    //            if (j + 1 == cy || (evenHeight &&j + 1 == cy - 1)) { posY = tiles[i][j].transform.position.y; }
    //            else { posY = tiles[i][j].transform.position.y + (cy - j - 1) * deltaSize; }
    //            //movement
    //            if (state == AnimState.IDLE) { 
    //                tiles[i][j].transform.position = new Vector3(i, bbm.getCurrentHeight() - j - 1, 0);
    //                if (rocks[i][j] != null) { rocks[i][j].transform.position = new Vector3(i, bbm.getCurrentHeight() - j - 1, -1); }
    //            }
    //            else { 
    //                tiles[i][j].transform.position = new Vector3(posX, posY, posZ);
    //                if (rocks[i][j] != null) { rocks[i][j].transform.position = new Vector3(posX, posY, -1); }
    //            }
    //            //scale
    //            tiles[i][j].transform.localScale = new Vector3(currentTileWidth, currentTileWidth, transform.localScale.z);
    //            if (rocks[i][j] != null) { rocks[i][j].transform.localScale = new Vector3(currentTileWidth, currentTileWidth, transform.localScale.z); }
    //        }
    //    }
    //}
    public void goUp()
    {
        state = State.BACKTRACK;
        currentTileWidth = baseWidth;
    }
    //private void zoomIn()
    //{
    //    //get center
    //    int cx = bbm.getCurrentWidth() / 2 + 1;
    //    int cy = bbm.getCurrentHeight() / 2 + 1;
    //    //get width
    //    float deltaSize = (maxWidth - minWidth) / frames;
    //    currentTileWidth -= deltaSize;
    //    //done
    //    if (currentTileWidth <= minWidth)
    //    {
    //        state = AnimState.IDLE;
    //        currentTileWidth = baseWidth;
    //    }
    //    float posX = 0;
    //    float posY = 0;
    //    float posZ = -2;
    //    //transform
    //    for (int i = 0; i < tiles.Length; i++)
    //    {
    //        for (int j = 0; j < tiles[i].Length; j++)
    //        {
    //            //position
    //            //if at center, stay where you are; else move by sizeDelta
    //            if (i + 1 == cx || (evenWidth && i + 1 == cx - 1)) { posX = tiles[i][j].transform.position.x; }
    //            else { posX = tiles[i][j].transform.position.x + (cx - i - 1) * deltaSize * 3/2; }
    //            //if at center, stay where you are; else move by sizeDelta
    //            if (j + 1 == cy || (evenHeight && j + 1 == cy - 1)) { posY = tiles[i][j].transform.position.y; }
    //            else { posY = tiles[i][j].transform.position.y - (cy - j - 1) * deltaSize * 3/2; }
    //            //movement
    //            if (state == AnimState.IDLE) { 
    //                tiles[i][j].transform.position = new Vector3(i, bbm.getCurrentHeight() - j - 1, 0);
    //                if (rocks[i][j] != null) { rocks[i][j].transform.position = new Vector3(i, bbm.getCurrentHeight() - j - 1, -1); }
    //            }
    //            else { 
    //                tiles[i][j].transform.position = new Vector3(posX, posY, posZ);
    //                if (rocks[i][j] != null) { rocks[i][j].transform.position = new Vector3(posX, posY, -1); }
    //            }
    //            //scale
    //            tiles[i][j].transform.localScale = new Vector3(currentTileWidth, currentTileWidth, transform.localScale.z);
    //            if (rocks[i][j] != null) { rocks[i][j].transform.localScale = new Vector3(currentTileWidth, currentTileWidth, transform.localScale.z); }
    //        }
    //    }
    //}
    //peek
    private bool moveBoardRight()
    {
        Vector2 delta = (eAnchor - sAnchor) / frames;
        float x = tiles[0][0].transform.position.x + delta.x;
        float y = tiles[0][0].transform.position.y - delta.y;
        //done
        if (currentTileWidth <= minWidth || x > eAnchor.x || y < eAnchor.y )
        {
            state = State.IDLE;
            currentTileWidth = baseWidth;
        }
        else
        {
            float deltaSize = (baseWidth - minWidth) / frames;
            currentTileWidth -= deltaSize;
            //transform
            for (int i = 0; i < tiles.Length; i++)
            {
                for (int j = 0; j < tiles[i].Length; j++)
                {
                    tiles[i][j].transform.position = new Vector3(x + (i * currentTileWidth), y - (j * currentTileWidth), TILE_Z - 2);
                    tiles[i][j].transform.localScale = new Vector3(currentTileWidth, currentTileWidth, transform.localScale.z);
                    if (rocks[i][j] != null)
                    {
                        rocks[i][j].transform.position = new Vector3(x + (i * currentTileWidth), y - (j * currentTileWidth), ROCK_Z - 2);
                        rocks[i][j].transform.localScale = new Vector3(currentTileWidth, currentTileWidth, transform.localScale.z);
                    }
                }
            }
        }
        return state == State.IDLE;
    }
    private bool moveMapRight()
    {
        Vector2 delta = (eAnchor - sAnchor) / frames;
        float x = mTiles[0][0].transform.position.x + delta.x;
        float y = mTiles[0][0].transform.position.y - delta.y;
        //done
        if (nextTileWidth <= minWidth || x > eAnchor.x || y < eAnchor.y)
        {
            state = State.IDLE;
            nextTileWidth = minWidth;
        }
        else
        {
            float deltaSize = (baseWidth - minWidth) / frames;
            nextTileWidth -= deltaSize;
            //transform
            for (int i = 0; i < mTiles.Length; i++)
            {
                for (int j = 0; j < mTiles[i].Length; j++)
                {
                    mTiles[i][j].transform.position = new Vector3(x + (i * nextTileWidth), y - (j * nextTileWidth), TILE_Z - 2);
                    mTiles[i][j].transform.localScale = new Vector3(nextTileWidth, nextTileWidth, transform.localScale.z);
                    if (mRocks[i][j] != null)
                    {
                        mRocks[i][j].transform.position = new Vector3(x + (i * nextTileWidth), y - (j * nextTileWidth), ROCK_Z - 2);
                        mRocks[i][j].transform.localScale = new Vector3(nextTileWidth, nextTileWidth, transform.localScale.z);
                    }
                }
            }
        }
        return state == State.IDLE;
    }
    public void peek()
    {
        peeking = true;
        state = State.PEEKING;
    }
    public void unpeek()
    {
        peeking = false;
        state = State.UNPEEKING;
        nextTileWidth = baseWidth;
    }
    //reset
    public void resetBoard(bool placeRockAtExit)
    {
        //clear
        clearTiles();
        clearRocks();
        movingRocks.Clear();
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
        movingRocks.Clear();
        bbm = new BoardManager();
        drawTiles();
        drawRocks();
    }
    // methods for pushing rocks
    public bool hasRock(int x, int y)
    {
        return bbm.currentHasRockAt(x, y);

    }
    public void pushRock(int sx, int sy, int dx, int dy)
    {
        if (bbm.currentHasRockAt(sx, sy))
        {
            // move rock to pos (dx,dy) from (sx, sy)
            bbm.currentRemoveAt(sx, sy);
            bbm.currentPlaceRockAt(dx, dy);
            // push rock
            StartCoroutine(rocks[sx][sy].GetComponent<Rock>().move(new Vector3(rocks[sx][sy].transform.position.x + (dx - sx), rocks[sx][sy].transform.position.y - (dy - sy), rocks[sx][sy].transform.position.z), 0.5f));
            //synching animations
            movingRocks.Add(new KeyValuePair<Vector2, Vector2>(new Vector2(sx, sy), new Vector2(dx, dy)));
        }
    }
}
