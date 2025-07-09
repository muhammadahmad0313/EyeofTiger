using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Indica o estado do Player. Essa enum ira sair daqui, pois ela nao deve estar junto com o controller de input
public enum EstadoPlayer { Movendo=0, Atacando, Defendendo, Parado, Combando, Atingido, Esquivando, None };  // define o estado do player

public class BoxerController : MonoBehaviour {
	public Boundary limites;
	public GameObject player;
	public GameObject inimigo;
	//public LevelManager levelManager;
	public EstadoPlayer estado;
	public InputManager inputMngr;
	
	public float velocMove = 2.3f;
	public float velocGira = 5f;
	public float alcance   = 1.6f;
	public float distanciaInimigo;
	public bool  podeMover = true;

	public int golpesEfetivos = 0;
	public int totalGolpes = 0;
	public int efetividade = 0;
	public float vida = 100;
	public float stamina = 100;
	private bool  morto  = false;
	private float angulo = 0.0f;
	private float movH   = 0.0f;			// deslocamento horizontal
	private float movV   = 0.0f;			// deslocamento vertical
	private float distInimigo;		// distancia do jogador ao adversario
	private float magDeslocaMin;	// magnitude do menor deslocamento que o jogador pode fazer
	private static float anguloBoxeador   = 40.0f;	// angulo do jogador em relacao ao seu vetor forward		
	
	private Vector3 direcaoMove;		// direcao do jogador relativo ao adversario
	private Vector2 deslocar;		// indica se o jogador precisa se deslocar 
	private Quaternion rotacaoInicial;
	private CharacterController controller;
	private Animator        animator;
	private List<KeyCombo>  combos   = new List<KeyCombo>();
	private Dictionary<string, string> movimentos = new Dictionary<string, string> ();		
	private KeyCombo comboAtivo = null;

	private bool podeDano = true;
	private float tempoDano = 0.5f;
		
	/*
	public int stateGuardaDir { get; private set; }	
	public int stateGuardaEsq { get; private set; }	
	public int stateGuardaDirBaixo { get; private set; }	
	public int stateGuardaEsqBaixo { get; private set; }
	public int stateGuardaFrente { get; private set; }
*/
	protected int lastAnimState { get; set; }
	
	/** 
 	* Inicializa a lista de teclas do jogos
	*  Os nomes das teclas foram definidos no InputManager do Unity,
	*  mas eu preferi definir variaveis estaticas dentro do meu InputManager
	*  pra poder lidar mais facil e caso precise trocar de string para outro 
	*  tipo. Ou caso precise reutilizar esse codigo, seja mais facil de trocar.
	**/
	void InicializaGameKeys()
	{
		List<GameKey> gameKeys = new List<GameKey> ();
		
		gameKeys.Add (new GameKey(InputManager.jabKey,       "Jab", 	   this.Golpe, null, EstadoKey.Released));
		gameKeys.Add (new GameKey(InputManager.cruzEsqKey,   "CruzadoEsq", this.Golpe, null, EstadoKey.Released));
		gameKeys.Add (new GameKey(InputManager.cruzDirKey,   "CruzadoDir", this.Golpe, null, EstadoKey.Released));
		gameKeys.Add (new GameKey(InputManager.diretoKey,    "Direto",     this.Golpe, null, EstadoKey.Released));
		gameKeys.Add (new GameKey(InputManager.guardaEsqKey, "GuardaEsq",  this.GuardaOn, this.GuardaOff, EstadoKey.Released));
		gameKeys.Add (new GameKey(InputManager.guardaDirKey, "GuardaDir",  this.GuardaOn, this.GuardaOff, EstadoKey.Released));
		gameKeys.Add (new GameKey(InputManager.upKey,    "Movimenta", this.Movimenta, true, null,  EstadoKey.Released));
		gameKeys.Add (new GameKey(InputManager.downKey,  "Movimenta", this.Movimenta, true, null, EstadoKey.Released));
		gameKeys.Add (new GameKey(InputManager.leftKey,  "Movimenta", this.Movimenta, true, null, EstadoKey.Released));
		gameKeys.Add (new GameKey(InputManager.rightKey, "Movimenta", this.Movimenta, true, null, EstadoKey.Released));
		
		inputMngr.InitializeGameKeys (gameKeys);
	}
	
