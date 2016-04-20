using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour {

	private static SaveLoadManager instance;

	//save information
	public BoardInfo currentBoardsInfo;
	public GameObject gameManager;
	public GameManager gameInfo;

	public bool fromLoad;

	void Awake () {
		//loads into the game, if already exists, delete
		DontDestroyOnLoad(this);
		if (instance == null) {
			instance = this;
		}
		else {
			Destroy(gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		fromLoad = false;
		currentBoardsInfo = new BoardInfo();
		currentBoardsInfo.listOfRocksX = new List<int>();
		currentBoardsInfo.listOfRocksY = new List<int>();
		currentBoardsInfo.listOfmRocksX = new List<int>();
		currentBoardsInfo.listOfmRocksY = new List<int>();
		currentBoardsInfo.listOfTilesX = new List<int>();
		currentBoardsInfo.listOfTilesY = new List<int>();
		currentBoardsInfo.listOfmTilesX = new List<int>();
		currentBoardsInfo.listOfmTilesY = new List<int>();
		currentBoardsInfo.listOfHolesX = new List<int>();
		currentBoardsInfo.listOfHolesY = new List<int>();
		currentBoardsInfo.listOfStepOnsX = new List<int>();
		currentBoardsInfo.listOfStepOnsY = new List<int>();
		currentBoardsInfo.boards = new List<Board>();
	}

	public void save () {
		//gameSavedText.GetComponent<Text>().text = "Game saved";
		BinaryFormatter fb = new BinaryFormatter();
		FileStream myFile = File.Create ("save.csf");
		//save player info
		currentBoardsInfo.playerPositionX = gameInfo.player.transform.position.x;
		currentBoardsInfo.playerPositionY = gameInfo.player.transform.position.y;
		//save level info
		currentBoardsInfo.level = GameManager.level;

		GameManager copy = GameObject.Find ("GameManager").GetComponent<GameManager>();
		//currentBoardsInfo.bbmCopy = copy.gbm.bbm;

		currentBoardsInfo.listOfRocksX.Clear();
		currentBoardsInfo.listOfRocksY.Clear();
		currentBoardsInfo.listOfmRocksX.Clear();
		currentBoardsInfo.listOfmRocksY.Clear();
		currentBoardsInfo.listOfTilesX.Clear();
		currentBoardsInfo.listOfTilesY.Clear();
		currentBoardsInfo.listOfmTilesX.Clear();
		currentBoardsInfo.listOfmTilesY.Clear();
		currentBoardsInfo.listOfHolesX.Clear();
		currentBoardsInfo.listOfHolesY.Clear();
		currentBoardsInfo.listOfStepOnsX.Clear();
		currentBoardsInfo.listOfStepOnsY.Clear();
		currentBoardsInfo.boards.Clear();

		for (int i=0; i<copy.gbm.rocks.Length; i++) {
			for (int j=0; j<copy.gbm.rocks[i].Length; j++) {
				//Debug.Log (copy.gbm.rocks[i][j].gameObject.transform.position);
				if (copy.gbm.rocks[i][j] != null) {
					currentBoardsInfo.listOfRocksX.Add (i);
					currentBoardsInfo.listOfRocksY.Add (j);
				}
			}
		}
		/*
		for (int i=0; i<copy.gbm.mRocks.Length; i++) {
			for (int j=0; j<copy.gbm.mRocks[i].Length; j++) {
				//Debug.Log (copy.gbm.rocks[i][j].gameObject.transform.position);
				if (copy.gbm.mRocks[i][j] != null) {
					currentBoardsInfo.listOfmRocksX.Add (i);
					currentBoardsInfo.listOfmRocksY.Add (j);
				}
			}
		}
*/
		for (int i=0; i<copy.gbm.tiles.Length; i++) {
			for (int j=0; j<copy.gbm.tiles[i].Length; j++) {
				//Debug.Log (copy.gbm.rocks[i][j].gameObject.transform.position);
				if (copy.gbm.bbm.currentIsDamagedAt(i, j)) {
					currentBoardsInfo.listOfTilesX.Add (i);
					currentBoardsInfo.listOfTilesY.Add (j);
				}
			}
		}

		for (int i=0; i<copy.gbm.tiles.Length; i++) {
			for (int j=0; j<copy.gbm.tiles[i].Length; j++) {
				if (copy.gbm.bbm.currentIsDestroyedAt(i, j)) {
					currentBoardsInfo.listOfHolesX.Add (i);
					currentBoardsInfo.listOfHolesY.Add (j);
					//Debug.Log (i.ToString() + " " + j.ToString());
				}
			}
		}

		for (int i=0; i<copy.gbm.tiles.Length; i++) {
			for (int j=0; j<copy.gbm.tiles[i].Length; j++) {
				if (copy.gbm.bbm.currentIsSteppedAt(i, j)) {
					currentBoardsInfo.listOfStepOnsX.Add (i);
					currentBoardsInfo.listOfStepOnsY.Add (j);
					Debug.Log (i + " " + j);
				}
			}
		}

		/*
		for (int i=0; i<copy.gbm.mTiles.Length; i++) {
			for (int j=0; j<copy.gbm.mTiles[i].Length; j++) {
				//Debug.Log (copy.gbm.rocks[i][j].gameObject.transform.position);
				if (copy.gbm.mTiles[i][j] != null) {
					currentBoardsInfo.listOfmTilesX.Add (i);
					currentBoardsInfo.listOfmTilesY.Add (j);
				}
			}
		}
*/	
		for (int i=0; i<copy.gbm.bbm.boards.Count; i++) {
			currentBoardsInfo.boards.Add (copy.gbm.bbm.boards[i]);
		}
		//Debug.Log (currentBoardsInfo.boards.Count);

		fb.Serialize(myFile, currentBoardsInfo);
		myFile.Close();
	}

	public void load () {
		if (File.Exists("save.csf")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream myFile = File.Open ("save.csf", FileMode.Open);
			BoardInfo info = (BoardInfo)bf.Deserialize(myFile);
			myFile.Close();
			GameManager.state = GameManager.GameState.LOADSAVE;
			currentBoardsInfo.level = info.level;
			currentBoardsInfo.playerPositionX = info.playerPositionX;
			currentBoardsInfo.playerPositionY = info.playerPositionY;
			fromLoad = true;
			SceneManager.LoadScene("Main");
		}
	}

	void OnLevelWasLoaded (int levelNum) {
		if (Application.loadedLevelName == "Main") {
			gameManager = GameObject.Find("GameManager");
			gameInfo = gameManager.GetComponent<GameManager>();
			/*
			GameObject ac = GameObject.Find("Canvas");
			Component[] comps = ac.GetComponentsInChildren(typeof(Component), true);
			foreach (Component c in comps) {
				if (c.name == "Game Saved") {
					//Debug.Log(c.name);
					gameSavedText = c;
				}
			}
			*/
		}
		else if (Application.loadedLevelName == "MainMenu") {
			//Debug.Log (GameObject.Find("Load Button").ToString());
			Button loadButton = GameObject.Find("Load Button").GetComponent<Button>();
			loadButton.onClick.AddListener(load);
		}
	}
}

[System.Serializable]
public class BoardInfo {

	//player info
	public float playerPositionX;
	public float playerPositionY;

	//level info
	public int level;

	//objects/scripts
	public GameBoardManager gbmCopy;
	public BackTrack btScriptCopy;
	public ForwardTrack ftScriptCopy;
	public BoardManager bbmCopy;

	//rocks and tiles
	public List<int> listOfRocksX;
	public List<int> listOfRocksY;
	public List<int> listOfmRocksX;
	public List<int> listOfmRocksY;
	public List<int> listOfTilesX;
	public List<int> listOfTilesY;
	public List<int> listOfmTilesX;
	public List<int> listOfmTilesY;
	public List<int> listOfHolesX;
	public List<int> listOfHolesY;
	public List<int> listOfStepOnsX;
	public List<int> listOfStepOnsY;

	public List<Board> boards;

}