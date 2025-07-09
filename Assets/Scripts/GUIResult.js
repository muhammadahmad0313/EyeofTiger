#pragma strict

/*

This script only show the game results. Also have a Clock counter to show the info step-by-step.

*/

// Make the script also execute in edit mode.
@script ExecuteInEditMode()

private var currentSecond : int = 0;

function Start(){
	InvokeRepeating("Clock",1,1);
}

function OnGUI(){
	GUI.Box(Rect(10,10,(Screen.width / 2),Screen.height - 20),"RESULTADOS");

	if(currentSecond > 1){
		GUI.Label(Rect(20,50,(Screen.width / 2),Screen.height - 20),"Jogador - Total de Golpes: " + LevelManager.playerTotalHits);
	}
		
	if(currentSecond > 2){
		GUI.Label(Rect(20,70,(Screen.width / 2),Screen.height - 20),"Jogador - Golpes Efetivos: " + LevelManager.playerSuccessfulHits);
	}
	
	if(currentSecond > 3){
		GUI.Label(Rect(20,90,(Screen.width / 2),Screen.height - 20),"Jogador - Efetividade: " + LevelManager.playerEffectivity + "%");
	}
	
	if(currentSecond > 5){
		GUI.Label(Rect(20,140,(Screen.width / 2),Screen.height - 20),"Computador - Total de Golpes: " + LevelManager.enemyTotalHits);
	}
		
	if(currentSecond > 6){
		GUI.Label(Rect(20,160,(Screen.width / 2),Screen.height - 20),"Computador - Golpes Efetivos: " + LevelManager.enemySuccessfulHits);
	}
	
	if(currentSecond > 7){
		GUI.Label(Rect(20,180,(Screen.width / 2),Screen.height - 20),"Computador - Efetividade: " + LevelManager.enemyEffectivity + "%");
	}
	
	if(currentSecond > 9){
		if(LevelManager.isKnockOut){
			GUI.Label(Rect(20,220,(Screen.width / 2),Screen.height - 20),"Vencedor: " + LevelManager.winner + " por K.O.!");
		}else{
			GUI.Label(Rect(20,220,(Screen.width / 2),Screen.height - 20),"Vencedor: " + LevelManager.winner + " por Pontos!");
		}
		
	}
	
	GUI.enabled = false;
	
	if(currentSecond > 10){
		GUI.enabled = true;
	}
	
	if(GUI.Button(Rect(20,Screen.height - 80,(Screen.width / 2) - 30,60),"Voltar para o Inicio")){
		Application.LoadLevel("Entrada");
	}
}

function Clock(){
	currentSecond++;
}