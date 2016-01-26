using UnityEngine;
using System.Collections;

public class Change_Color : MonoBehaviour {

	public Material Bretaris_Material;
	public Color Colore;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void OnTriggerEnter (Collider MyCollider) {
	
		if (MyCollider.tag == "Player") {
			Bretaris_Material.color = Colore;
			Debug.Log("Triggered");
		}
	}



}
