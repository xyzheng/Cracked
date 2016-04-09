using UnityEngine;
using System.Collections;

public class Rock : MonoBehaviour {

    private bool shake;
    private Vector3 orig;
    private float deltaPos;

    void Start(){
        shake = false;
        deltaPos = 0.1f;
        orig = transform.position;
    }

    public void FixedUpdate()
    {
        if (shake)
        {
            sh();
        }
    }

	public IEnumerator moveAndScaleRock (Vector3 destination, float time) {
        shake = false;
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
			scaleElapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		Destroy (this);
	}

	public IEnumerator scaleRock (float time) {
        shake = false;
		float scaleElapsedTime = 0;
<<<<<<< HEAD
		Vector3 startingScale = aRock.transform.localScale;
=======
		Vector3 startingScale = transform.localScale;
>>>>>>> michael
		Vector3 endingScale = new Vector3 (0.0f, 0.0f, 0.0f);
		while (scaleElapsedTime < time) {
			//scale the rock
			transform.localScale = Vector3.Lerp (startingScale, endingScale, (scaleElapsedTime / (time/4)));
			scaleElapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		Destroy (this);
	}

    public void shakeRock(){
        shake = true; 
    } 
    private void sh()
    {
        transform.position = new Vector3(orig.x + Random.Range(-deltaPos, deltaPos), orig.y + Random.Range(-deltaPos, deltaPos), orig.z);
    }
}
