using UnityEngine;
using System.Collections;

public class TimeAnimationHandler : MonoBehaviour 
{
	public Animator cameraAnimator;
	public float animationSpeed = 1;
	public float timeScale = 1;


	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		cameraAnimator.speed = animationSpeed;
		Time.timeScale = timeScale;
	
	}
}
