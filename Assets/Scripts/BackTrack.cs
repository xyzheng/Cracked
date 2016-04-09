using UnityEngine;
using System.Collections;

public class BackTrack : MonoBehaviour {

    protected bool click;
    protected bool fade;
	// Use this for initialization
	void Start () {
        click = false;
        fade = false;
	}

    public bool clicked() { return click; }

    public void makeTransparent()
    {
        if (!fade) {
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

    public void setPosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }

    public void unclick()
    {
        click = false;
    }

    public void reset()
    {
        click = false;
        fade = false;
    }

    //mouseclick on this object
    void onMouseUp()
    {
        click = true;
    }

}
