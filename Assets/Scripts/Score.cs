using System;
using System.Collections;
using System.Collections.Generic;

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
	
	public int CalculateTotalScore() {
		int totalScore = 0;
		foreach (var item in scoreItems) {
			totalScore += item.Score();
		}
		
		return totalScore;
	}
}

