using UnityEngine;
using System.Collections;




namespace PigtailGames
{
	//[ExecuteInEditMode]
public class SplineParticlesRuntime : MonoBehaviour 
{

		[Tooltip("Toggle update particle speed. Disable it for increase performance, but particles will loose internal velocity, so velocity overlifetime modules wont work")]
		public bool updateSpeed = true;

		public RuntimeSplineController splineController;
		
		private ParticleSystem		myParticleSystem;
		private ParticleSystem.Particle[]	particleArray;
		private Vector3[]	particlesInitialPositions;

		private RuntimeSplineController.ParticleData particleData = new RuntimeSplineController.ParticleData();
	
		//Cache 
		private float 	particleStartLifetime;
		private float 	particleCurrentLifetime;
		private float 	normalizedLife;
		private int 	i;

	// Use this for initialization
		void Start () 
		{
			//Cache vars
			myParticleSystem	=	GetComponent<ParticleSystem>();
			particleArray = new ParticleSystem.Particle[GetComponent<ParticleSystem>().maxParticles];
			particlesInitialPositions = new Vector3[GetComponent<ParticleSystem>().maxParticles];
		}
	
	
		void LateUpdate () 
		{
		
			int particleCount = myParticleSystem.GetParticles(particleArray);
		
			for (i = 0; i<particleCount ; i++)
			{
				particleStartLifetime 	= particleArray[i].startLifetime;
				particleCurrentLifetime 	= particleArray[i].lifetime;

				if (particleStartLifetime - particleCurrentLifetime <= Time.deltaTime)
				{
					particlesInitialPositions[i] = particleArray[i].position; //Record Initial position
				}
				
				normalizedLife = 1 - particleCurrentLifetime/particleStartLifetime;
				particleData = splineController.GetPositionByLife(normalizedLife,GetComponent<ParticleSystem>().simulationSpace);

				if (updateSpeed)
					particleArray[i].velocity = particleData.speed;

				particleArray[i].position =particleData .position +particlesInitialPositions[i];

			}
		
		myParticleSystem.SetParticles(particleArray,particleCount); 
		
		}
}
}
