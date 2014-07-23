using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreBreakdown : MonoBehaviour {
	private tk2dTextMesh scoreMesh;
	
	// Use this for initialization
	void Start () {
		MessageManager.Instance.RegisterListener(new Listener("ScoreChange", gameObject, "OnScoreChange"));
		MessageManager.Instance.RegisterListener(new Listener("GameStateChange", gameObject, "OnGameStateChange"));

		scoreMesh = GetComponent<tk2dTextMesh>();
	}

	public void OnScoreChange(Message message) {
		ScoreChangeMessage realMessage = (ScoreChangeMessage)message;
		scoreMesh.text = string.Format("Score\n{0}", realMessage.score.GetScoreBreakdown());
		scoreMesh.Commit();
	}

	public void OnGameStateChange(Message message) {
		GameStateChangeMessage realMessage = (GameStateChangeMessage)message;
		if (realMessage.newState == GameStateEnum.PlayerWon) {
			GetComponent<MeshRenderer>().enabled = true;

			MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer mesh in meshes) {
				mesh.enabled = true;
			}
		}
		else {
			if (GetComponent<MeshRenderer>().enabled) {
				GetComponent<MeshRenderer>().enabled = false;
				
				MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
				foreach (MeshRenderer mesh in meshes) {
					mesh.enabled = false;
				}
			}
		}
	}
}
