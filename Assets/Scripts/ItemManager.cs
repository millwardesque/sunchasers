﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour {
	public GameObject[] ItemTypes;	// Types of items to generate.
	public int MaxItems = 0;			// Max items that can be onscreen at once. Zero for infinite.
	public float RegenerationRate = 30;	// Item regeneration rate (in seconds).

	private GameState gameState;
	private MovementGrid movementGrid;
	private float timeToRegenerate = 0;
	
	/// <summary>
	/// Start hook.
	/// </summary>
	void Start () {
		gameState = GameObject.FindGameObjectWithTag("World").GetComponent<GameState>();
		movementGrid = GameObject.FindGameObjectWithTag("Movement Grid").GetComponent<MovementGrid>();
	}
	
	/// <summary>
	/// Update hook.
	/// </summary>
	void Update () {
		if (ItemTypes.Length == 0 || gameState.State != GameStateEnum.Running) {
			return;
		}

		if (MaxItems == 0 || CountActiveItems() < MaxItems) {
			timeToRegenerate -= Time.deltaTime;
		}
		
		// Timer is up, build a new item if allowed.
		if (timeToRegenerate <= 0.0f) {
			timeToRegenerate = RegenerationRate;
			
			// Pick a new row / column for the item to be created at.
			List<GridSquare> potentialSquares = new List<GridSquare>();
			for (int row = 0; row < movementGrid.NumRows; row++) {
				for (int column = 0; column < movementGrid.NumColumns; column++) {
					GridSquare gridSquare = movementGrid.SquarePositions[row][column];
					if (gridSquare.IsTraversable && gridSquare.Consumable == null && gridSquare.Component == null) {
						potentialSquares.Add (gridSquare);	
					}
				}
			}
			if (potentialSquares.Count == 0) {
				Debug.Log("Couldn't find a valid square for the new item. Bailing.");
				return;
			}
			GridSquare newGridSquare = potentialSquares[Random.Range(0, potentialSquares.Count)];
			
			// Position the item on-screen.
			Vector3 newPosition = new Vector3(newGridSquare.Column * movementGrid.GridSquareWidth, newGridSquare.Row * movementGrid.GridSquareHeight, 0.0f);
			
			// Create a new item.
			int itemIndex = Random.Range(0, ItemTypes.Length);
			GameObject newItem = (GameObject)GameObject.Instantiate(ItemTypes[itemIndex]);
			newItem.transform.parent = movementGrid.gameObject.transform;
			newItem.GetComponent<GridConsumable>().Location = new GridCoordinates(newGridSquare.Row, newGridSquare.Column);
			movementGrid.SquarePositions[newGridSquare.Row][newGridSquare.Column].Consumable = newItem.GetComponent<GridConsumable>();
			newItem.transform.localPosition = newPosition;
		}
	}
	
	/// <summary>
	/// Counts the number of active items in the game.
	/// </summary>
	/// <returns>
	/// The number of active items in the game
	/// </returns>
	int CountActiveItems() {
		return GameObject.FindGameObjectsWithTag("Item").Length;	
	}

	/// <summary>
	/// Resets the item manager to start fresh.
	/// </summary>
	public void Reset() {
		// Removes any active items in the system.
		GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
		for (int i = 0; i < items.Length; ++i) {
			GameObject.Destroy(items[i]);
		}
	}
}
