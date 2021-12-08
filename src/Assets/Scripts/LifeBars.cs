using UnityEngine;
using System.Collections;

public class UserInterfaceGraphics : MonoBehaviour {
	
	public float currentLife=100.0f;
	
	public GUIStyle myGUIStyle;
	
	void OnGUI () {
		
		//For example you have 100 life’s maximum.
		
		GUI.Box(new Rect(10.0f, 10.0f, 0.001f * Screen.width * currentLife,  0.1f * Screen.height), "LIFE", myGUIStyle);
		
	}
	
}