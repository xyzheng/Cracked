using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	Text buttonText;
	AudioSource hoverSound;
	bool isPlaying;
	AudioSource[] clips;

	// Use this for initialization
	void Start () {
		buttonText = GetComponentInChildren<Text>();
		if (SceneManager.GetActiveScene().name == "MainMenu") {
			hoverSound = GameObject.Find("Menu Manager").GetComponent<AudioSource>();
		}
		else if (SceneManager.GetActiveScene().name == "Main"){
			clips = GameObject.Find("GameManager").GetComponents<AudioSource>();
			hoverSound = clips[4];
		}
		isPlaying = false;
	}
	
	public void OnPointerEnter (PointerEventData e) {
		buttonText.fontSize = 25;
		hoverSound.Play();
		isPlaying = true;
	}

	public void OnPointerExit (PointerEventData e) {
		buttonText.fontSize = 20;	
		if (isPlaying) {
			hoverSound.Stop();
		}
	}
}
