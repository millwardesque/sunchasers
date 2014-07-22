using UnityEngine;
using System.Collections;

public class RestartLevel : MonoBehaviour {
	private GameState gameState;
	private NPCManager npcManager;
	private ItemManager itemManager;
	private ScoreKeeper scoreKeeper;
	private PlayerController player;
	private MovementGrid movementGrid;

	// Use this for initialization
	void Start () {
		GameObject movementGridObject = GameObject.FindGameObjectWithTag("Movement Grid");

		gameState = (GameState)GameObject.FindGameObjectWithTag("World").GetComponent("GameState");
		movementGrid = (MovementGrid)movementGridObject.GetComponent("MovementGrid");
		npcManager = (NPCManager)movementGridObject.GetComponent("NPCManager");
		itemManager = (ItemManager)movementGridObject.GetComponent("ItemManager");
		scoreKeeper = (ScoreKeeper)GameObject.FindGameObjectWithTag("ScoreKeeper").GetComponent("ScoreKeeper");
		player = (PlayerController)GameObject.FindGameObjectWithTag("Player").GetComponent("PlayerController");
	}
	
	// Update is called once per frame
	void Update () {
		// @DEBUG Use to restart level until UI button is present
		if (Input.GetKeyUp(KeyCode.R)) {
			Restart();
		}
	}

	public void Restart() {
		Debug.Log("Restarting level.");
	
		gameState.State = GameStateEnum.WaitingToStart;
		itemManager.Reset();
		scoreKeeper.Reset();

		// @TODO Reset NPCs
		//npcManager.Reset ();

		// Reset the player attributes.
		player.SetCurrentSquareAndPosition(player.StartRow, player.StartColumn);
		player.SetSprite("player/front-0");
		player.Relaxation = 0.0f;
		player.Bladder = 0.0f;
		player.Hunger = 0.0f;

		gameState.State = GameStateEnum.Running;

		Debug.Log("Level restarted.");
	}
}
