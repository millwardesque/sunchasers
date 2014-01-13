using UnityEngine;
using System.Collections;

/// <summary>
/// Controls a single wave game object.
/// </summary>
public class Wave : MonoBehaviour {
	GameState gameState;
	Vector3 start_position;
	tk2dSprite sprite;

	float startAtBottomOffset = Mathf.PI;	// Offset to make the wave start at the bottom of the cycle
	float currentLifetime = 0.0f;	// Current lifetime of the wave.

	public float amplitude = 1.0f;	// Amplitude of the wave.
	public float frequency = 1.0f;	// Frequency of the wave cycle in cycles / second.
	public float horizontalSpeed = 1.0f;	// Speed of the wave in game units / second.
	public float lifetime = 2.0f;	// Lifetime of the wave in cycles. If loopForever is false, the wave is destroyed when its lifetime is up.
	public bool loopForever = false;	// If true, the lifetime parameter is ignored and the wave loops forever.
		
	public bool IsActive {
		get { return loopForever || (currentLifetime < lifetime / frequency); }
	}

	// Use this for initialization
	void Start () {
		gameState = GameObject.FindGameObjectWithTag("World").GetComponent<GameState>();
		start_position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		sprite = GetComponent<tk2dSprite>();
				
		// Change the alpha channel to be invisible at the start.
		sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0.0f);
	}
	
	// Update is called once per frame
	void Update () {
		if (gameState.State != GameStateEnum.Running) {
			return;
		}

		if (!IsActive) {
			GameObject.Destroy(gameObject);
			return;
		}

		currentLifetime += Time.deltaTime;
		float curveChange = Mathf.Cos(currentLifetime * frequency * 2 * Mathf.PI + startAtBottomOffset) * amplitude;

		// Change the wave position.
		float xChange = Time.deltaTime * horizontalSpeed;
		float yChange = start_position.y + curveChange - transform.position.y;
		transform.Translate(xChange, yChange, 0);

		// Change the alpha channel to be more visible at the height of the curve.
		float alpha = (curveChange / (amplitude * 2.0f)) + 0.5f;	// Take the amplitude out of the alpha equation, and shift the range of values to (0, 1)
		sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);
	}

	/// <summary>
	/// Reset the wave to its default parameters.
	///
	public void Reset() {
		start_position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0.0f);
		currentLifetime = 0.0f;	// Current lifetime of the wave.
	}
}
