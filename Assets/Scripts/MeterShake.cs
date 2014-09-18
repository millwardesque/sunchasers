using UnityEngine;
using System.Collections;

public enum MeterShakeState {
	AtRest,
	Shaking,
};

public class MeterShake : MonoBehaviour {
	private Vector4 basePosition;
	private float sineFactor = 0.0f;

	public float shakeDistance;
	
	protected MeterShakeState state = MeterShakeState.Shaking;
	public MeterShakeState State {
		get { return state; }
		set {
			if (value == MeterShakeState.AtRest) {
				transform.position = basePosition;
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
		if (State == MeterShakeState.Shaking) {
			sineFactor += Time.deltaTime * 2 * Mathf.PI * 5;
			transform.position = new Vector3(transform.position.x + Mathf.Sin(sineFactor) * shakeDistance, transform.position.y, transform.position.z);

		}
	}
	
	/// <summary>
	/// Triggers a camera shake.
	/// </summary>
	public void TriggerShake() {
		if (State != MeterShakeState.Shaking) {
			State = MeterShakeState.Shaking;
		}
	}
}
