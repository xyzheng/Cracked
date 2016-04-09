using UnityEngine;
using System.Collections;

public class Eye : MonoBehaviour {

    public void blink()
    {
        GetComponent<Animator>().SetBool("Blink", true);
    }

    public void unblink()
    {
        GetComponent<Animator>().SetBool("Blink", false); 
    }

    public void setPosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }
}
