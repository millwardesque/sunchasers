#pragma strict

var speed = 0.1;
function Start () {

}

function Update () {
	transform.Translate(new UnityEngine.Vector3(0, speed, 0));
}