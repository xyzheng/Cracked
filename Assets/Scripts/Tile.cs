using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	// Use this for initialization
	void Start () { }

    public void steppedOnTile() {
        GetComponent<Animator>().SetBool("Step", true); 
    }

    public void stepCrackTile()
    {
        GetComponent<Animator>().SetBool("SCrack", true);
    }

    public void crackTile() {
        GetComponent<Animator>().SetBool("Crack", true); 
    }

    public void breakTile() {
        GetComponent<Animator>().SetBool("Break", true); 
    }
}
