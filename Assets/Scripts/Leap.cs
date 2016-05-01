﻿using UnityEngine;
using System.Collections;

public class Leap : MonoBehaviour {

    private bool fade;
    private bool busy;
    private bool toggled;
    private int color = -1;      // -1 = white       // 0 = red      // 1 = green
    private Vector3 orig;
    private Vector3 togglePos;
    private const float DELTA_X = 0.35f;
    private const int FRAMES = 20;
    private enum AnimationState { IDLE, JUMP_UP, JUMP_DOWN }
    private AnimationState currentState;
    private const float BASE_SIZE = 1.0f;
    private const float MAX_SIZE = 2.0f;
    private float currentSize = 1.0f;

    // Use this for initialization
    void Start()
    {
        fade = false;
        busy = false;
        toggled = false;
        orig = transform.position;
        togglePos = new Vector3(orig.x - DELTA_X, orig.y, orig.z);
        currentState = AnimationState.IDLE;
    }

    void Update()
    {
        if (busy && moveToOrig()) { makeTransparent(); }
    }

    //move
    public void toggle()
    {
        if (busy)
        {
            Debug.Log("Leap icon busy in toggle()");
            return;
        }
        if (toggled)
        {
            toggled = false;
            transform.position = orig;
        }
        else
        {
            toggled = true;
            transform.position = togglePos;
        }
    }
    public void untoggle()
    {
        toggled = false;
        transform.position = orig;
    }
    public void startJump()
    {
        if (toggled) { busy = true; }
    }
    private bool moveToOrig()
    {
        float deltaPosition = 1f / FRAMES;
        float deltaSize = (MAX_SIZE - BASE_SIZE) / FRAMES;
        //update position
        transform.position = new Vector3(transform.position.x + deltaPosition, transform.position.y, transform.position.z);
        //do scaling
        if (Mathf.Abs((int)orig.x) - Mathf.Abs(transform.position.x) < (togglePos.x - orig.x) / 2)
        {
            //going up
            currentSize += deltaSize;
        }
        else if (transform.position.x >= orig.x)
        {
            //check at/passed destination
            busy = false;
            currentState = AnimationState.IDLE;
            //landed = true;
            currentSize = BASE_SIZE;
            transform.position = new Vector3(orig.x, transform.position.y, transform.position.z);
        }
        else
        {
            //coming down
            currentSize -= deltaSize;
        }
        //apply new size
        transform.localScale = new Vector3(currentSize, currentSize, transform.localScale.z);
        return !busy;
    }

    //fade
    public void makeTransparent()
    {
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        fade = true;
        Debug.Log("A");
    }
    public void makeFullColor()
    {
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        fade = false;
        Debug.Log("B");
    }
    public void makeRed()
    {
        GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 1f);
        color = 0;
    }
    public void makeGreen()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.0f, 162f/255f, 232f/255f, 1f);
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
