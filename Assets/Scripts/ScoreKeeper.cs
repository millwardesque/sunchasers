﻿using UnityEngine;
using System.Collections;

public class ScoreKeeper : MonoBehaviour {
	private Score score;
	
	public Score Score {
		get { return score; }
	}
	
	// Use this for initialization
	void Start () {
		score = new Score();
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));
	}

	public void Add (ScoreItem item) {
		if (item != null) {
			score.Add(item);
			MessageManager.Instance.SendToListeners(new ScoreChangeMessage(gameObject, "ScoreChange", score, item));
		}
	}

	public void Reset() {
		score.Reset ();
		MessageManager.Instance.SendToListeners(new ScoreChangeMessage(gameObject, "ScoreChange", score, null));
	}
}
