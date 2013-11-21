using UnityEngine;
using System.Collections;

public class Sunshine : MonoBehaviour {
	private Light sunlight;
	private GameTimer gameTime;
	private Color fromColour;
	private Color toColour;

	public Color DawnColour;
	public Color MiddayColour;
	public Color DuskColour;

	// Use this for initialization
	void Start () {
		sunlight = GetComponent<Light>();
		gameTime = GameObject.FindGameObjectWithTag("World").GetComponent<GameTimer>();
	}
	
	// Update is called once per frame
	void Update () {
		float t = gameTime.Elapsed() / gameTime.duration;

		if (t < 0.5f) {
			fromColour = DawnColour;
			toColour = MiddayColour;
			t /= 0.5f;
		}
		else {
			fromColour = MiddayColour;
			toColour = DuskColour;
			t = (t - 0.5f) / 0.5f;
		}

		sunlight.color = Color.Lerp(fromColour, toColour, t);
	}
}
