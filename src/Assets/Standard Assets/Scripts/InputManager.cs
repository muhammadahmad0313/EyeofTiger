using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
 * Durante todo o estagio de desenvolvimento o estados JustPressed e Pressed sao tratados como eventos distintos.
 * Ainda que o Unity retorne JustPressed e Pressed como true no momento que a tecla eh pressionada. 
 * Por isso que existem dois metodos distintos (isPressed e wasJustPressed)
 * Tentei deixar os nomes de variaveis e nomes de metodos todos em ingles, mas um ou outro deve ter escapado. 
 * 
 * Como usar o Input Manager:
 * 
 * As lista de Combos e de Botoes do Jogo (GameKeys) deve ser inicializada por meio de algum controller.  
 * O Input Manager possui referencias (varias propriedades public static string) que indicam as teclas de acordo 
 * com os nomes criados no Input do Unity. Caso va utilizar essa classe para outro jogo, eh preciso alterar os nomes
 * dessas propriedades para coincidirem com os botoes do Input do Unity.
 * 
 **/


// Estado de um botao/tecla em determinado instante
public enum EstadoKey { JustPressed, JustReleased, Pressed, Released, None };

// Tipo de combos possiveis de serem executados 
public enum TipoCombo { Sequencia, Agrupado, None }; 

/**
 * Classe que identifica cada tecla do jogo e armazena o seu status 
 * Os dois delegates que hoje existem para serem chamados quando a tecla eh pressionado ou liberada,
 * sao chamados no metodo que atualiza o status dos botoes e nao na propria classe
 * 
 */
public class GameKey 
{	
	public  string  id;						// um identificador do botao - normalmente a mesma string que identifica ela no Input do Unity
	public  EstadoKey estado;				// estado da tecla
	private string pAcao = "";
	public bool acaoPressionado = false;			// identifica se a tecla repete a acao definida por keyOn quando mantida pressionada

	// acao a ser executada pelo botao quando pressionado - normalmente o trigger que dispara a animacao

	public delegate void ActionKeyOn(string sAcao);		// definicao da assinatura do metodo que sera chamado no keyPress
	public ActionKeyOn keyOn;							// metodo a ser chamado no KeyPress
	public delegate void ActionKeyUp();					// definicao da assinatura do metodo a ser chamado no KeyUp
	public ActionKeyUp keyUp;							// metodo a ser chamado no KeyUp



	// Construtor para botoes que nao possuem eventos a serem disparados nem acoes relacionadas
	public GameKey(string identificador, EstadoKey estado =  EstadoKey.None)
	{
		this.id     = identificador;	
		this.estado = estado;
	}

	// Construtor para botoes que possuem pelo menos um evento de KeyPress definido
	public GameKey(string identificador, string acao, ActionKeyOn acaoKeyOn, ActionKeyUp acaoKeyUp = null, EstadoKey estado =  EstadoKey.None)
	{
		this.id     = identificador;	
		this.estado = estado;
		this.pAcao  = acao;
		this.keyOn  = acaoKeyOn;
		this.keyUp  = acaoKeyUp;
	}

	// Construtor para botoes que possuem pelo menos um evento de KeyPress definido
	public GameKey(string identificador, string acao, ActionKeyOn acaoKeyOn, bool repetePressionado, ActionKeyUp acaoKeyUp = null, EstadoKey estado =  EstadoKey.None)
	{
		this.id     = identificador;	
		this.estado = estado;
		this.pAcao  = acao;
		this.keyOn  = acaoKeyOn;
		this.keyUp  = acaoKeyUp;
		this.acaoPressionado = repetePressionado;
	}

	// Retorna o identificador do botao
	public string getId()
	{
		return (this.id);
	}

	// Retorna o estado corrente do botao
	public EstadoKey getEstado()
	{
		return (estado);	
	}
	
	// Seta o estado do botao
	public void setEstado(EstadoKey ek)
	{
		this.estado = ek;
	}


