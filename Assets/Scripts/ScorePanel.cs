using UnityEngine;
using System.Collections;

public class ScorePanel : MonoBehaviour {
	private CanvasGroup canvasGroup;
	public GameObject DefaultWinPanel;
	private CanvasGroup cnvDefaultWinPanel;

	void Awake () {
		// Save the canvas group for later.
		canvasGroup = GetComponent<CanvasGroup>();
		if (!canvasGroup) {
			throw new UnityException("Error setting up score panel: No canvas group is attached to the main panel.");
		}

		// Save the default win panels's canvas for later.
		if (!DefaultWinPanel) {
			throw new UnityException("Error setting up score panel: No default win-panel is set.");
		}
		else if (!DefaultWinPanel.GetComponent<CanvasGroup>()) {
			throw new UnityException("Error setting up score panel: The default win-panel doesn't have a canvas group.");
		}
		cnvDefaultWinPanel = DefaultWinPanel.GetComponent<CanvasGroup>();

		// Register the appropriate event listeners.
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));
	}
	
	/// <summary>
	/// Called whenever the game state changes
	/// </summary>
	/// <param name="message">Message.</param>
	public void OnGameStateChange(Message message) {
		GameStateChangeMessage realMessage = (GameStateChangeMessage)message;

		if (realMessage.newState == GameStateEnum.PlayerWon) {		// Show the scores if the player has won.
			Show();
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

		// Hide all internal panels except for the default win panel
		CanvasGroup[] childCanvases = GetComponentsInChildren<CanvasGroup>();
		foreach (CanvasGroup cnvChild in childCanvases) {
			if (cnvChild == canvasGroup) {
				continue;
			}

			cnvChild.alpha = 0.0f;
			cnvChild.blocksRaycasts = false;
			cnvChild.interactable = false;
		}

		cnvDefaultWinPanel.alpha = 1.0f;
		cnvDefaultWinPanel.blocksRaycasts = true;
		cnvDefaultWinPanel.interactable = true;
	}

	/// <summary>
	/// Hides the score panel.
	/// </summary>
	public void Hide() {
		canvasGroup.alpha = 0.0f;
		canvasGroup.blocksRaycasts = false;
		canvasGroup.interactable = false;
	}
}
