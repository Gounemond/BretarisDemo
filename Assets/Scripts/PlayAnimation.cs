using UnityEngine;
using System.Collections;

public class PlayAnimation : MonoBehaviour {

	public string animationName;
	public bool playAnimation = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (playAnimation)
		{
			PlayThisAnimation();
			playAnimation = false;
		}
	}
	
	void PlayThisAnimation()
	{
		GetComponent<Animator>().Play (animationName);
	}
}
