using UnityEngine;
using System.Collections;

//*****************************************************************
//a script to place the projector at an offset from a bone and orientation
//relative to the player
//*****************************************************************
public class PlayerProjectorFakeShadow : MonoBehaviour 
{
	//-------------------------------------------------------------
	public GameObject parentObject 			= null;//a gameobject to move this projector to.
	public GameObject meshBone 				= null;//a gameobject to move this projector to.
	
	public float offsetX 					= 0.0f;//offset from target position on x
	public float offsetY 					= 0.0f;//offset from target position on y
	public float offsetZ 					= 0.0f;//offset from target position on z

	//euler orientation - to start make them the same as your projector as in the editor
	public float eulerOrientX 				= 0.0f;
	public float eulerOrientY 				= 0.0f;
	public float eulerOrientZ 				= 0.0f;

	//-------------------------------------------------------------
	//on start
	void Start () 
	{
		//---------------------------------------------------------
		//---------------------------------------------------------
	}
	//-------------------------------------------------------------
	//on update
	void Update () 
	{
		//---------------------------------------------------------
		//if all our objects are present proceed...
		if (parentObject != null && meshBone !=null) 
		{
			//offset position
			Vector3 v1 = new Vector3(offsetX,offsetY,offsetZ);
			v1 = parentObject.transform.rotation * v1;
			Vector3 v2 = new Vector3(meshBone.transform.position.x+v1.x,
			                         meshBone.transform.position.y+v1.y,
			                         meshBone.transform.position.z+v1.z);
			//offset orientation
			Quaternion q1 = new Quaternion(0.0f,0.0f,0.0f,1.0f);
			q1 = Quaternion.Euler (eulerOrientX,parentObject.transform.rotation.eulerAngles.y+eulerOrientY,offsetZ);

			//apply position and orientation
			transform.position = v2;
			transform.rotation = q1;
		}
	}
	//-------------------------------------------------------------
}
