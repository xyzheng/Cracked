using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

	public GameObject menuPanel;
	public GameObject optionsPanel;
	Fade fadeScript;

	public void Start () {
		fadeScript = GetComponent<Fade>();
	}

	public void playButton () {
		StartCoroutine(fadeScript.fadeOut());
	}

	public void optionsButton () {
		menuPanel.SetActive(false);
		optionsPanel.SetActive(true);
	}

	public void backButton () {
		menuPanel.SetActive(true);
		optionsPanel.SetActive(false);
	}

	void Update () {
		if (fadeScript.fadingPanel.alpha == 0) {
			SceneManager.LoadScene ("Main");
		}
	}

}
