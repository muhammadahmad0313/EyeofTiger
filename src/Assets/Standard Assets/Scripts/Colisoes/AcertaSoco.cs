using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AcertaSoco : MonoBehaviour {

	public enum TipoColisao { LuvaTorax=0, LuvaAbdomen, LuvaCabeca, LuvaAnteBraco, LuvaBraco, LuvaLuva, None };
	public AudioClip somJab;
	public AudioClip somDireto;
	public AudioClip somGancho;
	public AudioClip somCruzado;
	public AudioClip somGolpeGuarda;
	public AudioClip somGolpeGuarda2;
	public AudioClip somUgh;
	AudioSource audio;
	Animator animator;
	private GameObject jogador;
	private GameObject inimigo;
	private BoxerController controllerJogador;
	private COMController controllerCOM;
	private float dano = 0.5f;
	private float limiarImpactoSom = 1;
	
	void Start() {
		   audio = GetComponent<AudioSource>();
		 jogador = GameObject.FindGameObjectWithTag("Player");
		 inimigo = GameObject.FindGameObjectWithTag ("COM");
		animator = jogador.GetComponent<Animator>();
		    controllerCOM = inimigo.GetComponent<COMController> ();
		controllerJogador = jogador.GetComponent<BoxerController> ();
	}


	void OnCollisionEnter(Collision col)
	{

		bool colisaoValida = false;
		AnimatorStateInfo animAtual = animator.GetCurrentAnimatorStateInfo (0);
		int estadoNome = animAtual.GetHashCode ();

		string colidiuCom = col.collider.gameObject.tag;

		if ((controllerCOM != null) && (controllerJogador != null))
		{
			if (controllerJogador.estado == EstadoPlayer.Atacando)
			{

				if ((colidiuCom == "LuvaEsqCOM") || (colidiuCom == "LuvaDirCOM"))
				{
					audio.clip = somGolpeGuarda;
					controllerJogador.SendMessage("AddGolpe");
					controllerCOM.SendMessage("AplicaDano", dano);
					colisaoValida = true;
				}

				if ((colidiuCom == "AnteBracoEsqCOM") || (colidiuCom == "AnteBracoDirCOM") ||
				    (colidiuCom == "BracoEsqCOM") || (colidiuCom == "BracoDirCOM"))
				{
					audio.clip = somGolpeGuarda2;
					controllerJogador.SendMessage("AddGolpe");
					controllerCOM.SendMessage("AplicaDano", dano);
					colisaoValida = true;
				}

				if ((colidiuCom == "CabecaCOM") || (colidiuCom == "DorsoCOM"))
				{
					if (col.gameObject.CompareTag("LuvaEsq"))
						audio.clip = somJab;
					else if (col.gameObject.CompareTag("LuvaDir"))
						audio.clip = somDireto;

					controllerJogador.SendMessage("AddGolpeEfetivo");
					controllerCOM.SendMessage("AplicaDano", dano*2);
					colisaoValida = true;
				}

				if (colidiuCom == "AbdomenCOM")
				{
					audio.clip = somUgh;
					controllerJogador.SendMessage("AddGolpeEfetivo");
					controllerCOM.SendMessage("AplicaDano", dano*4);
					colisaoValida = true;
				}

				if (colisaoValida)
				{

					// Calcula o volume
					float volume = Mathf.InverseLerp(limiarImpactoSom, limiarImpactoSom*4, col.relativeVelocity.magnitude);
					
					// Toca o som correspondente
					audio.volume = volume;
					if (!audio.isPlaying)
						audio.Play();
				}
			}
		}
	}

}

