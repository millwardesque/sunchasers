using UnityEngine;
using System.Collections;

public class ScorePanel : MonoBehaviour {
	private CanvasGroup canvasGroup;

	public GameObject LocalHighScorePanel;
	private CanvasGroup cnvLocalHighScorePanel;

	public GameObject ScoreBreakdownPanel;
	private CanvasGroup cnvScoreBreakdownPanel;

	private bool visible = true;

	void Awake () {
		// Save the canvas group for later.
		canvasGroup = GetComponent<CanvasGroup>();
		if (!canvasGroup) {
			throw new UnityException("Error setting up score panel: No canvas group is attached to the main panel.");
		}

		// Save the sub-panels canvases for later.
		if (!LocalHighScorePanel) {
			throw new UnityException("Error setting up score panel: No local highscore panel is set.");
		}
		else if (!LocalHighScorePanel.GetComponent<CanvasGroup>()) {
			throw new UnityException("Error setting up score panel: The local highscore panel doesn't have a canvas group.");
		}
		cnvLocalHighScorePanel = LocalHighScorePanel.GetComponent<CanvasGroup>();

		if (!ScoreBreakdownPanel) {
			throw new UnityException("Error setting up score panel: No score breakdown panel is set.");
		}
		else if (!ScoreBreakdownPanel.GetComponent<CanvasGroup>()) {
			throw new UnityException("Error setting up score panel: The score breakdown doesn't have a canvas group.");
		}
		cnvScoreBreakdownPanel = ScoreBreakdownPanel.GetComponent<CanvasGroup>();

		// Register the appropriate event listeners.
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.L)) {
			GameObject.FindGameObjectWithTag("World").GetComponent<GameState>().State = GameStateEnum.PlayerLost;
		}
		else if (Input.GetKeyDown(KeyCode.W)) {
			GameObject.FindGameObjectWithTag("World").GetComponent<GameState>().State = GameStateEnum.PlayerWon;
		}
	}
	
	/// <summary>
	/// Called whenever the game state changes
	/// </summary>
	/// <param name="message">Message.</param>
	public void OnGameStateChange(Message message) {
		GameStateChangeMessage realMessage = (GameStateChangeMessage)message;

		if (realMessage.newState == GameStateEnum.PlayerWon) {		// Show the scores if the player has won.
			ShowScoreBreakdownPanel();
			Show();
		}
		else if (realMessage.newState == GameStateEnum.PlayerLost) {
			ShowLocalHighScorePanel();
			Show ();
		}
		else {
			Hide ();
		}
	}

	/// <summary>
	/// Shows the score panel and its default child panel.
	/// </summary>
	public void Show() {
		canvasGroup.alpha = 1.0f;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.interactable = true;

		visible = true;
	}

	/// <summary>
	/// Hides the score panel.
	/// </summary>
	public void Hide() {
		canvasGroup.alpha = 0.0f;
		canvasGroup.blocksRaycasts = false;
		canvasGroup.interactable = false;
		visible = false;
	}

	/// <summary>
	/// Shows the score breakdown panel the next time the score panel is visible / open.
	/// </summary>
	private void ShowScoreBreakdownPanel() {
		// Hide all internal panels except for the score breakdown panel
		CanvasGroup[] childCanvases = GetComponentsInChildren<CanvasGroup>();
		foreach (CanvasGroup cnvChild in childCanvases) {
			if (cnvChild == canvasGroup) {
				continue;
			}
			
			cnvChild.alpha = 0.0f;
			cnvChild.blocksRaycasts = false;
			cnvChild.interactable = false;
		}
		
		cnvScoreBreakdownPanel.alpha = 1.0f;
		cnvScoreBreakdownPanel.blocksRaycasts = true;
		cnvScoreBreakdownPanel.interactable = true;
	}

	/// <summary>
	/// Shows the score breakdown panel the next time the score panel is visible / open.
	/// </summary>
	private void ShowLocalHighScorePanel() {
		// Hide all internal panels except for the local highscore panel
		CanvasGroup[] childCanvases = GetComponentsInChildren<CanvasGroup>();
		foreach (CanvasGroup cnvChild in childCanvases) {
			if (cnvChild == canvasGroup) {
				continue;
			}
			
			cnvChild.alpha = 0.0f;
			cnvChild.blocksRaycasts = false;
			cnvChild.interactable = false;
		}
		
		cnvLocalHighScorePanel.alpha = 1.0f;
		cnvLocalHighScorePanel.blocksRaycasts = true;
		cnvLocalHighScorePanel.interactable = true;
	}

	public void Toggle() {
		if (visible) {
			Hide();
		}
		else {
			Show();
		}
	}
}
