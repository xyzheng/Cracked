using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwapBoards
{
    protected int currentBoardIndex;
    protected List<Board> boards;
    protected Board current; //current
    protected Board next; //next Board
    //getter
    public int getCurrentWidth() { return current.getWidth(); }
    public int getCurrentHeight() { return current.getHeight(); }
    public int getNextWidth() { return next.getWidth(); }
    public int getNextHeight() { return next.getHeight(); }
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

public class BoardManager : SwapBoards
{
    //health values/states of a 'tile' of a board
    protected enum STATE { TILE, CRACK, HOLE, ROCK, ROCK_N_CRACK };
    //keep track of start, goal, board
    protected List<Vector2> starts;
    protected List<Vector2> goals;

    //constructor
    public BoardManager(int startX = 0, int startY = 4, int goalX = 4, int goalY = 0)
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
        currentBoardIndex = 0;
	}
    //getters
    public Vector2 getStart() { return starts[currentBoardIndex]; }
    public Vector2 getGoal() { return goals[currentBoardIndex]; }
    public bool currentIsValidAt(int x, int y) { return current.isValid(x, y); }
    public bool currentIsHealthyAt(int x, int y) { return current.getValueAt(x, y) == (int)STATE.TILE || current.getValueAt(x, y) == (int)STATE.ROCK; }
    public bool currentIsDamagedAt(int x, int y) { return current.getValueAt(x, y) == (int)STATE.CRACK || current.getValueAt(x, y) == (int)STATE.ROCK_N_CRACK; }
    public bool currentIsDestroyedAt(int x, int y) { return current.getValueAt(x, y) == (int)STATE.HOLE; }
    public bool nextIsValidAt(int x, int y) { return next.isValid(x, y); }
    public bool nextIsHealthyAt(int x, int y) { return next.getValueAt(x, y) == (int)STATE.TILE || next.getValueAt(x, y) == (int)STATE.ROCK; }
    public bool nextIsDamagedAt(int x, int y) { return next.getValueAt(x, y) == (int)STATE.CRACK || next.getValueAt(x, y) == (int)STATE.ROCK_N_CRACK; }
    public bool nextIsDestroyedAt(int x, int y) { return next.getValueAt(x, y) == (int)STATE.HOLE; }
    public bool currentHasRockAt(int x, int y) { return current.getValueAt(x, y) == (int)STATE.ROCK || current.getValueAt(x, y) == (int)STATE.ROCK_N_CRACK; }
    public bool nextHasRockAt(int x, int y) { return next.getValueAt(x, y) == (int)STATE.ROCK || next.getValueAt(x, y) == (int)STATE.ROCK_N_CRACK; }
    //setters
    public bool damageCurrentBoard(int x, int y)
    {
        int value = current.getValueAt(x, y);
        if (current.isValid(x, y) && value != (int)STATE.HOLE)
        {
            if (value == (int)STATE.TILE) { current.setValueAtTo(x, y, (int)STATE.CRACK); }
            else if (value == (int)STATE.CRACK) { current.setValueAtTo(x, y, (int)STATE.HOLE); }
            else if (value == (int)STATE.ROCK) { current.setValueAtTo(x, y, (int)STATE.ROCK_N_CRACK); }
            else if (value == (int)STATE.ROCK_N_CRACK) { current.setValueAtTo(x, y, (int)STATE.HOLE); }
            return true;
        }
        return false;
    }
    public bool unDamageCurrentBoard(int x, int y)
    {
        int value = current.getValueAt(x, y);
        if (current.isValid(x, y) && value != (int)STATE.ROCK && value != (int)STATE.TILE)
        {
            if (value == (int)STATE.CRACK) { current.setValueAtTo(x, y, (int)STATE.TILE); }
            else if (value == (int)STATE.HOLE) { current.setValueAtTo(x, y, (int)STATE.CRACK); }
            else if (value == (int)STATE.ROCK_N_CRACK) { current.setValueAtTo(x, y, (int)STATE.ROCK); }
            return true;
        }
        return false;
    }
    public bool damageNextBoard(int x, int y)
    {
        int value = next.getValueAt(x, y);
        if (next.isValid(x, y) && value != (int)STATE.HOLE)
        {
            if (value == (int)STATE.TILE) { next.setValueAtTo(x, y, (int)STATE.CRACK); }
            else if (value == (int)STATE.CRACK) { next.setValueAtTo(x, y, (int)STATE.HOLE); }
            else if (value == (int)STATE.ROCK) { next.setValueAtTo(x, y, (int)STATE.ROCK_N_CRACK); }
            else if (value == (int)STATE.ROCK_N_CRACK) { next.setValueAtTo(x, y, (int)STATE.HOLE); }
            return true;
        }
        return false;
    }
    public bool unDamageNextBoard(int x, int y)
    {
        int value = next.getValueAt(x, y);
        if (next.isValid(x, y) && value != (int)STATE.ROCK && value != (int)STATE.TILE)
        {
            if (value == (int)STATE.CRACK) { next.setValueAtTo(x, y, (int)STATE.TILE); }
            else if (value == (int)STATE.HOLE) { next.setValueAtTo(x, y, (int)STATE.CRACK); }
            else if (value == (int)STATE.ROCK_N_CRACK) { next.setValueAtTo(x, y, (int)STATE.ROCK); }
            return true;
        }
        return false;
    }
    public bool currentRemoveAt(int x, int y)
    {
        if (current.isValid(x, y))
        {
            if (current.getValueAt(x, y) == (int)STATE.ROCK) { current.setValueAtTo(x, y, (int)STATE.TILE); }
            else if (current.getValueAt(x, y) == (int)STATE.ROCK_N_CRACK) { current.setValueAtTo(x, y, (int)STATE.CRACK); }
            return true;
        }
        return false;
    }
    public bool currentPlaceRockAt(int x, int y)
    {
        if (current.isValid(x, y) && current.getValueAt(x, y) != (int)STATE.HOLE)
        {
            if (current.getValueAt(x, y) == (int)STATE.TILE) { current.setValueAtTo(x, y, (int)STATE.ROCK); }
            else if (current.getValueAt(x, y) == (int)STATE.CRACK) { current.setValueAtTo(x, y, (int)STATE.ROCK_N_CRACK); }
            return true;
        }
        return false;
    }
    public bool nextRemoveRockAt(int x, int y)
    {
        if (next.isValid(x, y))
        {
            if (next.getValueAt(x, y) == (int)STATE.ROCK) { next.setValueAtTo(x, y, (int)STATE.TILE); }
            else if (next.getValueAt(x, y) == (int)STATE.ROCK_N_CRACK) { next.setValueAtTo(x, y, (int)STATE.CRACK); }
            return true;
        }
        return false;
    }
    public bool nextPlaceRockAt(int x, int y)
    {
        if (next.isValid(x, y) && next.getValueAt(x,y) != (int)STATE.HOLE)
        {
            if (next.getValueAt(x, y) == (int)STATE.TILE) { next.setValueAtTo(x, y, (int)STATE.ROCK); }
            else if (next.getValueAt(x, y) == (int)STATE.CRACK) { next.setValueAtTo(x, y, (int)STATE.ROCK_N_CRACK); }
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
    public void clearedCurrentBoard(int startX = 0, int startY = 4, int goalX = 4, int goalY = 0)
    {
        //establish currentboard as the futureboard
        starts.Add(new Vector2(startX, startY));
        goals.Add(new Vector2(goalX, goalY));
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

