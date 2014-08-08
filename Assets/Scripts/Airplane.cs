using UnityEngine;
using System.Collections;

public class Airplane : MonoBehaviour {
	public float speed = 1.0f;	// Units / second
	public Vector2 destination;

	private Vector3 start;
	private float t = 0.0f;
	private bool isPaused = true;

	/// <summary>
	/// Awake hook.
	/// </summary>
	void Awake() {
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));
	}

	// Use this for initialization
	void Start () {
		start = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (isPaused) {
			return;
		}
		bool hasFinished = false;

		t += Time.deltaTime * speed;
		if (t > 1.0f) {
			t = 1.0f;
			hasFinished = true;
		}

		Vector3 newPosition = new Vector3(
			Mathf.Lerp(start.x, destination.x, t),
			Mathf.Lerp(start.y, destination.y, t),
			start.z
		);

		transform.position = newPosition;

		if (hasFinished) {
			GameObject.Destroy(gameObject);
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
