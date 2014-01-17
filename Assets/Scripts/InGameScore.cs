using UnityEngine;
using System.Collections;

public class InGameScore : MonoBehaviour {
	private tk2dTextMesh scoreMesh;
	
	// Use this for initialization
	void Start () {
		MessageManager.Instance.RegisterListener(new Listener("ScoreChange", gameObject, "OnScoreChange"));
		scoreMesh = GetComponent<tk2dTextMesh>();
	}
	
	public void OnScoreChange(Message message) {
		ScoreChangeMessage realMessage = (ScoreChangeMessage)message;
		scoreMesh.text = string.Format("Score: {0}", realMessage.score.CalculateTotalScore());
		scoreMesh.Commit();
	}
}
