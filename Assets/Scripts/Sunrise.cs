using UnityEngine;
using System.Collections;

public class Sunrise : MonoBehaviour {
	GameTimer timer;
	public float TargetHeight = 28.0f;
	float heightChangePerSecond;

	public float TargetWidth = 0.5f;
	float widthChangePerSecond;

	public float TargetXPos = 50.0f;
	float xPosChangePerSecond;

	bool isRunning = false;
	private float startXPos = 0.0f;
	private float startHeight = 0.0f;
	private float startWidth = 0.0f;
	
	/// <summary>
	/// Awake hook.
	/// </summary>
	void Awake() {
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));

		startHeight = transform.position.y;
		startWidth = transform.localScale.x;
		startXPos = transform.position.x;
	}
	
	// Use this for initialization
	void Start () {
		timer = GameObject.FindGameObjectWithTag("World").GetComponent<GameTimer>();
		heightChangePerSecond = (TargetHeight - startHeight) / (timer.duration * 0.5f);
		widthChangePerSecond = (TargetWidth - startWidth) / (timer.duration * 0.5f);
		xPosChangePerSecond = (TargetXPos - startXPos) / timer.duration;
	}
	
	// Update is called once per frame
	void Update () {
		if (isRunning) {
			float direction = timer.Elapsed() / timer.duration < 0.5f ? 1.0f : -1.0f;
			float heightChange = Time.deltaTime * direction * heightChangePerSecond;
			float widthChange = Time.deltaTime * direction * widthChangePerSecond;
			float xPosChange = Time.deltaTime * xPosChangePerSecond;
			
			transform.Translate(new Vector3(xPosChange, heightChange, 0));
			transform.localScale += new Vector3(widthChange, widthChange, 0);
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
			Vector3 newPosition = new Vector3(startXPos, startHeight, transform.position.z);
			transform.position = newPosition;
			transform.localScale = new Vector3(startWidth, transform.localScale.y, transform.localScale.z);
			break;
		default:
			break;
		}
	}
}
