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

	/// <summary>
	/// Resets the player's score.
	/// </summary>
	public void Reset() {
		scoreItems.Clear();
	}

	public string GetScoreBreakdown() {
		string breakdown = "";
		Dictionary<string, List<ScoreItem> > scoreTypes = new Dictionary<string, List<ScoreItem> >();

		// Group the different score items by type.
		foreach (ScoreItem score in scoreItems) {
			if (!scoreTypes.ContainsKey(score.Label)) {
				scoreTypes.Add (score.Label, new List<ScoreItem>());
			}
			scoreTypes[score.Label].Add (score);
		}

		// @TODO Sort scores alphabetically by type.

		// List the different types.
		foreach (string key in scoreTypes.Keys) {
			int subscore = 0;
			foreach (ScoreItem item in scoreTypes[key]) {
				subscore += item.Score();
			}

			if (scoreTypes[key].Count > 1) {
				breakdown += string.Format("{0} x{1}: {2}\n", key, scoreTypes[key].Count, subscore);
			}
			else {
				breakdown += string.Format("{0}: {1}\n", key, subscore);
			}
		}

		// Add the total score.
		int totalScore = CalculateTotalScore();
		breakdown += string.Format("##########\n");
		breakdown += string.Format("Total Score: {0}", totalScore);

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

