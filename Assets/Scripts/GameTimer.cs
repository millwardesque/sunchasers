using UnityEngine;
using System.Collections;

/// <summary>
/// A simple game timer.
/// </summary>
public class GameTimer : MonoBehaviour {
	
	public float duration = 999.0f;	// Duration of the timer.
	public bool loop = false;		// Should the timer loop once the duration has been reached once?
	public bool isPaused = true;	// Is the timer paused or not?
	private bool hasElapsed = false;	// True if the duration has elapsed at least once
	
	private float time = 0.0f;			// Elapsed time since the timer started.
	public float Elapsed() {
		return time;
	}	
	
	/// <summary>
	/// Awake hook.
	/// </summary>
	void Awake() {
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));
	}
	
	/// <summary>
	/// Update hook.
	/// </summary>
	void Update ()
	{
		if (isPaused || (hasElapsed && !loop))
		{
			return;
		}
			
		// Update the timer.
		time += Time.deltaTime;
		
		// Check for timer completion.
		if (time > duration) {
			if (loop) {
				while (time > duration) {
					MessageManager.Instance.SendToListeners(new Message(gameObject, "GameTimerElapsed", ""));		
					time -= duration;
				}
			}
			else {
				MessageManager.Instance.SendToListeners(new Message(gameObject, "GameTimerElapsed", ""));
			}
			
			hasElapsed = true;
		}	
	}
	
	/// <summary>
	/// GUI hook.
	/// </summary>
	void OnGUI ()
	{
	   int minutes = (int)(time / 60);
	   int seconds = (int)(time) % 60;
	   int fraction = (int)(time * 100 % 100);
	 
	   string text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction); 
	   GUI.Label(new Rect(20, 5, 100, 30), text);
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
			time = 0.0f;
			hasElapsed = false;
			break;
		default:
			break;
		}
	}
}

	