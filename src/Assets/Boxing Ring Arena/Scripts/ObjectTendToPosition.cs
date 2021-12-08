using UnityEngine;
using System.Collections;

//*****************************************************************
//a script to tend a GameObject to the position of another GameObject
//*****************************************************************
public class ObjectTendToPosition : MonoBehaviour
{
	//-------------------------------------------------------------
	public GameObject target 			= null;//a gameobject to move this mesh to.
	//data tend game object to position...
	public float smoothSpeed 			= 1.0f;//(min 1.0f - max - inf) speed of tending (larger the value the slower the tending)

	public float offsetX 				= 0.0f;//offset from target position on x
	public float offsetY 				= 0.0f;//offset from target position on y
	public float offsetZ 				= 0.0f;//offset from target position on z
	//variable for holding the tendto data as it must be persistant.
	private Vector3 tendVector 			= new Vector3 (0.0f, 0.0f, 0.0f);

	//other data used with iFloatTendTo() function...
	private int mainRefreshRate 		= 0;
	//-------------------------------------------------------------
	//on start
	void Start () 
	{
		//---------------------------------------------------------
		//get the refresh rate. this is used with the smoothing function (iFloatTendTo()).
		//when multiplied by delta time will keep the TendTo value the same whether vsync is on or off.
		mainRefreshRate = Screen.currentResolution.refreshRate;
		//---------------------------------------------------------
	}
	//-------------------------------------------------------------
	//on update
	void Update () 
	{
		//---------------------------------------------------------
		//if all our objects are present proceed...
		if (target != null) 
		{
			Vector3 v1 = new Vector3(target.transform.position.x+offsetX,
			                         target.transform.position.y+offsetY,
			                         target.transform.position.z+offsetZ);

			tendVector.x = iFloatTendTo (tendVector.x,v1.x,smoothSpeed);
			tendVector.y = iFloatTendTo (tendVector.y,v1.y,smoothSpeed);
			tendVector.z = iFloatTendTo (tendVector.z,v1.z,smoothSpeed);

			transform.position = tendVector;
		}
	}
	//*************************************************************
	//a function to tend to a certain value without ever reaching it.
	//
	//note: TendValue should be a global value and should also be the return.
	//e.g.
	//private float myTendValue = 0.0f;
	//
	//void Update()
	//{
	// myTendValue = iFloatTendTo(myTendValue,100.0f,10.0f);
	//}
	//
	//*************************************************************
	float iFloatTendTo(float TendValue, float TargetValue, float TendSpeed)
	{
		//---------------------------------------------------------
		if (TendSpeed < 1.0f) //tend speed must be greater or equal to 1.0f
		{
			TendSpeed = 1.0f;
		}
		TendValue = TendValue + ((TargetValue - TendValue)/TendSpeed) * ((float)mainRefreshRate * Time.deltaTime);
		return TendValue;
		//---------------------------------------------------------
	}
	//-------------------------------------------------------------
}
