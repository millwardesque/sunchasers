using UnityEngine;
using System.Collections;

public class RestartLevel : MonoBehaviour {
	private GameState gameState;
	private NPCManager npcManager;
	private ItemManager itemManager;
	private ScoreKeeper scoreKeeper;
	private PlayerController player;

	void Awake () {
		MessageManager.Instance.RegisterListener(new Listener("ReadyCountdownFinishedMessage", gameObject, "OnReadyCountdownFinishedMessage"));
	}

	// Use this for initialization
	void Start () {
		GameObject movementGridObject = GameObject.FindGameObjectWithTag("Movement Grid");

		gameState = (GameState)GameObject.FindGameObjectWithTag("World").GetComponent("GameState");
		npcManager = (NPCManager)movementGridObject.GetComponent("NPCManager");
		itemManager = (ItemManager)movementGridObject.GetComponent("ItemManager");
		scoreKeeper = (ScoreKeeper)GameObject.FindGameObjectWithTag("ScoreKeeper").GetComponent("ScoreKeeper");
		player = (PlayerController)GameObject.FindGameObjectWithTag("Player").GetComponent("PlayerController");
	}
	
	// Update is called once per frame
	void Update () {
		// @DEBUG Use to restart level until UI button is present
		if (Input.GetKeyUp(KeyCode.R)) {
			gameState.State = GameStateEnum.Paused;
			GameObject.FindGameObjectWithTag("World").GetComponent<ReadyCountdown>().StartCountdown();

		}
	}

	public void Restart() {
		Debug.Log("Restarting level.");
	
		gameState.State = GameStateEnum.WaitingToStart;
		scoreKeeper.Reset();
		itemManager.Reset();
		npcManager.Reset();
		player.Reset();

		gameState.State = GameStateEnum.Running;

		Debug.Log("Level restarted.");
	}

	public void OnReadyCountdownFinishedMessage(Message message) {
		Restart ();
	}
}
