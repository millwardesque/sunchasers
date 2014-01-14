using UnityEngine;
using System.Collections;

public class PlayerStatsUI : MonoBehaviour {
	private PlayerController player;

	public tk2dClippedSprite relaxationProgress;
	public tk2dClippedSprite bladderProgress;
	public tk2dClippedSprite hungerProgress;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
		relaxationProgress.clipTopRight = new Vector2(player.Relaxation / 100.0f, relaxationProgress.clipTopRight.y);
		bladderProgress.clipTopRight = new Vector2(player.Bladder / 100.0f, bladderProgress.clipTopRight.y);
		hungerProgress.clipTopRight = new Vector2(player.Hunger / 100.0f, hungerProgress.clipTopRight.y);
	}
}
