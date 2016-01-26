/// <summary>
/// Author: Cesar Rios (Pigtail Games) 2013
/// </summary>

using UnityEngine;
using System.Collections;


namespace PigtailGames
{
[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class SplineParticlesEmitterFollowPath : MonoBehaviour {

	/// <summary>
	/// The particle path to follow
	/// </summary>
	public SplineParticles 		particlePath;
	
	/// <summary>
	/// This will orient the z axis to the direction of the movement
	/// </summary>
	public bool					orientToPath;
	
	/// <summary>
	/// How much time is going to take to travel through the path. If 0 it will use the particle Duration 
	/// </summary>
	public float 				customTime;
	
	/// <summary>
	/// Set an offset from the origin
	/// </summary>
	public Vector3				offset;
	
	//Cache variables
	private BaseSpline.SplineIterator 	splineIterator;
	private Transform  					splineTansform;
	private Transform					myTransform;
	private ParticleSystem				myParticleSystem;
	
	
	void Start () {
		
		//Cache variables
		if (particlePath != null)
		{
			myTransform = transform;
			
			splineIterator = particlePath.Spline.GetIterator();
			splineTansform = particlePath.transform;
			myParticleSystem = GetComponent<ParticleSystem>();
		}
		else
			Debug.LogWarning("You have to set a path to follow");
	}
	
	
	void Update () {
		
		if (splineIterator == null)
			Start(); //To avoid problems when we are in editmode
		
		else
		{
			float timeToUse =	GetComponent<ParticleSystem>().duration;
			
			if (customTime > 0)  //Use custom time?
				timeToUse = customTime;
					
			splineIterator.SetOffsetPercent(myParticleSystem.time/timeToUse); //Get the position
			
			Vector3 offsetVector = myTransform.right*offset.x + myTransform.up*offset.y + myTransform.forward * offset.z;
			
			myTransform.position = splineTansform.TransformPoint(splineIterator.GetPosition()) + offsetVector;  //Set the position
			
			if (orientToPath) //Change rotation is needed
				myTransform.rotation = Quaternion.LookRotation(splineIterator.GetTangent());
		}
	}
}
}
