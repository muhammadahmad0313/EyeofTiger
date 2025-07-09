using UnityEngine;
using System.Collections;
//*****************************************************************
//a script to oscillate a game objects euler angles
//*****************************************************************
public class OscillateEulerOrientation : MonoBehaviour
{
	//-------------------------------------------------------------
	//some user set values to define if oscillation is active and the frequency and amplitude
	public bool axisX 					= false;//if true oscillation will effect this axis
	public float frequencyX 			= 0.0f;//controls the frequency of oscillation
	public float amplitudeX 			= 0.0f;//controls the range in degrees (amplitude) of oscillation
	public bool axisY = false;
	public float frequencyY 			= 0.0f;//controls the frequency of oscillation
	public float amplitudeY 			= 0.0f;//controls the range in degrees (amplitude) of oscillation
	public bool axisZ = false;
	public float frequencyZ 			= 0.0f;//controls the frequency of oscillation
	public float amplitudeZ 			= 0.0f;//controls the range in degrees (amplitude) of oscillation
	//-------------------------------------------------------------
	//some values to handle timing
	private int mainRefreshRate 		= 0;
	private float MyTime 				= 0.0f;
	private float MyTimeScale 			= 0.01f;

	//values to hold our oscillation euler angles
	private float eor_x 				= 0.0f;
	private float eor_y 				= 0.0f;
	private float eor_z 				= 0.0f;

	//values to store our GameObject start euler rotations
	private float startValueX					= 0.0f;
	private float startValueY					= 0.0f;
	private float startValueZ					= 0.0f;
	//a quaternion to store our GameObject start orientation
	private Quaternion quatOriginal		= new Quaternion(0.0f,0.0f,0.0f,1.0f);
	void Start () 
	{
		//---------------------------------------------------------
		//set up our start values based on GameObject transforms
		quatOriginal = transform.rotation;

		startValueX = quatOriginal.eulerAngles.x;
		startValueY = quatOriginal.eulerAngles.y;
		startValueZ = quatOriginal.eulerAngles.z;

		mainRefreshRate = Screen.currentResolution.refreshRate;
		//---------------------------------------------------------
	}
	//-------------------------------------------------------------
	void Update () 
	{
		//---------------------------------------------------------
		//update time
		MyTime += MyTimeScale * (mainRefreshRate * Time.deltaTime);
		//---------------------------------------------------------
		//calculate oscillation
		if (axisX) 
		{
			eor_x = iFloatOscillate(startValueX,frequencyX,amplitudeX,MyTime);
		}
		if (axisY) 
		{
			eor_y = iFloatOscillate(startValueY,frequencyY,amplitudeY,MyTime);
		}
		if (axisZ) 
		{
			eor_z = iFloatOscillate(startValueZ,frequencyZ,amplitudeZ,MyTime);
		}
		Quaternion q1 = Quaternion.Euler (eor_x, eor_y, eor_z);

		transform.rotation = q1;//quatOriginal * q1;
		//---------------------------------------------------------
	}
	//*************************************************************
	//a function to oscilate a float variable via a frequency and amplitude
	//*************************************************************
	float iFloatOscillate(float StartValue,float Frequency,float Amplitude,float fTime)
	{
		//---------------------------------------------------------
		float Value = StartValue + Mathf.Cos(fTime * Frequency) * Amplitude;
		return Value;
		//---------------------------------------------------------
	}
	//-------------------------------------------------------------
}
