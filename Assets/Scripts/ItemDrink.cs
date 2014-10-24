using UnityEngine;
using System.Collections;

public class ItemDrink : GridConsumable {
	public float BladderChange= 10.0f;
	public float RelaxationChange = 15.0f;
	public float HungerChange = -5.0f;
	
	private PlayerController player = null;

	/// <summary>
	/// GridConsumable's OnStart hook.
	/// </summary>
	protected override void OnStart() {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		if (player == null) {
			Debug.LogError("Unable to start drink item: Unable to find GameObject with Player tag, or Player GameObject doesn't have PlayerController component.");
		}
	}
	
	/// <summary>
	/// Called when the consumable is used.
	/// </summary>
	public override void OnUse() {
		if (!player) {
			player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		}
		player.Relaxation += RelaxationChange;
		player.Bladder += BladderChange;
		player.Hunger += HungerChange;

		base.OnUse ();
	}
}
