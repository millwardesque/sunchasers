﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPC : Actor {
	public GridCoordinates TargetSquare = new GridCoordinates(-1, -1);
	public float MoveThreshold = 1.0f;
	private List<GridCoordinates> pathToTarget = new List<GridCoordinates>();
	private GridCoordinates lastTarget = null;
	float timeUntilMove = 0.0f;
	
	/// <summary>
	/// Start hook.
	/// </summary>
	override protected void OnStart() {
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
				if (TargetSquare.Equals(CurrentSquare.GridCoords)) {
					if (CurrentSquare.Component is Chair && 	// Kick the player out of the chair.
						(CurrentSquare.Occupier && CurrentSquare.Occupier.CompareTag("Player"))) {
						CurrentSquare.Occupier.ChangeState(ActorState.Upright);
						ChangeState(ActorState.InChair);
						
					}
					else if (CurrentSquare.Component is Chair && !CurrentSquare.IsOccupied()) { // Take the unoccupied chair.
						ChangeState(ActorState.InChair);
					}
					else if (CurrentSquare.Component is Restroom && !CurrentSquare.IsOccupied()) { // Use the restroom.
						ChangeState(ActorState.InRestroom);
					}
					else if (CurrentSquare.Component is SnackBar && !CurrentSquare.IsOccupied()) { // Use the snackbar.
						ChangeState(ActorState.InSnackBar);
					}
					else { // Otherwise, move.
						FindNewSquare();
					}
				}
				else { // Otherwise, move.
					FindNewSquare();
				}
			}
			else if (State == ActorState.InChair) {
				float probabilityOfGettingUp = 0.08f;
				float value = Random.Range (0.0f, 1.0f);
				if (value < probabilityOfGettingUp) {
					ChangeState (ActorState.Upright);
					FindNewTarget();
				}
			}
			else if (State == ActorState.InRestroom || State == ActorState.InSnackBar) {
				float probabilityOfLeaving = 0.16f;
				float value = Random.Range (0.0f, 1.0f);
				if (value < probabilityOfLeaving) {
					ChangeState (ActorState.Upright);
					FindNewTarget();
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
		lastTarget = TargetSquare;
		
		for (int row = 0; row < movementGridScript.NumRows; ++row) {
			for (int column = 0; column < movementGridScript.NumColumns; ++column) {
				GridSquare square = movementGridScript.SquarePositions[row][column];
				if (square.GridCoords.Equals(lastTarget) || square.GridCoords.Equals(CurrentSquare.GridCoords)) {
					continue;	
				}
				float score = (float)(movementGridScript.NumRows + movementGridScript.NumColumns - CurrentSquare.GridCoords.DistanceTo(square.GridCoords));
				
				if (square.Component != null && square.Component is Chair) {
					score *= 2;
				}
				
				score *= Random.Range (0.8f, 1.2f);	// Add some randomness to the selection
				
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
