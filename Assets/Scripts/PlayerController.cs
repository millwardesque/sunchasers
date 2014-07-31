using UnityEngine;
using System.Collections;

public class PlayerController : Actor {
	private GameObject victoryText;
	private GameObject defeatText;
	private GameObject towel;

	private GameObject world;

	public ScoreKeeper Score;
	
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
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));
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
		ToggleTowel(false);
		
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

				// See if the player landed on any items.
				if (CurrentSquare.Consumable) {
					CurrentSquare.Consumable.OnUse();
					GameObject.Destroy(CurrentSquare.Consumable.gameObject);
					CurrentSquare.Consumable = null;
					Score.Add (new ScoreItem(50, "Item"));
				}

				// Allow player to walk without pausing at each square
				if (Input.GetKey(KeyCode.RightArrow)) {
					WalkEast ();
				}
				else if (Input.GetKey(KeyCode.LeftArrow)) {
					WalkWest ();
				}
				else if (Input.GetKey(KeyCode.UpArrow)) {
					WalkNorth ();
				}
				else if (Input.GetKey(KeyCode.DownArrow)) {
					WalkSouth ();
				}
				else {
					ChangeState (ActorState.Upright);
				}
			}
			else {
				if (Mathf.Abs(distance.x) > Mathf.Abs (distance.y)) {	// If the player is moving horizontally, check for a direction reversal
					if (Input.GetKeyDown(KeyCode.RightArrow) && distance.x < 0) {
						WalkEast ();
					}
					else if (Input.GetKeyDown(KeyCode.LeftArrow) && distance.x >= 0) {
						WalkWest ();
					}
				}
				else if (Mathf.Abs(distance.x) <= Mathf.Abs (distance.y)) {
					if (Input.GetKeyDown(KeyCode.UpArrow) && distance.y < 0) {
						WalkNorth ();
					}
					else if (Input.GetKeyDown(KeyCode.DownArrow) && distance.y >= 0) {
						WalkSouth ();
					}
				}
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
				Score.Add (new ScoreItem(50, "Item"));
			}
			
			if (Input.GetKeyDown(KeyCode.RightArrow)) {
				WalkEast ();
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
				WalkWest ();
			}
			else if (Input.GetKeyDown(KeyCode.UpArrow)) {
				WalkNorth ();
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow)) {
				WalkSouth ();
			}
			else if (Input.GetKeyDown(KeyCode.Space)) {	// See if the player is trying to enter a building.
				if (currentSquare.Component) {
					if (currentSquare.Component is Restroom && !currentSquare.IsOccupied()) {
						((Restroom)(currentSquare.Component)).openAndCloseDoor();
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
			
			if (Input.GetKeyDown(KeyCode.Space)) {
				if (currentSquare.Component is Restroom) {
					((Restroom)(currentSquare.Component)).openAndCloseDoor();
				}
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
			Score.Add (new ScoreItem((int)(timer.duration - timer.Elapsed()), "Time"));
			State = ActorState.Upright;
			
			world.GetComponent<GameState>().State = GameStateEnum.PlayerWon;
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
			ToggleTowel(true);
		}
		else if (State == ActorState.InChair && newState == ActorState.Upright) {
			ToggleTowel(false);
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
			world.GetComponent<GameState>().State = GameStateEnum.PlayerLost;
		}
	}
	
	public void OnGameStateChange(Message message) {
		GameStateChangeMessage realMessage = (GameStateChangeMessage)message;
		if (realMessage.newState == GameStateEnum.PlayerWon) {
			victoryText.SetActive(true);
		}
		else if (realMessage.newState == GameStateEnum.PlayerLost) {
			defeatText.SetActive(true);
		}
	}

	public void SetSprite(string spriteName) {
		actorSprite.SetSprite(spriteName);
	}

	public void ToggleTowel(bool showTowel) {
		towel.SetActive(showTowel);
	}

	public void Reset() {
		SetCurrentSquareAndPosition(StartRow, StartColumn);
		SetSprite("player/front-0");
		Relaxation = 0.0f;
		Bladder = 0.0f;
		Hunger = 0.0f;
		victoryText.SetActive(false);
		defeatText.SetActive(false);
		ToggleTowel(false);
	}

	protected void WalkNorth() {
		SetSprite("player/back-0");
		
		if (movementGridScript.IsTraversableSquare(currentSquare.Row + 1, currentSquare.Column)) {
			animWalkNorth();
			CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row + 1][currentSquare.Column];
			ChangeState(ActorState.Walking);
		}
	}

	protected void WalkSouth() {
		SetSprite("player/front-0");
		
		if (movementGridScript.IsTraversableSquare(currentSquare.Row - 1, currentSquare.Column)) {
			animWalkSouth();
			CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row - 1][currentSquare.Column];
			ChangeState(ActorState.Walking);
		}
	}

	protected void WalkWest() {
		SetSprite("player/left-0");
		
		if (movementGridScript.IsTraversableSquare(currentSquare.Row, currentSquare.Column - 1)) {
			stopAnimations();
			CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column - 1];
			ChangeState(ActorState.Walking);
		}
	}

	protected void WalkEast() {
		SetSprite("player/right-0");
		
		if (movementGridScript.IsTraversableSquare(currentSquare.Row, currentSquare.Column + 1)) {
			stopAnimations();
			CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column + 1];
			ChangeState(ActorState.Walking);
		}
	}
}
