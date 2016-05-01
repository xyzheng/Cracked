using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	//panels
	public GameObject menuPanel;
	public GameObject optionsPanel;
	public GameObject tutorialPanel;

	//music and sounds
	private AudioSource backgroundMusic;
	private AudioSource hoverSound;
	//sliders and toggles
	public Slider soundSlider;
	public Slider musicSlider;
	public Toggle soundToggle;
	public Toggle musicToggle;

	Fade fadeScript;

	public void Start () {
		fadeScript = GetComponent<Fade>();
		hoverSound = GetComponents<AudioSource>()[0];
		backgroundMusic = GetComponents<AudioSource>()[1];
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


	//sound slider
	public void updateSoundSlider () {
		if (soundToggle.isOn) {
			hoverSound.volume = soundSlider.value;
		}
	}

	//sound toggle
	public void toggleSound () {
		if (!soundToggle.isOn) {
			hoverSound.volume = 0f;
		}
		else {
			hoverSound.volume = soundSlider.value;
		}
	}

	//music slider
	public void updateMusicSlider () {
		if (musicToggle.isOn) {
			backgroundMusic.volume = musicSlider.value;
		}
	}
	public void toggleMusic () {
		if (!musicToggle.isOn) {
			backgroundMusic.volume = 0f;
		}
		else {
			backgroundMusic.volume = musicSlider.value;
		}
	}


	void Update () {
		if (fadeScript.faded) {
			SceneManager.LoadScene ("Main");
		}
	}

}
