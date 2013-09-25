using UnityEngine;
using System.Collections;

/// <summary>
/// States an actor can be in.
/// </summary>
public enum ActorState {
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
	protected bool isRunning = false;
	
	// The grid on which the player moves.
	protected GameObject movementGrid = null;
	protected MovementGrid movementGridScript = null;
	protected GridSquare currentSquare = null;
	public GridSquare CurrentSquare {
		get { return currentSquare; }
		set { 
			currentSquare = value;
			CalculateNewPosition();
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
			transform.Translate(new Vector3(0.0f, 1.0f));
		}
		else if (State == ActorState.InChair && newState == ActorState.Upright) {
			CurrentSquare.Occupier = null;
			transform.Translate(new Vector3(0.0f, -1.0f));
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
	/// Calculates the new position based on the grid-square the player current occupies.
	/// </summary>
	void CalculateNewPosition() {
		float newX = movementGrid.transform.position.x + (float)currentSquare.X;
		float newY = movementGrid.transform.position.y + (float)currentSquare.Y;
		Vector3 newPosition = new Vector3(newX, newY, transform.position.z);
		transform.position = newPosition;
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
