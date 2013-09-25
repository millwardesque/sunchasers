using UnityEngine;
using System.Collections;

public class SkyColour : MonoBehaviour {
	tk2dSprite skySprite;
	GameTimer timer;
	bool isRunning = false;
	
	/// <summary>
	/// Awake hook.
	/// </summary>
	void Awake() {
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));	
	}
	
	// Use this for initialization
	void Start () {
		skySprite = gameObject.GetComponent<tk2dSprite>();
		timer = GameObject.FindGameObjectWithTag("World").GetComponent<GameTimer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (isRunning) {
			float change = Time.deltaTime / 100.0f;
			if (timer.Elapsed() / timer.duration > 0.5f) {
				change *= -1.0f;	
			}
			
			skySprite.color = new Color(skySprite.color.r + change, skySprite.color.g + change, skySprite.color.b + change);
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
			break;
		default:
			break;
		}
	}
}
