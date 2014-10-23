using UnityEngine;
using System.Collections;

public class TargetedSquare : MonoBehaviour {
	GameObject movementGrid;
	public Vector2 positionOffset;	// Offset applied to the calculated grid positions in order to get the sprite in just the right spot.

	void Start() {
		MessageManager.Instance.RegisterListener(new Listener("TargetChangeFromTap", gameObject, "OnTargetChangeFromTap"));
		movementGrid = GameObject.FindGameObjectWithTag("Movement Grid");
	}

	public void OnTargetChangeFromTap(Message message) {
		TargetChangeFromTapMessage realMessage = (TargetChangeFromTapMessage)message;

		if (realMessage.newTarget != null) {
			this.renderer.enabled = true;
			this.transform.position = CalculateSquarePosition(realMessage.newTarget);
		}
		else {
			this.renderer.enabled = false;
		}
	}

	Vector3 CalculateSquarePosition(GridSquare square) {
		if (square == null) {
			return new Vector3();
		}
		
		float newX = positionOffset.x + movementGrid.transform.position.x + (float)square.X;
		float newY = positionOffset.y + movementGrid.transform.position.y + (float)square.Y;
		return new Vector3(newX, newY, transform.position.z);
	}
}
