using UnityEngine;
using System.Collections;

public class Increase_ParticleEffect : MonoBehaviour {

	ParticleSystem ParticleVariables;
	float time;
	// Use this for initialization
	void Start () {
		ParticleVariables = gameObject.GetComponent<ParticleSystem> ();
	}
	
	// Update is called once per frame
	void Update () {
	
		time += 1.0f * Time.deltaTime;
		if(time>4.0f)
		ParticleVariables.emissionRate = 10.0f;

	}
}
