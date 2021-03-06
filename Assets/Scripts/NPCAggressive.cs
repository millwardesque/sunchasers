﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCAggressive : Actor {
	public GridCoordinates TargetSquare = new GridCoordinates(-1, -1);
	private List<GridCoordinates> pathToTarget = new List<GridCoordinates>();
	private GridCoordinates lastTarget = null;
	private tk2dSprite actorSprite;
	private GameObject towel;
	
	/// <summary>
	/// Start hook.
	/// </summary>
	override protected void OnStart() {
		actorSprite = GetComponent<tk2dSprite>();

		towel = GameObject.Find("NPC-Aggressive-Towel");
		towel.GetComponent<MeshRenderer>().enabled = true;
		towel.SetActive (false);
	}
	
	/// <summary>
	/// Update hook.
	/// </summary>
	void Update() {
		if (!isRunning) {
			return;
		}
		if (State == ActorState.Walking) {
			Vector3 distance = movementDirection * WalkSpeed * Time.deltaTime;
			
			if (distance.magnitude >= (movementTarget - transform.position).magnitude) {
				distance = movementTarget - transform.position;
				ChangeState (ActorState.Upright);
			}
			transform.Translate(distance);
		}
		else if (State == ActorState.Upright) {
			if (TargetSquare.Equals(CurrentSquare.GridCoords)) {
				if (CurrentSquare.Component is Chair && 	// Kick the player out of the chair.
					(CurrentSquare.Occupier && CurrentSquare.Occupier.CompareTag("Player"))) {
					CurrentSquare.Occupier.ChangeState(ActorState.Upright);
					ChangeState(ActorState.InChair);

					GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>().TriggerShake();
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
			float probabilityOfGettingUp = 0.005f;
			float value = Random.Range (0.0f, 1.0f);
			if (value < probabilityOfGettingUp) {
				ChangeState (ActorState.Upright);
				FindNewTarget();
			}
		}
		else if (State == ActorState.InRestroom || State == ActorState.InSnackBar) {
			float probabilityOfLeaving = 0.02f;
			float value = Random.Range (0.0f, 1.0f);
			if (value < probabilityOfLeaving) {
				ChangeState (ActorState.Upright);
				FindNewTarget();
			}
		}
	}

	/// <summary>
	/// Changes the actor's state.
	/// </summary>
	/// <param name='newState'>
	/// New state.
	/// </param>
	public override void ChangeState(ActorState newState) {
		if (newState == ActorState.InChair) {
			towel.SetActive(true);
		}
		else if (State == ActorState.InChair && newState == ActorState.Upright) {
			towel.SetActive(false);
		}
		else if (newState == ActorState.Upright) {
			stopAnimations();
		}
		base.ChangeState(newState);
	}
	
	/// <summary>
	/// Finds the next square the NPC should move to.
	/// </summary>
	void FindNewSquare() {	
		if (null == pathToTarget) {
			Debug.Log (string.Format ("Unable find path from {0} to {1}: {2}", CurrentSquare.GridCoords, TargetSquare, movementGridScript.SquarePositions[TargetSquare.Row][TargetSquare.Column]));
		}

		if (pathToTarget.Count == 0) {
			FindNewTarget();
			pathToTarget = movementGridScript.FindPathToSquare(CurrentSquare.GridCoords, TargetSquare);
		}

		if (pathToTarget.Count > 0) {
			GridCoordinates nextSquare = pathToTarget[0];
			pathToTarget.RemoveAt(0);

			if (nextSquare.Column > CurrentSquare.Column) {
				actorSprite.SetSprite("NPC-Aggressive/right-0");
			}
			else if (nextSquare.Column < CurrentSquare.Column) {
				actorSprite.SetSprite("NPC-Aggressive/left-0");
			}
			else if (nextSquare.Row > CurrentSquare.Row) {
				actorSprite.SetSprite("NPC-Aggressive/back-0");
				animWalkNorth ();
			}
			else if (nextSquare.Row < CurrentSquare.Row) {
				actorSprite.SetSprite("NPC-Aggressive/front-0");
				animWalkSouth ();
			}

			CurrentSquare = movementGridScript.SquarePositions[nextSquare.Row][nextSquare.Column];
			ChangeState(ActorState.Walking);
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
		else {
			Debug.Log (string.Format("Unable to find new target from {0}, {1}", CurrentSquare.Column, CurrentSquare.Row));
		}
	}

	public void TargetPlayerSquare() {
		PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		if (player) {
			ChangeState(ActorState.Upright);
			TargetSquare = player.CurrentSquare.GridCoords;
			pathToTarget = movementGridScript.FindPathToSquare(CurrentSquare.GridCoords, TargetSquare);

			Debug.Log (string.Format ("Targeting {0}", player.CurrentSquare.GridCoords));
		}
	}
}
