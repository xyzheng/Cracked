using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

	public GameObject menuPanel;
	public GameObject optionsPanel;

	public void playButton () {
		SceneManager.LoadScene ("Main");
	}

	public void optionsButton () {
		menuPanel.SetActive(false);
		optionsPanel.SetActive(true);
	}

	public void backButton () {
		menuPanel.SetActive(true);
		optionsPanel.SetActive(false);
	}

}
