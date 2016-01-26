using UnityEngine;
using System.Collections;

public class Initialize_Materials : MonoBehaviour {

	public Material BretarisParticle1;
	public Material BretarisParticle2;
	public Material BretarisParticle3;

	public Color Material_Color;

	// Use this for initialization
	void Awake () {
		BretarisParticle1.color = Material_Color;
		BretarisParticle2.color = Material_Color;
		BretarisParticle3.color = Material_Color;

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
