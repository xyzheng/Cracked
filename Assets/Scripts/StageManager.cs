using UnityEngine;
using System.Collections;

public class StageManager : MonoBehaviour {

	StageManager instance;
	GameObject gameManager;
	GameManager gameManagerScript;
	public int stage;

	void Awake () {
		//loads into the game, if already exists, delete
		DontDestroyOnLoad(this);
		if (instance == null) {
			instance = this;
		}
		else {
			Destroy(gameObject);
		}
		stage = 0;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnLevelWasLoaded (int levelNum) {
		if (Application.loadedLevelName == "Main") {
			gameManager = GameObject.Find("GameManager");
			gameManagerScript = gameManager.GetComponent<GameManager>();
		}
	}
}
