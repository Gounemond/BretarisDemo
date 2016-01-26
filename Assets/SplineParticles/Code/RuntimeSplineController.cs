using UnityEngine;
using System.Collections;

namespace PigtailGames
{

public class RuntimeSplineController : MonoBehaviour 
{
		[Tooltip("Spline path subdivision. The higher the number the better the particles will follow the spline but at a cost of performance")]
		public int splineDivisions = 16;

		[Tooltip("If you are going to use the particlesystem with Simulation space world you have to enable this. It will have a minnor performance cost")]
		public bool getWorldPosition;

		[Tooltip("Iterpolate spline points for better precission. This needs more CPU resources")]
		public bool highQualityFollow = true;

		[Tooltip("Toggle update particle speed. Disable it for increase performance, but particles will loose internal velocity, so velocity overlifetime modules wont work")]
		public bool updateSpeed = true;

		public class ParticleData
		{
			public Vector3 position;
			public Vector3 speed;
		}


		#region private vars
		private BezierSpline spline;

		private Vector3[] pointArray;
		private Vector3[] pointArrayWorld;
		private Vector3[] speedAtPoint;

		private int pointArrayLenght;

		private ParticleData cacheParticleData = new ParticleData();

		private BaseSpline.SplineIterator iterator;


		#endregion


		void Awake()
		{
			BezierSplineComponent splineComponent = GetComponent<BezierSplineComponent>();

			if (splineComponent == null)
			{
				Debug.LogError("[ERROR] You have to add a BezerierSplineComponent to this GameObject. Disabling...");
				gameObject.SetActive(false);
				return;
			}
			else
			{
				spline = splineComponent.Spline;
				iterator = spline.GetIterator();
				UpdatePointArray();
			}
		}

		void OnEnable()
		{
			if (spline != null)
				spline.OnSplineRebuild += UpdatePointArray;
		}

		void OnDisable()
		{
			spline.OnSplineRebuild -= UpdatePointArray;
		}

		/// <summary>
		/// Updates and cahces de particle positions. Call this method if you modify the transform and you are using WorldSpace particles 
		/// </summary>
		public void UpdatePointArray()
		{
			pointArray = spline.GenerateSplinePoints(splineDivisions);

			if (pointArrayWorld == null)
				pointArrayWorld = new Vector3[pointArray.Length];

			pointArray.CopyTo(pointArrayWorld,0);

			//Position
			if (getWorldPosition)
			{
				for (int i = 0; i< pointArrayWorld.Length; i++)
				{
					pointArray[i] = transform.TransformPoint(pointArray[i]); //Take position rotation and scale into account
				}
			}

			//Speed
			if (updateSpeed)
			{
				if (speedAtPoint == null)
					speedAtPoint = new Vector3[pointArray.Length];

				for (int i = 0; i< speedAtPoint.Length; i++)
				{
					iterator.SetOffsetPercent((float)i/speedAtPoint.Length);
					speedAtPoint[i] = iterator.GetTangent();
				}
			}


			pointArrayLenght = pointArray.Length-1; //We don't rally need the exact lentght , but the last element index
		}

		private Vector3 cacheReturnPoint;
		float appliedNormalizedValue;
		int upperPoint;
		int lowerPoint;
		float t;


		public ParticleData GetPositionByLife(float _normalizedLifeTime, ParticleSystemSimulationSpace _simulationSpace)
		{
			appliedNormalizedValue = _normalizedLifeTime*(pointArrayLenght);
			lowerPoint = Mathf.FloorToInt(appliedNormalizedValue);

			if (highQualityFollow) 
			{
				upperPoint = Mathf.Clamp(lowerPoint+1,0,pointArrayLenght);
				t = Mathf.InverseLerp(lowerPoint,upperPoint,appliedNormalizedValue);

				if (_simulationSpace == ParticleSystemSimulationSpace.World)
					cacheReturnPoint = Vector3.Lerp(pointArrayWorld[lowerPoint],pointArrayWorld[upperPoint], t);
				else
					cacheReturnPoint = Vector3.Lerp(pointArray[lowerPoint],pointArray[upperPoint], t);

				if (updateSpeed)
				{	
					cacheParticleData.speed = Vector3.Lerp(speedAtPoint[lowerPoint],speedAtPoint[upperPoint],t);
				}
			}
			else
			{
				if (_simulationSpace == ParticleSystemSimulationSpace.World)
					cacheReturnPoint = pointArrayWorld[lowerPoint];
				else
					cacheReturnPoint = pointArray[lowerPoint];

				if (updateSpeed)
				{
					cacheParticleData.speed = speedAtPoint[lowerPoint];
				}
			}

			cacheParticleData.position = cacheReturnPoint;

			return cacheParticleData;
		}


}

}