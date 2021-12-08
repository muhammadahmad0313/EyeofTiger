using UnityEngine;
using System.Collections;

public class EncontaCordas : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "COM")
		{
			Debug.Log("Colidiu com as cordas - ");
			col.gameObject.SendMessage("MoveUltPos");
		}
	}
	
	void OnCollisionExit(Collision col)
	{
		if (col.gameObject.tag == "COM")
		{
			Debug.Log("DESColidiu com as cordas");	
		}
	}
}
