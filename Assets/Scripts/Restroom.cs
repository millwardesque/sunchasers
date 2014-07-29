using UnityEngine;
using System.Collections;

public class Restroom : GridComponent {
	public float BladderDecreaseRate = 5.0f;
	public float RelaxationDecreaseRate = 0.1f;
	public float HungerIncreaseRate = 0.5f;
	private PlayerController player;
	private tk2dSpriteAnimator animator;
	
	
	/// <summary>
	/// GridComponent's OnStart hook.
	/// </summary>
	protected override void OnStart() {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		animator = (tk2dSpriteAnimator)GetComponent("tk2dSpriteAnimator");
	}
	
	/// <summary>
	/// Called during update whenever the component is active.
	/// </summary>
	public override void OnUpdate() {
		player.Relaxation -= RelaxationDecreaseRate * Time.deltaTime;
		player.Bladder -= BladderDecreaseRate * Time.deltaTime;
		player.Hunger += HungerIncreaseRate * Time.deltaTime;
		
		if (player.Bladder <= Mathf.Epsilon) {
			animator.AnimationCompleted = openAndCloseDoorCompleteDelegate;
			this.openAndCloseDoor();
		}
	}

	public void openAndCloseDoor() {
		if (animator && !animator.IsPlaying("Open and close")) {
			animator.Play("Open and close");
		}
	}

	// This is called once the hit animation has compelted playing
	// It returns to playing whatever animation was active before hit
	// was playing.
	void openAndCloseDoorCompleteDelegate(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip) {
		player.ChangeState (ActorState.Upright);
		animator.AnimationCompleted = null;
	}
}
