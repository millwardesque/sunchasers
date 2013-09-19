using UnityEngine;
using System.Collections;

public class Sunrise : MonoBehaviour {
	private float radians = 0.0f;
	private float maxHeight = 0.01f;
	private float rateOfClimb = 0.05f;
	
	// Update is called once per frame
	void Update () {
		radians += Time.deltaTime * rateOfClimb;
		while (radians > 2 * Mathf.PI) {
			radians -= 2 * Mathf.PI;
		}
		
		transform.Translate(new Vector3(0, maxHeight * Mathf.Sin(radians), 0));
	}
}
