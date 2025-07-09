using UnityEngine;
using System.Collections;

public class BarraVida : MonoBehaviour {

	public GUIStyle barraVida;
	public Texture2D imageninterior;
	public Texture2D imagenexterior;
	public float energiajugador=256f;
	
	//Se ejecuta cada instante del juego
	void OnGUI(){
		 GUI.BeginGroup (new Rect (10, 10, 256, 32));
		 GUI.Box (new Rect (0, 0, 256, 32), imageninterior, barraVida);

		 GUI.BeginGroup (new Rect (0, 0, energiajugador, 32));
		 GUI.Box (new Rect (0, 0, 256, 32), imagenexterior, barraVida);

		 GUI.EndGroup ();
		 GUI.EndGroup ();
	}
	void reducirVida(){
				energiajugador -=1.0f;
		//si la vida llega a cero se aplicara el mundo perdedor
		}
}