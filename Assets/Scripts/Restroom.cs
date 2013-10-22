using UnityEngine;
using System.Collections;

public class Restroom : GridComponent {
	public float BladderDecreaseRate = 5.0f;
	public float RelaxationDecreaseRate = 0.1f;
	public float HungerIncreaseRate = 0.5f;
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
		player.Relaxation -= RelaxationDecreaseRate * Time.deltaTime;
		player.Bladder -= BladderDecreaseRate * Time.deltaTime;
		player.Hunger += HungerIncreaseRate * Time.deltaTime;
		
		if (player.Bladder <= Mathf.Epsilon) {
			player.ChangeState(ActorState.Upright);
		}
	}
}
