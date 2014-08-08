using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NPCSkydiverState {
	Waiting,
	Diving,
	Landed,
}

public class NPCSkydiver : MonoBehaviour {
	public Vector2 destination;
	public float timeToLaunch = 1.0f;	// Seconds until the NPC jumps from the plane
	public float diveSpeed = 1.0f;	// Speed (units / second) travelled when the NCP is diving.

	private bool isPaused = true;
	private float timeRemaining;
	private NPCSkydiverState _state;
	private Vector3 jumpPoint;
	private float t = 0;

	private NPCSkydiverState State {
		get { return _state; }
		set {
			if (value == NPCSkydiverState.Diving) {
				jumpPoint = transform.position;
				t = 0;
				transform.parent = null;
				renderer.enabled = true;
			}
			else if (value == NPCSkydiverState.Landed) {
				// Find nearby chairs.
				List<GridSquare> nearbySquares = new List<GridSquare>();

				// Eject actors from chairs
				foreach (GridSquare square in nearbySquares) {
					if (square.IsOccupied() && square.Component is Chair) {
						square.Occupier.ChangeState(ActorState.Upright);
					}
				}

				// @DEBUG Destroy self until I have something else to do.
				GameObject.Destroy(gameObject);
			}

			_state = value;
		}
	}

	/// <summary>
	/// Awake hook.
	/// </summary>
	void Awake() {
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));
	}

	// Use this for initialization
	void Start () {
		timeRemaining = timeToLaunch;

		renderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (isPaused) {
			return;
		}

		if (State == NPCSkydiverState.Waiting) {
			timeRemaining -= Time.deltaTime;

			if (timeRemaining < 0) {
				State = NPCSkydiverState.Diving;
			}
		}
		else if (State == NPCSkydiverState.Diving) {
			bool hasLanded = false;
			t += Time.deltaTime * diveSpeed;
			if (t > 1.0f) {
				t = 1.0f;
				hasLanded = true;
			}

			Vector3 newPosition = new Vector3(
				Mathf.Lerp(jumpPoint.x, destination.x, t),
				Mathf.Lerp(jumpPoint.y, destination.y, t),
				jumpPoint.z
			);
			transform.position = newPosition;

			if (hasLanded) {
				State = NPCSkydiverState.Landed;
			}
		}
	}
	
	/// <summary>
	/// Called whenever the game state changes.
	/// </summary>
	/// <param name='message'>
	/// Message.
	/// </param>
	void OnGameStateChange(Message message) {
		GameStateChangeMessage realMessage = (GameStateChangeMessage)message;
		
		switch (realMessage.newState) {
		case GameStateEnum.Running:
			isPaused = false;
			break;
		case GameStateEnum.Paused:
		case GameStateEnum.PlayerWon:
		case GameStateEnum.PlayerLost:
			isPaused = true;
			break;
		case GameStateEnum.WaitingToStart:
			isPaused = false;
			break;
		default:
			break;
		}
	}
}
