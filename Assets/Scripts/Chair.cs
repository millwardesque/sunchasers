using UnityEngine;
using System.Collections;

public class Chair : GridComponent {
	public float BladderIncreaseRate = 1.5f;
	public float RelaxationIncreaseRate = 1.0f;
	public float HungerIncreaseRate = 0.75f;
	
	private PlayerController player;
	
	/// <summary>
	/// GridComponent's OnStart hook.
	/// </summary>
	protected override void OnStart() {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
	}
	
	/// <summary>
	/// Called during update whenever the component is active.
	/// </summary>
	public override void OnUpdate() {
		player.Relaxation += RelaxationIncreaseRate * Time.deltaTime;
		
		player.Hunger += HungerIncreaseRate * Time.deltaTime;
		if (Mathf.Abs(player.Hunger - 100.0f) <= Mathf.Epsilon) {
			player.Relaxation = 0.0f;
			player.ChangeState(PlayerState.Upright);
		}
		
		player.Bladder += BladderIncreaseRate * Time.deltaTime;
		if (Mathf.Abs(player.Bladder - 100.0f) <= Mathf.Epsilon) {
			player.Relaxation = 0.0f;
			player.ChangeState(PlayerState.Upright);
		}
	}
}
