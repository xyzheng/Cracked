using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour {

	public CanvasGroup fadingPanel;
	private float fadeSpeed = 1.0f;
	public bool isFading;

	// Use this for initialization
	void Start () {
		isFading = false;
		if (SceneManager.GetActiveScene().name == "MainMenu") {
			fadingPanel = GameObject.Find("Canvas").GetComponentInChildren<CanvasGroup>();
		}
		else if (SceneManager.GetActiveScene().name == "Main") {
			fadingPanel = GameObject.Find("FadeCanvas").GetComponentInChildren<CanvasGroup>();
		}
	}

	void Update () {
//		Debug.Log (fadingPanel);
	}
		
	public IEnumerator fadeOut () {
		isFading = true;
		float fadeTime = 1.0f;
		while (fadingPanel.alpha > 0) {
			fadingPanel.alpha -= Time.deltaTime / fadeTime;
			yield return null;
		}
	}

	public IEnumerator gameFadeToBlack () {
		isFading = true;
		float fadeTime = 1.0f;
		while (fadingPanel.alpha < 1) {
			fadingPanel.alpha += Time.deltaTime / fadeTime;
			yield return null;
		}
	}
}
