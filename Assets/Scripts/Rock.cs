using UnityEngine;
using System.Collections;

public class Rock : MonoBehaviour {

	public IEnumerator moveAndScaleRock (GameObject aRock, Vector3 destination, float time) {
		//float deltaPosition = 1.0f / 5.0f;
		float moveElapsedTime = 0;
		Vector3 startingPos = aRock.transform.position;
		while (moveElapsedTime < time) {
			aRock.transform.position = Vector3.Lerp (startingPos, destination, (moveElapsedTime / (time/4)));
			moveElapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		float scaleElapsedTime = 0;
		Vector3 startingScale = aRock.transform.localScale;
		Vector3 endingScale = new Vector3 (0.0f, 0.0f, 0.0f);
		while (scaleElapsedTime < time) {
			//scale the rock
			aRock.transform.localScale = Vector3.Lerp (startingScale, endingScale, (scaleElapsedTime / (time/4)));
			scaleElapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		Destroy (aRock);
	}

	public IEnumerator scaleRock (GameObject aRock, float time) {
		float scaleElapsedTime = 0;
		Vector3 startingScale = aRock.transform.localScale;
		Vector3 endingScale = new Vector3 (0f, 0f, 0.0f);
		while (scaleElapsedTime < time) {
			//scale the rock
			aRock.transform.localScale = Vector3.Lerp (startingScale, endingScale, (scaleElapsedTime / (time/4)));
			scaleElapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		Destroy (aRock);
	}

}
