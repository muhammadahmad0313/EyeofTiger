using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AcertaSocoCOM : MonoBehaviour {

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
			if (controllerCOM.estado == EstadoPlayer.Atacando) {

				if ((colidiuCom == "LuvaEsq") || (colidiuCom == "LuvaDir"))
				{
					audio.clip = somGolpeGuarda;
					controllerCOM.SendMessage("AddGolpe");
					controllerJogador.SendMessage("AplicaDano", dano);
					colisaoValida = true;
				}

				if ((colidiuCom == "AnteBracoEsq") || (colidiuCom == "AnteBracoDir") ||
				    (colidiuCom == "BracoEsq") || (colidiuCom == "BracoDir"))
				{
					audio.clip = somGolpeGuarda2;
					controllerCOM.SendMessage("AddGolpe");
					controllerJogador.SendMessage("AplicaDano", dano);
					colisaoValida = true;
				}

				if ((colidiuCom == "Cabeca") || (colidiuCom == "Dorso"))
				{
					if (col.gameObject.CompareTag("LuvaEsqCOM"))
						audio.clip = somJab;
					else if (col.gameObject.CompareTag("LuvaDirCOM"))
						audio.clip = somDireto;

					controllerCOM.SendMessage("AddGolpeEfetivo");
					controllerJogador.SendMessage("AplicaDano", dano*2.0f);
					colisaoValida = true;
				}

				if (colidiuCom == "Abdomen")
				{
					audio.clip = somUgh;
					controllerCOM.SendMessage("AddGolpeEfetivo");
					controllerJogador.SendMessage("AplicaDano", dano*4.0f);
					colisaoValida = true;
				}

				if (colisaoValida)
				{
					//Debug.Log (col.collider.gameObject.tag + " teve " + col.contacts.Length + "colisoes");

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

