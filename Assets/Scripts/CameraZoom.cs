using UnityEngine;
using System.Collections;

public class CameraZoom : MonoBehaviour {
	public Camera gameCamera;

	private tk2dCamera camera2d;
	private bool isZooming = false;
	private float t = 0.0f;	// Interpolation value.
	private float startZoom = 1;
	private float endZoom = 1;
	private Vector3 startPosition;
	private Vector3 endPosition;
	private float zoomDuration = 1.0f;

	// Use this for initialization
	void Start () {
		if (gameCamera) {
			camera2d = gameCamera.GetComponent<tk2dCamera>();
			if (!camera2d) {
				Debug.LogWarning("Problem starting CameraZoom script: There's no tk2dCamera attached the camera. Pan will work, but zoom won't.");
			}
		}
		else {
			Debug.LogWarning("Problem starting CameraZoom script: There's no game camera attached to the script.");
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (isZooming) {
			t += Time.deltaTime / zoomDuration;

			if (camera2d) {
				camera2d.ZoomFactor = Mathf.Lerp(startZoom, endZoom, t);
			}

			if (gameCamera) {
				gameCamera.transform.position = Vector3.Lerp(startPosition, endPosition, t) + new Vector3(0, 0, gameCamera.transform.position.z);
			}
		}

		if (Input.GetKeyUp(KeyCode.Z)) {
			ZoomCamera (new Vector2(5.0f, 5.0f), new Vector2(), 5, 1, 1);
			Debug.Log ("Manual zoom requested");
		}
	}

	public void ZoomCamera(Vector2 startPosition, Vector2 endPosition, float startZoom, float endZoom, float zoomDuration) {
		if (isZooming) {
			t = 0.0f;
		}

		isZooming = true;
		this.startPosition = startPosition;
		this.endPosition = endPosition;
		this.startZoom = startZoom;
		this.endZoom = endZoom;
		this.zoomDuration = zoomDuration;
	}
}
