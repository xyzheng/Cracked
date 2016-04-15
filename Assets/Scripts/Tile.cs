using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

    public void stepTile() { GetComponent<Animator>().SetBool("Step", true); }
    public void stepCrackTile() { GetComponent<Animator>().SetBool("SCrack", true); }
    public void crackTile() { GetComponent<Animator>().SetBool("Crack", true); }
    public void breakTile() { GetComponent<Animator>().SetBool("Break", true);  }

    //skip animation
    public void forceSteppedTile() { GetComponent<Animator>().SetBool("FStep", true); }
    public void forceSteppedCrackedTile() { GetComponent<Animator>().SetBool("FSCrack", true); }
    public void forceCrackedTile() { GetComponent<Animator>().SetBool("FCrack", true); }
    public void forceBrokenTile() { GetComponent<Animator>().SetBool("FBreak", true); }
}
