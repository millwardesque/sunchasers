using UnityEngine;
using System;
using System.Collections.Generic;

public class HighScore : IComparable<HighScore> {
	string name;
	int score = 0;

	public int Score {
		get { return score; }
	}

	public string Name {
		get { return name; }
	}

	public HighScore(int score, string name) {
		this.name = name;
		this.score = score;
	}

	public override string ToString() {
		return string.Format ("{0}: {1}", Name, Score);
	}

	public int CompareTo(HighScore other) {
		if (other == null) {
			return 1;
		}

		return Score.CompareTo(other.Score);
	}
}
