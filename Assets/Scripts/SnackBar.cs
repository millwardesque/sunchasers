using UnityEngine;
using System.Collections;

public class SnackBar : GridComponent {

	public float BladderIncreaseRate = 0.5f;
	public float RelaxationDecreaseRate = 0.1f;
	public float HungerDecreaseRate = 3.0f;
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
		player.Bladder += BladderIncreaseRate * Time.deltaTime;
		player.Hunger -= HungerDecreaseRate * Time.deltaTime;
		if (player.Hunger <= Mathf.Epsilon) {
			player.ChangeState(PlayerState.Upright);
		}
	}
}
