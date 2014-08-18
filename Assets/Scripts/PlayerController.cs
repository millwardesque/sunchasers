using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : Actor {
	private GameObject victoryText;
	private GameObject defeatText;
	private GameObject towel;

	private GridCoordinates targetSquare = new GridCoordinates(-1, -1);	// Target square used when the user clicks a grid square.
	private List<GridCoordinates> pathToTarget = new List<GridCoordinates>();	// A list of squares that lead to the target square.

	private GameObject world;
	private Camera gameCamera;

	private bool bladderIsFull = false;	// Used to track if the player's bladder gauge is full.
	private bool hungerIsFull = false; // Used to track if the player's hunger gauge is full.

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

		gameCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

		Idle ();
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

			// Update the player's meters.
			Bladder += BladderIncreaseRate * Time.deltaTime;
			Hunger += HungerIncreaseRate * Time.deltaTime;
			Relaxation -= RelaxationDecreaseRate * Time.deltaTime;
			
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
				if (pathToTarget != null && pathToTarget.Count > 0) {
					FindNextSquare();
				}
				else if (Input.GetKey(KeyCode.RightArrow)) {
					WalkEast ();
					CancelAutoPathfind();
				}
				else if (Input.GetKey(KeyCode.LeftArrow)) {
					WalkWest ();
					CancelAutoPathfind();
				}
				else if (Input.GetKey(KeyCode.UpArrow)) {
					WalkNorth ();
					CancelAutoPathfind();
				}
				else if (Input.GetKey(KeyCode.DownArrow)) {
					WalkSouth ();
					CancelAutoPathfind();
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

			if (Input.GetMouseButtonUp(0)) {
				Vector3 worldClickPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
				targetSquare = movementGridScript.GetSquareFromPosition(worldClickPosition).GridCoords;
				Debug.Log(string.Format("Nearest grid square is {0}", targetSquare));

				pathToTarget = movementGridScript.FindPathToSquare(CurrentSquare.GridCoords, targetSquare);
				FindNextSquare();
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow)) {
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
		
		if (Mathf.Abs(Bladder - 100.0f) <= Mathf.Epsilon) {
			Relaxation = 0.0f;
			if (State == ActorState.InChair) {
				ChangeState(ActorState.Upright);
			}

			if (!bladderIsFull) {	// Ensure the camera only shakes when the player reaches full-bladder and not every frame after.
				GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>().TriggerShake();
			}
			bladderIsFull = true;
		}
		else {
			bladderIsFull = false;
		}

		if (Mathf.Abs(Hunger - 100.0f) <= Mathf.Epsilon) {
			Relaxation = 0.0f;
			if (State == ActorState.InChair) {
				ChangeState(ActorState.Upright);
			}

			if (!hungerIsFull) {	// Ensure the camera only shakes when the player reaches full-hunger and not every frame after.
				GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>().TriggerShake();
			}
			hungerIsFull = true;
		}
		else {
			hungerIsFull = false;
		}

		if (Mathf.Abs(Relaxation - 100.0f) <= Mathf.Epsilon) {
			GameTimer timer = world.GetComponent<GameTimer>();
			Score.Add (new ScoreItem((int)(timer.duration - timer.Elapsed()), "Time"));
			
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
			animSleepSouth();
			ToggleTowel(true);
		}
		else if (newState == ActorState.Upright) {
			Idle();
		}
		else if (newState == ActorState.InSnackBar) {
			animAtSnackbar();
		}

		if (State == ActorState.InChair && newState == ActorState.Upright) {
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
		SetSprite("walk-south-0");
		Relaxation = 0.0f;
		Bladder = 0.0f;
		Hunger = 0.0f;
		victoryText.SetActive(false);
		defeatText.SetActive(false);
		ToggleTowel(false);
	}

	protected void WalkNorth() {
		SetSprite("walk-north-0");
		int row = CurrentSquare.Row + 1;
		int column = CurrentSquare.Column;
		
		if (movementGridScript.IsTraversableSquare(row, column) && !movementGridScript.SquareHasChair(CurrentSquare.Row, CurrentSquare.Column)) {
			animWalkNorth();
			CurrentSquare = movementGridScript.SquarePositions[row][column];
			ChangeState(ActorState.Walking);
		}
	}

	protected void WalkSouth() {
		SetSprite("walk-south-0");
		int row = CurrentSquare.Row - 1;
		int column = CurrentSquare.Column;
		
		if (movementGridScript.IsTraversableSquare(row, column) && !movementGridScript.SquareHasChair(row, column)) {
			animWalkSouth();
			CurrentSquare = movementGridScript.SquarePositions[row][column];
			ChangeState(ActorState.Walking);
		}
	}

	protected void WalkWest() {
		SetSprite("walk-west-0");
		
		if (movementGridScript.IsTraversableSquare(currentSquare.Row, currentSquare.Column - 1)) {
			animWalkWest ();
			CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column - 1];
			ChangeState(ActorState.Walking);
		}
	}

	protected void WalkEast() {
		SetSprite("walk-east-0");
		
		if (movementGridScript.IsTraversableSquare(currentSquare.Row, currentSquare.Column + 1)) {
			animWalkEast();
			CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column + 1];
			ChangeState(ActorState.Walking);
		}
	}

	protected void Idle() {
		SetSprite("idle-0");
		animIdle();
	}

	void CancelAutoPathfind() {
		pathToTarget = null;
	}

	/// <summary>
	/// Finds the next square the actor should move to.
	/// </summary>
	void FindNextSquare() {
		if (null == pathToTarget) {
			Debug.Log (string.Format ("Unable find path from {0} to {1}: {2}", CurrentSquare.GridCoords, targetSquare, movementGridScript.SquarePositions[targetSquare.Row][targetSquare.Column]));
		}

		if (pathToTarget.Count > 0) {
			GridCoordinates nextSquare = pathToTarget[0];
			pathToTarget.RemoveAt(0);

			Debug.Log (string.Format("Next step is from {0} to {1}", CurrentSquare.GridCoords, nextSquare));
			if (nextSquare.Column > CurrentSquare.Column) {
				WalkEast();
			}
			else if (nextSquare.Column < CurrentSquare.Column) {
				WalkWest();
			}
			else if (nextSquare.Row > CurrentSquare.Row) {
				WalkNorth();
			}
			else if (nextSquare.Row < CurrentSquare.Row) {
				WalkSouth();
			}

			ChangeState(ActorState.Walking);
		}
	}
}