	/*
	 * Determina se a tecla esta pressionada. Esse metodo so retorna true passado pelo menos "1 frame" de execucacao
	 * por que o estado JustPressed nao e considerado equivalente. Assim, metodos que utilizarem esse metodo deverao
	 * esperar pelo menos o segundo frame quando o estado da tecla ira mudar de JustPressed para Pressed.
	 **/
	public bool isPressed()
	{
		if ((this.getEstado() == EstadoKey.Pressed) && (this.getEstado() != EstadoKey.JustPressed))
			return (true);

		return (false);
	}

	/*  Identifica se a tecla acabou de ser pressionada */
	public bool wasJustPressed()
	{
		if (this.getEstado() == EstadoKey.JustPressed)
			return (true);
		
		return (false);
	}

	/* Identifica se a tecla acabou de ser liberada */
	public bool wasReleased()
	{
		if (this.getEstado() == EstadoKey.JustReleased)
		    return (true);
		
		return (false);
	}

	// Atualiza o estado dos botoes
	public void updateStatus()
	{
			
			if (Input.GetButton (this.id) && Input.GetButtonDown (this.id))
				this.estado = EstadoKey.JustPressed;
			else if (Input.GetButton (this.id) && !Input.GetButtonDown (this.id))
				this.estado = EstadoKey.Pressed;
			else if (!Input.GetButton (this.id) && ((this.estado == EstadoKey.Pressed)||(this.estado == EstadoKey.JustPressed))) {
				this.estado = EstadoKey.JustReleased;
			} else {
				this.estado = EstadoKey.Released;
			}
				
		if ((this.estado == EstadoKey.JustPressed) || (this.estado == EstadoKey.Pressed))
			InputManager.ultimoBotao = this;
	}

	// Executa o metodo definido para o KeyPress
	public void ExecActionKeyOn()
	{
		if (this.keyOn != null)
		{			
			//Debug.Log ("Key: " + id + "Acao: " + sAcao);
			this.keyOn(this.pAcao);
		}
	}

	// Executa o metodo definido para o KeyUp
	public void ExecActionKeyUp()
	{
		if (this.keyUp != null)
			this.keyUp();
	}
}


/**
 * Classe de combos de teclas
 * 
 * O id do combo deve ser unico.
 * TODO: Incluir na inicializacao da lista de combos InputManager um codigo para verificar a unicidade
 * 
 */
public class KeyCombo
{	
	private string id;							// indentificado do combo
	private string[]  keys;						// lista de botoes que compoem o combo (inclusive com o estado)
	private EstadoPlayer estadoExecuta;			// estado do Player durante a execucao do combo (isso nao deveria estar aqui)
	private TipoCombo tipo = TipoCombo.None;	// tipo de combo	
	private int       indx = 0;					// indice do botao corrente dentro da lista de botoes do combo
	//private float timeEntreBotoes = 0.3f;		// tempo maximo entre os botoes para considerar os combos (isso nao deveria estar aqui - ja existe um membro no InputManager para isso)
	private string pAcao = "";				    // acao a ser executada quando o combo for completado

	public  bool ativo = false;					// indica que se o combo esta ativo ou nao

	public delegate void AcaoCombo(string sAcao);	// define a assinatura do metodo a ser chamado quando o combo for ativado
	public AcaoCombo comboOn;						// metodo chamado quando o combo eh ativado

	public delegate void AcaoEndCombo();
	public AcaoEndCombo comboEnding;

	// Construtor de um combo que nao possui um metodo associado
	public KeyCombo(string[] keys, string nome, EstadoPlayer estado, TipoCombo tipo = TipoCombo.Sequencia)
	{
		this.id    = nome;
		this.pAcao = nome;
		this.keys  = keys;
		this.estadoExecuta = estado;
		this.tipo = tipo;
	}

	// Construtor de um combo que possui um metodo associado a ser invocado quando o combo for ativado
	public KeyCombo(string[] keys, string nome, EstadoPlayer estado, AcaoCombo acao, TipoCombo tipo = TipoCombo.Sequencia)
	{
		this.id    = nome;		
		this.pAcao = nome;
		this.keys  = keys;
		this.estadoExecuta = estado;
		this.tipo    = tipo;
		this.comboOn = acao;
	}

