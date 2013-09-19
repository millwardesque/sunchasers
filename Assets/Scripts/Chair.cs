using UnityEngine;
using System.Collections;

public class Chair : GridComponent {
	public float BladderIncreaseRate = 1.5f;
	public float RelaxationIncreaseRate = 1.0f;
	private PlayerController player;
	
	/// <summary>
	/// GridComponent's OnStart hook.
	/// </summary>
	protected override void OnStart() {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
	}
	
	/// <summary>
	/// Called when the component is activated.
	/// </summary>
	public override void OnUpdate() {
		player.Relaxation += RelaxationIncreaseRate * Time.deltaTime;
		
		player.Bladder += BladderIncreaseRate * Time.deltaTime;
		if (Mathf.Abs(player.Bladder - 100.0f) <= Mathf.Epsilon) {
			player.Relaxation = 0.0f;
			player.ChangeState(PlayerState.Upright);
		}
	}
}
