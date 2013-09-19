using UnityEngine;
using System.Collections;

public enum PlayerState {
	Upright,
	InChair,
	InRestroom
}

public class PlayerController : MonoBehaviour {
	public PlayerState State = PlayerState.Upright;
	public float RelaxationDecreaseRate = 2.0f;
	
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
	public float BladderIncreaseRate = 1.5f;
	
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
	/// Start hook.
	/// </summary>
	void Start () {
		// Get the movement script and set the starting square.
		movementGrid = GameObject.FindGameObjectWithTag("Movement Grid");
		movementGridScript = movementGrid.GetComponent<MovementGrid>();
		CurrentSquare = movementGridScript.SquarePositions[0][0];
	}
	
	/// <summary>
	/// Update hook.
	/// </summary>
	void Update () {
		if (State == PlayerState.Upright) {
			if (Input.GetKeyUp(KeyCode.RightArrow)) {
				if (currentSquare.Column + 1 < movementGridScript.NumColumns) {
					var newSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column + 1];
					if (newSquare.IsTraversable) {
						CurrentSquare = newSquare;
					}
				}
			}
			
			if (Input.GetKeyUp(KeyCode.LeftArrow)) {
				if (currentSquare.Column - 1 >= 0) {
					var newSquare = movementGridScript.SquarePositions[currentSquare.Row][currentSquare.Column - 1];
					if (newSquare.IsTraversable) {
						CurrentSquare = newSquare;
					}
				}
			}
			
			if (Input.GetKeyUp(KeyCode.UpArrow)) {
				if (currentSquare.Row + 1 < movementGridScript.NumRows) {
					var newSquare = movementGridScript.SquarePositions[currentSquare.Row + 1][currentSquare.Column];
					if (newSquare.IsTraversable) {
						CurrentSquare = newSquare;
					}
				}
			}
			
			if (Input.GetKeyUp(KeyCode.DownArrow)) {
				if (currentSquare.Row - 1 >= 0) {
					var newSquare = movementGridScript.SquarePositions[currentSquare.Row - 1][currentSquare.Column];
					if (newSquare.IsTraversable) {
						CurrentSquare = newSquare;
					}
				}
			}
			
			if (Input.GetKeyUp(KeyCode.Space)) {
				if (currentSquare.Components.Count > 0) {
					for (int i = 0; i < currentSquare.Components.Count; ++i) {
						GridComponent component = currentSquare.Components[i];
						if (component is Restroom) {
							ChangeState(PlayerState.InRestroom);
						}
						else if (component is Chair) {
							ChangeState (PlayerState.InChair);	
						}
					}
				}
			}
			
			Relaxation -= RelaxationDecreaseRate * Time.deltaTime;
		}
		else if (State == PlayerState.InChair) {
			if (Input.GetKeyUp(KeyCode.Space)) {
				ChangeState(PlayerState.Upright);
			}	
			
			for (int i = 0; i < currentSquare.Components.Count; ++i) {
				GridComponent component = currentSquare.Components[i];
				if (component is Chair) {
					component.OnUpdate();
				}
			}
		}
		else if (State == PlayerState.InRestroom) {
			if (Input.GetKeyUp(KeyCode.Space)) {
				ChangeState(PlayerState.Upright);
			}
			
			for (int i = 0; i < currentSquare.Components.Count; ++i) {
				GridComponent component = currentSquare.Components[i];
				if (component is Restroom) {
					component.OnUpdate();
				}
			}
		}
		
		Relaxation = Mathf.Clamp(Relaxation, 0.0f, 100.0f);
		Bladder += BladderIncreaseRate * Time.deltaTime;
	}
	
	/// <summary>
	/// OnGUI hook.
	/// </summary>
	void OnGUI () {	 
		string relaxationText = string.Format("Relaxation: {0:00.0}%", Relaxation);
		string bladderText = string.Format("Bladder: {0:00.0}%", Bladder);
	   	
		GUI.Label(new Rect(20, 25, 200, 30), relaxationText);
		GUI.Label(new Rect(20, 45, 200, 30), bladderText);
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
		else if (newState == PlayerState.InRestroom) {
			GetComponent<MeshRenderer>().enabled = false;
		}
		else if (State == PlayerState.InRestroom) {
			GetComponent<MeshRenderer>().enabled = true;	
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
}
