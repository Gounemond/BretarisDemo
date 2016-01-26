using UnityEngine;
using System.Collections;

public class LerpColorBretaris : MonoBehaviour {

	public Material BretarisParticlesMaterial;
	public Color Colore;
	bool Triggered;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Triggered == true) 
			BretarisParticlesMaterial.color = Color.Lerp (BretarisParticlesMaterial.color, Colore, Time.time/4000);
	}

	bool OnTriggerEnter(Collider MyCollider){
		
		Triggered = true;
		return Triggered;
		
	}


}
