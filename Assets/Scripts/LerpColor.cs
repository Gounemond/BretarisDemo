using UnityEngine;
using System.Collections;

public class LerpColor : MonoBehaviour {
	public Material BretarisParticlesMaterial;
	bool Triggered;
	public Color Colore;
	Color ColoreParticles;
	float alpha;
	Color col;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	if (Triggered == true) {
			BretarisParticlesMaterial.color = Color.Lerp (BretarisParticlesMaterial.color, Colore, Time.time/200);
			col = BretarisParticlesMaterial.color;
			col.a = Mathf.Lerp(col.a,0f,0.01f);
			BretarisParticlesMaterial.color = col;
		}
	}


	bool OnTriggerEnter(Collider MyCollider){

		Triggered = true;
		return Triggered;

	}

}
