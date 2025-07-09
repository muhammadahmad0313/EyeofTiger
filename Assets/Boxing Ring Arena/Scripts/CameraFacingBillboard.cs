using UnityEngine;
using System.Collections;

//*****************************************************************
//used for lens flares on lighting mesh to make the face the camera
//*****************************************************************
public class CameraFacingBillboard : MonoBehaviour
{
	//-------------------------------------------------------------
	//camera as input for this game object to look at
	public Camera m_Camera = null;
	//-------------------------------------------------------------
	void Update()
	{
		//---------------------------------------------------------
		//if all our objects are present proceed...
		if (m_Camera != null) 
		{
			transform.LookAt (transform.position + m_Camera.transform.rotation * Vector3.back,
			              	  m_Camera.transform.rotation * Vector3.up);
		}
	}
	//-------------------------------------------------------------
}
