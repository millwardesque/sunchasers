using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour {
	public GameObject[] NPCTypes;	// Types of NPCs to generate.
	public int MaxNPCs = 0;			// Max items that can be onscreen at once. Zero for infinite.

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
		for (int i = 0; i < MaxNPCs; ++i) {
			GenerateNPC();
		}
	}

	// Use this for initialization
	void Start () {
		movementGrid = GameObject.FindGameObjectWithTag("Movement Grid").GetComponent<MovementGrid>();

		for (int i = 0; i < MaxNPCs; ++i) {
			GenerateNPC();
		}
	}

	/// <summary>
	/// Creates an NPC.
	/// </summary>
	void GenerateNPC() {
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
			Debug.Log("Couldn't find a valid square for the new NPC. Bailing.");
			return;
		}
		GridSquare newGridSquare = potentialSquares[Random.Range(0, potentialSquares.Count)];
				
		// Create a new NPC.
		int npcIndex = Random.Range(0, NPCTypes.Length);
		GameObject newNPC = (GameObject)GameObject.Instantiate(NPCTypes[npcIndex]);
		
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
