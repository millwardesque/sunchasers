using UnityEngine;
using System.Collections;

public class Sunrise : MonoBehaviour {
	GameTimer timer;
	public float TargetHeight = 28.0f;
	float changePerSecond;
	bool isRunning = false;
	private float startHeight = 0.0f;
	
	/// <summary>
	/// Awake hook.
	/// </summary>
	void Awake() {
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));

		startHeight = transform.position.y;
	}
	
	// Use this for initialization
	void Start () {
		timer = GameObject.FindGameObjectWithTag("World").GetComponent<GameTimer>();
		changePerSecond = TargetHeight / (timer.duration * 0.5f);

	}
	
	// Update is called once per frame
	void Update () {
		if (isRunning) {
			float direction = timer.Elapsed() / timer.duration < 0.5f ? 1.0f : -1.0f;
			float change = Time.deltaTime * direction * changePerSecond;
			
			transform.Translate(new Vector3(0, change, 0));
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
		case GameStateEnum.WaitingToStart:
			isRunning = false;

			// Reset the position of the sun.
			Vector3 newPosition = new Vector3(transform.position.x, startHeight, transform.position.z);
			transform.position = newPosition;
			break;
		default:
			break;
		}
	}
}
