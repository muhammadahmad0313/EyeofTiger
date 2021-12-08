using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Boundary
{
	public float xMin, xMax, zMin, zMax;
}

public class COMController : MonoBehaviour {
	public Boundary limites;
	public EstadoPlayer estado;
	Animator animator; 
	public GameObject player;
	public GameObject inimigo;
	public float alcance = 1.6f;
	public float distanciaInimigo;
	public float      velocMove;
	public bool podeMover = true;
	
	private List<string>  golpes     = new List<string>();
	private List<string>  guardas    = new List<string>();
	private List<string>  esquivas   = new List<string>();
	private List<string>  movimentos = new List<string>();
	
	public int golpesEfetivos = 0;
	public int totalGolpes = 0;
	public int efetividade = 0;
	public float vida = 100;
	public float stamina = 100;
	private bool morto = false;
	private float angulo = 0.0f;
	private float movH   = 0.0f;	// deslocamento horizontal
	private float movV   = 0.0f;	// deslocamento vertical
	private float distInimigo = 0.0f;		// distancia do jogador ao adversario
	private static float anguloBoxeador   = 40.0f;	// angulo do jogador em relacao ao seu vetor forward		
	private Vector3 ultimaPos;
	private Vector3 direcaoMove;		// direcao do jogador relativo ao adversario
	private Vector2 deslocar;		// indica se o jogador precisa se deslocar 
	private float magDeslocaMin;	// magnitude do menor deslocamento que o jogador pode fazer
	
	private float tempoAgir = 0.75f;					// determina quanto tempo o boxeador espera para iniciar alguma acao
	private float contaTempoAgir = 0.0f;
	private float tempoGuarda = 0.5f;					// determina o tempo que o boxeador mantera a guarda (gerado randomicamente e atualizado constantemente)
	private float contaGuarda = 0.0f;					// conta o tempo que o boxeador esta mantendo a guarda
	private float tempoAnterior = 0.0f;			// utilizado para conter um valor de tempo que se queira utilizar posteriormente
	private bool  usaGuardaDir = true;			// utilizado para alternar entre a guarda direita e esquerda (utilizado para testes)
	private float tempoEstrategia = 10.0f; 		// determina o tempo que o boxeador devera mudar sua estrategia 
	private float contaEstrategia = 0;

	private bool podeDano = true;
	private float tempoDano = 0.5f;

	
	// Inicializacao
	void Start () {
		estado = EstadoPlayer.Parado;
		
		// Define a lista de movimentos
		movimentos.Add ("MoveEsq");
		movimentos.Add ("MoveFrente");
		movimentos.Add ("MoveTras");
		movimentos.Add ("MoveDir");
		
		esquivas.Add ("PenduloEsq");
		esquivas.Add ("PenduloDir");
		esquivas.Add ("Avanca");
		esquivas.Add ("Recua");
		esquivas.Add ("EvadeEsq");
		esquivas.Add ("EvadeDir");
		
		// Define a lista de guardas
		guardas.Add ("GuardaEsq");
		guardas.Add ("GuardaDir");
		guardas.Add ("GuardaFrente");
		guardas.Add ("GuardaEsqBaixo");
		guardas.Add ("GuardaDirBaixo");
		
		// Define a lista de golpes
		golpes.Add ("Jab");
		golpes.Add ("Direto");
		golpes.Add ("CruzadoEsq");
		golpes.Add ("CruzadoDir");
		golpes.Add ("GanchoEsq");
		golpes.Add ("GanchoDir");
		golpes.Add ("CruzadoEsqBaixo");
		golpes.Add ("CruzadoDirBaixo");
		golpes.Add ("GanchoEsqBaixo");
		golpes.Add ("GanchoDirBaixo");
		
		golpes.Add ("JabJab");
		golpes.Add ("JabDireto");
		golpes.Add ("CruzadoDirEsq");
		golpes.Add ("CruzadoEsqDir");
		
		
		animator = GetComponent<Animator> ();
		tempoAnterior = Time.time;
		estado = EstadoPlayer.Parado;
		Random.seed = (int)System.DateTime.Now.Ticks;
		
		contaTempoAgir = tempoAgir;
		
		// Determina um tempo aleatorio que o computador ira utilizar para manter guarda
		tempoGuarda = Random.Range (0.5f, 2.0f);
		contaGuarda = tempoGuarda;
		contaEstrategia = tempoEstrategia;

		/* Inicializa o angulo entre o player e o adversario */
		Vector3 vPlayer  = new Vector3 (transform.position.x, 0, transform.position.z);
		Vector3 vInimigo = new Vector3 (inimigo.transform.position.x, 0, inimigo.transform.position.z);		
		direcaoMove = inimigo.transform.position - player.transform.position;
		angulo  = Vector3.Angle (player.transform.forward, direcaoMove);
		
		
	}
	
