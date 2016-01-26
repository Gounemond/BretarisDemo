using UnityEngine;
using System.Collections;
using PigtailGames;

public class RuntimeTester : MonoBehaviour {


	public BezierSplineComponent bezierSpline;
	
	private Vector3 originalPointPosition1;
	private Vector3 originalPointPosition2;

	// Use this for initialization
	void Start () 
	{
		originalPointPosition2 = bezierSpline.Spline.m_points[1].m_point;
		originalPointPosition1 = bezierSpline.Spline.m_points[bezierSpline.Spline.m_points.Count-1].m_point;

	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 randomPosition = originalPointPosition1 + bezierSpline.transform.TransformPoint(Vector3.up*Mathf.Sin(Time.timeSinceLevelLoad)*20);
		Vector3 randomPosition2 = originalPointPosition2 + bezierSpline.transform.TransformPoint(Vector3.right*Mathf.Cos(Time.timeSinceLevelLoad*4)*20);
//		randomPosition = bezierSpline.transform.InverseTransformPoint(randomPosition);

		bezierSpline.Spline.m_points[bezierSpline.Spline.m_points.Count-1].m_point = randomPosition; //Assign new position
		bezierSpline.Spline.m_points[1].m_point = randomPosition2; //Assign new position
		bezierSpline.Spline.Build(); //Call build to recalc spline points 

	}
}
