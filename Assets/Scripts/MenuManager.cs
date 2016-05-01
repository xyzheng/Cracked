using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	public GameObject menuPanel;
	public GameObject optionsPanel;
	public GameObject tutorialPanel;
	Fade fadeScript;

	public void Start () {
		fadeScript = GetComponent<Fade>();
	}

	public void playButton () {
		StartCoroutine(fadeScript.fadeOut());
	}

	public void optionsButton () {
		menuPanel.GetComponentsInChildren<Button>()[2].GetComponentInChildren<Text>().fontSize = 20;
		menuPanel.SetActive(false);
		optionsPanel.SetActive(true);
	}

	public void backButton () {
		optionsPanel.GetComponentInChildren<Button>().GetComponentInChildren<Text>().fontSize = 20;
		tutorialPanel.GetComponentsInChildren<Button>()[3].GetComponentInChildren<Text>().fontSize = 20;
		menuPanel.SetActive(true);
		optionsPanel.SetActive(false);
		tutorialPanel.SetActive(false);
	}

	public void tutorialButton () {
		menuPanel.GetComponentsInChildren<Button>()[0].GetComponentInChildren<Text>().fontSize = 20;
		menuPanel.SetActive(false);
		tutorialPanel.SetActive(true);
	}

	public void exitButton () {
		//Application.Quit (); 		//exits game
		EditorApplication.isPlaying = false;	//stops scene in editor
	}

	void Update () {
		if (fadeScript.fadingPanel.alpha == 0) {
			SceneManager.LoadScene ("Main");
		}
	}

}
