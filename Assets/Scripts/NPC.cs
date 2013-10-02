using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPC : Actor {
	public GridCoordinates TargetSquare = new GridCoordinates(-1, -1);
	public float MoveThreshold = 1.0f;
	private List<GridCoordinates> pathToTarget = new List<GridCoordinates>();
	
	float timeUntilMove = 0.0f;
	int directionX = 0;
	int directionY = 0;
	bool justGotUp = false;
	
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
		if (!isRunning) {
			return;
		}
		timeUntilMove -= Time.deltaTime;
		if (timeUntilMove <= float.Epsilon) {
			timeUntilMove = MoveThreshold;
			if (State == ActorState.Upright) {
				if (!justGotUp && CurrentSquare.Component is Chair && 	// Kick the player out of the chair.
					(CurrentSquare.Occupier && CurrentSquare.Occupier.CompareTag("Player"))) {
					CurrentSquare.Occupier.ChangeState(ActorState.Upright);
					ChangeState(ActorState.InChair);
					
				}
				else if (!justGotUp && CurrentSquare.Component is Chair && !CurrentSquare.IsOccupied()) { // Take the unoccupied chair.
					ChangeState(ActorState.InChair);
				}
				else if (!justGotUp && CurrentSquare.Component is Restroom && !CurrentSquare.IsOccupied()) { // Use the restroom.
					ChangeState(ActorState.InRestroom);
				}
				else if (!justGotUp && CurrentSquare.Component is SnackBar && !CurrentSquare.IsOccupied()) { // Use the snackbar.
					ChangeState(ActorState.InSnackBar);
				}
				else { // Otherwise, move.
					FindNewSquare();
					justGotUp = false;
				}	
			}
			else if (State == ActorState.InChair) {
				float probabilityOfGettingUp = 0.05f;
				float value = Random.Range (0.0f, 1.0f);
				if (value < probabilityOfGettingUp) {
					ChangeState (ActorState.Upright);
					justGotUp = true;
				}
			}
			else if (State == ActorState.InRestroom || State == ActorState.InSnackBar) {
				float probabilityOfLeaving = 0.2f;
				float value = Random.Range (0.0f, 1.0f);
				if (value < probabilityOfLeaving) {
					ChangeState (ActorState.Upright);
					justGotUp = true;
				}
			}
		}
	}
	
	/// <summary>
	/// Finds the next square the NPC should move to.
	/// </summary>
	void FindNewSquare() {
		if (pathToTarget.Count == 0 && TargetSquare.Row != -1 && TargetSquare.Column != -1 && TargetSquare.Row != CurrentSquare.Row && TargetSquare.Column != CurrentSquare.Column) {
			pathToTarget = movementGridScript.FindPathToSquare(CurrentSquare.GridCoords, TargetSquare);	
			Debug.Log(string.Format ("Found path from {0} to {1}", CurrentSquare.GridCoords, TargetSquare));
			/*foreach (GridCoordinates node in pathToTarget) {
				Debug.Log (node);
			} */
		}
		else if (pathToTarget.Count > 0) {
			GridCoordinates nextSquare = pathToTarget[0];
			pathToTarget.RemoveAt(0);
			
			CurrentSquare = movementGridScript.SquarePositions[nextSquare.Row][nextSquare.Column];
		}
	}
	
	/// <summary>
	/// Finds a random square.
	/// </summary>
	void FindRandomSquare() {
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
			float changeProbability = 0.3f;
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
