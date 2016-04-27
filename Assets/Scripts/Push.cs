using UnityEngine;
using System.Collections;

public class Push : MonoBehaviour {

    private bool fade;
    private bool busy;
    public int color = -1;   // -1 = white       // 0 = red      // 1 = green
    private Vector3 orig;
    private const float deltaPos = 0.2f;

    // Use this for initialization
    void Start()
    {
        fade = false;
        busy = false;
        orig = transform.position;
    }

    void FixedUpdate()
    {
        if (busy) { shake(); }
    }

    //move
    public void startShake() { busy = true; }
    public void stopShake()
    {
        transform.position = orig;
        busy = false;
        makeTransparent();
    }

    private void shake() {
        transform.position = new Vector3(orig.x + Random.Range(-deltaPos, deltaPos), orig.y, orig.z); 
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
        GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, 1f);
        color = 1;
    }
    public void unColor()
    {
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        color = -1;
    }
    public int getColor() { return color; }
    public bool isFaded() { return fade; }
    //setter
    public void setPosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }

}
