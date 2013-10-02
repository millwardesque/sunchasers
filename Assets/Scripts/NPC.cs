using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPC : Actor {
	public GridCoordinates TargetSquare = new GridCoordinates(-1, -1);
	public float MoveThreshold = 1.0f;
	private List<GridCoordinates> pathToTarget = new List<GridCoordinates>();
	
	float timeUntilMove = 0.0f;
	bool justGotUp = false;
	
	/// <summary>
	/// Start hook.
	/// </summary>
	override protected void OnStart() {
		if (TargetSquare.Equals (new GridCoordinates(-1, -1))) {
			FindNewTarget();
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
		if (pathToTarget.Count == 0) {
			FindNewTarget();
			pathToTarget = movementGridScript.FindPathToSquare(CurrentSquare.GridCoords, TargetSquare);	
		}
		else if (pathToTarget.Count > 0) {
			GridCoordinates nextSquare = pathToTarget[0];
			pathToTarget.RemoveAt(0);
			
			CurrentSquare = movementGridScript.SquarePositions[nextSquare.Row][nextSquare.Column];
		}
	}
	
	void FindNewTarget() {
		float bestScore = 0.0f;
		GridSquare bestTarget = null;
		
		for (int row = 0; row < movementGridScript.NumRows; ++row) {
			for (int column = 0; column < movementGridScript.NumColumns; ++column) {
				GridSquare square = movementGridScript.SquarePositions[row][column];
				float score = (float)CurrentSquare.GridCoords.DistanceTo(square.GridCoords);
				
				if (square.Component != null) {
					score *= 2;
				}
				
				if (score > bestScore || bestTarget == null) {
					bestScore = score;
					bestTarget = square;
				}
			}
		}
		
		if (bestTarget != null) {
			TargetSquare = bestTarget.GridCoords;
		}
	}
}
