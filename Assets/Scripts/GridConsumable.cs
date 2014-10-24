using UnityEngine;
using System.Collections;

public class GridConsumable : MonoBehaviour {
	private MovementGrid movementGridScript = null;
	private ScoreKeeper score = null;
	public GridCoordinates Location;
	public int ScoreValue;

	// Use this for initialization
	void Start () {
		movementGridScript = GameObject.FindGameObjectWithTag("Movement Grid").GetComponent<MovementGrid>();
		if (movementGridScript == null) {
			Debug.LogError("Unable to start GridConsumable: Unable to find GameObject with Movement Grid tag, or Movement Grid GameObject doesn't have MovementGrid component.");
		}
		movementGridScript.SetConsumable(Location, this);

		score = GameObject.FindGameObjectWithTag("ScoreKeeper").GetComponent<ScoreKeeper>();
		if (score == null) {
			Debug.LogError("Unable to start drink item: Unable to find GameObject with ScoreKeeper tag, or ScoreKeeper GameObject doesn't have ScoreKeeper component.");
		}
		
		// Let subclasses initialize too.
		OnStart();
	}
	
	protected virtual void OnStart() { }
	
	/// <summary>
	/// Called when the consumable is used.
	/// </summary>
	public virtual void OnUse() { 
		Debug.Log ("USING!");
		score.Add (new ScoreItem(ScoreValue, "Item"));
	}
}
