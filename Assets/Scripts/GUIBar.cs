using UnityEngine;
using System.Collections;

public class GUIBar : MonoBehaviour {
	public float progress = 0;
	public Vector2 pos = new Vector2(0, 0);
	public Vector2 size = new Vector2(100, 20);
 	
	/// <summary>
	/// OnGUI hook.
	/// </summary>
	void OnGUI()
	{
	    // draw the background:
	    GUI.BeginGroup(new Rect(pos.x, pos.y,  size.x, size.y));
	        GUI.Box(new Rect(0, 0, size.x, size.y), "");
	 
	        // draw the filled-in part:
	        GUI.BeginGroup(new Rect(0, 0, size.x * progress, size.y));
	            GUI.Box(new Rect(0, 0, size.x, size.y), "");
	        GUI.EndGroup ();
	    GUI.EndGroup ();
	}
}
