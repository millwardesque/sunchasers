using UnityEngine;
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
		State = GameStateEnum.Running;
	}
	
	public void Update() {
		if (Input.GetKeyUp(KeyCode.P)) {
			if (State == GameStateEnum.Running) {
				State = GameStateEnum.Paused;	
			}
			else if (State == GameStateEnum.Paused) {
				State = GameStateEnum.Running;	
			}
		}
	}
}
