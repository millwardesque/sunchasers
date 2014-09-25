using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ScoreBreakdown : MonoBehaviour {
	private Text scoreText;
	
	// Use this for initialization
	void Start () {
		MessageManager.Instance.RegisterListener(new Listener("ScoreChange", gameObject, "OnScoreChange"));
		scoreText = gameObject.GetComponent<Text>();

		if (!scoreText) {
			Debug.LogError("Error setting up score breakdown: No text is available for rendering.");
		}
	}

	public void OnScoreChange(Message message) {
		ScoreChangeMessage realMessage = (ScoreChangeMessage)message;

		if (scoreText) {
			scoreText.text = realMessage.score.GetScoreBreakdown();
		}
	}
}
