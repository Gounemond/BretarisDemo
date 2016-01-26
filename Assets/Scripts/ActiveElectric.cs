using UnityEngine;
using System.Collections;

public class ActiveElectric : MonoBehaviour {

	public GameObject ElectricParticle;

	// Use this for initialization
	void Awake () {
		ElectricParticle.SetActive (false);
	}
	
	// Update is called once per frame
	void OnTriggerEnter (Collider MyCollider) {
		if (MyCollider.tag == "ElichettaCollider") {
			ElectricParticle.SetActive (true);

		}



		}

	void OnTriggerExit(Collider MyCollider){
		ElectricParticle.SetActive (false);
	}


}
