using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocalHighScores : MonoBehaviour {
	public int NumberToShow = 5;
	public ScoreKeeper scoreKeeper;

	List<HighScore> scores = new List<HighScore>();
	tk2dTextMesh scoreMesh;

	void Awake () {
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));
	}

	// Use this for initialization
	void Start () {
		scoreMesh = gameObject.GetComponent<tk2dTextMesh>();
		if (!scoreMesh) {
			Debug.LogError("Error setting up local highscores: No text-mesh is available for rendering.");
		}
	}

	/// <summary>
	/// Waits for the player to win the game.
	/// </summary>
	/// <param name="message">Message.</param>
	public void OnGameStateChange(Message message) {
		GameStateChangeMessage realMessage = (GameStateChangeMessage)message;
		if (realMessage.newState == GameStateEnum.PlayerWon) {

			// Load the high scores
			FetchHighScores();

			// Add the new score to the high-score list.
			Add("CPM", scoreKeeper.Score.CalculateTotalScore());

			// Render the final list of high scores.
			RenderScores();

			// Show the high-score list.
			GetComponent<MeshRenderer>().enabled = true;
			MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer mesh in meshes) {
				mesh.enabled = true;
			}
		}
		else { // For all other game states, hide the high-score list.
			if (GetComponent<MeshRenderer>().enabled) {
				GetComponent<MeshRenderer>().enabled = false;
				MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
				foreach (MeshRenderer mesh in meshes) {
					mesh.enabled = false;
				}
			}
		}
	}

	/// <summary>
	/// Attempt to add a highscore to the list.
	/// </summary>
	/// <returns><c>true</c>, if add was attempted, <c>false</c> otherwise.</returns>
	/// <param name="name">Name.</param>
	/// <param name="score">Score.</param>
	void Add(string name, int score) {
		HighScore newScore = new HighScore(score, name);

		// Figure out where to insert the score in the list.
		for (int i = 0; i < scores.Count; ++i) {
			HighScore highScore = scores[i];

			if (newScore.Score > highScore.Score) {
				scores.Insert (i, newScore);
				return;
			}
		}

		// If we're still here, this score was lower than all the others so add it at the end.
		scores.Add (newScore);
	}

	/// <summary>
	/// Retrieves the high scores and loads them in descending order of score.
	/// </summary>
	void FetchHighScores () {
		scores.Clear();

		// @TODO Incorporate level name into highscore keys.

		// If we haven't generated any user-created highscores yet, load the defaults
		if (!PlayerPrefs.HasKey("highscore0_name")) {
			for (int i = 0; i < NumberToShow; ++i) {
				int score = Random.Range(50, 150);
				string name = "Player" + i.ToString();

				// Save default high scores to local storage.
				string prefix = "highscore" + i + "_";
				PlayerPrefs.SetInt (prefix + "score", score);
				PlayerPrefs.SetString (prefix + "name", name);
			}
		}

		// Load the high-scores from local storage.
		for (int i = 0; i < NumberToShow; ++i) {
			string prefix = "highscore" + i + "_";
			if (PlayerPrefs.HasKey(prefix + "name") && PlayerPrefs.HasKey(prefix + "score")) {
				scores.Add (new HighScore(PlayerPrefs.GetInt(prefix + "score"), PlayerPrefs.GetString(prefix + "name")));
			}
		}

		// Order the scores for rendering.
		scores.Sort();
		scores.Reverse();
	}

	/// <summary>
	/// Renders the high score list onto the text mesh.
	/// </summary>
	void RenderScores () {
		string highScoreText = "";

		int count = 0;
		foreach (HighScore score in scores) {
			highScoreText += score.ToString() + "\n";
			count++;
			if (count >= NumberToShow) {
				break;
			}
		}
		scoreMesh.text = string.Format("High Scores\n{0}", highScoreText);
		scoreMesh.Commit();
	}
}
