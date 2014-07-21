using UnityEngine;
using System.Collections;

public class RestartLevel : MonoBehaviour {
	private GameState gameState;
	private NPCManager npcManager;
	private ItemManager itemManager;

	// Use this for initialization
	void Start () {
		gameState = (GameState)GameObject.FindGameObjectWithTag("World").GetComponent("GameState");	
		npcManager = (NPCManager)GameObject.FindGameObjectWithTag("Movement Grid").GetComponent("NPCManager");
		itemManager = (ItemManager)GameObject.FindGameObjectWithTag("Movement Grid").GetComponent("ItemManager");
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
		// @TODO Reset timer
		// @TODO Reset sun position
		// @TODO Reset ambient light
		// @TODO Reset player position
		// @TODO Reset items
		// @TODO Reset score
		// @TODO Reset score breakdown items

		gameState.State = GameStateEnum.Running;
		npcManager.Reset(); // @TODO NPCs don't move when reset!

		Debug.Log("Level restarted.");
	}
}
