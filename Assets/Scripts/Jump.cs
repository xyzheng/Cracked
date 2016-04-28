using UnityEngine;
using System.Collections;

public class Jump : MonoBehaviour {

    private bool fade;
    private bool busy;
    private int color = -1;     // -1 = white       // 0 = red      // 1 = green
    private const int jumpingFrames = 15;
    private enum AnimationState { IDLE, JUMP_UP, JUMP_DOWN }
    AnimationState currentState;
    private const float BASE_SIZE = 1.0f;
    private const float MAX_SIZE = 2.0f;
    float currentSize = 1.0f;

    // Use this for initialization
    void Start()
    {
        fade = false;
        busy = false;
        currentState = AnimationState.IDLE;
    }

    void Update()
    {
        if (busy && jump()) { makeTransparent(); }
    }

    //move
    public void startJump()
    {
        busy = true;
        currentState = AnimationState.JUMP_UP;
    }
    private bool jump()
    {
        float deltaSize = (MAX_SIZE - BASE_SIZE) / jumpingFrames;
        if (currentState == AnimationState.JUMP_UP)
        {
            currentSize += deltaSize;
            if (currentSize >= MAX_SIZE)
            {
                currentState = AnimationState.JUMP_DOWN;
                currentSize = MAX_SIZE;
            }
        }
        else if (currentState == AnimationState.JUMP_DOWN)
        {
            currentSize -= deltaSize * 3;
            if (currentSize < BASE_SIZE)
            {
                busy = false;
                currentState = AnimationState.IDLE;
                currentSize = BASE_SIZE;
            }
        }
        transform.localScale = new Vector3(currentSize, currentSize, transform.localScale.z);
        return !busy;
    }

    //fade
    public void makeTransparent()
    {
        if (!fade)
        {
            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
            fade = true;
        }
    }
    public void makeFullColor()
    {
        if (fade)
        {
            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            fade = false;
        }
    }
    public void makeRed()
    {
        GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 1f);
        color = 0;
    }
    public void makeGreen()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.0f, 162f / 255f, 232f / 255f, 1f);
        color = 1;
    }
    public void unColor()
    {
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        color = -1;
    }
    public int getColor() { return color; }
    public bool isFaded() { return fade; }
    //position
    public void setPosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }
}