	// Update is called once per frame
	void Update () {

		contaTempoAgir -= Time.deltaTime;

		Debug.DrawLine(player.transform.position, player.transform.forward*100, Color.yellow);
		Debug.DrawLine (player.transform.position, direcaoMove, Color.magenta);

		//Debug.Log ("contaTempoAgir: " + contaTempoAgir);
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

			tempoDano -= Time.deltaTime;
			if (tempoDano < 0)
			{
				tempoDano = 0.5f;
				podeDano = true;
			}

			if (Longe ())
			{
				estado = EstadoPlayer.Movendo;
				Movimenta ("MoveFrente");
			}
			else 
			{
				if (estado == EstadoPlayer.Defendendo) {
					contaGuarda -= Time.deltaTime;	

					if (contaGuarda < 0.0f)
						GuardaOff();
				} else if (contaTempoAgir < 0.0f) {
					contaGuarda = tempoGuarda;
					contaTempoAgir = tempoAgir;

					EscolheProxMov(estado);
				}
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


	/* Atualiza posicionamento do jogador */
	void AtualizaPos()
	{   
		direcaoMove = inimigo.transform.position - player.transform.position;
		angulo  = Vector3.Angle (player.transform.forward, direcaoMove);
	}
	
	void setTriggerGolpe (string trigger)
	{
		estado = EstadoPlayer.Atacando;
		animator.SetTrigger (trigger);
	}
	
	void setTriggerMove (string trigger)
	{
		estado = EstadoPlayer.Movendo;
		animator.SetTrigger (trigger);
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
				//inimigo.SendMessage("AddGolpe");
				
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
			//animator.SetTrigger("Atingido");		
		}	
	}


	bool Longe()
	{
		if (distanciaInimigo > 2.0f*alcance)
			return (true);
		
		return (false);
	}

	bool Perto()
	{
		if ((distanciaInimigo > alcance) && (distanciaInimigo < alcance*1.5f))
			return (true);

		return (false);
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
		if (deslocar == Vector2.zero)
			deslocar = new Vector2 (h, v);
		
		if ((h != 0.0f) || (v != 0.0f))
		{
			
			if (deslocar.magnitude <= magDeslocaMin)     // se chegou a deslocamento minimo possivel faz ele
			{   
				Move (deslocar.x, deslocar.y);
				deslocar = Vector2.zero;
				
			} else {
				Vector2 vDesloc = new Vector2(this.deslocar.x, this.deslocar.y);
				vDesloc.Normalize();
				vDesloc = vDesloc*deslocar.magnitude/10;
				
				Move (vDesloc.x, vDesloc.y);
				deslocar = deslocar - vDesloc;
			}
		} 		
	}



	/* Executa movimentacoes. Tanto as que sao ativadas por combos, quanto as ativadas por teclas simples */
	public void Movimenta(string trigger)
	{
		float deslocamento = 1.5f;
		estado = EstadoPlayer.Movendo;
		
		switch (trigger) 
		{	
			case "MoveFrente":
					estado = EstadoPlayer.Movendo;
				//if (distInimigo - deslocamento > alcance )
					Move (0.0f, deslocamento);
				//else
				//	Move (0.0f, distInimigo - alcance);
				break;
			case "MoveTras":
				estado = EstadoPlayer.Movendo;
				Move (0.0f, -deslocamento);
				break;
			case "MoveDir":
				estado = EstadoPlayer.Movendo;
				Move (deslocamento, 0.0f);
				break;
			case "MoveEsq":
				estado = EstadoPlayer.Movendo;
				Move (-deslocamento, 0.0f);
				break;
			case "PenduloEsq":
			case "PenduloDir":	
				estado = EstadoPlayer.Esquivando;
				animator.SetTrigger (trigger);
				break;
			case "Recua":
				deslocamento = -3.0f;
				estado = EstadoPlayer.Movendo;
				if (podeMover)				
					MoveRapido (0.0f, deslocamento);
				break;
			case "Avanca":
				deslocamento = 3.0f;
				estado = EstadoPlayer.Movendo;
				if (podeMover)
					MoveRapido (0.0f, deslocamento);				
				break;
			case "EvadeEsq":
				estado = EstadoPlayer.Esquivando;
				if (podeMover)				
					MoveRapido (-30.0f, 0.0f);
				break;
			case "EvadeDir":
				estado = EstadoPlayer.Esquivando;
				if (podeMover)				
					MoveRapido (30.0f, 0.0f);
				break;			
		}		
	}
	
	void EscolheProxMov(EstadoPlayer estadoAtual)
	{
		Random.seed = (int)System.DateTime.Now.Ticks;
		int escolha = Random.Range(0, 99);
		string acao = "";
		
		//Debug.Log ("Estado Player: " + estadoAtual);
		//Debug.Log ("seed: " + Random.seed + " - escolha: " + escolha);
		switch (estadoAtual)
		{
		case EstadoPlayer.Parado:
			escolha %= 5;
			// pode escolher mover, atacar, defender ou esquivar
			if (escolha == 0) // Ataca
			{
				acao = EscolheAtaque();
				setTriggerGolpe(acao);
			}
			else if ((escolha == 1) || (escolha == 2))  // Defende
			{
				acao = EscolheDefesa();
				GuardaOn(acao);
			}
			else if (escolha == 3)  // Esquiva
			{
				estado = EstadoPlayer.Esquivando;
				acao = EscolheEsquiva();
				Movimenta (acao);
			}
			else
			{
				estado = EstadoPlayer.Movendo;
				acao = EscolheMovimento();
				Movimenta (acao);
			}
			break;
		case EstadoPlayer.Movendo:
			escolha %= 3;
			// pode escolher mover, atacar ou defender, com uma tendencia maior a escolher mover
			if (escolha == 0) // Defende
			{
				acao = EscolheDefesa();
				GuardaOn(acao);
			} 
			else if (escolha == 1) // Ataca
			{
				acao = EscolheAtaque();
				setTriggerGolpe(acao);
			}
			else   // Escolher mover
			{
				estado = EstadoPlayer.Movendo;
				acao = EscolheMovimento();
				Movimenta (acao);
			}			
			break;
		case EstadoPlayer.Combando:
			estado = EstadoPlayer.Parado;					
			break;
		case EstadoPlayer.Atacando:
			estado = EstadoPlayer.Parado;					
			break;
		case EstadoPlayer.Esquivando:
			estado = EstadoPlayer.Parado;					
			break;
		case EstadoPlayer.Atingido:
			escolha %= 2;
			// pode escolher defender ou mover
			if (escolha == 0)   // Defende
			{
				acao = EscolheDefesa();
				GuardaOn(acao);
			}
			else if (escolha == 1) // Move
			{
				acao = EscolheMovimento();
				Movimenta (acao);
			}
			break;			
		}
		//Debug.Log ("Acao: " + acao);
	}
	
	string EscolheMovimento()
	{
		Random.seed = (int)System.DateTime.Now.Ticks;
		int escolha = Random.Range(0, movimentos.Count);
		
		return( movimentos[escolha]);
	}

	string EscolheEsquiva()
	{
		Random.seed = (int)System.DateTime.Now.Ticks;
		int escolha = Random.Range(0, esquivas.Count);
		
		return( esquivas[escolha]);
	}

	string EscolheAtaque()
	{
		Random.seed = (int)System.DateTime.Now.Ticks;
		int escolha = Random.Range(0, golpes.Count);
		
		return( golpes[escolha]);
	}
	
	string EscolheDefesa()
	{
		Random.seed = (int)System.DateTime.Now.Ticks;
		int escolha = Random.Range(0, guardas.Count);
		
		return( guardas[escolha]);
	}
}
