﻿using UnityEngine;
using System.Collections;

public class PlayerController : Actor {
	private GameObject victoryText;
	private GameObject defeatText;
	private GameObject towel;
	
	private Score score = new Score();
	private GameObject world;
	
	// Player traits.
	private float relaxation = 0;
	public float Relaxation {
		get { return relaxation; }
		set { relaxation = Mathf.Clamp (value, 0.0f, 100.0f); }
	}
	
	private float bladder = 0;
	public float Bladder {
		get { return bladder; }
		set { bladder = Mathf.Clamp (value, 0.0f, 100.0f); }
	}
	
	private float hunger = 0;
	public float Hunger {
		get { return hunger; }
		set { hunger = Mathf.Clamp (value, 0.0f, 100.0f); }
	}
	
	// Upright rate of change for player traits (in units / second).
	public float RelaxationDecreaseRate = 2.0f;
	public float BladderIncreaseRate = 1.5f;
	public float HungerIncreaseRate = 0.5f;

	private tk2dSprite actorSprite;
	
	/// <summary>
	/// Awake hook.
	/// </summary>
	protected override void OnAwake() {
		MessageManager.Instance.RegisterListener(new Listener("GameTimerElapsed", gameObject, "OnGameTimerElapsed"));
	}
	
	/// <summary>
	/// Start hook.
	/// </summary>
	protected override void OnStart () {		
		victoryText = GameObject.Find("Victory Text");
		victoryText.GetComponent<MeshRenderer>().enabled = true;
		victoryText.SetActive(false);

		defeatText = GameObject.Find("Defeat Text");
		defeatText.GetComponent<MeshRenderer>().enabled = true;
		defeatText.SetActive(false);

		towel = GameObject.Find("Towel");
		towel.GetComponent<MeshRenderer>().enabled = true;
		towel.SetActive (false);

		world = GameObject.FindGameObjectWithTag("World");
		actorSprite = GetComponent<tk2dSprite>();
	}
	
	/// <summary>
	/// Update hook.
	/// </summary>
	void Update () {
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
			// Update the player's meters.
			Bladder += BladderIncreaseRate * Time.deltaTime;
			Hunger += HungerIncreaseRate * Time.deltaTime;
			Relaxation -= RelaxationDecreaseRate * Time.deltaTime;

			// See if the player landed on any items.
			if (CurrentSquare.Consumable) {
				CurrentSquare.Consumable.OnUse();
				GameObject.Destroy(CurrentSquare.Consumable.gameObject);
				CurrentSquare.Consumable = null;
				score.Add (new ScoreItem(50, "Item"));
			}

			if (Input.GetKeyUp(KeyCode.RightArrow)) {
				actorSprite.SetSprite("player/right-0");

				if (movementGridScript.IsTraversableSquare(currentSquare.Row, currentSquare.Column + 1)) {
					CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column + 1];
					ChangeState(ActorState.Walking);
				}
			}
			else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
				actorSprite.SetSprite("player/left-0");

				if (movementGridScript.IsTraversableSquare(currentSquare.Row, currentSquare.Column - 1)) {
					CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column - 1];
					ChangeState(ActorState.Walking);
				}
			}
			else if (Input.GetKeyUp(KeyCode.UpArrow)) {
				actorSprite.SetSprite("player/back-0");

				if (movementGridScript.IsTraversableSquare(currentSquare.Row + 1, currentSquare.Column)) {
					CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row + 1][currentSquare.Column];
					ChangeState(ActorState.Walking);
				}
			}
			else if (Input.GetKeyUp(KeyCode.DownArrow)) {
				actorSprite.SetSprite("player/front-0");

				if (movementGridScript.IsTraversableSquare(currentSquare.Row - 1, currentSquare.Column)) {
					CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row - 1][currentSquare.Column];
					ChangeState(ActorState.Walking);
				}
			}
			else if (Input.GetKeyUp(KeyCode.Space)) {	// See if the player is trying to enter a building.
				if (currentSquare.Component) {
					if (currentSquare.Component is Restroom && !currentSquare.IsOccupied()) {
						ChangeState(ActorState.InRestroom);
					}
					else if (currentSquare.Component is Chair && !currentSquare.IsOccupied()) {
						ChangeState (ActorState.InChair);
					}
					else if (currentSquare.Component is SnackBar) {
						ChangeState (ActorState.InSnackBar);	
					}
				}
			}
		}
		else if (State == ActorState.InChair ||
				 State == ActorState.InRestroom ||
				 State == ActorState.InSnackBar) {
			
			if (Input.GetKeyUp(KeyCode.Space)) {
				ChangeState(ActorState.Upright);
			}
			else if (currentSquare.Component) {				
				currentSquare.Component.OnUpdate();
			}
		}
		
		if (Mathf.Abs(Bladder - 100.0f) <= Mathf.Epsilon ||
			Mathf.Abs(Hunger - 100.0f) <= Mathf.Epsilon) {
			Relaxation = 0.0f;
			if (State == ActorState.InChair) {
				ChangeState(ActorState.Upright);
			}
		}
		if (Mathf.Abs(Relaxation - 100.0f) <= Mathf.Epsilon) {
			GameTimer timer = world.GetComponent<GameTimer>();
			score.Add (new ScoreItem((int)(timer.duration - timer.Elapsed()), "Time"));
			victoryText.SetActive(true);
			State = ActorState.Upright;
			world.GetComponent<GameState>().State = GameStateEnum.PlayerWon;
		}
	}
	
	/// <summary>
	/// OnGUI hook.
	/// </summary>
	void OnGUI () {	 
		if (victoryText.activeSelf) {
			GUI.Label(new Rect(600, 25, 200, 30), string.Format("Score: {0}", score.CalculateTotalScore()));		
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
		base.ChangeState(newState);
	}
		
	/// <summary>
	/// Called when the game timer elapsed event.
	/// </summary>
	/// <param name='message'>
	/// Message.
	/// </param>
	public void OnGameTimerElapsed(Message message) {
		if (Object.ReferenceEquals(message.MessageSource, GameObject.FindGameObjectWithTag("World"))) {
			world.GetComponent<GameState>().State = GameStateEnum.PlayerWon;
			defeatText.SetActive(true);
		}
	}
}