	// Construtor de um combo que possui um metodo associado a ativacao e outro associado ao termino do combo
	public KeyCombo(string[] keys, string nome, EstadoPlayer estado, AcaoCombo acao, AcaoEndCombo fimCombo, TipoCombo tipo = TipoCombo.Sequencia)
	{
		this.id    = nome;
		this.pAcao = nome;
		this.keys  = keys;
		this.estadoExecuta = estado;
		this.tipo    = tipo;
		this.comboOn = acao;
		this.comboEnding = fimCombo;
	}

	// Construtor de um combo que possui um metodo associado a ativacao e outro associado ao termino do combo
	public KeyCombo(string[] keys, string nome, EstadoPlayer estado, string triggerAcao, AcaoCombo acao, AcaoEndCombo fimCombo, TipoCombo tipo = TipoCombo.Sequencia)
	{
		this.id    = nome;
		this.pAcao = triggerAcao;
		this.keys  = keys;
		this.estadoExecuta = estado;
		this.tipo    = tipo;
		this.comboOn = acao;
		this.comboEnding = fimCombo;
	}

	// Verifica se ocorreu um combo
	public bool Check(List<GameKey> gameKeys)
	{
		if (gameKeys == null)
			throw new System.ArgumentNullException ("keysPress");

		if (Time.time > InputManager.timeUltBotao + InputManager.timeKeyCombo) 
			indx = 0;
		else
		{
			if (indx < keys.Length)
			{
				GameKey gk = gameKeys.Find(delegate(GameKey k){ return (k.id == keys[indx]); });
				if (gk != null)
				{
					if (((this.tipo == TipoCombo.Sequencia) && (gk.estado == EstadoKey.JustPressed)) ||
					    ((this.tipo == TipoCombo.Agrupado) && ((gk.estado == EstadoKey.Pressed) || (gk.estado == EstadoKey.JustPressed))))
					indx++;
				}
				else
				{
					return (false);
				}
				
				if (indx >= keys.Length)
				{
					indx = 0;
					return (true);
				}
				else return (false);
			}
			
		}
		
		return (false);
	}

	// Retorna o tipo do combo
	public TipoCombo getTipo()
	{
		return (this.tipo);
	}

	// Retorna o identificador do combo
	public string getId()
	{
		return (this.id);
	}

	// Ativa o combo
	public void ativa()
	{
		this.ativo = true;
		if (this.comboOn != null)
			comboOn (this.pAcao);
 	}

	// Desativa o combo
	public void desativa()
	{
		if ((this.ativo) && (this.comboEnding != null))
		    this.comboEnding();

		this.ativo = false;
	}
}


public class InputManager : MonoBehaviour {
	/* BOTOES do Input */
	public static string jabKey     = "Jab";
	public static string cruzEsqKey = "CruzadoEsq";
	public static string cruzDirKey = "CruzadoDir";
	public static string diretoKey  = "Direto";
	public static string guardaEsqKey = "GuardaEsq";
	public static string guardaDirKey = "GuardaDir";
	public static string upKey     = "up";
	public static string downKey   = "down";
	public static string leftKey   = "left";
	public static string rightKey  = "right";
	
	public static float timeUltBotao = 0;			// quando o ultimo botao foi pressionado
	public static float timeKeyCombo = 0.15f;		// tempo maximo para pressionar teclas dentro de um combo (combos sequenciais)
	//private static float timeKeyPress = 0.3f;		

	public static GameKey ultimoBotao = null;		// qual a ultima tecla pressionada
	public static bool hasActiveCombo = false;				// tem algum combo ativado?
	public static KeyCombo comboAtivo = null;

	public static List<GameKey>  gameKeys = new List<GameKey>();	// lista de botoes do jogo
	private List<KeyCombo> combos = new List<KeyCombo>();			// lista de combos do jogo	


	// Inicializa a lista de botoes do jogo
	public void InitializeGameKeys (List<GameKey> keys)				
	{
		gameKeys = keys;
	}

