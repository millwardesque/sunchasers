using UnityEngine;
using System.Collections;

public class Restroom : GridComponent {
	public float BladderDecreaseRate = 5.0f;
	public float RelaxationDecreaseRate = 0.1f;
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
		player.Relaxation -= RelaxationDecreaseRate * Time.deltaTime;
		player.Bladder -= BladderDecreaseRate * Time.deltaTime;
		if (player.Bladder <= Mathf.Epsilon) {
			player.ChangeState(PlayerState.Upright);
		}
	}
}
