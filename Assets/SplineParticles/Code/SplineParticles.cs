/// <summary>
/// Author: Cesar Rios (Pigtail Games) 2013
/// Spline particles.
/// </summary>

using UnityEngine;
using System.Collections;



namespace PigtailGames
{
[RequireComponent(typeof(ParticleSystem))]
public class SplineParticles : BezierSplineComponent {
	
	/// <summary>
	/// This will determine the number of points that the animationcurves are going to have. A new key frame will we add in each pathQuality interval
	/// </summary>
	[HideInInspector]
	public float pathQuality 		= 0.01f;
	
	/// <summary>
	/// When simplifying the path this is the error value that is used to determine if we have to remove a keyframe or not
	/// </summary>
	[HideInInspector]
	public float pathSimplifyError  = 0;
	
	
	/// <summary>
	/// The number of loops the particles are going to travell. Wrap mode must be set to "Loop"
	/// </summary>
	[HideInInspector]
	public int 		loopNumber 		= 1;
	
	/// <summary>
	/// Set if we are going to make a spiral movement. Note that it can also be done drawing a spline with spiral form
	/// </summary>
	[HideInInspector]
	public bool		useSpiral;
	
	/// <summary>
	///  Number of loops that he spiral is going to have
	/// </summary>
	[HideInInspector]
	public float	spiralLoops    	= 1;
	
	/// <summary>
	/// The size of each spiral loop
	/// </summary>
	[HideInInspector]
	public float	spiralAmplitude = 1;
	
	/// <summary>
	/// In which axis are we going to create the spiral
	/// </summary>
	public enum SPIRAL_AXIS 
	{
		X,
		Y,
		Z
	}
	
	[HideInInspector]
	public SPIRAL_AXIS spiralAxis;
	
	/// <summary>
	/// This are the curves that are going to be passed to the particleSystem
	/// </summary>
	public AnimationCurve 			velocityCurveX;
	public AnimationCurve 			velocityCurveY;
	public AnimationCurve 			velocityCurveZ;
	
	/// <summary>
	/// When this variable is set to true each change made to the path or inspector attributes will regenerate the spline
	/// </summary>
	public bool						enableContinuosEditorUpdate;
	
	/// <summary>
	/// Auto configures the particle system to be able to use the speed curves
	/// </summary>
	public bool						autoEnableParticleVelocityCurves = true;
	
	
	/// <summary>
	/// Used to autoconfigure the spline the first time
	/// </summary>
	[HideInInspector]
	public bool 					hasCreatedTheCurveOnce;
}
}
