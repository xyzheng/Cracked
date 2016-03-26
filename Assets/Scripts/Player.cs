using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	protected Vector2 position;
	protected bool jumped;
	//protected bool landed;
	protected bool busy;
	
	//animation stuff
	public float currentSize = 0.8f;
	private enum AnimationState { IDLE, HOP_UP, HOP_DOWN, HOP_EAST, HOP_WEST, HOP_NORTH, HOP_SOUTH, JUMP_UP, JUMP_DOWN }
	AnimationState currentState;
	protected int hoppingInPlaceFrames;
	protected int hoppingtodirectionframes;
	protected int jumpingFrames;
	
	// Use this for initialization
	void Start () {
		position = new Vector2(); //start at 0,0 in origin; in terms of board, y is flipped
		transform.position = new Vector3(transform.position.x, transform.position.y, -2f);
		jumped = false;
		//landed = false;
		busy = false;
		hoppingInPlaceFrames = 5;
		hoppingtodirectionframes= 8;
		jumpingFrames = 20;
		currentState = AnimationState.IDLE;
	}
	
	// Update is called once per frame
	void Update () {
		if (busy)
		{
			if (currentState == AnimationState.HOP_UP || currentState == AnimationState.HOP_DOWN)
			{
				doHopInPlace();
			}
			else if (currentState == AnimationState.HOP_SOUTH)
			{
				doHopToSouth();
			}
			else if (currentState == AnimationState.HOP_WEST)
			{
				doHopToWest();
			}
			else if (currentState == AnimationState.HOP_EAST)
			{
				doHopToEast();
			}
			else if (currentState == AnimationState.HOP_NORTH)
			{
				doHopToNorth();
			}
			else if (currentState == AnimationState.JUMP_UP || currentState == AnimationState.JUMP_DOWN)
			{
				doJump();
			}
		}
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
			currentState = AnimationState.HOP_NORTH;
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
			currentState = AnimationState.HOP_SOUTH;
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
			currentState = AnimationState.HOP_WEST;
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
			currentState = AnimationState.HOP_EAST;
		}
	}
	public void hopInPlace() {
		if (!busy)
		{
            //landed = false;
			busy = true;
			currentState = AnimationState.HOP_UP;
		}
	}
	public void jump()
	{
		if (!busy)
		{
            //landed = false;
			jumped = true;
			busy = true;
			currentState = AnimationState.JUMP_UP;
		}
	}
	//getters
	public Vector2 getPosition()
	{
		return position;
	}
	public bool didJump() { return jumped; }
	//public bool didLand() { return landed; }
	public bool isBusy() { return busy; }
	//setters
	public void setPosition(Vector2 newPosition)
	{
		//stop animation
		busy = false;
        //landed = true;
		currentState = AnimationState.IDLE;
		//update position
		position = newPosition;
		transform.position = new Vector3(position.x, position.y, transform.position.z);
		currentSize = 0.8f;
		transform.localScale = new Vector3(0.8f, 0.8f, transform.localScale.z);
	}
	public void notJump()
	{
		jumped = false;
		//landed = false;
		busy = false;
		currentState = AnimationState.IDLE;
	}
	public void reset(int startX, int startY)
	{
		busy = false;
		currentState = AnimationState.IDLE;
		
		jumped = false;
		//landed = false;
		position = new Vector2(startX, startY);
        currentState = AnimationState.IDLE;
		//update position
		transform.position = new Vector3(position.x, position.y, transform.position.z);
		//reset animations
		currentSize = 0.8f;
		transform.localScale = new Vector3(0.8f, 0.8f, transform.localScale.z);
	}
	
	//animation stuff
	public void doHopInPlace()
	{
		float deltaSize = (1 - 0.8f) / (hoppingInPlaceFrames);
		if (currentState == AnimationState.HOP_UP)
		{
			//going up
			currentSize += deltaSize;
			if (currentSize >= 1.0f)
			{
				currentState = AnimationState.HOP_DOWN;
				currentSize = 1.0f;
			}
		}
		else if (currentState == AnimationState.HOP_DOWN){
			currentSize -= deltaSize;
			if (currentSize < 0.8f)
			{
				busy = false;
				currentState = AnimationState.IDLE;
                //landed = true;
				currentSize = 0.8f;
			}
		}
		transform.localScale = new Vector3(currentSize, currentSize, transform.localScale.z);
	}
	
	public void doHopToEast()
	{
		float deltaPosition = 1f / (hoppingtodirectionframes);
		float deltaSize = (2 - 0.8f) / (hoppingtodirectionframes);
		int startX = (int)position.x - 1;
		//update position
		transform.position = new Vector3(transform.position.x + deltaPosition, transform.position.y, transform.position.z);
		if (transform.position.x - startX < 0.5f)
		{
			//going up
			currentSize += deltaSize;
		}
		else if (transform.position.x - startX >= 1)
		{
			//check at/passed destination
			busy = false;
			currentState = AnimationState.IDLE;
            //landed = true;
			currentSize = 0.8f;
			transform.position = new Vector3(position.x, transform.position.y, transform.position.z);
		}
		else
		{
			//coming down
			currentSize -= deltaSize;
		}
		//apply new size
		transform.localScale = new Vector3(currentSize, currentSize, transform.localScale.z);
	}
	public void doHopToWest()
	{
		float deltaPosition = 1f / (hoppingtodirectionframes);
		float deltaSize = (2 - 0.8f) / (hoppingtodirectionframes);
		int startX = (int)position.x + 1;
		//update position
		transform.position = new Vector3(transform.position.x - deltaPosition, transform.position.y, transform.position.z);
		//do scaling
		if (startX - transform.position.x < 0.5f)
		{
			//going up
			currentSize += deltaSize;
		}
		else if (startX - transform.position.x >= 1)
		{
			//check at/passed destination
			busy = false;
			currentState = AnimationState.IDLE;
            //landed = true;
			currentSize = 0.8f;
			transform.position = new Vector3(position.x, transform.position.y, transform.position.z);
		}
		else
		{
			//coming down
			currentSize -= deltaSize;
		}
		//apply new size
		transform.localScale = new Vector3(currentSize, currentSize, transform.localScale.z);
	}
	public void doHopToNorth()
	{
		float deltaPosition = 1f / (hoppingtodirectionframes);
		float deltaSize = (2 - 0.8f) / (hoppingtodirectionframes);
		int startY = (int)position.y - 1;
		//update position
		transform.position = new Vector3(transform.position.x, transform.position.y + deltaPosition, transform.position.z);
		//do scaling
		if (transform.position.y - startY < 0.5f)
		{
			//going up
			currentSize += deltaSize;
		}
		else if (transform.position.y - startY >= 1)
		{
			//check at/passed destination
			busy = false;
			currentState = AnimationState.IDLE;
            //landed = true;
			currentSize = 0.8f;
			transform.position = new Vector3(transform.position.x, position.y, transform.position.z);
		}
		else
		{
			//coming down
			currentSize -= deltaSize;
		}
		
		//apply new size
		transform.localScale = new Vector3(currentSize, currentSize, transform.localScale.z);
	}
	public void doHopToSouth()
	{
		float deltaPosition = 1f / (hoppingtodirectionframes);
		float deltaSize = (2 - 0.8f) / (hoppingtodirectionframes);
		int startY = (int)position.y + 1;
		//update position
		transform.position = new Vector3(transform.position.x, transform.position.y - deltaPosition, transform.position.z);
		//scaling
		if (startY - transform.position.y < 0.5f)
		{
			//going up
			currentSize += deltaSize;
		}
		else if (startY - transform.position.y >= 1)
		{
			//check at/passed destination
			busy = false;
			currentState = AnimationState.IDLE;
            //landed = true;
			currentSize = 0.8f;
			transform.position = new Vector3(transform.position.x, position.y, transform.position.z);
		}
		else
		{
			//coming down
			currentSize -= deltaSize;
		}
		//apply new size
		transform.localScale = new Vector3(currentSize, currentSize, transform.localScale.z);
	}
	
	public void doJump()
	{
		float deltaSize = (2.5f - 0.8f) / jumpingFrames;
		if (currentState == AnimationState.JUMP_UP)
		{
			//going up
			currentSize += deltaSize;
			if (currentSize >= 2.5f)
			{
				currentState = AnimationState.JUMP_DOWN;
				currentSize = 2.5f;
			}
		}
		else if (currentState == AnimationState.JUMP_DOWN)
		{
			currentSize -= deltaSize * 3;
			if (currentSize < 0.8f)
			{
				busy = false;
				//landed = true;
				currentState = AnimationState.IDLE;
				currentSize = 0.8f;
			}
		}
		transform.localScale = new Vector3(currentSize, currentSize, transform.localScale.z);
	}
}
