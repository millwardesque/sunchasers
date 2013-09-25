using UnityEngine;
using System.Collections;

public class NPC : Actor {
	float moveThreshold = 1.0f;
	float timeUntilMove = 0.0f;
	int directionX = 0;
	int directionY = 0;
	
	/// <summary>
	/// Start hook.
	/// </summary>
	override protected void OnStart() {
		// Pick a starting direction.
		while (directionX == 0 && directionY == 0) {
			directionX = GetRandomOffset (CurrentSquare.Column, 0, movementGridScript.NumColumns - 1);
			directionY = GetRandomOffset (CurrentSquare.Row, 0, movementGridScript.NumRows - 1);
			
			// If both X and Y have a non-zero direction, pick one since the NPC can't move on angles.
			if (directionX != 0 && directionY != 0) {
				if (Random.Range(0, 2) == 0) {
					directionX = 0;	
				}
				else {
					directionY = 0;
				}
			}
		}
	}
	
	/// <summary>
	/// Update hook.
	/// </summary>
	void Update() {
		if (State == ActorState.Upright) {
			timeUntilMove -= Time.deltaTime;
			
			if (timeUntilMove <= float.Epsilon) {
				timeUntilMove = moveThreshold;
				FindNewSquare();
			}	
		}
	}
	
	/// <summary>
	/// Finds the next square the NPC should move to.
	/// </summary>
	void FindNewSquare() {
		if (CurrentSquare.Column + directionX > movementGridScript.NumColumns - 1 ||	// If we've hit a horizontal axis extent, change to a vertical direction.
			CurrentSquare.Column + directionX < 0) {	
			directionX = 0;
			
			// Make sure the direction isn't (0, 0).
			while (directionY == 0) {
				directionY = GetRandomOffset(CurrentSquare.Row, 0, movementGridScript.NumRows - 1);
			}
		}
		else if (CurrentSquare.Row + directionY > movementGridScript.NumRows - 1 ||	// If we've hit a vertical axis extent, change to a horizontal direction.
			CurrentSquare.Row + directionY < 0) {
			directionY = 0;
			
			// Make sure the direction isn't (0, 0).
			while (directionX == 0) {
				directionX = GetRandomOffset(CurrentSquare.Column, 0, movementGridScript.NumColumns - 1);
			}
		}
		else {
			float changeProbability = 0.2f;
			float value = Random.Range (0.0f, 1.0f);
			
			// Randomly change direction.
			if (value < changeProbability) {
				directionX = GetRandomOffset (CurrentSquare.Column, 0, movementGridScript.NumColumns - 1);
				directionY = GetRandomOffset (CurrentSquare.Row, 0, movementGridScript.NumRows - 1);
				
				if (directionX != 0 && directionY != 0) {
					if (Random.Range(0, 2) == 0) {
						directionX = 0;	
					}
					else {
						directionY = 0;
					}
				}
			}
		}
	
		// Ensure the new square is actually traversable.
		if (movementGridScript.IsTraversableSquare(currentSquare.Row + directionY, currentSquare.Column + directionX)) {
			CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row + directionY][currentSquare.Column + directionX];
		}
	}
	
	/// <summary>
	/// Calculates a random offset between -1, 0, 1 taking into account the axis size.
	/// </summary>
	/// <returns>
	/// The random offset.
	/// </returns>
	/// <param name='current'>
	/// The current position.
	/// </param>
	/// <param name='min_extent'>
	/// The minimum extent of the axis. Usually 0.
	/// </param>
	/// <param name='max_extent'>
	/// The maximum extent of the axis.
	/// </param>
	int GetRandomOffset(int current, int min_extent, int max_extent) {
		int offset = 0;
		if (current == min_extent) {
			offset = Random.Range (0, 2);
		}
		else if (current == max_extent) {
			offset = Random.Range (-1, 1);
		}
		else {
			offset = Random.Range (-1, 2);
		}
		
		return offset;
	}
}
