#pragma strict

/*
This script controlls all the game rounds.
*/

// Player and Enemy GameObjects
var player : GameObject;
var enemy : GameObject;

// Game Configuration
var roundsPerGame : int = 2;
var secondsPerRound : int = 60;

// Audio Clips
var BellAudio : AudioClip;
var PeopleAudio : AudioClip;
var ApplauseAudio : AudioClip;

// GUI Textures GameObjects
var fight : GameObject;
var knock_out : GameObject;
var time_out : GameObject;
var round_one : GameObject;
var round_two : GameObject;
var round_three : GameObject;
/*
If you want more rounds, just add "var round_four : GameObject;", etc...
Then, go to Hierarchy, expand the LevelManager childrens, then copy and paste "round_3" game object.
Rename it to "round_4". Then change the Texture for this game object in the Inspector View.
*/

// You must dont touch this variables.
private var currentRound : int = 1;
private var currentSecond : int;
private var gameFinished : boolean = false;
private var playerInitPosition : Vector3;
private var playerInitRotation : Quaternion;
private var enemyInitPosition : Vector3;
private var enemyInitRotation : Quaternion;

// This statics variables will be accessed from the Result scene.
static var playerSuccessfulHits : int = 0;
static var playerTotalHits : int = 0;
static var playerEffectivity : int;
static var enemySuccessfulHits : int = 0;
static var enemyTotalHits : int = 0;
static var enemyEffectivity : int;
static var winner : String;
static var isKnockOut : boolean = false;

function Start(){
	if(!player){
		print("WARNING! You must set the Player GameObject for this script in the Inspector View.");
	}
	if(!enemy){
		print("WARNING! You must set the Enemy GameObject for this script in the Inspector View.");
	}
	
	// Resets Player and Enemy position and rotation.
	playerInitPosition = player.transform.position;
	playerInitRotation = player.transform.rotation;
	enemyInitPosition = enemy.transform.position;
	enemyInitRotation = enemy.transform.rotation;
	
	// Clear some variables to start new a game.
	playerSuccessfulHits = 0;
	playerTotalHits = 0;
	enemySuccessfulHits = 0;
	enemyTotalHits = 0;
	winner = "";
	isKnockOut = false;
	
	StartNewRound();
}

function Update(){
	// Every frame I check the time for the current round.
	if(currentSecond == secondsPerRound){
		TimeOutRound();
	}
}

function StartNewRound(){
		
	// Play the Textures animation in the beginning of each round.
	if(currentRound == 1){
		round_one.animation.Play();
	}
	if(currentRound == 2){
		round_two.animation.Play();
	}
	if(currentRound == 3){
		round_three.animation.Play();
	}
	fight.animation.Play();
	
	// Before start the round, disable all player and enemy scripts...
	DisablePlayerScripts();
	DisableEnemyScripts();
	
	audio.PlayOneShot(PeopleAudio);
	
	// Wait for 3 seconds...
	yield WaitForSeconds(3);
	
	// When I start a New Round, I execute every second a function called Clock.
	// This function increments the seconds for the current round.
	InvokeRepeating("Clock",1,1);
	
	// Play the Bell Sound
	audio.PlayOneShot(BellAudio);
	
	
	// And then enable all player and enemy scripts.
	EnablePlayerScripts();
	EnableEnemyScripts();

}

// This function is executed every second by the function StartNewRound();
// It's a clock that increase the currentSecond by 1 for every second.
function Clock(){
	currentSecond++;
}

function TimeOutRound(){
	
	// Reset the currentSecond to 0.
	currentSecond = 0;
	
	// When a round is finished, stop the Clock.
	CancelInvoke("Clock");
	// Play the Texture animation that show "Time Out" message in the screen.
	time_out.animation.Play();
	
	// Play the Bell Sound
	audio.PlayOneShot(BellAudio);
	
	player.GetComponent(BoxerController).Movimenta("Recua");
	enemy.GetComponent(COMController).Movimenta("Recua");
	
	// Disable all player and enemy scripts.
	DisablePlayerScripts();
	DisableEnemyScripts();
	
	// Set the default idle animation for player and enemy.
	//player.animation.CrossFade("idle",0.1);
	//enemy.animation.CrossFade("idle",0.1);
	
	// Wait 5 seconds.
	yield WaitForSeconds(5);
	
	// Increment the round by 1.
	currentRound++;
	
	// If all rounds has been played.
	if(currentRound > roundsPerGame){
		// End the Game by Time Out.
		EndGame("timeOut");
	}else{ // If not, reset player and enemy, and Start a New Round.
		ResetPlayer();
		ResetEnemy();	
		StartNewRound();
	}
}

