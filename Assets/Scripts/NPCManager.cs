using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour {
	public GameObject[] StartingNPCs;	// NPCs to place in the scene.

	private float chanceToUnseatPlayer = 0.1f;
	private MovementGrid movementGrid;

	/// <summary>
	/// Resets the state of the game.
	/// </summary>
	public void Reset() {
		// Remove existing NPCs.
		GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
		for (int i = 0; i < npcs.Length; ++i) {
			GameObject.Destroy(npcs[i]);
		}

		// Clear the grid so the system doesn't think deleted NPCs are occupying them.
		for (int row = 0; row < movementGrid.NumRows; row++) {
			for (int column = 0; column < movementGrid.NumColumns; column++) {
				GridSquare gridSquare = movementGrid.SquarePositions[row][column];
				gridSquare.Occupier = null;
			}
		}
		
		// Re-seed the NPCs in starting locations
		GenerateNPCs();
	}

	// Use this for initialization
	void Start () {
		movementGrid = GameObject.FindGameObjectWithTag("Movement Grid").GetComponent<MovementGrid>();
	}

	void Update() {
		chanceToUnseatPlayer += Time.deltaTime * 0.001f;

		// Force an aggro NPC to target the player.
		if (Input.GetKeyDown("t")) {
			GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");

			// Make a list of aggro NPCs.
			List<NPCAggressive> aggroNPCs = new List<NPCAggressive>();
			foreach (GameObject npc in npcs) {
				if (npc.GetComponent<NPCAggressive>()) {
					aggroNPCs.Add(npc.GetComponent<NPCAggressive>());
				}
			}
			chanceToUnseatPlayer = 0.0f;
			// Pick an aggro NPC and target the player.
			if (aggroNPCs.Count > 0) {
				int npcIndex = Random.Range (0, aggroNPCs.Count - 1);
				aggroNPCs[npcIndex].TargetPlayerSquare();
			}
		}
	}

	/// <summary>
	/// Creates the NPCs for the level
	/// </summary>
	void GenerateNPCs() {
		foreach (GameObject npc in StartingNPCs) {
			// Pick a new row / column for the item to be created at.
			List<GridSquare> potentialSquares = new List<GridSquare>();
			for (int row = 0; row < movementGrid.NumRows; row++) {
				for (int column = 0; column < movementGrid.NumColumns; column++) {
					GridSquare gridSquare = movementGrid.SquarePositions[row][column];
					if (gridSquare.IsTraversable && gridSquare.IsOccupied() == false && (gridSquare.Component == null || gridSquare.Component.tag == "Chair")) {
						potentialSquares.Add (gridSquare);
					}
				}
			}
			if (potentialSquares.Count == 0) {
				Debug.Log("Couldn't find a valid square for new NPCs. Bailing.");
				return;
			}
			GridSquare newGridSquare = potentialSquares[Random.Range(0, potentialSquares.Count)];
					
			// Create a new NPC.
			GameObject newNPC = (GameObject)GameObject.Instantiate(npc);
			
			// Figure out what kind of NPC this is.
			if (newNPC.GetComponent<NPCAggressive>()) {
				newNPC.GetComponent<NPCAggressive>().StartRow = newGridSquare.Row;
				newNPC.GetComponent<NPCAggressive>().StartColumn = newGridSquare.Column;
			}
			else if (newNPC.GetComponent<NPCBasic>()) {
				newNPC.GetComponent<NPCBasic>().StartRow = newGridSquare.Row;
				newNPC.GetComponent<NPCBasic>().StartColumn = newGridSquare.Column;	
			}
		}
	}
}
