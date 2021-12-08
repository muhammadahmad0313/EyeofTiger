using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {
	public GameObject player;
	public GameObject COM;

	// Configuracao do jogo
	public int roundsPorJogo;
	public int segsPorRound;

	// Sons
	public AudioClip BellAudio;
	public AudioClip PeopleAudio;
	public AudioClip ApplauseAudio;

	// GUI Textures GameObjects
	public Animation fight;
	public GameObject knock_out;
	public GameObject time_out;
	public Animation round_one;
	public GameObject round_two;
	public GameObject round_three;

	private int        currentRound = 1;
	private int        segAtual;
	private float      tempoInicial;
	private bool       jogoEncerrado = false;
	private Vector3    playerInitPosition;
	private Quaternion playerInitRotation;
	private Vector3    COMInitPosition;
	private Quaternion COMInitRotation;
	
	// This statics variables will be accessed from the Result scene.
	static int playerGolpesEfetivos = 0;
	static int playerTotalGolpes = 0;
	static int playerEfetividade;
	static int COMGolpesEfetivos = 0;
	static int COMTotalGolpes = 0;
	static int COMEfetividade;
	static string vencedor;
	static bool isKO = false;


	// Use this for initialization
	void Start () {
		// Resets Player and Enemy position and rotation.
		playerInitPosition = player.transform.position;
		playerInitRotation = player.transform.rotation;
		COMInitPosition  = COM.transform.position;
		COMInitRotation  = COM.transform.rotation;
		
		// Clear some variables to start new a game.
		playerGolpesEfetivos = 0;
		playerTotalGolpes = 0;
		COMGolpesEfetivos = 0;
		COMTotalGolpes = 0;
		vencedor = "";
		isKO = false;
		StartNewRound();
	}
	
	// Update is called once per frame
	void Update () {

		//float t = Time.time - tempoInicial;
		segAtual = (int) (Time.time - tempoInicial);
		// A cada frame verifica o tempo do round
		if(segAtual == segsPorRound){
			TimeOutRound();	
			
		}	
	}

	IEnumerator StartNewRound(){
		
		// Play the Textures animation in the beginning of each round.
		if(currentRound == 1){
			Debug.Log ("Primeiro round!");

			round_one.animation.Play();
		}
		if(currentRound == 2){
			round_two.animation.Play();
		}
		if(currentRound == 3){
			round_three.animation.Play();
		}
		//fight.animation.Play();
		fight.Play ();
		// Before start the round, disable all player and enemy scripts...
		DisablePlayerScripts();
		DisableEnemyScripts();
		
		audio.PlayOneShot(PeopleAudio);
		
		// Wait for 3 seconds...
		yield return new WaitForSeconds(3);

		// Play the Bell Sound
		audio.PlayOneShot(BellAudio);
		tempoInicial = Time.time;

		// And then enable all player and enemy scripts.
		EnablePlayerScripts();
		EnableEnemyScripts();
		
	}

	IEnumerator TimeOutRound(){
		
		// Reset a conta de segundos
		segAtual = 0;
		
		// Exibe a mensagem de Timeout na tela
		time_out.animation.Play();
		
		// Play the Bell Sound
		audio.PlayOneShot(BellAudio);
		
		// Desabilita jogadores
		DisablePlayerScripts();
		DisableEnemyScripts();
		
		// Seta a animacao default dos jogadores
		player.animation.CrossFade("idle",0.1f);
		COM.animation.CrossFade("idle",0.1f);
		
		// Aguarda 5 segundos
		yield return new WaitForSeconds(5);
		
		// Incrementa o round
		currentRound++;
		
		// Se todos os rounds foram jogados
		if(currentRound > roundsPorJogo){
			// Termina o jogo por tempo
			EndGame("timeOut");
		}else{ // Senao zera as variaves dos jogadores e comeca de novo
			ResetPlayer();
			ResetCOM();	
			StartNewRound();
		}
	}

	// Exibe a contagem de tempo na tela
	void OnGUI(){
		GUI.depth = 0;
		
		if(segAtual < 10){
			GUI.Label(new Rect(Screen.width/2 - 3, 10, 100, 20), "0" + segAtual.ToString());
		}else{
			GUI.Label(new Rect(Screen.width/2 - 3, 10, 100, 20), segAtual.ToString());
		}
	}
	
	// Encerra o jogo
	void EndGame(string type){
		
		// Calculate the player and Enemy Effectivity.
		if(playerTotalGolpes > 0){
			playerEfetividade = playerGolpesEfetivos * 100 / playerTotalGolpes;
		}
		if(COMTotalGolpes > 0){
			COMEfetividade = COMGolpesEfetivos * 100 / COMTotalGolpes;
		}
		
		// Determina o vencedor por pontos
		if(type == "timeOut"){
			if(playerGolpesEfetivos > COMGolpesEfetivos){
				vencedor = "Player";
			}
			if(playerGolpesEfetivos < COMGolpesEfetivos){
				vencedor = "Computador";
			}
			if(playerGolpesEfetivos == COMGolpesEfetivos){
				if(playerEfetividade > COMEfetividade){
					vencedor = "Player";
				}
				if(playerEfetividade < COMEfetividade){
					vencedor = "Computador";
				}
			}
		}
		
		// Set isKnockOut to true. This variable will be accessed from GUIResult.js to know if the game
		// was finished by KnockOut.
		if(type == "knockOut"){
			isKO = true;
		}
		
		// Load Results scene.
		Application.LoadLevel("Resultado");
		
	}

	// Desabilita scripts do jogador
	void DisablePlayerScripts(){
		player.GetComponent<InputManager>().enabled = false;
		player.GetComponent<BoxerController>().enabled = false;
		
	}
	
	// Habilita os scripts do jogador
	void EnablePlayerScripts(){
		player.GetComponent<InputManager>().enabled = true;
		player.GetComponent<BoxerController>().enabled = true;
		
	}
	
	// Desabilita scripts do adversario
	void DisableEnemyScripts(){
		COM.GetComponent<COMController>().enabled = false;
	}
	
	// Habilita scripts do adversario
	void EnableEnemyScripts(){
		COM.GetComponent<COMController>().enabled = true;
	}
	
	void ResetPlayer(){
		// Reset Player position and rotation.
		player.transform.position = playerInitPosition;
		player.transform.rotation = playerInitRotation;
		// Set stamina to 100.
		player.GetComponent<BoxerController>().stamina = 100;
		// Gain some random life.
		float lifeAleatorio = Random.Range(10,20);
		player.GetComponent<BoxerController>().vida += lifeAleatorio;
		
		if(player.GetComponent<BoxerController>().vida > 100){
			player.GetComponent<BoxerController>().vida = 100;
		}
	}
	
	void ResetCOM(){
		// Reset Enemy position and rotation.
		COM.transform.position = COMInitPosition;
		COM.transform.rotation = COMInitRotation;
		// Set stamina to 100.
		COM.GetComponent<COMController>().stamina = 100;
		// Gain some random life.
		float lifeAleatorio = Random.Range(10,20);
		COM.GetComponent<COMController>().vida += lifeAleatorio;
		
		if(COM.GetComponent<COMController>().vida > 100){
			COM.GetComponent<COMController>().vida = 100;
		}
	}
	
	// Acrescenta a contagem de golpes efetivos
	public void AddGolpeEfetivo(string jogador){
		if(jogador == "COM"){
			COMGolpesEfetivos++;
		}
		if(jogador == "player"){
			playerGolpesEfetivos++;
		}
	}
	
	// Acrescenta a contagem de golpes
	public void AddGolpe(string jogador){
		if(jogador == "COM"){
			COMTotalGolpes++;
		}
		if(jogador == "player"){
			playerTotalGolpes++;
		}
	}

	IEnumerator KO(string perdedor){
		if(!jogoEncerrado){
			// Exibe na tela a mensagem de KO
			knock_out.animation.Play();

			// Encerra o jogo
			jogoEncerrado = true;
			// Executa o som dos aplausos
			audio.PlayOneShot(ApplauseAudio);
			// Espera 3 segundos		
			yield return new WaitForSeconds(6);
			
			// Determina o vencedor
			if(perdedor == "player"){
				vencedor = "COM";
			}
			if(perdedor == "COM"){
				vencedor = "player";
			}
			
			// End the Game by KnockOut.
			EndGame("knockOut");
		}
	}

}

//////////////////////////////////////////////////////////////////////////////////////////////////////////