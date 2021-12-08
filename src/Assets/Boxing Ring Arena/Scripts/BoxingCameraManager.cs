using UnityEngine;
using System.Collections;

//*****************************************************************
//a script for keeping the camera at an offset to the parent (cube) game object
//based on destance between two players (game objects).
//*****************************************************************
public class BoxingCameraManager : MonoBehaviour 
{
	//-------------------------------------------------------------
	public GameObject player2 			= null;
	public GameObject Player1 			= null;
	public GameObject emptyGameObject 	= null;//an empty game object to use for transform calculations
	public Camera gameCamera 			= null;//the main camera being used

	public float DistPlayersMin 		= 0.0f;//the minimum distance between the two players

	public float DistPlayersMax 		= 12.0f;//the maximum distance between the two players

	public float CameraMinPosY 			= 0.0f;//when the distance between the two players is at min 
											   //this is the offset on y the camera will be from the parent.
	public float CameraMaxPosY 			= 10.0f;//when the distance between the two players is at max 
												//this is the offset on y the camera will be from the parent.

	public float CameraMinPosZ 			= 0.0f;//when the distance between the two players is at min 
											   //this is the offset on z the camera will be from the parent.
	public float CameraMaxPosZ 			= 10.0f;//when the distance between the two players is at max 
												//this is the offset on z the camera will be from the parent.

	public float cameraOffsetY			= 0.5f;//the camera looks at the parent, this offset value shifts the
											   //look at above or below the parent on y axis.

	//data to help tend (smooth) camera to new position (if useSmoothing is enabled)...
	public bool useSmoothing 			= false;
	public float smoothSpeed 			= 50.0f;
	private float tend_z 				= 0.0f;
	private float tend_y 				= 0.0f;
	//other data used with iFloatTendTo() function...
	private int mainRefreshRate 		= 0;
	//-------------------------------------------------------------
	void Start () 
	{
		//get the refresh rate. this is used with the smoothing function (iFloatTendTo()).
		//when multiplied by delta time will keep the TendTo value the same whether vsync is on or off.
		mainRefreshRate = Screen.currentResolution.refreshRate;
	}
	//-------------------------------------------------------------
	//
	void Update () 
	{
		//---------------------------------------------------------
		//if all our objects are present proceed...
		if (player2 != null && Player1 != null && emptyGameObject != null && gameCamera != null) 
		{
			//*****************************************************
			//offset for this gameobject the script belongs to.
			//keeps the parent (cube) gameobject at the center point between the two players.
			//*****************************************************
			//get distance between the two players
			float Length = Vector3.Distance (player2.transform.position,Player1.transform.position);
			//create a vector with a length of half the distance between the two players
			Vector3 v1 = new Vector3(0.0f,0.0f,(Length/2.0f));
			//use the empty tranform position to copy the player 1 (target for offset) transform position
			emptyGameObject.transform.position = Player1.transform.position;
			//make empty transform to at the target (player 2)
			emptyGameObject.transform.LookAt (player2.transform);
			//orientate our offset vector
			Quaternion q1 = emptyGameObject.transform.rotation;
			v1 = q1 * v1;
			//set the position of our cube at the offset from player 1 position
			transform.position = new Vector3(Player1.transform.position.x,
			                                 Player1.transform.position.y,
			                                 Player1.transform.position.z) + v1;
			//make the cube look at player 1
			transform.LookAt (Player1.transform);
			//*****************************************************
			//camera offset to place relative to gameobject and looking at the (cube) gameobject this script belongs to
			//*****************************************************
			//create some offsets based on the distance between the two players (length)
			float Pos_Y = iFloatInterpolate (Length,DistPlayersMin,DistPlayersMax,CameraMinPosY,CameraMaxPosY);
			float Pos_Z = iFloatInterpolate (Length,DistPlayersMin,DistPlayersMax,CameraMinPosZ,CameraMaxPosZ);
			//create a vector based on the above offsets
			Vector3 CameraOffset = new Vector3(0.0f,Pos_Y,-Pos_Z);
			//tend to new position if smoothing is enabled
			if(useSmoothing)
			{
				tend_y = iFloatTendTo (tend_y,Pos_Y,smoothSpeed);
				tend_z = iFloatTendTo (tend_z,Pos_Z,smoothSpeed);
				CameraOffset.y = tend_y;
				CameraOffset.z = -tend_z;
			}
			//orientate the camera offset by the cube look at orientation and add a -90 degree rotation
			CameraOffset = q1 * Quaternion.Euler (0.0f,-90.0f,0.0f) * CameraOffset;
			//create the final offset by adding the cube position and the camera offset
			CameraOffset = new Vector3(transform.position.x,transform.position.y,transform.position.z) + CameraOffset;
			//apply the final offset position to the camera
			gameCamera.transform.position = CameraOffset;
			//make the camera look at the cube
			v1 = new Vector3(transform.position.x,(transform.position.y+cameraOffsetY),transform.position.z);
			gameCamera.transform.LookAt (v1);
			//*****************************************************
			//make player1 and player2 look at eachother (though at thier own height (y) to keep them looking staight)...
			//*****************************************************
			v1 = new Vector3(Player1.transform.position.x,
			                 player2.transform.position.y,
			                 Player1.transform.position.z);
			emptyGameObject.transform.position = v1;
			player2.transform.LookAt (emptyGameObject.transform);
			v1 = new Vector3(player2.transform.position.x,
			                 Player1.transform.position.y,
			                 player2.transform.position.z);
			emptyGameObject.transform.position = v1;
			Player1.transform.LookAt (emptyGameObject.transform);

		}
	}
	//*************************************************************
	//a function to interpolate between a DestMin and DestMax by ValueInput and it's source min and max values.
	//e.g.
	//float Length = Vector3.Distance (GameObject_1.transform.position,GameObject_2.transform.position);
	//float Pos_Z = iFloatInterpolate(Length,1.0f,10.0f,0.0f,50.0f);
	//transform.position.z = Pos_Z;
	//
	//in the above example the position of z will interpolate (lerp) between 0.0f and 50.0f
	//depending on the distance between GameObject_1 and GameObject_2 is between 1.0f and 10.0f.
	//
	//*************************************************************
	float iFloatInterpolate(float ValueInput,float SourceMin,float SourceMax,float DestMin, float DestMax)
	{
		//---------------------------------------------------------
		float lerp_Input = (ValueInput - SourceMin)/(SourceMax - SourceMin);
		if(lerp_Input > 1.0f){lerp_Input = 1.0f;}
		else if(lerp_Input < 0.0f){lerp_Input = 0.0f;}
		return (DestMin*(1.0f-lerp_Input)+DestMax*lerp_Input);
		//---------------------------------------------------------
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
		if (TendSpeed < 1.0f) //tend speed must be greater or equal to 1.0f
		{
			TendSpeed = 1.0f;
		}
		//---------------------------------------------------------
		TendValue = TendValue + ((TargetValue - TendValue)/TendSpeed) * ((float)mainRefreshRate * Time.deltaTime);
		return TendValue;
		//---------------------------------------------------------
	}
	//-------------------------------------------------------------
}
