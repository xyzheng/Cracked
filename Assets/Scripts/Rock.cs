using UnityEngine;
using System.Collections;

public class Rock : MonoBehaviour {
    private bool busy;
    private bool shaking;
    private Vector3 orig;
    private const float deltaPos = 0.1f;
    //init
    void Start(){
        shaking = false;
        orig = transform.position;
    }
    //update
    public void FixedUpdate() { if (shaking) { shake(); } }

    //getter
    public bool isBusy() { return busy; }
    //movement
    public IEnumerator move (Vector3 destination, float time)
    {
        busy = true;
        shaking = false;
        float moveElapsedTime = 0;
        int slowDown = 3;
        Vector3 startingPos = transform.position;
        while (moveElapsedTime < time)
        {
            if (this == null) { break; }
            transform.position = Vector3.Lerp(startingPos, destination, (moveElapsedTime / (time / slowDown)));
            moveElapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        orig = destination;
        busy = false;
    }
	public IEnumerator moveAndScale (Vector3 destination, float time) {
        busy = true;
        shaking = false;
		//float deltaPosition = 1.0f / 5.0f;
		float moveElapsedTime = 0;
		Vector3 startingPos = transform.position;
		while (moveElapsedTime < time) {
			transform.position = Vector3.Lerp (startingPos, destination, (moveElapsedTime / (time/4)));
			moveElapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		float scaleElapsedTime = 0;
		Vector3 startingScale = transform.localScale;
		Vector3 endingScale = new Vector3 (0.0f, 0.0f, 0.0f);
		while (scaleElapsedTime < time) {
			//scale the rock
			transform.localScale = Vector3.Lerp (startingScale, endingScale, (scaleElapsedTime / (time/4)));
            if (transform.localScale.x <= 0.2f || transform.localScale.y <= 0.2f || transform.localScale.z <= 0.2f) busy = false;
			scaleElapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		Destroy (gameObject);
	}
	public IEnumerator scale (float time) {
        busy = true;
        shaking = false;
		float scaleElapsedTime = 0;
		Vector3 startingScale = transform.localScale;
		Vector3 endingScale = new Vector3 (0.0f, 0.0f, 0.0f);
		while (scaleElapsedTime < time) {
			//scale the rock
			transform.localScale = Vector3.Lerp (startingScale, endingScale, (scaleElapsedTime / (time/4)));
            if (transform.localScale.x <= 0.5f || transform.localScale.y <= 0.5f || transform.localScale.z <= 0.5f) { busy = false; }
			scaleElapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
        Destroy(gameObject);
	}
    //shake
    public void startShake(){
        shaking = true;  
    }
    public void stopShake()
    {
        shaking = false;
        transform.position = orig;
    }
    private void shake() {
        if (!busy) { 
            //not moving around
            transform.position = new Vector3(orig.x + Random.Range(-deltaPos, deltaPos) / 5, orig.y + Random.Range(-deltaPos, deltaPos) / 7.5f, orig.z); 
        }
    }
}
