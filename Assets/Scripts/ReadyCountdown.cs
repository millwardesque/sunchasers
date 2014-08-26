using UnityEngine;
using System.Collections;

/// <summary>
/// Game state change message.
/// </summary>
public class ReadyCountdownFinishedMessage : Message {

	public ReadyCountdownFinishedMessage(GameObject messageSource) : base(messageSource, "ReadyCountdownFinishedMessage"	, "") { }
	
	public override string ToString () {
		return string.Format("Name: {0} From: {1} ", MessageName, MessageSource);
	}
}

public class ReadyCountdown : MonoBehaviour {
	public int CountdownStart = 3;
	public tk2dTextMesh CountdownText;

	private bool isRunning = false;

	private float currentCountdown;
	private int displayCountdown;
	
	private RestartLevel restartLevel;
	
	// Use this for initialization
	void Start () {
		if (!CountdownText) {
			Debug.LogWarning("Error starting ReadyCountdown: No tk2dTextMesh is assigned to the ReadyCountdown gameobject.");
		}

		GameObject world = GameObject.FindGameObjectWithTag("World");
		if (!world) {
			Debug.LogWarning ("Error starting ReadyCountdown: No GameObject is tagged with World");
		}
		else {
			restartLevel = world.GetComponent<RestartLevel>();
			if (!restartLevel) {
				Debug.LogWarning ("Error starting ReadyCountdown: World GameObject doesn't have RestartLevel script");
			}
		}

		currentCountdown = (float)CountdownStart;
		isRunning = false;
		UpdateCountdownDisplay();
	}
	
	// Update is called once per frame
	void Update () {
		if (!isRunning) {
			return;
		}
		else if (currentCountdown > 0.0f) {
			currentCountdown -= Time.deltaTime;

			UpdateCountdownDisplay();
		}
		else {
			MessageManager.Instance.SendToListeners(new ReadyCountdownFinishedMessage(gameObject));

			// Disable the countdown.
			StopCountdown();
		}
	}

	/// <summary>
	/// Updates the displayed count variable and the textmesh.
	/// </summary>
	void UpdateCountdownDisplay() {
		int newDisplay = Mathf.CeilToInt(currentCountdown); // Round instead of floor so that the first countdown number appears onscreen slightly longer. This effectively makes the countdown last for CountdownStart + 0.5 seconds.
		if (newDisplay != displayCountdown) {
			displayCountdown = newDisplay;

			if (CountdownText) {
				CountdownText.text = string.Format ("Ready... {0}", displayCountdown);
				CountdownText.Commit();
			}
		}
	}

	/// <summary>
	/// Starts the countdown.
	/// </summary>
	public void StartCountdown() {
		isRunning = true;
		CountdownText.gameObject.renderer.enabled = true;
		currentCountdown = (float)CountdownStart;
	}

	/// <summary>
	/// Stops the countdown.
	/// </summary>
	public void StopCountdown() {
		CountdownText.gameObject.renderer.enabled = false;
		isRunning = false;
	}
}
