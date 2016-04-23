using UnityEngine;
using System.Collections;

public class Push : MonoBehaviour {

    private bool fade;
    private bool busy;
    public bool red;
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
        red = true;
    }
    public void unRed()
    {
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        red = false;
    }
    public bool isRed() { return red; }
    public bool isFaded() { return fade; }
    //setter
    public void setPosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }

}
