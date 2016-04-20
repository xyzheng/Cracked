using UnityEngine;
using System.Collections;

[System.Serializable]
public class BackTrack : MonoBehaviour {

    private bool fade;
	// Use this for initialization
	void Start () {
        fade = false;
	}

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
}
