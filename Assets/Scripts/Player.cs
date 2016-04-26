using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	private Vector2 position;
	private bool jumped;
    //protected bool landed;
    private bool leaped;
    private bool busy;
	
	//animation stuff
    private const int FADE_FRAMES = 10;
    private const float MIN_ALPHA = 0.6f;
    private Color baseColor;
    private Color color;
	public float size;
    private enum AnimationState { IDLE, HOP_UP, HOP_DOWN, HOP_EAST, HOP_WEST, HOP_NORTH, HOP_SOUTH, JUMP_UP, JUMP_DOWN, FADE_IN, FADE_OUT,
                                  SM_MOVE_EAST, SM_MOVE_WEST, SM_MOVE_NORTH, SM_MOVE_SOUTH }
    private bool smallMove = false;        // Used for moving slightly in a certain direction to indicate that it's an invalid move
        // Used instead of hopping in place. If the player presses UP, and it's invalid, you move up a little, then back down
        // -1 = Not small-moving     0 = UP       1 = RIGHT       2 = DOWN        3 = LEFT            (Clockwise starting from UP)
    public bool duringMove = false;        // Used for outside functions to see if we are currently small-moving
	private AnimationState state;
	private const int HOP_IN_PLACE_FRAMES = 5;
	private const int HOP_TO_FRAMES = 8;
	private const int JUMP_FRAMES = 20;
	
	// Use this for initialization
	void Start () {
		position = new Vector2(); //start at 0,0 in origin; in terms of board, y is flipped
		transform.position = new Vector3(transform.position.x, transform.position.y, -2f);
		jumped = false;
		busy = false;
        leaped = false;
        state = AnimationState.IDLE;
        baseColor = GetComponent<SpriteRenderer>().color;
        color = baseColor;
	}
	
	// Update is called once per frame
	void Update () {
		if (busy)
		{
            if (state == AnimationState.HOP_UP || state == AnimationState.HOP_DOWN)
            {
                doHopInPlace();
            }
            else if (state == AnimationState.HOP_SOUTH)
            {
                doHopToSouth();
            }
            else if (state == AnimationState.HOP_WEST)
            {
                doHopToWest();
            }
            else if (state == AnimationState.HOP_EAST)
            {
                doHopToEast();
            }
            else if (state == AnimationState.HOP_NORTH)
            {
                doHopToNorth();
            }
            else if (state == AnimationState.JUMP_UP || state == AnimationState.JUMP_DOWN)
            {
                doJump();
            }
            else if (state == AnimationState.FADE_IN)
            {
                doUnfade();
            }
            else if (state == AnimationState.FADE_OUT)
            {
                doFade();
            }
            else if (state == AnimationState.SM_MOVE_EAST) doSmMoveEast();
            else if (state == AnimationState.SM_MOVE_WEST) doSmMoveWest();
            else if (state == AnimationState.SM_MOVE_NORTH) doSmMoveNorth();
            else if (state == AnimationState.SM_MOVE_SOUTH) doSmMoveSouth();
        }
        //if (smallMoveDirection != -1) checkSmallMove();
        checkMove();
    }
	//movement
	public void moveUp()
	{
		if (!busy)
		{
            //landed = false;
			busy = true;
			position = new Vector2(position.x, position.y + 1);
			//update position
			//gameObject.GetComponent<Transform>().position = new Vector3(position.x, position.y, gameObject.GetComponent<Transform>().position.z);
			state = AnimationState.HOP_NORTH;
		}
	}
    public void moveUp2()
    {
        if (!busy)
        {
            //landed = false;
            busy = true;
            position = new Vector2(position.x, position.y + 2);
            //update position
            //gameObject.GetComponent<Transform>().position = new Vector3(position.x, position.y, gameObject.GetComponent<Transform>().position.z);
            state = AnimationState.HOP_NORTH;
        }
    }
    public void moveUpSmall()
    {
        if (!busy)
        {
            busy = true;
            //position = new Vector2(position.x, position.y + 0.4f);
            state = AnimationState.SM_MOVE_NORTH;
            //smallMoveDirection = 0;
            smallMove = true;
        }
    }
    public void moveDown()
    {
        if (!busy)
        {
            //landed = false;
            busy = true;
            position = new Vector2(position.x, position.y - 1);
            //update position
            //gameObject.GetComponent<Transform>().position = new Vector3(position.x, position.y, gameObject.GetComponent<Transform>().position.z);
            state = AnimationState.HOP_SOUTH;
        }
    }
    public void moveDown2()
    {
        if (!busy)
        {
            //landed = false;
            busy = true;
            position = new Vector2(position.x, position.y - 2);
            //update position
            //gameObject.GetComponent<Transform>().position = new Vector3(position.x, position.y, gameObject.GetComponent<Transform>().position.z);
            state = AnimationState.HOP_SOUTH;
        }
    }
    public void moveDownSmall()
    {
        if (!busy)
        {
            busy = true;
            //position = new Vector2(position.x, position.y - 0.4f);
            state = AnimationState.SM_MOVE_SOUTH;
            //smallMoveDirection = 2;
            smallMove = true;
        }
    }
    public void moveLeft()
    {
        if (!busy)
        {
            //landed = false;
            busy = true;
            position = new Vector2(position.x - 1, position.y);
            //update position
            //gameObject.GetComponent<Transform>().position = new Vector3(position.x, position.y, gameObject.GetComponent<Transform>().position.z);
            state = AnimationState.HOP_WEST;
        }
    }
    public void moveLeft2()
    {
        if (!busy)
        {
            //landed = false;
            busy = true;
            position = new Vector2(position.x - 2, position.y);
            //update position
            //gameObject.GetComponent<Transform>().position = new Vector3(position.x, position.y, gameObject.GetComponent<Transform>().position.z);
            state = AnimationState.HOP_WEST;
        }
    }
    public void moveLeftSmall()
    {
        if (!busy)
        {
            busy = true;
            //position = new Vector2(position.x - 0.4f, position.y);
            state = AnimationState.SM_MOVE_WEST;
            //smallMoveDirection = 3;
            smallMove = true;
        }
    }
    public void moveRight()
    {
        if (!busy)
        {
            //landed = false;
            busy = true;
            position = new Vector2(position.x + 1, position.y);
            //update position
            //gameObject.GetComponent<Transform>().position = new Vector3(position.x, position.y, gameObject.GetComponent<Transform>().position.z);
            state = AnimationState.HOP_EAST;
        }
    }
    public void moveRight2()
    {
        if (!busy)
        {
            //landed = false;
            busy = true;
            position = new Vector2(position.x + 2, position.y);
            //update position
            //gameObject.GetComponent<Transform>().position = new Vector3(position.x, position.y, gameObject.GetComponent<Transform>().position.z);
            state = AnimationState.HOP_EAST;
        }
    }
    public void moveRightSmall()
    {
        if (!busy)
        {
            busy = true;
            //position = new Vector2(position.x + 0.4f, position.y);
            state = AnimationState.SM_MOVE_EAST;
            //smallMoveDirection = 1;
            smallMove = true;
        }
    }
    public void hopInPlace() {
		if (!busy)
		{
            //landed = false;
			busy = true;
			state = AnimationState.HOP_UP;
		}
	}
	public void jump()
	{
		if (!busy)
		{
            //landed = false;
			jumped = true;
			busy = true;
			state = AnimationState.JUMP_UP;
            // play jump audio
            GameManager.aSrc[3].PlayOneShot(GameManager.jump, 1.0f);
		}
	}
    public void leap()
    {
        if (!busy)
        {
            leaped = true;
            busy = true;
            state = AnimationState.JUMP_UP;
            GameManager.aSrc[3].PlayOneShot(GameManager.jump, 1.0f);
        }
    }

    //getters
    public Vector2 getPosition()
	{
		return position;
	}
	public bool didJump() { return jumped; }
    public bool didLeap() { return leaped; }
    public bool isIdle() { return state == AnimationState.IDLE; }
    //public bool didLand() { return landed; }
    public bool isBusy() { return busy; }
	//setters
	public void setPosition(Vector2 newPosition)
	{
		//stop animation
		busy = false;
        //landed = true;
		state = AnimationState.IDLE;
		//update position
		position = newPosition;
		transform.position = new Vector3(position.x, position.y, transform.position.z);
		size = 0.8f;
		transform.localScale = new Vector3(0.8f, 0.8f, transform.localScale.z);
	}
	public void notJump()
	{
		jumped = false;
		//landed = false;
		busy = false;
		state = AnimationState.IDLE;
	}
    public void notLeap()
    {
        leaped = false;
        busy = false;
        state = AnimationState.IDLE;
    }
    public void reset(int startX, int startY)
	{
		busy = false;
		state = AnimationState.IDLE;
		
		jumped = false;
        leaped = false;
        //landed = false;
        position = new Vector2(startX, startY);
        state = AnimationState.IDLE;
		//update position
		transform.position = new Vector3(position.x, position.y, transform.position.z);
		//reset animations
		size = 0.8f;
		transform.localScale = new Vector3(0.8f, 0.8f, transform.localScale.z);
	}

    public void fadePlayer()
    {
        if (!busy)
        {
            busy = true;
            state = AnimationState.FADE_OUT;
        }
    }
    private void doFade()
    {
        //calc color
        color = new Color(baseColor.r, baseColor.g, baseColor.b, color.a - (1f - MIN_ALPHA) / FADE_FRAMES);
        //done
        if (color.a <= MIN_ALPHA)
        {
            busy = false;
            state = AnimationState.IDLE;
            color = new Vector4(baseColor.r, baseColor.g, baseColor.b, MIN_ALPHA);
        }
        GetComponent<SpriteRenderer>().color = color;
    }
    public void forceUnfade()
    {
        color = baseColor;
        GetComponent<SpriteRenderer>().color = color;
    }
    public void unfadePlayer()
    {
        busy = true;
        state = AnimationState.FADE_IN;
    }
    private void doUnfade()
    {
        //calc color
        color = new Color(baseColor.r, baseColor.g, baseColor.b, color.a + (1f - MIN_ALPHA) / FADE_FRAMES);
        //done
        if (color.a >= 1f)
        {
            busy = false;
            state = AnimationState.IDLE;
            color = baseColor;
        }
        GetComponent<SpriteRenderer>().color = color;
    }

	//animation stuff
	private void doHopInPlace()
	{
		float deltaSize = (1 - 0.8f) / (HOP_IN_PLACE_FRAMES);
		if (state == AnimationState.HOP_UP)
		{
			//going up
			size += deltaSize;
			if (size >= 1.0f)
			{
				state = AnimationState.HOP_DOWN;
				size = 1.0f;
			}
		}
		else if (state == AnimationState.HOP_DOWN){
			size -= deltaSize;
			if (size < 0.8f)
			{
				busy = false;
				state = AnimationState.IDLE;
                //landed = true;
				size = 0.8f;
			}
		}
		transform.localScale = new Vector3(size, size, transform.localScale.z);
	}
	
	private void doHopToEast()
	{
		float deltaPosition = 1f / (HOP_TO_FRAMES);
		float deltaSize = (2 - 0.8f) / (HOP_TO_FRAMES);
		int startX = (int)position.x - 1;
		//update position
		transform.position = new Vector3(transform.position.x + deltaPosition, transform.position.y, transform.position.z);
		if (transform.position.x - startX < 0.5f)
		{
			//going up
			size += deltaSize;
		}
		else if (transform.position.x - startX >= 1)
		{
			//check at/passed destination
			busy = false;
			state = AnimationState.IDLE;
            //landed = true;
			size = 0.8f;
			transform.position = new Vector3(position.x, transform.position.y, transform.position.z);
		}
		else
		{
			//coming down
			size -= deltaSize;
		}
		//apply new size
		transform.localScale = new Vector3(size, size, transform.localScale.z);
	}
	private void doHopToWest()
	{
		float deltaPosition = 1f / (HOP_TO_FRAMES);
		float deltaSize = (2 - 0.8f) / (HOP_TO_FRAMES);
		int startX = (int)position.x + 1;
		//update position
		transform.position = new Vector3(transform.position.x - deltaPosition, transform.position.y, transform.position.z);
		//do scaling
		if (startX - transform.position.x < 0.5f)
		{
			//going up
			size += deltaSize;
		}
		else if (startX - transform.position.x >= 1)
		{
			//check at/passed destination
			busy = false;
			state = AnimationState.IDLE;
            //landed = true;
			size = 0.8f;
			transform.position = new Vector3(position.x, transform.position.y, transform.position.z);
		}
		else
		{
			//coming down
			size -= deltaSize;
		}
		//apply new size
		transform.localScale = new Vector3(size, size, transform.localScale.z);
	}
	private void doHopToNorth()
	{
		float deltaPosition = 1f / (HOP_TO_FRAMES);
		float deltaSize = (2 - 0.8f) / (HOP_TO_FRAMES);
		int startY = (int)position.y - 1;
		//update position
		transform.position = new Vector3(transform.position.x, transform.position.y + deltaPosition, transform.position.z);
		//do scaling
		if (transform.position.y - startY < 0.5f)
		{
			//going up
			size += deltaSize;
		}
		else if (transform.position.y - startY >= 1)
		{
			//check at/passed destination
			busy = false;
			state = AnimationState.IDLE;
            //landed = true;
			size = 0.8f;
			transform.position = new Vector3(transform.position.x, position.y, transform.position.z);
		}
		else
		{
			//coming down
			size -= deltaSize;
		}
		
		//apply new size
		transform.localScale = new Vector3(size, size, transform.localScale.z);
	}
	private void doHopToSouth()
	{
		float deltaPosition = 1f / (HOP_TO_FRAMES);
		float deltaSize = (2 - 0.8f) / (HOP_TO_FRAMES);
		int startY = (int)position.y + 1;
		//update position
		transform.position = new Vector3(transform.position.x, transform.position.y - deltaPosition, transform.position.z);
		//scaling
		if (startY - transform.position.y < 0.5f)
		{
			//going up
			size += deltaSize;
		}
		else if (startY - transform.position.y >= 1)
		{
			//check at/passed destination
			busy = false;
			state = AnimationState.IDLE;
            //landed = true;
			size = 0.8f;
			transform.position = new Vector3(transform.position.x, position.y, transform.position.z);
		}
		else
		{
			//coming down
			size -= deltaSize;
		}
		//apply new size
		transform.localScale = new Vector3(size, size, transform.localScale.z);
	}

    private void doSmMoveEast()
    {
        float deltaPosition = 1f / (HOP_TO_FRAMES);
        int start = (int)Mathf.Round(position.x);
        float end = (float)(start + 0.4f);
        if (smallMove)
        {
            transform.position = new Vector3(transform.position.x + deltaPosition, transform.position.y, transform.position.z);
            if (transform.position.x >= end) { smallMove = false; }
        }
        else {
            transform.position = new Vector3(transform.position.x - deltaPosition, transform.position.y, transform.position.z);
            if (transform.position.x <= start)
            {
                busy = false;
                state = AnimationState.IDLE;
            }
        }
    }
    private void doSmMoveWest()
    {
        float deltaPosition = 1f / (HOP_TO_FRAMES);
        float start = Mathf.Round(position.x);
        float end = (float)(start - 0.4f);
        if (smallMove)
        {
            transform.position = new Vector3(transform.position.x - deltaPosition, transform.position.y, transform.position.z);
            if (transform.position.x <= end) { smallMove = false; }
        }
        else
        {
            transform.position = new Vector3(transform.position.x + deltaPosition, transform.position.y, transform.position.z);
            if (transform.position.x >= start)
            {
                busy = false;
                state = AnimationState.IDLE;
            }
        }
    }
    private void doSmMoveNorth()
    {
        float deltaPosition = 1f / (HOP_TO_FRAMES);
        float start = Mathf.Round(position.y);
        float end = (float)(start + 0.4f);
        if (smallMove)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + deltaPosition, transform.position.z);
            if (transform.position.y >= end) { smallMove = false; }
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - deltaPosition, transform.position.z);
            if (transform.position.y <= start)
            {
                busy = false;
                state = AnimationState.IDLE;
            }
        }
    }
    private void doSmMoveSouth()
    {
        float deltaPosition = 1f / (HOP_TO_FRAMES);
        float start = Mathf.Round(position.y);
        float end = (float)(start - 0.4f);
        if (smallMove)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - deltaPosition, transform.position.z);
            if (transform.position.y <= end) { smallMove = false; }
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + deltaPosition, transform.position.z);
            if (transform.position.y >= start)
            {
                busy = false;
                state = AnimationState.IDLE;
            }
        }
    }

    private void doJump()
	{
		float deltaSize = (2.5f - 0.8f) / JUMP_FRAMES;
		if (state == AnimationState.JUMP_UP)
		{
			//going up
			size += deltaSize;
			if (size >= 2.5f)
			{
				state = AnimationState.JUMP_DOWN;
				size = 2.5f;
			}
		}
		else if (state == AnimationState.JUMP_DOWN)
		{
			size -= deltaSize * 3;
			if (size < 0.8f)
			{
				busy = false;
				//landed = true;
				state = AnimationState.IDLE;
				size = 0.8f;
			}
		}
		transform.localScale = new Vector3(size, size, transform.localScale.z);
	}

    //private void checkSmallMove()       // Used to see if we are either moving forward or backwards while small-moving
    //{
    //    busy = true;
    //    if (smallMoveDirection == 0)            // small-moved UP
    //    {
    //        position = new Vector2(position.x, position.y - 0.4f);
    //        state = AnimationState.HOP_SOUTH;
    //    }
    //    else if (smallMoveDirection == 1)       // small-moved RIGHT
    //    {
    //        position = new Vector2(position.x - 0.4f, position.y);
    //        state = AnimationState.HOP_WEST;
    //    }
    //    else if (smallMoveDirection == 2)       // small-moved DOWN
    //    {
    //        position = new Vector2(position.x, position.y + 0.4f);
    //        state = AnimationState.HOP_NORTH;
    //    }
    //    else                                    // small-moved LEFT
    //    {
    //        position = new Vector2(position.x + 0.4f, position.y);
    //        state = AnimationState.HOP_EAST;
    //    }
    //    smallMoveDirection = -1;
    //}
    private void checkMove()      // Used to see if we are currently moving
    {
        if (position.x != Mathf.Floor(position.x))
        {
            duringMove = true;
            return;
        }
        if (position.y != Mathf.Floor(position.y))
        {
            duringMove = true;
            return;
        }
        duringMove = false;
    }
}
