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

	//stage manager
	GameObject stageManager;
	StageManager stageManagerScript;

	Fade fadeScript;

	public void Start () {
		fadeScript = GetComponent<Fade>();
		hoverSound = GetComponents<AudioSource>()[0];
		backgroundMusic = GetComponents<AudioSource>()[1];
		stageManager = GameObject.Find ("Stage Manager");
		stageManagerScript = stageManager.GetComponent<StageManager>();
		//StartCoroutine(fadeScript.fadeIn());
	}
		
	//different play buttons
	public void endlessButton () {
		stageManagerScript.stage = 0;
		StartCoroutine(fadeScript.fadeOut ());
	}

	public void jumpButton () {
		stageManagerScript.stage = 1;
		StartCoroutine(fadeScript.fadeOut ());
	}

	public void leapButton () {
		stageManagerScript.stage = 2;
		StartCoroutine(fadeScript.fadeOut ());
	}

	public void pushButton () {
		stageManagerScript.stage = 3;
		StartCoroutine(fadeScript.fadeOut ());
	}

    public void crackButton()
    {
        stageManagerScript.stage = 4;
        StartCoroutine(fadeScript.fadeOut());
    }

    public void jumpLeapButton()
    {
        stageManagerScript.stage = 5;
        StartCoroutine(fadeScript.fadeOut());
    }

    public void jumpLeapPushButton()
    {
        stageManagerScript.stage = 6;
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
