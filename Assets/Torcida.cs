using UnityEngine;
using System.Collections;

public class Torcida : MonoBehaviour {
	GameObject[] torcedores;
	Animation anima;
	float intervalo = 5.0f;
	string[] clips = new string[6];

	// Use this for initialization
	void Start () {
		torcedores = GameObject.FindGameObjectsWithTag ("Torcida");
		clips[0] = "idle";
		clips[1] = "applause";
		clips[2] = "applause2";
		clips[3] = "celebration";
		clips[4] = "celebration2";
		clips[5] = "celebration3";

	}
	
	// Update is called once per frame
	void Update () {
		Random.seed = (int)System.DateTime.Now.Ticks;
		intervalo -= Time.deltaTime;

		if (intervalo < 0)
		{
			Debug.Log ("Intervalo!");
			intervalo = 5.0f;
			Debug.Log ("Quantos torcedores: " + torcedores.Length);
			for (int i=0; i < torcedores.Length; i++)
			{

					int indexAnima = Random.Range (0,5);
					anima = torcedores[i].GetComponent<Animation>();					
					anima.CrossFade(clips[indexAnima], 0.1f);

			}
		}
	
	}

	string ClipIndexToName(int index, Animation animation)
	{
		AnimationClip clip = GetClipByIndex(index, animation);
		if (clip == null)
			return null;
		return clip.name;
	}
	
	AnimationClip GetClipByIndex(int index, Animation animation)
	{
		int i = 0;

		foreach (AnimationState animationState in animation)
		{
			if (i == index)
				return animationState.clip;
			i++;
		}
		return null;
	}
}
