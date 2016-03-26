using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager
{
    protected int currentBoardIndex;
    protected List<Board> boards;
    protected Board current; //current
    protected Board next; //next Board
    //getter
    public int getCurrentWidth() { return current.getWidth(); }
    public int getCurrentHeight() { return current.getHeight(); }
    public bool didBackTrack() { return currentBoardIndex != boards.Count - 1; }
    public bool backTrackPossible() { return currentBoardIndex - 1 >= 0; }
    public bool forwardTrackPossible() { return currentBoardIndex + 1 < boards.Count; } 
    //setter
    public virtual void reset()
    {
        current = boards[currentBoardIndex].getCopy();
        next.reset();
    }
    public virtual void clear()
    {
        currentBoardIndex = 0;
        boards.Clear();
        current.reset();
        next.reset();
    }
    public virtual void clearedCurrentBoard()
    {
        //swap current and next
        current = next.getCopy();
        //add this as a new board start
        boards.Add(current.getCopy());
        //clear futureboard
        next = new Board();
        //update currentboard index
        currentBoardIndex = boards.Count - 1;
    }
    public virtual void backTrack()
    {
        //check there is a board to backtrack to
        if (currentBoardIndex - 1 >= 0)
        {
            //make the last board the current
            currentBoardIndex -= 1;
            current = boards[currentBoardIndex].getCopy();
            //clear nex
            next.reset();
        }
    }
    public virtual void forwardTrack()
    {
        //check there is a board to forwardTrack to
        if (currentBoardIndex + 1 < boards.Count)
        {
            //make the last board the current
            currentBoardIndex += 1;
            current = boards[currentBoardIndex].getCopy();
            //clear next
            next.reset();
        }

    }
    public virtual void clearForwardBoards()
    {
        for (int i = boards.Count - 1; i > currentBoardIndex; i--)
        {
            boards.RemoveAt(i);
        }
    }

}

public class RockBoardManager : BoardManager {
    //constructor
    public RockBoardManager(){
        current = new Board();
        next = new Board();
        //add to boards
        boards = new List<Board>();
        boards.Add(current.getCopy());
        currentBoardIndex = 0;
	}
    //getters
    public bool currentHasRockAt(int x, int y) { return current.getValueAt(x, y) == 1; }
    public bool nextHasRockAt(int x, int y) { return next.getValueAt(x, y) == 1; }
    public bool currentIsValidAt(int x, int y) { return current.isValid(x, y); }
    public bool nextIsValidAt(int x, int y) { return next.isValid(x, y); }
    //setters
    public bool currentRemoveAt(int x, int y)
    {
        if (current.isValid(x, y))
        {
            current.setValueAtTo(x, y, 0);
            return true;
        }
        return false;
    }
    public bool currentPlaceAt(int x, int y) {
        if (current.isValid(x, y))
        {
            current.setValueAtTo(x, y, 1);
            return true;
        }
        return false;
    }
    public bool nextRemoveAt(int x, int y) {
        if (next.isValid(x, y))
        {
            next.setValueAtTo(x, y, 0);
            return true;
        }
        return false;
    }
    public bool nextPlaceAt(int x, int y) {
        if (next.isValid(x, y))
        {
            next.setValueAtTo(x, y, 1);
            return true;
        }
        return false;
    }
}

public class TileBoardManager : BoardManager
{
    //health values/states of a 'tile' of a board
    protected enum TILE_HEALTH { HEALTHY, DAMAGED, DESTROYED };
    //keep track of start, goal, board
    protected List<Vector2> starts;
    protected List<Vector2> goals;

    // constructor
    public TileBoardManager(int startX = 0, int startY = 4, int goalX = 4, int goalY = 0)
    {
        current = new Board();
        next = new Board();
        starts = new List<Vector2>();
        starts.Add(new Vector2(startX, startY));
        goals = new List<Vector2>();
        goals.Add(new Vector2(goalX, goalY));
        //add to boards
        boards = new List<Board>();
        boards.Add(current.getCopy());
        //backtracked
        currentBoardIndex = 0;
    }
    //getters
    public Vector2 getStart() { return starts[currentBoardIndex]; }
    public Vector2 getGoal() { return goals[currentBoardIndex]; }
    public bool currentIsValidAt(int x, int y) { return current.isValid(x, y); }
    public bool currentIsHealthyAt(int x, int y) { return current.getValueAt(x, y) == (int)TILE_HEALTH.HEALTHY; }
    public bool currentIsDamagedAt(int x, int y) { return current.getValueAt(x, y) == (int)TILE_HEALTH.DAMAGED; }
    public bool currentIsDestroyedAt(int x, int y) { return current.getValueAt(x, y) == (int)TILE_HEALTH.DESTROYED; }
    public bool nextIsValidAt(int x, int y) { return next.isValid(x, y); }
    public bool nextIsHealthyAt(int x, int y) { return next.getValueAt(x, y) == (int)TILE_HEALTH.HEALTHY; }
    public bool nextIsDamagedAt(int x, int y) { return next.getValueAt(x, y) == (int)TILE_HEALTH.DAMAGED; }
    public bool nextIsDestroyedAt(int x, int y) { return next.getValueAt(x, y) == (int)TILE_HEALTH.DESTROYED; }
    //setters
    public bool damageCurrentBoard(int x, int y)
    {
        if (current.isValid(x, y))
        {
            int value = current.getValueAt(x, y);
            if (value + 1 > (int)TILE_HEALTH.DESTROYED) { return false; }
            current.setValueAtTo(x, y, value + 1);
            return true;
        }
        return false;
    }
    public bool unDamageCurrentBoard(int x, int y)
    {
        if (current.isValid(x, y))
        {
            int value = current.getValueAt(x, y);
            if (value - 1 < (int)TILE_HEALTH.HEALTHY) { return false; }
            current.setValueAtTo(x, y, value - 1);
            return true;
        }
        return false;
    }
    public bool damageNextBoard(int x, int y)
    {
        if (next.isValid(x, y))
        {
            int value = next.getValueAt(x, y);
            if (value + 1 > (int)TILE_HEALTH.DESTROYED) { return false; }
            next.setValueAtTo(x, y, value + 1);
            return true;
        }
        return false;
    }
    public bool unDamageNextBoard(int x, int y)
    {
        if (next.isValid(x, y))
        {
            int value = next.getValueAt(x, y);
            if (value - 1 < (int)TILE_HEALTH.HEALTHY) { return false; }
            next.setValueAtTo(x, y, value - 1);
            return true;
        }
        return false;
    }
    public override void clear()
    {
        base.clear();
        starts.Clear();
        goals.Clear();
    }
    public override void clearedCurrentBoard()
    {
        //establish currentboard as the futureboard
        starts.Add(new Vector2(0, 4));
        goals.Add(new Vector2(4, 0));
        base.clearedCurrentBoard();
    }
    public override void clearForwardBoards()
    {
        for (int i = boards.Count - 1; i > currentBoardIndex; i--)
        {
            //boards.RemoveAt(i);
            starts.RemoveAt(i);
            goals.RemoveAt(i);
        }
        base.clearForwardBoards();
    }
}

