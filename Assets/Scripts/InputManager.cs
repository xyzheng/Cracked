using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Simple InputManager
public class InputManager {
    //prior key pressed
    //public List<KeyCode> priorKeys;
    //keys for movement of player
    protected KeyCode moveUp;
    protected KeyCode moveLeft;
    protected KeyCode moveRight;
    protected KeyCode moveDown;
    protected KeyCode jump;
    //keys for game mechanics
    protected KeyCode pause; //pause game
    protected KeyCode peek; //peek at next floor
    //protected KeyCode undoMove; //reset a move done on the floor 
    protected KeyCode resetBoard; //reset current floor
    protected KeyCode backTrack; //go back a floor
    protected KeyCode forwardTrack;
    protected KeyCode resetGame;

	// Use this for initialization
	public InputManager () {
        //movement
        moveUp = KeyCode.W;
        moveDown = KeyCode.S;
        moveLeft = KeyCode.A;
        moveRight = KeyCode.D;
        jump = KeyCode.Space;
        //game mechanics
        pause = KeyCode.Escape;
        peek = KeyCode.P;
        //undoMove = KeyCode.Q;
        resetBoard = KeyCode.E;
        backTrack = KeyCode.Q;
        forwardTrack = KeyCode.F;
        resetGame = KeyCode.R;
        //priorkeys
        //priorKeys = new List<KeyCode>();
	}

    //getters
    public KeyCode getMoveUpKey() { return moveUp; }
    public KeyCode getMoveDownKey() { return moveDown; }
    public KeyCode getMoveLeftKey() { return moveLeft; }
    public KeyCode getMoveRightKey() { return moveRight; }
    public KeyCode getJumpKey() { return jump; }
    public KeyCode getPauseKey() { return pause; }
    public KeyCode getPeekKey() { return peek; }
    //public KeyCode getUndoMoveKey() { return undoMove; }
    public KeyCode getResetBoardKey() { return resetBoard; }
    public KeyCode getBacktrackKey() { return backTrack; }
    public KeyCode getForwardTrackKey() { return forwardTrack; }
    public KeyCode getResetGameKey() { return resetGame; }

    //setters
    public void rebindMoveUpKey(KeyCode newKey) { moveUp = newKey; }
    public void rebindMoveDownKey(KeyCode newKey) { moveDown = newKey; }
    public void rebindMoveLeftKey(KeyCode newKey) { moveLeft = newKey; }
    public void rebindMoveRightKey(KeyCode newKey) { moveRight = newKey; }
    public void rebindJumpKey(KeyCode newKey) { jump = newKey; }
    public void rebindPauseKey(KeyCode newKey) { pause = newKey; }
    public void rebindPeekKey(KeyCode newKey) { peek = newKey; }
    //public void rebindUndoMoveKey(KeyCode newKey) { undoMove = newKey; }
    public void rebindResetBoardKey(KeyCode newKey) { resetBoard = newKey; }
    public void rebindBacktrackKey(KeyCode newKey) { backTrack = newKey; }
    public void rebindForwardTrackKey(KeyCode newKey) { forwardTrack = newKey; }
    public void rebindResetGameKey(KeyCode newKey) { resetGame = newKey; }

    //modifying priorkeys
    //public void addToPriorKeys(KeyCode key) { priorKeys.Add(key); }
    //public void clearPriorKeys() { priorKeys.Clear(); }
    //public void removeLastKeyPressed()
    //{
    //    if (getLastKeyPressed() != KeyCode.Escape)
    //    {
    //        priorKeys.RemoveAt(priorKeys.Count - 1);
    //    }
    //}
    //public KeyCode getLastKeyPressed() { 
    //    if (priorKeys.Count != 0) {
    //        return priorKeys[priorKeys.Count - 1];
    //    }
    //    //list empty, return escape
    //    return KeyCode.Escape; 
    //}
}
