using UnityEngine;
using System.Collections;
using System;

public enum CameraShakeState {
	AtRest,
	Shaking,
};

public class CameraShake : MonoBehaviour {
	private Vector4 basePosition;
	private float shakeElapsed;
	private float sineFactor = 0.0f;

	public float shakeDuration;
	public float shakeDistance;
	public float numLoops;

	protected CameraShakeState state;
	public CameraShakeState State {
		get { return state; }
		set {
			if (value == CameraShakeState.AtRest) {
				transform.position = basePosition;
				shakeElapsed = 0.0f;
				sineFactor = 0.0f;
			}

			state = value; 
		}
	}

	// Use this for initialization
	void Start () {
		basePosition = transform.position;	
	}
	
	// Update is called once per frame
	void Update () {
		if (State == CameraShakeState.Shaking) {
			shakeElapsed += Time.deltaTime;
			if (shakeElapsed >= shakeDuration) {
				State = CameraShakeState.AtRest;
			}
			else {
				sineFactor += Time.deltaTime * 2 * Mathf.PI * numLoops / (shakeDuration);
				transform.position = new Vector3(transform.position.x + Mathf.Sin(sineFactor) * shakeDistance, transform.position.y, transform.position.z);
			}
		}
		else if (State == CameraShakeState.AtRest) { // @DEBUG. Manually trigger a camera shake.
			if (Input.GetKeyUp(KeyCode.S)) {
				Debug.Log ("Shaking camera.");
				TriggerShake();
			}
		}
	}

	/// <summary>
	/// Triggers a camera shake.
	/// </summary>
	public void TriggerShake() {
		if (State != CameraShakeState.Shaking) {
			State = CameraShakeState.Shaking;
		}
	}
}
