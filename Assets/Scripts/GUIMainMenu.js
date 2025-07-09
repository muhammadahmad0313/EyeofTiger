#pragma strict

/*

This script show the Start New Game button and the Controls Help in the Main Menu screen.

*/

// Make the script also execute in edit mode.
@script ExecuteInEditMode()

var controlsTexture : Texture2D;

function OnGUI(){
	
	GUI.depth = 0;
	
	if(GUI.Button(Rect(30,210,(Screen.width / 6) - 30,100),"Novo Jogo")){
		Application.LoadLevel("Luta");
	}
	if(GUI.Button(Rect(30,330,(Screen.width / 6) - 30,100),"Sair")){
		Application.Quit();
	} 
	
	
	GUI.Box(Rect(0,Screen.height-110,Screen.width,110),"");
	GUI.DrawTexture(Rect(20,Screen.height-controlsTexture.height,controlsTexture.width,controlsTexture.height),controlsTexture);
}