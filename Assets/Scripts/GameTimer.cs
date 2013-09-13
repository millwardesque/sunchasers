using UnityEngine;
using System.Collections;

/// <summary>
/// A simple game timer.
/// </summary>
public class GameTimer : MonoBehaviour {
	
	public float duration = 999.0f;	// Duration of the timer.
	public bool loop = false;		// Should the timer loop once the duration has been reached once?
	public bool isPaused = true;	// Is the timer paused or not?
	
	private float time = 0.0f;			// Elapsed time since the timer started.
	private bool hasElapsed = false;	// True if the duration has elapsed at least once
	
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
	   GUI.Label(new Rect(400, 25, 100, 30), text);
	}
}

	