	/*
	 * Inicializa a lista de combos 
	*/
	void InicializaKeyCombos()
	{
		// Combos de golpes simples
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.cruzEsqKey}, "CruzadoEsqBaixo", EstadoPlayer.Atacando, this.GolpeCombo));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.cruzDirKey}, "CruzadoDirBaixo", EstadoPlayer.Atacando, this.GolpeCombo));
		combos.Add (new KeyCombo(new string[] {InputManager.upKey,   InputManager.jabKey},     "GanchoEsq", 	  EstadoPlayer.Atacando, this.GolpeCombo));
		combos.Add (new KeyCombo(new string[] {InputManager.upKey,   InputManager.diretoKey},  "GanchoDir", 	  EstadoPlayer.Atacando, this.GolpeCombo));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey,   InputManager.jabKey},     "GanchoEsqBaixo",  EstadoPlayer.Atacando, this.GolpeCombo));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey,   InputManager.diretoKey},  "GanchoDirBaixo",  EstadoPlayer.Atacando, this.GolpeCombo));
		
		// Combos de golpes duplos
		combos.Add (new KeyCombo(new string[] {InputManager.jabKey,  	InputManager.jabKey}, 		 "JabJab",        EstadoPlayer.Atacando, this.GolpeCombo));
		combos.Add (new KeyCombo(new string[] {InputManager.diretoKey,  InputManager.diretoKey}, "JabDireto",     EstadoPlayer.Atacando, this.GolpeCombo));
		combos.Add (new KeyCombo(new string[] {InputManager.cruzDirKey, InputManager.cruzDirKey},"CruzadoDirEsq", EstadoPlayer.Atacando, "CruzadoDir", this.GolpeDirEsq, this.GolpeDuploOff));
		combos.Add (new KeyCombo(new string[] {InputManager.cruzEsqKey, InputManager.cruzEsqKey},"CruzadoEsqDir", EstadoPlayer.Atacando,"CruzadoEsq",  this.GolpeEsqDir, this.GolpeDuploOff));		
		
		/*  Golpes duplos dos ganchos estao temporariamente desativados - problema de coincidencia de combos (Willian em 04/02/2015)
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.jabKey, InputManager.jabKey},       "GanchoEsqDir", EstadoPlayer.Atacando,  "GanchoEsq", this.GolpeEsqDir, this.GolpeDuploOff));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.diretoKey, InputManager.diretoKey}, "GanchoDirEsq", EstadoPlayer.Atacando, "GanchoDir", this.GolpeDirEsq, this.GolpeDuploOff));
		combos.Add (new KeyCombo(new string[] {InputManager.upKey,   InputManager.jabKey, InputManager.jabKey},       "GanchoEsqDirBaixo", EstadoPlayer.Atacando, "GanchoEsqBaixo", this.GolpeEsqDir, this.GolpeDuploOff));
		combos.Add (new KeyCombo(new string[] {InputManager.upKey,   InputManager.diretoKey, InputManager.diretoKey}, "GanchoDirEsqBaixo", EstadoPlayer.Atacando, "GanchoDirBaixo", this.GolpeDirEsq, this.GolpeDuploOff));
		*/
		
		// Combos de guardas
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.guardaEsqKey}, "GuardaEsqBaixo",  EstadoPlayer.Defendendo, this.GuardaOn, this.GuardaOff, TipoCombo.Agrupado));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.guardaDirKey}, "GuardaDirBaixo",  EstadoPlayer.Defendendo, this.GuardaOn, this.GuardaOff, TipoCombo.Agrupado));
		combos.Add (new KeyCombo(new string[] {InputManager.guardaDirKey, InputManager.guardaEsqKey}, "GuardaFrente",  EstadoPlayer.Defendendo, this.GuardaOn, this.GuardaOff, TipoCombo.Agrupado));
		
		// Combos de esquisvas
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.downKey},  "Recua",      EstadoPlayer.Movendo, this.Movimenta));
		combos.Add (new KeyCombo(new string[] {InputManager.upKey,   InputManager.upKey},    "Avanca",     EstadoPlayer.Movendo, this.Movimenta));	
		combos.Add (new KeyCombo(new string[] {InputManager.rightKey,InputManager.rightKey}, "EvadeDir",   EstadoPlayer.Movendo, this.Movimenta));
		combos.Add (new KeyCombo(new string[] {InputManager.leftKey, InputManager.leftKey},  "EvadeEsq",   EstadoPlayer.Movendo, this.Movimenta));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.rightKey}, "PenduloDir", EstadoPlayer.Movendo, this.Movimenta));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.leftKey},  "PenduloEsq", EstadoPlayer.Movendo, this.Movimenta));
		
		/*combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.cruzEsqKey},   "CruzadoEsqBaixo", EstadoPlayer.Atacando));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.cruzDirKey},   "CruzadoDirBaixo", EstadoPlayer.Atacando));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.guardaEsqKey}, "GuardaEsqBaixo",  EstadoPlayer.Defendendo));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.guardaDirKey}, "GuardaDirBaixo",  EstadoPlayer.Defendendo));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.leftKey},       "PenduloEsq",      EstadoPlayer.Movendo));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.rightKey},       "PenduloDir",      EstadoPlayer.Movendo));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.downKey},      "RecuaRapido",  EstadoPlayer.Movendo));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.upKey},        "AvancaRapido", EstadoPlayer.Movendo));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.rightKey},       "EvadeDir", EstadoPlayer.Movendo));
		combos.Add (new KeyCombo(new string[] {InputManager.downKey, InputManager.leftKey},       "EvadeEsq", EstadoPlayer.Movendo));	
*/
		inputMngr.InitializeCombos (combos);
	}
	
	/*
	 * Inicializa a lista de movimentos e relaciona os triggers associados com cada tecla para a realizaçao
	 * de movimentos simples. Como golpes simples e movimentaçao personagem
	 */
	void InicializaMoves()
	{
		/* Inicializa a lista de golpes */
		movimentos.Add (InputManager.jabKey,        "Jab");
		movimentos.Add (InputManager.cruzEsqKey,    "CruzadoEsq");
		movimentos.Add (InputManager.cruzDirKey,    "CruzadoDir");
		movimentos.Add (InputManager.diretoKey,     "Direto");   
		movimentos.Add (InputManager.guardaDirKey,  "GuardaDirAlta"); 
		movimentos.Add (InputManager.guardaEsqKey,  "GuardaEsqAlta"); 
	}
	
	void Start () 
	{
		/* Inicializa objetos do jogo */
		animator = GetComponent<Animator> ();
		controller = player.GetComponent<CharacterController> ();
		inputMngr = GetComponent<InputManager> ();
		deslocar = Vector2.zero;
		magDeslocaMin = 1.0f;
		estado = EstadoPlayer.Parado;
		
		/* Inicializa as teclas do jogos e os combos */
		InicializaGameKeys ();
		InicializaKeyCombos ();
		
		/* Inicializa o angulo entre o player e o adversario */
		Vector3 vPlayer  = new Vector3 (transform.position.x, 0, transform.position.z);
		Vector3 vInimigo = new Vector3 (inimigo.transform.position.x, 0, inimigo.transform.position.z);		
		direcaoMove = inimigo.transform.position - player.transform.position;
		angulo  = Vector3.Angle (player.transform.forward, direcaoMove);

		rotacaoInicial = transform.rotation;
	}
	
	void Update()
	{	
		Debug.DrawLine(player.transform.position, player.transform.forward*100, Color.blue);
		Debug.DrawLine (player.transform.position, direcaoMove, Color.green);

		if (!morto)
		{

			distanciaInimigo = Vector3.Distance (player.transform.position, inimigo.transform.position);
			direcaoMove = inimigo.transform.position - player.transform.position;
			angulo  = Vector3.Angle (player.transform.forward, direcaoMove);

			// Gira o boxeador para ficar sempre de frente com o adversario
			if ((angulo > anguloBoxeador) || (angulo < anguloBoxeador))
			{
				Vector3 deltaAngulo = new Vector3(0,angulo - anguloBoxeador,0);		
				transform.Rotate(-deltaAngulo);
			}

			
			//transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(novaDirecao), velocGira * Time.deltaTime);
			//transform.rotation.Set (rotacaoInicial.x,transform.rotation.y, rotacaoInicial.z,transform.rotation.w);
			tempoDano -= Time.deltaTime;
			if (tempoDano < 0)
			{
				tempoDano = 0.5f;
				podeDano = true;
			}

			/** 
			 * Esse codigo eh para resolver o problema de desativar a guarda quando o botao de guarda eh apenas pressionado e soltado (rapidamente)
			 * O codigo delegado para executar a acao de keyUp executa, mas o Mechanim nao executa a animacao. Deixando travado na animcao de segurar
			 * a guarda em cima. Para resolver o problema, inclui um trigger na transicao de SegGrdaAlta para New State, e verifico a animaçao que esta
			 * sendo executada e seto o trigger caso a animacao esteja executando e nao haja nenhuma tecla pressionada.
			 */
			AnimatorStateInfo animSDir = animator.GetCurrentAnimatorStateInfo (1);
			AnimatorStateInfo animSEsq = animator.GetCurrentAnimatorStateInfo (2);
			
			if (estado == EstadoPlayer.Defendendo)
			{
				if ((animSDir.IsName("SegGrdaDirAlta") && (!InputManager.isPressed(InputManager.guardaDirKey))) ||
				    (animSEsq.IsName("SegGrdaEsqAlta") && (!InputManager.isPressed(InputManager.guardaEsqKey))))
					animator.SetTrigger ("SaiGuarda");
				
				/*	if ((animSDir.IsName("SegGrdaDirBaixo") && (!InputManager.isPressed(InputManager.guardaDirKey) && !InputManager.isPressed(InputManager.downKey))) ||
				    (animSEsq.IsName("SegGrdaEsqBaixo") && (!InputManager.isPressed(InputManager.guardaEsqKey) && !InputManager.isPressed(InputManager.downKey))))
					animator.SetTrigger ("SaiGuarda");   */
			}
		}
	}
	
	
	void FixedUpdate()
	{
		/* Corrigir o problema de virar de frente quando executa o combo de esquiva lateral */
		if (this.deslocar != Vector2.zero)
		{
			MoveRapido (this.deslocar.x, this.deslocar.y);
		}
		
		//ViraFrente (); 
	}

	void AddGolpeEfetivo()
	{
		golpesEfetivos++;
		totalGolpes++;

	}

	void AddGolpe()
	{
		totalGolpes++;
	}

	void Animating(float h, float v)
	{
		Move (h,v);
	}
	
	void GuardaOn(string k)
	{
		estado = EstadoPlayer.Defendendo;
		animator.SetBool("Guarda", true);
		animator.SetTrigger(k);		
	}
	
	void GuardaOff()
	{
		estado = EstadoPlayer.Parado;
		animator.SetBool("Guarda", false);
	}

	void Morto()
	{
		morto = true;
	}

	void AplicaDano(float dano)
	{
		if (podeDano)
		{
			if (estado == EstadoPlayer.Defendendo)
			{
				dano /= 5;

			}
			/*else
				inimigo.SendMessage ("AddGolpeEfetivo");
			*/

			vida -= dano;

			if (vida <= 0)
			{
				vida = 0f;
				Morto ();
			}

			podeDano = false;
		}

	}

	/* Seta a animacao de um golpe simples. Recupera a string do Trigger a partir da tecla do golpe 
	 * Esse metodo e utilizado apenas para golpes simples. Movimentacoes rapidas e combos, funcionam 
	 * de outra forma. 
	 */
	void Golpe(string sGolpe)
	{		
		animator.SetBool ("DuploEsqDir", false); 
		animator.SetBool ("DuploDirEsq", false);
		estado = EstadoPlayer.Atacando;
		animator.SetTrigger (sGolpe);
	}
	
	void GolpeCombo(string trigger)
	{		
		animator.SetBool ("DuploEsqDir", false); 
		animator.SetBool ("DuploDirEsq", false);
		estado = EstadoPlayer.Atacando;
		if ((trigger == "GanchoEsqBaixo") || (trigger == "GanchoDirBaixo"))
		{
			MoveRapido (0f, -0.3f);
		}

		animator.SetTrigger (trigger);
	}
	
	/* Essa funcao podera ser utilizada para substituir chamadas delegate para Golpe(string) e GolpeCombo(string)
	   Essenciamente as duas fazem a mesma coisa: setam o trigger que dispara a animacao (TESTAR)  */
	void setTriggerGolpe (string trigger)
	{
		animator.SetTrigger (trigger);
	}
	
	/* Atualiza posicionamento do jogador */
	void AtualizaPos()
	{   
		direcaoMove = inimigo.transform.position - player.transform.position;
		angulo = Vector3.Angle (player.transform.forward, direcaoMove);
	}
	
	/* Move o player pegando o deslocamento a partir do Input dos eixos */
	void Move()
	{
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");
		Move (h, v);
	}
	
	
	/* Move o player de acordo com os valores de eixo passados. 
	 * Esse metodo eh utilizado para deslocamentos rapidos, evasivas 
	 * e outras movimentaçoes enquanto nao temos animacoes apropriadas
	 * para isso 
	 * 
	 * TODO: Corrigir as situaçoes onde as teclas de movimentaçao basica (up, down, esq, dir) sao pressionadas
	 *       dentro dos combos e causam movimentaçao do avatar, quando nao deveriam
	 * TODO: Corrigir o problema da movimentaçao do avatar so acontecer a cada vez que a tecla e pressionada. 
	 *       Talvez uma chamada de movimentacao no Update para a funcao movimenta ou a configuraçao que respondam ao 
	 *       status de pressed das teclas de movimentacao. 
	 */
	void Move(float h, float v)
	{
		Vector3 movimento = Vector3.zero;
		if (h != 0.0f)
		{
			player.transform.RotateAround (inimigo.transform.position, inimigo.transform.up, -h);
		}
		
		if (v != 0.0f)
		{
			//movimento = player.transform.forward * v;
			movimento = direcaoMove * v;
			movimento = movimento.normalized * velocMove  * Time.deltaTime;
			//Debug.Log ("Distancia: " + Vector3.Distance(transform.position + movimento, inimigo.transform.position));
			if (Vector3.Distance(transform.position + movimento, inimigo.transform.position) > 2.0f)
			{
				Vector3 novaPos = transform.position + movimento;
				player.transform.position = new Vector3(Mathf.Clamp (novaPos.x, limites.xMin, limites.xMax), 
				                                        0.0f, 
				                                        Mathf.Clamp (novaPos.z, limites.zMin, limites.zMax));
			}
		}
		
		
		
	}
	
	void MoveRapido(float h, float v)
	{
		if (this.deslocar == Vector2.zero)
			this.deslocar = new Vector2 (h, v);
		
		//Debug.Log ("Vetor Deslocar: " + this.deslocar.x + " , " + this.deslocar.y);
		
		if ((h != 0.0f) || (v != 0.0f))
		{
			
			if (this.deslocar.magnitude <= this.magDeslocaMin)     // se chegou a deslocamento minimo possivel faz ele
			{   
				Move (this.deslocar.x, this.deslocar.y);
				this.deslocar = Vector2.zero;
				//AtualizaPos();
				//ViraFrente();
				
			} else {
				Vector2 vDesloc = new Vector2(this.deslocar.x, this.deslocar.y);
				vDesloc.Normalize();
				vDesloc = vDesloc*deslocar.magnitude/10;
				
				Move (vDesloc.x, vDesloc.y);
				this.deslocar = this.deslocar - vDesloc;
			}
		} 
		
	}
	
	/* Ativa um golpe duplo a ser executado */
	void GolpeEsqDir(string trigger)
	{
		animator.SetTrigger (trigger);
		animator.SetBool ("DuploEsqDir", true); 
	}
	
	/* Ativa um golpe duplo a ser executado */
	void GolpeDirEsq(string trigger)
	{
		animator.SetTrigger (trigger);
		animator.SetBool ("DuploDirEsq", true); 
	}
	
	
	void GolpeDuploOff()
	{
		animator.SetBool ("DuploEsqDir", false); 
		animator.SetBool ("DuploDirEsq", false);
	}
	
	/* Executa movimentacoes. Tanto as que sao ativadas por combos, quanto as ativadas por teclas simples */
	public void Movimenta(string trigger)
	{

		switch (trigger) 
		{
		case "Movimenta":
			if (podeMover)
				Move();
			estado = EstadoPlayer.Movendo;	
			break;
		case "PenduloEsq":
		case "PenduloDir":
			animator.SetTrigger (trigger);
			estado = EstadoPlayer.Esquivando;
			break;
		case "Recua":
			MoveRapido (0.0f, -3.0f);
			estado = EstadoPlayer.Movendo;
			break;
		case "Avanca":
			if (podeMover)
				MoveRapido (0.0f, 3.0f);
			estado = EstadoPlayer.Movendo;
			break;
		case "EvadeEsq":
			MoveRapido (-30.0f, 0.0f);
			estado = EstadoPlayer.Esquivando;
			break;
		case "EvadeDir":
			MoveRapido (30.0f, 0.0f);
			estado = EstadoPlayer.Esquivando;
			break;
			
		}
		
	}
	
}





/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////