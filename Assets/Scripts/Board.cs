using UnityEngine;
using System.Collections;

public class Board {

    protected int[][] board;
    protected int w;
    protected int h;
    //constructor
    public Board(int width = 5, int height = 5){
        w = width;
        h = height;
        //initialize board
        board = new int[w][];
        for (int i = 0; i < w; i++) { board[i] = new int[h]; }
        //fill board with clean tiles
        reset();
    }
    //getter
    public int getWidth() { return w; }
    public int getHeight() { return h; }
    public bool isValid(int x, int y)
    {
        return x >= 0 && x < w && y >= 0 && y < h;
    }
    public int getValueAt(int x, int y)
    {
        if (isValid(x, y)) { return board[x][y]; }
        else return -1;
    }
    public Board getCopy()
    {
        Board copy = new Board(w, h);
        for (int i = 0; i < getWidth(); i++)
        {
            for (int j = 0; j < getHeight(); j++)
            {
                copy.setValueAtTo(i, j, getValueAt(i, j));
            }
        }
        return copy;
    }
    //setter
    public bool setValueAtTo(int x, int y, int value)
    {
        if (isValid(x, y)) { 
            board[x][y] = value;
            return true;
        }
        else return false; ;
    }
    public void reset()
    {
        for (int i = 0; i < board.Length; i++)
        {
            for (int j = 0; j < board[i].Length; j++)
            {
                board[i][j] = 0;
            }
        }
    }
}
