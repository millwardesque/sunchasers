﻿using UnityEngine;
using System.Collections;

/// <summary>
/// States a game can be in.
/// </summary>
public enum GameStateEnum {
	WaitingToStart,
	Paused,
	Running,
	PlayerWon,
	PlayerLost
};

/// <summary>
/// Game state change message.
/// </summary>
public class GameStateChangeMessage : Message {
	public GameStateEnum oldState;
	public GameStateEnum newState;
	
	public GameStateChangeMessage(GameObject messageSource, string messageName, GameStateEnum oldState, GameStateEnum newState) : base(messageSource, messageName, "") {
		this.oldState = oldState;
		this.newState = newState;
	}
	
	public override string ToString () {
		 return string.Format("Name: {0} Old: {1} New: {2} From: {3} ", MessageName, oldState, newState, MessageSource);
	}
}

/// <summary>
/// Controls the game state.
/// </summary>
public class GameState : MonoBehaviour {
	private GameStateEnum state = GameStateEnum.WaitingToStart;
	public GameObject PausedText;
	public GameStateEnum State {
		get { return state; }
		set {
			GameStateEnum oldState = state;
			state = value;

			MessageManager.Instance.SendToListeners(new GameStateChangeMessage(gameObject, "GameStateChange", oldState, state));
		}
	}


	
	/// <summary>
	/// Start hook.
	/// </summary>
	public void Start() {
		State = GameStateEnum.WaitingToStart;
		gameObject.GetComponent<ReadyCountdown>().StartCountdown();

		// Start by zooming out from the player 
		PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		MovementGrid grid = GameObject.FindGameObjectWithTag("Movement Grid").GetComponent<MovementGrid>();
		Vector2 startPosition = player.CurrentSquare.PixelCoords + new Vector2(grid.transform.position.x, grid.transform.position.y);
		gameObject.GetComponent<CameraZoom>().ZoomCamera (startPosition, new Vector2(0, 0), 3, 1, 1.0f);
	}
	
	public void Update() {
		if (Input.GetKeyUp(KeyCode.P)) {
			if (State == GameStateEnum.Running) {
				State = GameStateEnum.Paused;
				PausedText.SetActive(true);
			}
			else if (State == GameStateEnum.Paused) {
				State = GameStateEnum.Running;
				PausedText.SetActive(false);
			}
		}
	}
}
