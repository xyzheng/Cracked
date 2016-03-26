using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour {

    float deltaTime = 0.0f;
    public float framesPSec;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	    deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        framesPSec = 1 / deltaTime;
	}
}
