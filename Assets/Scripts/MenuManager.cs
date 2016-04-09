using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour {

	public GameObject menuPanel;
	public GameObject optionsPanel;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void playButton () {
		Application.LoadLevel("Main");
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
