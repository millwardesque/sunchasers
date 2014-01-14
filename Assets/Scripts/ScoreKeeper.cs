using UnityEngine;
using System.Collections;

public class ScoreKeeper : MonoBehaviour {
	private Score score;
	private tk2dTextMesh textmesh;

	public Score Score {
		get { return score; }
	}

	string ScoreText {
		get {
			return string.Format("Score: {0}", Score.CalculateTotalScore());
		}
	}

	// Use this for initialization
	void Start () {
		score = new Score();
		textmesh = GetComponent<tk2dTextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		string newText = ScoreText;
		if (textmesh.text != newText) {
			textmesh.text = newText;
			textmesh.Commit();
		}
	}
}
