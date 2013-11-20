using UnityEngine;
using System.Collections;

public class PlayerStatsUI : MonoBehaviour {
	private PlayerController player;
	private tk2dTextMesh meters;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		meters = GetComponent<tk2dTextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		meters.text = string.Format("Relaxation: {0:00}%\nBladder: {1:00}%\nHunger: {2:00}%", player.Relaxation, player.Bladder, player.Hunger);
		meters.Commit();
	}
}