// This function is called externally from playerStatus.js (Dead function).
// Also is called too from AiScript.js (Dead function).
// Its a Knockout.
function KO(loser : String){
	if(!gameFinished){
		// Play the Texture animation thats show "KnockOut" message.
		knock_out.animation.Play();
		// Stop the clock.
		CancelInvoke("Clock");
		// Finish the game.
		gameFinished = true;
		// Play Applause audioclip
		audio.PlayOneShot(ApplauseAudio);
		// Wait 3 seconds.		
		yield WaitForSeconds(6);
		
		// Set the winner.
		if(loser == "player"){
			winner = "PC AI";
		}
		if(loser == "enemy"){
			winner = "Player";
		}
		
		// End the Game by KnockOut.
		EndGame("knockOut");
	}
}


// Here I use OnGUI only to show the Clock in ths creen.
function OnGUI(){
	GUI.depth = 0;

	if(currentSecond < 10){
		GUI.Label(Rect(Screen.width/2 - 3, 10, 100, 20), "0" + currentSecond.ToString());
	}else{
		GUI.Label(Rect(Screen.width/2 - 3, 10, 100, 20), currentSecond.ToString());
	}
}

function EndGame(type:String){
	
	     playerTotalHits = player.GetComponent(BoxerController).totalGolpes;
	playerSuccessfulHits = player.GetComponent(BoxerController).golpesEfetivos;
	 enemyTotalHits = enemy.GetComponent(COMController).totalGolpes;
	enemySuccessfulHits = enemy.GetComponent(COMController).golpesEfetivos;
	
	// Calculate the player and Enemy Effectivity.
	if(playerTotalHits > 0){
		playerEffectivity = playerSuccessfulHits * 100 / playerTotalHits;
	}
	if(enemyTotalHits > 0){
		enemyEffectivity = enemySuccessfulHits * 100 / enemyTotalHits;
	}
	
	// Set the Winner by Time Out.
	if(type == "timeOut"){
		if(playerSuccessfulHits > enemySuccessfulHits){
			winner = "Player";
		}
		if(playerSuccessfulHits < enemySuccessfulHits){
			winner = "PC AI";
		}
		if(playerSuccessfulHits == enemySuccessfulHits){
			if(playerEffectivity > enemyEffectivity){
				winner = "Player";
			}
			if(playerEffectivity < enemyEffectivity){
				winner = "PC AI";
			}
		}
	}
	
	// Set isKnockOut to true. This variable will be accessed from GUIResult.js to know if the game
	// was finished by KnockOut.
	if(type == "knockOut"){
		isKnockOut = true;
	}
	
	// Load Results scene.
	Application.LoadLevel("Resultado");
	
}

// Disable player scripts to prevent movements.
function DisablePlayerScripts(){
	player.GetComponent(InputManager).enabled = false;
	player.GetComponent(BoxerController).enabled = false;
	
}

// Disable player scripts to allow movements.
function EnablePlayerScripts(){
	player.GetComponent(InputManager).enabled = true;
	player.GetComponent(BoxerController).enabled = true;
	
}

// Disable enemy scripts to prevent movements.
function DisableEnemyScripts(){
	enemy.GetComponent(COMController).enabled = false;	
}

// Disable enemy scripts to allow movements.
function EnableEnemyScripts(){
	enemy.GetComponent(COMController).enabled = true;	
}

function ResetPlayer(){
	if(currentRound == 1)
	{
		// Reset Player position and rotation.
		player.transform.position = playerInitPosition;
		player.transform.rotation = playerInitRotation;
	}
	// Set stamina to 100.
	player.GetComponent(BoxerController).stamina = 100;
	// Gain some random life.
	var randomLifePlayer : float = Random.Range(10,40);
	player.GetComponent(BoxerController).vida += randomLifePlayer;
	
	if(player.GetComponent(BoxerController).vida > 100){
		player.GetComponent(BoxerController).vida = 100;
	}
}

function ResetEnemy(){
	if(currentRound == 1)
	{
		// Reset Enemy position and rotation.
		enemy.transform.position = enemyInitPosition;
		enemy.transform.rotation = enemyInitRotation;	
	}
	// Set stamina to 100.
	enemy.GetComponent(COMController).stamina = 100;
	// Gain some random life.
	var randomLifeEnemy : float = Random.Range(10,20);
	enemy.GetComponent(COMController).vida += randomLifeEnemy;
	
	if(enemy.GetComponent(COMController).vida > 100){
		enemy.GetComponent(COMController).vida = 100;
	}
}

// This function is called externally, and count the player and enemy successful hits.
function AddSuccessfulHit(character : String){
	if(character == "enemy"){
		enemySuccessfulHits++;
	}
	if(character == "player"){
		playerSuccessfulHits++;
	}
}

// This function is called externally, and count the player and enemy hits.
function AddHit(character : String){
	if(character == "enemy"){
		enemyTotalHits++;
	}
	if(character == "player"){
		playerTotalHits++;
	}
}