using UnityEngine;
using System.Collections;

public enum PlayerState {
	Upright,
	InChair,
	InRestroom,
	InSnackBar,
}

public class PlayerController : MonoBehaviour {
	public PlayerState State = PlayerState.Upright;
	private bool isRunning = false;
	private GameObject victoryText;
	private GameObject defeatText;
	
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
	
	// The grid on which the player moves.
	private GameObject movementGrid = null;
	private MovementGrid movementGridScript = null;
	private GridSquare currentSquare = null;
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
		MessageManager.Instance.RegisterListener(new Listener("GameTimerElapsed", gameObject, "OnGameTimerElapsed"));
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));
	}
	
	/// <summary>
	/// Start hook.
	/// </summary>
	void Start () {
		// Get the movement script and set the starting square.
		movementGrid = GameObject.FindGameObjectWithTag("Movement Grid");
		movementGridScript = movementGrid.GetComponent<MovementGrid>();
		CurrentSquare = movementGridScript.SquarePositions[0][0];
		victoryText = GameObject.Find("Victory Text");
		victoryText.SetActive(false);
		defeatText = GameObject.Find("Defeat Text");
		defeatText.SetActive(false);
		
		world = GameObject.FindGameObjectWithTag("World");
	}
	
	/// <summary>
	/// Update hook.
	/// </summary>
	void Update () {
		if (!isRunning) {
			return;
		}
		
		if (State == PlayerState.Upright) {
			if (Input.GetKeyUp(KeyCode.RightArrow)) {
				if (movementGridScript.IsTraversableSquare(currentSquare.Row, currentSquare.Column + 1)) {
					CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column + 1];
				}
			}
			
			if (Input.GetKeyUp(KeyCode.LeftArrow)) {
				if (movementGridScript.IsTraversableSquare(currentSquare.Row, currentSquare.Column - 1)) {
					CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column - 1];
				}
			}
			
			if (Input.GetKeyUp(KeyCode.UpArrow)) {
				if (movementGridScript.IsTraversableSquare(currentSquare.Row + 1, currentSquare.Column)) {
					CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row + 1][currentSquare.Column];
				}
			}
			
			if (Input.GetKeyUp(KeyCode.DownArrow)) {
				if (movementGridScript.IsTraversableSquare(currentSquare.Row - 1, currentSquare.Column)) {
					CurrentSquare = movementGridScript.SquarePositions[currentSquare.Row - 1][currentSquare.Column];
				}
			}
			
			// See if the player landed on any items.
			if (CurrentSquare.Consumable) {
				CurrentSquare.Consumable.OnUse();
				GameObject.Destroy(CurrentSquare.Consumable.gameObject);
				CurrentSquare.Consumable = null;
			}
			
			// See if the player is trying to enter a building.
			if (Input.GetKeyUp(KeyCode.Space)) {
				if (currentSquare.Component) {
					if (currentSquare.Component is Restroom) {
						ChangeState(PlayerState.InRestroom);
					}
					else if (currentSquare.Component is Chair) {
						ChangeState (PlayerState.InChair);	
					}
					else if (currentSquare.Component is SnackBar) {
						ChangeState (PlayerState.InSnackBar);	
					}
				}
			}
			
			Bladder += BladderIncreaseRate * Time.deltaTime;
			Hunger += HungerIncreaseRate * Time.deltaTime;
			Relaxation -= RelaxationDecreaseRate * Time.deltaTime;
		}
		else if (State == PlayerState.InChair ||
				 State == PlayerState.InRestroom ||
				 State == PlayerState.InSnackBar) {
			
			if (Input.GetKeyUp(KeyCode.Space)) {
				ChangeState(PlayerState.Upright);
			}
			else if (currentSquare.Component) {				
				currentSquare.Component.OnUpdate();
			}
		}
		
		// Check for a win condition
		if (Mathf.Abs(Relaxation - 100.0f) <= Mathf.Epsilon) {
			GameTimer timer = world.GetComponent<GameTimer>();
			score.Add (new ScoreItem((int)(timer.duration - timer.Elapsed()), "Time"));
			victoryText.SetActive(true);
			State = PlayerState.Upright;
			world.GetComponent<GameState>().State = GameStateEnum.PlayerWon;
			
		}
	}
	
	/// <summary>
	/// OnGUI hook.
	/// </summary>
	void OnGUI () {	 
		string relaxationText = string.Format("Relaxation: {0:00}%", Relaxation);
		string bladderText = string.Format("Bladder: {0:00}%", Bladder);
		string hungerText = string.Format ("Hunger: {0:00}%", Hunger);
	   	
		GUI.Label(new Rect(20, 25, 200, 30), relaxationText);
		GUI.Label(new Rect(20, 45, 200, 30), bladderText);
		GUI.Label(new Rect(20, 65, 200, 30), hungerText);
		
		if (victoryText.activeSelf) {
			GUI.Label(new Rect(300, 25, 200, 30), string.Format("Score: {0}", score.CalculateTotalScore()));		
		}
	}
	
	/// <summary>
	/// Changes the player's state.
	/// </summary>
	/// <param name='newState'>
	/// New state.
	/// </param>
	public void ChangeState(PlayerState newState) {
		if (State == PlayerState.Upright && newState == PlayerState.InChair) {
			transform.Translate(new Vector3(0.0f, 1.0f));
		}
		else if (State == PlayerState.InChair && newState == PlayerState.Upright) {
			transform.Translate(new Vector3(0.0f, -1.0f));
		}
		else if ((State == PlayerState.InRestroom || State == PlayerState.InSnackBar) && newState == PlayerState.Upright) {
			GetComponent<MeshRenderer>().enabled = true;
		}
		else if (newState == PlayerState.InRestroom || newState == PlayerState.InSnackBar) {
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
			State = PlayerState.Upright;
			Relaxation = 0.0f;
			Bladder = 0.0f;
			isRunning = false;
			break;
		default:
			break;
		}
	}
}
