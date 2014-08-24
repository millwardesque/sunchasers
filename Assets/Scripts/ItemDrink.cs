using UnityEngine;
using System.Collections;

public class ItemDrink : GridConsumable {
	public float BladderChange= 10.0f;
	public float RelaxationChange = 15.0f;
	public float HungerChange = -5.0f;

	private PlayerController player;

	/// <summary>
	/// GridConsumable's OnStart hook.
	/// </summary>
	protected override void OnStart() {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
	}
	
	/// <summary>
	/// Called when the consumable is used.
	/// </summary>
	public override void OnUse() {
		player.Relaxation += RelaxationChange;
		player.Bladder += BladderChange;
		player.Hunger += HungerChange;
	}
}
