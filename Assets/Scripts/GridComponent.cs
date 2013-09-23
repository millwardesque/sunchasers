using UnityEngine;
using System.Collections;

public class GridComponent : MonoBehaviour {
	public GridCoordinates[] EntryPoints;
	private MovementGrid movementGridScript = null;

	// Use this for initialization
	void Start () {
		movementGridScript = GameObject.FindGameObjectWithTag("Movement Grid").GetComponent<MovementGrid>();
		
		for (int i = 0; i < EntryPoints.Length; ++i) {
			movementGridScript.SetComponent(EntryPoints[i], this);
		}
		
		// Let subclasses initialize too.
		OnStart();
	}
	
	protected virtual void OnStart() { }
	
	/// <summary>
	/// Called when the component is active during the Unity Update hook.
	/// </summary>
	public virtual void OnUpdate() { }
}
