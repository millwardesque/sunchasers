using UnityEngine;
using System.Collections;

public class PlayerController : Actor {
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
		
		if (State == ActorState.Upright) {
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
						ChangeState(ActorState.InRestroom);
					}
					else if (currentSquare.Component is Chair) {
						ChangeState (ActorState.InChair);	
					}
					else if (currentSquare.Component is SnackBar) {
						ChangeState (ActorState.InSnackBar);	
					}
				}
			}
			
			Bladder += BladderIncreaseRate * Time.deltaTime;
			Hunger += HungerIncreaseRate * Time.deltaTime;
			Relaxation -= RelaxationDecreaseRate * Time.deltaTime;
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
		
		
		if (Mathf.Abs(Bladder - 100.0f) <= Mathf.Epsilon) {
			Relaxation = 0.0f;
			if (State == ActorState.InChair) {
				ChangeState(ActorState.Upright);
			}
		}
		if (Mathf.Abs(Hunger - 100.0f) <= Mathf.Epsilon) {
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
	/// Changes the actor's state.
	/// </summary>
	/// <param name='newState'>
	/// New state.
	/// </param>
	public override void ChangeState(ActorState newState) {
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
