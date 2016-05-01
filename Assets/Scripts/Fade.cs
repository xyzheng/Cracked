using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour {

	public CanvasGroup fadingPanel;
	private float fadeSpeed = 1.0f;
	public bool isFading;
	public bool faded;

	void Start () {
		isFading = false;
		faded = false;
	}

	//fade to black effect
	public IEnumerator fadeOut () {
		isFading = true;
		float fadeTime = 1.0f;
		while (fadingPanel.alpha < 1) {
			fadingPanel.alpha += Time.deltaTime / fadeTime;
			yield return null;
		}
		faded = true;
		isFading = false;
	}

	//fade from black effect
	public IEnumerator fadeIn () {
		isFading = true;
		float fadeTime = 1.0f;
		while (fadingPanel.alpha > 0) {
			fadingPanel.alpha -= Time.deltaTime / fadeTime;
			yield return null;
		}
		faded = true;
		isFading = false;
	}
}
