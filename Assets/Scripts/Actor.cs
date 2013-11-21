using UnityEngine;
using System.Collections;

/// <summary>
/// States an actor can be in.
/// </summary>
public enum ActorState {
	Walking,
	Upright,
	InChair,
	InRestroom,
	InSnackBar,
}

/// <summary>
/// Base actor class.
/// </summary>
public class Actor : MonoBehaviour {
	public ActorState State = ActorState.Upright;
	public int StartRow = 0;
	public int StartColumn = 0;
	public float WalkSpeed = 50.0f;
	protected bool isRunning = false;
	
	// The grid on which the player moves.
	protected GameObject movementGrid = null;
	protected MovementGrid movementGridScript = null;
	protected Vector3 movementDirection = new Vector3();
	protected Vector3 movementTarget = new Vector3();
	protected GridSquare lastSquare = null;
	protected GridSquare currentSquare = null;
	public GridSquare CurrentSquare {
		get { return currentSquare; }
		set { 
			lastSquare = currentSquare;
			currentSquare = value;
			movementTarget = CalculatewNewPosition();
			movementDirection = movementTarget - CalculateSquarePosition(lastSquare);
			movementDirection.Normalize();
		}
	}
	
	/// <summary>
	/// Awake hook.
	/// </summary>
	void Awake() {
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));	
		
		OnAwake();
	}
	
	/// <summary>
	/// Start hook.
	/// </summary>
	void Start() {
		// Get the movement script and set the starting square.
		movementGrid = GameObject.FindGameObjectWithTag("Movement Grid");
		movementGridScript = movementGrid.GetComponent<MovementGrid>();
		CurrentSquare = movementGridScript.SquarePositions[StartRow][StartColumn];
		transform.position = CalculateSquarePosition(CurrentSquare);
		
		OnStart();
	}

	/// <summary>
	/// Allows subclasses to run their own version of Unity's Awake()
	/// </summary>
	protected virtual void OnAwake() { }
	
	/// <summary>
	/// Allows subclasses to run their own version of Unity's Start()
	/// </summary>
	protected virtual void OnStart() { }
	
	/// <summary>
	/// Changes the actor's state.
	/// </summary>
	/// <param name='newState'>
	/// New state.
	/// </param>
	public virtual void ChangeState(ActorState newState) {
		if (State == ActorState.Upright && newState == ActorState.InChair) {
			CurrentSquare.Occupier = this;
			transform.Translate(new Vector3(0.0f, 1.0f, 0.5f));
		}
		else if (State == ActorState.InChair && newState == ActorState.Upright) {
			CurrentSquare.Occupier = null;
			transform.Translate(new Vector3(0.0f, -1.0f, -0.5f));
		}
		else if ((State == ActorState.InRestroom || State == ActorState.InSnackBar) && newState == ActorState.Upright) {
			CurrentSquare.Occupier = null;
			GetComponent<MeshRenderer>().enabled = true;
		}
		else if (newState == ActorState.InRestroom || newState == ActorState.InSnackBar) {
			CurrentSquare.Occupier = this;
			GetComponent<MeshRenderer>().enabled = false;
		}
	
		State = newState;
	}

	/// <summary>
	/// Calculates the pixel position of a square
	/// </summary>
	/// <param name="square">The GridSquare.</param>
	protected Vector3 CalculateSquarePosition(GridSquare square) {
		if (square == null) {
			return new Vector3();
		}

		float newX = movementGrid.transform.position.x + (float)square.X;
		float newY = movementGrid.transform.position.y + (float)square.Y;
		return new Vector3(newX, newY, transform.position.z);
	}
	
	/// <summary>
	/// Calculates the new position based on the grid-square the player current occupies.
	/// </summary>
	protected Vector3 CalculatewNewPosition() {
		return CalculateSquarePosition(currentSquare);
	}
	
	/// <summary>
	/// Called when the game state changes.
	/// </summary>
	/// <param name='message'>
	/// Message.
	/// </param>
	public void OnGameStateChange(Message message) {
		GameStateChangeMessage realMessage = (GameStateChangeMessage)message;
		
		switch (realMessage.newState) {
		case GameStateEnum.Running:
			isRunning = true;
			break;
		case GameStateEnum.Paused:
		case GameStateEnum.PlayerWon:
		case GameStateEnum.PlayerLost:
			isRunning = false;
			break;
		case GameStateEnum.WaitingToStart:
			State = ActorState.Upright;
			isRunning = false;
			break;
		default:
			break;
		}
	} 
}
