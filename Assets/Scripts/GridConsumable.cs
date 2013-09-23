using UnityEngine;
using System.Collections;

public class GridConsumable : MonoBehaviour {
	private MovementGrid movementGridScript = null;
	public GridCoordinates Location;

	// Use this for initialization
	void Start () {
		movementGridScript = GameObject.FindGameObjectWithTag("Movement Grid").GetComponent<MovementGrid>();
		movementGridScript.SetConsumable(Location, this);
		
		// Let subclasses initialize too.
		OnStart();
	}
	
	protected virtual void OnStart() { }
	
	/// <summary>
	/// Called when the consumable is used.
	/// </summary>
	public virtual void OnUse() { }
}
