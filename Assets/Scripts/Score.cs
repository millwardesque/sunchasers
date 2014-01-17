using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds a single score item.
/// </summary>
public class ScoreItem {
	public int BaseScore { get; set; }
	public string Label { get; set; }
	
	public ScoreItem(int baseScore, string label) {
		BaseScore = baseScore;
		Label = label;
	}
	
	public int Score() {
		return BaseScore;
	}
	
	public override string ToString ()
	{
		return string.Format ("{0}: {1}", Label, Score());
	}
}

public class ScoreChangeMessage : Message {
	public Score score;
	public ScoreItem newItem;
	
	public ScoreChangeMessage(GameObject messageSource, string messageName, Score score, ScoreItem newItem) : base(messageSource, messageName, "") {
		this.score = score;
		this.newItem = newItem;
	}
	
	public override string ToString () {
		return string.Format("Score changed: New score is {0}, new item is {1}  ", score.CalculateTotalScore(), newItem);
	}
}

/// <summary>
/// Holds all the player's score items.
/// </summary>
public class Score
{
	private List<ScoreItem> scoreItems = new List<ScoreItem>();
	
	/// <summary>
	/// Add the specified item.
	/// </summary>
	/// <param name='item'>
	/// Item.
	/// </param>
	public void Add(ScoreItem item) {
		scoreItems.Add(item);
	}
	
	public string GetScoreBreakdown() {
		string breakdown = "";
		foreach (ScoreItem score in scoreItems) {
			breakdown += score.ToString() + "\n";
		}
		return breakdown;
	}
	
	public int CalculateTotalScore() {
		int totalScore = 0;
		foreach (var item in scoreItems) {
			totalScore += item.Score();
		}
		
		return totalScore;
	}
}