	// Inicializa a lista de combos do jogo
	public void InitializeCombos(List<KeyCombo> combos)
	{
		this.combos = combos;
	}
	
	// Inicializaçao
	void Start () {
		timeUltBotao = Time.time;
	}

	// Atualiza o estado das teclas do jogos e verifica se foram feitos combos
	public void UpdateKeyboard()
	{
		int numKeys = 0;
		GameKey gkPressed = null;
		//if (hasActiveCombo && (timeToCombo() && !someKeyPressed())) 	// so atualiza o estado das teclas quando nao tem combo sendo executado
        
		// Atualiza o status das teclas
		foreach (GameKey gk in gameKeys)
		{
			gk.updateStatus();

			if (gk.wasJustPressed() || gk.isPressed())
			{
			    numKeys++;
				gkPressed = gk;
			}

			if ((gk.getEstado() == EstadoKey.JustPressed) && (numKeys > 1)) 
			{
				InputManager.timeUltBotao = Time.time;
			}
		}

		if (numKeys == 1)
		{
			if (!timeToCombo())	
			{
				// Executa a acao atraves de um delegate, caso a tecla tenha sido pressionado ou
				// repete a acao caso a tecla seja mantida pressionada e ela tenha uma acao definida
				// para quando mantida pressionada
				if (gkPressed.wasJustPressed())
					InputManager.timeUltBotao = Time.time;

				if (gkPressed.wasJustPressed() || (gkPressed.isPressed() && gkPressed.acaoPressionado))								
					gkPressed.ExecActionKeyOn();
				
				// Executa a acao definida para quando a tecla for liberada (KeyUp)
				if (gkPressed.wasReleased())
					gkPressed.ExecActionKeyUp();
			}
		}

		if (hasActiveCombo)   // se tem um combo ativo, checa apenas ele
		{
			if (!comboAtivo.Check(gameKeys))
			{
				hasActiveCombo = false;
				comboAtivo.desativa();
				comboAtivo = null;
			}

		}
		else
		{
			// Verifica se algum combo foi ativado
			foreach (KeyCombo kc in combos)
			{
				if (kc.Check(gameKeys))
				{
					//Debug.Log ("Combo Ativo: " + kc.getId());
					hasActiveCombo = true;
					comboAtivo = kc;
					kc.ativa();				
				}
			/*	else
				{
					hasActiveCombo = false;
					if (kc.ativo)
					{
						Debug.Log ("Desativando Combo: " + kc.getId());
						comboAtivo = null;
						kc.desativa();
					}
				}  */
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdateKeyboard ();	
	}

	// Consulta o estado de uma tecla sem ter que acessa diretamente
	public static EstadoKey getStatusKey(string key)
	{
		GameKey gk = gameKeys.Find(delegate(GameKey k){ return (k.id == key); });
		return (gk.getEstado());
	}

	// Consulta se uma tecla acabou de ser pressionada sem acessa-la diretamente
	public static bool wasJustPressed(string key)
	{
		GameKey gk = gameKeys.Find(delegate(GameKey k){ return (k.id == key); });
		return (gk.estado == EstadoKey.JustPressed);
	}

	// Consulta se uma tecla esta pressionada sem acessa-la diretamente
	public static bool isPressed(string key)
	{
		GameKey gk = gameKeys.Find(delegate(GameKey k){ return (k.id == key); });
		return (gk.estado == EstadoKey.Pressed);
	}

	// Consulta se ha alguma tecla pressionada
	public static bool someKeyPressed()
	{
		GameKey gk = gameKeys.Find(delegate(GameKey k){ return (k.getEstado() == EstadoKey.Pressed); });
		return (gk != null);
	}

	// Verifica se ainda ha tempo para pressionar uma tecla dentro do tempo de um combo
	public bool timeToCombo()
	{
		if ((Time.time - timeUltBotao) > timeKeyCombo)
			return (false);

		return (true);
	}

	// Verifica se o tempo para pressionar uma tecla sem que ela entre na composicao de um combo eh suficiente
	public bool timeToKey()
	{
		if (!timeToCombo()) 
			return (true);
	
		return (false);
	}



}
