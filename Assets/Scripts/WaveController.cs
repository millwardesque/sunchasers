using UnityEngine;
using System.Collections;

public class WaveController : MonoBehaviour {
	private float timeToRegenerate = 0.0f;	// Time left until the next wave is generated.
	private GameState gameState;

	public GameObject WavePrototype;	// Wave prototype to generate.
	public int MaxWaves = 1;	// Number of active waves at one time. 0 for infinite.
	public float RegenerationRate = 4.0f;	// New wave generation rate (in seconds).
	public Vector2 ScaleRange = new Vector2(1.0f, 1.0f);	// Range of the wave size scaler.
	public Vector2 OceanDimensions = new Vector2(55.0f, 20.0f);	// Top-right coordinates of the bounding box of the ocean. Assumes a (0, 0) bottom-left.


	// Use this for initialization
	void Start () {
		gameState = GameObject.FindGameObjectWithTag("World").GetComponent<GameState>();
	}
	
	// Update is called once per frame
	void Update () {
		if (gameState.State != GameStateEnum.Running) {
			return;
		}

		if (MaxWaves == 0 || CountActiveWaves() < MaxWaves) {
			timeToRegenerate -= Time.deltaTime;
		}

		if (timeToRegenerate <= 0.0f) {
			timeToRegenerate = RegenerationRate;

			// Generate the wave position (bottom left).
			Vector3 newPosition = new Vector3(Random.Range(0, OceanDimensions.x), Random.Range (0, OceanDimensions.y), 0);

			// Change the scale of the wave a bit to offer some variation.
			float scaleFactor = Random.Range(ScaleRange.x, ScaleRange.y);
			Vector3 newScale = new Vector3(scaleFactor, scaleFactor, 1.0f);

			// Create the wave.
			GameObject newItem = (GameObject)GameObject.Instantiate(WavePrototype);
			newItem.transform.parent = gameObject.transform;
			newItem.transform.localPosition = newPosition;
			newItem.transform.localScale = newScale;
		}
	}

	/// <summary>
	/// Counts the number of active waves in the game.
	/// </summary>
	/// <returns>
	/// The number of active waves in the game
	/// </returns>
	int CountActiveWaves() {
		return GameObject.FindGameObjectsWithTag("Wave").Length;	
	}
}
