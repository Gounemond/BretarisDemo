using UnityEngine;
using System.Collections;

public class InizializeMaterialsParticleBronchi : MonoBehaviour {

	public Material BretarisParticleSpiral;
		
		
		public Color Material_Color;
		
		// Use this for initialization
		void Awake () {
		BretarisParticleSpiral.color = Material_Color;

		}
		
		
	}
