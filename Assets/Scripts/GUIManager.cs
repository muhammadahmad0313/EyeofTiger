using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {

	public Texture2D barTexture;
	public GameObject player;
	public GameObject COM;
	private BoxerController controllerPlayer;
	private COMController   controllerCOM;

	private float widthLifeBar;
	private float widthStaminaBar;

	// Use this for initialization
	void Start () {
		//levelManager = GetComponent<LevelManager> ();
		controllerPlayer = player.GetComponent<BoxerController> ();
		controllerCOM    = COM.GetComponent<COMController> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI(){
		
		GUI.depth = 1;
		
		widthLifeBar = (Screen.width / 2) - 10;
		widthStaminaBar = (Screen.width / 2) - 10;
		
		if(controllerPlayer != null){
			GUI.color = Color.red;
			GUI.DrawTexture(new Rect(10, 10, widthLifeBar * controllerCOM.vida / 100  , 15), barTexture);
			GUI.color = Color.white;
			
			GUI.color = Color.green;
			GUI.DrawTexture(new Rect(10, 30, widthStaminaBar * controllerCOM.stamina / 100  , 8), barTexture);
			GUI.color = Color.white;
		}
		
		if(controllerPlayer != null){
			GUI.color = Color.red;
			GUI.DrawTexture(new Rect(widthLifeBar + 20, 10, (widthLifeBar - 10) * controllerPlayer.vida / 100  , 15), barTexture);
			GUI.color = Color.white;
			
			GUI.color = Color.green;
			GUI.DrawTexture(new Rect(widthStaminaBar + 20, 30, (widthStaminaBar - 10) * controllerPlayer.stamina / 100  , 8), barTexture);
			GUI.color = Color.white;
		}
	}
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////q
