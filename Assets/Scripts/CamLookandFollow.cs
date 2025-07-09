using UnityEngine;
using System.Collections;

/*  
 * Configura a camera para ficar sempre posicionada junto ao boxeador jogavel e olhando para o 
 * boxeador NPC. 
 * 
 */

public class CamLookandFollow : MonoBehaviour {

	public Transform alvo;			
	public Transform observador;
	Transform cabeca;
	private Vector3 posAntCabeca;
	public float distance;

	private float smooth = 10.0f;
	private float altura;
	private Vector3 offset;

	void Start()
	{
		cabeca = SearchHierarchyForBone (observador, "Boxer_head_bone");
		//transform.parent = cabeca;
		transform.position = cabeca.position + (Vector3.forward * distance);
		Camera.main.fieldOfView = 80;
	}

	void Update () 
	{  
		float x = Mathf.Lerp(posAntCabeca.x, cabeca.position.x, Time.deltaTime*smooth); 
		float z = Mathf.Lerp(posAntCabeca.z,cabeca.position.z,Time.deltaTime*smooth);

		transform.position = new Vector3(x,transform.position.y,z) + (Vector3.forward * distance);	
		posAntCabeca = cabeca.position;
		// Aponta a camera para o alvo
		Vector3 targetPosition = new Vector3( alvo.position.x, 
		                                      this.transform.position.y, 
		                                      alvo.position.z ) ;
		this.transform.LookAt( targetPosition ) ;

	}

	Transform SearchHierarchyForBone(Transform current, string name)   
	{
		//Esse eh o bone que estamos procurando?
		if (current.name == name)
			return current;
		
		// busca pelo bone atraves dos childs
		for (int i = 0; i < current.childCount; ++i)
		{
			// chamada recursiva
			Transform found = SearchHierarchyForBone(current.GetChild(i), name);
			
			// se encontrar o bone, retonar ele
			if (found != null)
				return found;
		}
		
		// bone with name was not found
		return null;
	}

}
