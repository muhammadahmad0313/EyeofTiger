using UnityEngine;
using System.Collections;

public enum TipoColisao { LuvaTorax=0, LuvaAbdomen, LuvaCabeca, LuvaAnteBraco, LuvaBraco, LuvaLuva, None };

public class OnCollisionPlayer : MonoBehaviour {

	//public enum TipoColisao { LuvaTorax=0, LuvaAbdomen, LuvaCabeca, LuvaAnteBraco, LuvaBraco, LuvaLuva, None };
	/*
	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "CordaRing")
		{
			Debug.Log("Colidiu com as cordas - " + col. );
			col.gameObject.SendMessage("MoveUltPos");
		}
	}

	void OnCollisionExit(Collision col)
	{
		if (col.gameObject.tag == "CordaRing")
		{
			Debug.Log("DESColidiu com as cordas");	
		}
	}
	*/
}
