using UnityEngine;
using System.Collections;

public class TimeRemaining : MonoBehaviour {
	private GameTimer timer;
	private tk2dTextMesh textmesh;
	
	// Use this for initialization
	void Start () {
		timer = GameObject.FindGameObjectWithTag("World").GetComponent<GameTimer>();
		textmesh = GetComponent<tk2dTextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		string newRemainingText = TimeRemainingText;
		if (textmesh.text != newRemainingText) {
			textmesh.text = TimeRemainingText;
			textmesh.Commit();
		}
	}

	/// <summary>
	/// Gets the time remaining in a readable format.
	/// </summary>
	/// <value>The time remaining text.</value>
	string TimeRemainingText {
		get {
			float timeRemaining = timer.duration - timer.Elapsed();
			int minutes = (int)(timeRemaining / 60);
			int seconds = (int)(timeRemaining) % 60;
			return string.Format("Time Remaining: {0:00}:{1:00}", minutes, seconds);
		}
	}
}
