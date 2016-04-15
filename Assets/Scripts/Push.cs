using UnityEngine;
using System.Collections;

public class Push : MonoBehaviour {

    private bool fade;
    private bool busy;
    private Vector3 orig;
    private const float deltaPos = 0.4f;

    // Use this for initialization
    void Start()
    {
        fade = false;
        busy = false;
        orig = transform.position;
    }

    void Update()
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

    //setter
    public void setPosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }

}
