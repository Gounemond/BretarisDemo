using UnityEngine;
using System.Collections;

public class Set_Time : MonoBehaviour {

	public float TimeSpeed;
	public bool WithTrigger;
	public float TimeSpeedTrigger;
	float count=0;
	bool Triggered;
	// Use this for initialization
	void Awake () {
	
		if(WithTrigger==false)
		Time.timeScale = TimeSpeed;

	}
	
	// Update is called once per frame
	void Update () {
	
		if (WithTrigger == true && Triggered == true && count == 0) {
			Time.timeScale = TimeSpeedTrigger;
			count++;
		}

	}

	bool OnTriggerEnter(Collider Mycollider){

		Triggered = true;
		return Triggered;
	}
}
