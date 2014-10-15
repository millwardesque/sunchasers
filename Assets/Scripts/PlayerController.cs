using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : Actor {
	private GameObject victoryText;
	private GameObject defeatText;
	private GameObject towel;
	
	private List<GridCoordinates> pathToTarget = new List<GridCoordinates>();	// A list of squares that lead to the target square.
	private bool useOnArrival = false;	// Flag that can be set to cause the player to automatically 'use' a grid square's component upon arrival.

	private GameObject world;
	private Camera gameCamera;

	private bool bladderIsFull = false;	// Used to track if the player's bladder gauge is full.
	private bool hungerIsFull = false; // Used to track if the player's hunger gauge is full.

	private AudioSource audioSource;

	public ScoreKeeper Score;
	public AudioClip WalkSound;
	public AudioClip DrinkSound;

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
		audioSource = GetComponent<AudioSource>();

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
	
		// Set the movement variables to their default state.
		pathToTarget.Clear();
		useOnArrival = false;
	}
	
	/// <summary>
	/// Update hook.
	/// </summary>
	void Update () {
		if (!isRunning) {
			return;
		}

		// State-specific behaviours.
		if (State == ActorState.Walking) {
			OnUpdateWalking();
		}
		else if (State == ActorState.Upright) {
			OnUpdateUpright ();
		}
		else if (State == ActorState.InChair ||
		         State == ActorState.InRestroom ||
		         State == ActorState.InSnackBar) {
			OnUpdateUsingComponent();
		}

		// Process changes to the player's stats.
		UpdateBladder();
		UpdateHunger();
		UpdateRelaxation();
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
		else if (newState == ActorState.Upright && State != ActorState.Walking) {
			Idle();
		}
		else if (newState == ActorState.InSnackBar) {
			transform.Translate(new Vector3(0, 0, 0.5f));
			animAtSnackbar();
			CurrentSquare.Component.GetComponent<AudioSource>().Play();
		}

		if (State == ActorState.InChair && newState == ActorState.Upright) {
			ToggleTowel(false);
		}
		else if (State == ActorState.InSnackBar && newState != ActorState.InSnackBar) {
			transform.Translate(new Vector3(0, 0, -0.5f));
			CurrentSquare.Component.GetComponent<AudioSource>().Stop();
		}

		if (newState == ActorState.Walking) {
			if (!audioSource.isPlaying || audioSource.clip != WalkSound) {
				GetComponent<AudioSource>().clip = WalkSound;
				GetComponent<AudioSource>().Play();
			}
		}
		else {
			GetComponent<AudioSource>().Stop();
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
		else if (realMessage.newState == GameStateEnum.Running || realMessage.newState == GameStateEnum.WaitingToStart) {
			renderer.enabled = true;
			Idle ();
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

	/// <summary>
	/// Updates the player's stats
	/// </summary>
	/// <param name="deltaTime">Delta time.</param>
	protected void UpdateStats(float deltaTime) {
		Bladder += BladderIncreaseRate * deltaTime;
		Hunger += HungerIncreaseRate * deltaTime;
		Relaxation -= RelaxationDecreaseRate * deltaTime;
	}

	/// <summary>
	/// Called in the Update function if the player is in the Upright state.
	/// </summary>
	protected void OnUpdateUpright() {
		UpdateStats(Time.deltaTime);
		TryToConsume();
		
		if (Input.GetMouseButtonUp(0) && !EventSystemManager.currentSystem.IsPointerOverEventSystemObject()) {
			Vector3 worldClickPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
			GridSquare targetSquare = movementGridScript.GetSquareFromPosition(worldClickPosition);

			ReplaceMovementQueue(movementGridScript.FindPathToSquare(CurrentSquare.GridCoords, targetSquare.GridCoords));
			if (targetSquare.Component != null) {
				useOnArrival = true;
			}
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow)) {
			if (CanWalkTo (0, 1)) {
				pathToTarget.Clear();
				AppendMovementNode(movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column + 1].GridCoords);
				useOnArrival = false;
			}
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			if (CanWalkTo (0, -1))  {
				pathToTarget.Clear();
				AppendMovementNode(movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column - 1].GridCoords);
				useOnArrival = false;
			}
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow)) {
			if (CanWalkTo (1, 0)) {
				pathToTarget.Clear();
				AppendMovementNode(movementGridScript.SquarePositions[CurrentSquare.Row + 1][CurrentSquare.Column].GridCoords);
				useOnArrival = false;
			}
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow)) {
			if (CanWalkTo (-1, 0)) {
				pathToTarget.Clear();
				AppendMovementNode(movementGridScript.SquarePositions[CurrentSquare.Row - 1][CurrentSquare.Column].GridCoords);
				useOnArrival = false;
			}
		}
		else if (Input.GetKeyDown(KeyCode.Space)) {	// See if the player is trying to enter a building.
			TryToUseComponent(currentSquare.Component);
			useOnArrival = false;
		}

		// Find the next target to walk to.
		FindNextSquare();
	}

	/// <summary>
	/// Called in the Update function if the player is in the Walking state.
	/// </summary>
	protected void OnUpdateWalking() {
		Vector3 distance = movementDirection * WalkSpeed * Time.deltaTime;
		UpdateStats(Time.deltaTime);
		
		if (distance.magnitude >= (movementTarget - transform.position).magnitude) {
			distance = movementTarget - transform.position;

			TryToConsume();
			
			// Allow player to walk without pausing at each square
			if (pathToTarget.Count > 0) {
				FindNextSquare();
			}
			else if (Input.GetKey(KeyCode.RightArrow) && CanWalkTo (0, 1)) {
				pathToTarget.Clear();
				AppendMovementNode(movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column + 1].GridCoords);
				useOnArrival = false;
				FindNextSquare();	// Immediately fetch the next square to avoid a 1-frame pause while the actor switches to the upright state.
			}
			else if (Input.GetKey(KeyCode.LeftArrow) && CanWalkTo (0, -1)) {
				pathToTarget.Clear();
				AppendMovementNode(movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column - 1].GridCoords);
				useOnArrival = false;
				FindNextSquare();	// Immediately fetch the next square to avoid a 1-frame pause while the actor switches to the upright state.
			}
			else if (Input.GetKey(KeyCode.UpArrow) && CanWalkTo (1, 0)) {
				pathToTarget.Clear();
				AppendMovementNode(movementGridScript.SquarePositions[currentSquare.Row + 1][currentSquare.Column].GridCoords);
				useOnArrival = false;
				FindNextSquare();	// Immediately fetch the next square to avoid a 1-frame pause while the actor switches to the upright state.
			}
			else if (Input.GetKey(KeyCode.DownArrow) && CanWalkTo (-1, 0)) {
				pathToTarget.Clear();
				AppendMovementNode(movementGridScript.SquarePositions[currentSquare.Row - 1][currentSquare.Column].GridCoords);
				useOnArrival = false;
				FindNextSquare();	// Immediately fetch the next square to avoid a 1-frame pause while the actor switches to the upright state.
			}
			else {
				if (useOnArrival) {
					if (!TryToUseComponent(currentSquare.Component)) {
						ChangeState (ActorState.Upright);
					}
					useOnArrival = false;
				}
				else {
					ChangeState (ActorState.Upright);
				}
			}
		}
		else {
			if (Mathf.Abs(distance.x) > Mathf.Abs (distance.y)) {	// If the player is moving horizontally, check for a direction reversal
				if (Input.GetKeyDown(KeyCode.RightArrow) && distance.x < 0 && CanWalkTo (0, 1)) {
					pathToTarget.Clear();
					AppendMovementNode(movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column + 1].GridCoords);
					useOnArrival = false;
					FindNextSquare();	// Immediately fetch the next square to avoid a 1-frame pause while the actor switches to the upright state.
				}
				else if (Input.GetKeyDown(KeyCode.LeftArrow) && distance.x >= 0 && CanWalkTo (0, -1)) {
					pathToTarget.Clear();
					AppendMovementNode(movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column - 1].GridCoords);
					useOnArrival = false;
					FindNextSquare();	// Immediately fetch the next square to avoid a 1-frame pause while the actor switches to the upright state.
				}
			}
			else if (Mathf.Abs(distance.x) <= Mathf.Abs (distance.y) && CanWalkTo (1, 0)) {
				if (Input.GetKeyDown(KeyCode.UpArrow) && distance.y < 0) {
					pathToTarget.Clear();
					AppendMovementNode(movementGridScript.SquarePositions[currentSquare.Row + 1][currentSquare.Column].GridCoords);
					useOnArrival = false;
					FindNextSquare();	// Immediately fetch the next square to avoid a 1-frame pause while the actor switches to the upright state.
				}
				else if (Input.GetKeyDown(KeyCode.DownArrow) && distance.y >= 0 && CanWalkTo (-1, 0)) {
					pathToTarget.Clear();
					AppendMovementNode(movementGridScript.SquarePositions[currentSquare.Row - 1][currentSquare.Column].GridCoords);
					useOnArrival = false;
					FindNextSquare();	// Immediately fetch the next square to avoid a 1-frame pause while the actor switches to the upright state.
				}
			}
		}
		
		transform.Translate(distance);
	}

	/// <summary>
	/// Called in the Update function if the user is using a component like a chair, the washroom, or snackbar.
	/// </summary>
	protected void OnUpdateUsingComponent() {
		if (Input.GetMouseButtonUp(0) && !EventSystemManager.currentSystem.IsPointerOverEventSystemObject()) {
			currentSquare.Component.OnDeactivate(this);
			ChangeState(ActorState.Upright);
			
			Vector3 worldClickPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
			GridSquare targetSquare = movementGridScript.GetSquareFromPosition(worldClickPosition);
			
			ReplaceMovementQueue(movementGridScript.FindPathToSquare(CurrentSquare.GridCoords, targetSquare.GridCoords));
			if (targetSquare.Component != null) {
				useOnArrival = true;
			}
		}
		else if (Input.GetKeyDown(KeyCode.Space)) {
			currentSquare.Component.OnDeactivate(this);
			ChangeState(ActorState.Upright);
		}
		else if (currentSquare.Component) {				
			currentSquare.Component.OnUpdate();
		}
	}

	/// <summary>
	/// Updates the player's bladder state.
	/// </summary>
	protected void UpdateBladder() {
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
	}

	/// <summary>
	/// Updates the player's hunger state.
	/// </summary>
	protected void UpdateHunger() {
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
	}

	/// <summary>
	/// Updates the player's relaxation state.
	/// </summary>
	protected void UpdateRelaxation() {
		if (Mathf.Abs(Relaxation - 100.0f) <= Mathf.Epsilon) {
			GameTimer timer = world.GetComponent<GameTimer>();
			Score.Add (new ScoreItem((int)(timer.duration - timer.Elapsed()), "Time"));
			
			world.GetComponent<GameState>().State = GameStateEnum.PlayerWon;
		}
	}

	/// <summary>
	/// Tries to use the current square's component.
	/// </summary>
	/// <returns><c>true</c>, if the squares component was successfully used, <c>false</c> otherwise.</returns>
	/// <param name="gridComponent">Grid component to try to use.</param>
	protected bool TryToUseComponent(GridComponent gridComponent) {
		if (gridComponent != null && !currentSquare.IsOccupied()) {
			pathToTarget.Clear();
			gridComponent.OnActivate(this);
			if (gridComponent is Restroom) {
				ChangeState(ActorState.InRestroom);
			}
			else if (gridComponent is Chair) {
				ChangeState (ActorState.InChair);
			}
			else if (gridComponent  is SnackBar) {
				ChangeState (ActorState.InSnackBar);	
			}

			return true;
		}
		else {
			return false;
		}
	}
	
	/// <summary>
	/// Tries to consume if the current square has a consumable attached.
	/// </summary>
	/// <returns><c>true</c>, if a consumable was used, <c>false</c> otherwise.</returns>
	protected bool TryToConsume() {
		if (CurrentSquare.Consumable) {
			CurrentSquare.Consumable.OnUse();
			
			audioSource.PlayOneShot(DrinkSound);
			
			GameObject.Destroy(CurrentSquare.Consumable.gameObject);
			CurrentSquare.Consumable = null;
			Score.Add (new ScoreItem(50, "Item"));
			return true;
		}
		return false;
	}

	/// <summary>
	/// Helper function for determining if a player can walk to a square relative to his current position.
	/// </summary>
	/// <returns><c>true</c> if the player can walk to the specified location; otherwise, <c>false</c>.</returns>
	/// <param name="deltaRow">The delta between the player's current row and the new row.</param>
	/// <param name="deltaColumn">The delta between the player's current column and the new column.</param>
	protected bool CanWalkTo(int deltaRow, int deltaColumn) {
		int row = CurrentSquare.Row + deltaRow;
		int column = CurrentSquare.Column + deltaColumn;

		if (deltaRow == 1 && deltaColumn == 0) { // Moving north, make sure we don't have a chair in our way.
			return (movementGridScript.IsTraversableSquare(row, column) && !movementGridScript.SquareHasChair(CurrentSquare.Row, CurrentSquare.Column));
		}
		else if (deltaRow == -1 && deltaColumn == 0) {	// Moving south, make sure we don't have a chair in our way.
			return (movementGridScript.IsTraversableSquare(row, column) && !movementGridScript.SquareHasChair(row, column));
		}
		else {
			return movementGridScript.IsTraversableSquare(row, column);
		}
	}

	protected void WalkNorth() {
		SetSprite("walk-north-0");
		int row = CurrentSquare.Row + 1;
		int column = CurrentSquare.Column;
		
		if (CanWalkTo(1, 0)) {
			animWalkNorth();
			CurrentSquare = movementGridScript.SquarePositions[row][column];
			ChangeState(ActorState.Walking);
		}
	}

	protected void WalkSouth() {
		SetSprite("walk-south-0");
		int row = CurrentSquare.Row - 1;
		int column = CurrentSquare.Column;
		
		if (CanWalkTo(-1, 0)) {
			animWalkSouth();
			CurrentSquare = movementGridScript.SquarePositions[row][column];
			ChangeState(ActorState.Walking);
		}
	}

	protected void WalkWest() {
		SetSprite("walk-west-0");
		
		if (CanWalkTo (0, -1)) {
			animWalkWest ();
			CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column - 1];
			ChangeState(ActorState.Walking);
		}
	}

	protected void WalkEast() {
		SetSprite("walk-east-0");
		
		if (CanWalkTo (0, 1)) {
			animWalkEast();
			CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column + 1];
			ChangeState(ActorState.Walking);
		}
	}

	protected void Idle() {
		SetSprite("idle-0");
		animIdle();
	}

	/// <summary>
	/// Appends a GridCoordinates node to the movement queue.
	/// </summary>
	/// <param name="node">The GridCoordinates node to append.</param>
	protected void AppendMovementNode(GridCoordinates node) {
		pathToTarget.Add(node);
	}

	/// <summary>
	/// Replaces the existing movement queue with a new queue.
	/// </summary>
	/// <param name="newQueue">New queue.</param>
	protected void ReplaceMovementQueue(List<GridCoordinates> newQueue) {
		if (newQueue != null) {
			pathToTarget = newQueue;
		}
		else {
			pathToTarget.Clear();
		}
	}

	void CancelAutoPathfind() {
		pathToTarget.Clear();
	}

	/// <summary>
	/// Finds the next square the actor should move to.
	/// </summary>
	void FindNextSquare() {
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
		}
	}
}
