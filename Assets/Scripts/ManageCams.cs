using UnityEngine;
using System.Collections;

public class ManageCams : MonoBehaviour {
	public Camera cam1;
	public Camera cam2;

	private float duration  = 0.2F;
	private float magnitude = 0.1F;
	private float speed     = 1.0F;
	public bool habilita1   = true;

	void Start() {
		cam1.enabled = habilita1;
		cam2.enabled = !cam1.enabled;
	}
	
	void Update() {
		
		if (Input.GetKeyDown (KeyCode.F1)) {
				cam1.enabled = true;
				cam2.enabled = !cam1.enabled;
		} else if (Input.GetKeyDown (KeyCode.F2)) {
				cam1.enabled = false;
				cam2.enabled = !cam1.enabled;
		} else if (Input.GetKeyDown (KeyCode.M)) {
			if (cam1.enabled)
			{
				StopAllCoroutines();
				StartCoroutine("Shake");
			}
		}
	}

	IEnumerator Shake() {
		
		float elapsed = 0.0f;
		
		Vector3 originalCamPos = Camera.main.transform.position;
		float randomStart = Random.Range (-1000.0f, 1000.0f);
		
		while (elapsed < duration) {
			
			elapsed += Time.deltaTime;          
			
			float percentComplete = elapsed / duration;         
			float damper = 1.0f - Mathf.Clamp(2.0f * percentComplete - 1.0f, 0.0f, 1.0f);
			float alpha = randomStart + speed * percentComplete;
			
			// map value to [-1, 1]
			//float x = Random.value * 2.0f - 1.0f;
			//float y = Random.value * 2.0f - 1.0f;
			float x = SimplexNoise.Noise(alpha, 0.0f, 0.0f, 0.0f)*2.0f - 1.0f;
			float z = SimplexNoise.Noise(0.0f, alpha, 0.0f, 0.0f)*2.0f - 1.0f;

			x *= magnitude * damper;
			z *= magnitude * damper;
			
			Camera.main.transform.position = new Vector3(x, originalCamPos.y, z);
			
			yield return null;
		}
		
		Camera.main.transform.position = originalCamPos;
	}
}