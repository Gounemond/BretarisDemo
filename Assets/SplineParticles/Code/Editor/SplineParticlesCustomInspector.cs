using UnityEngine;
using System.Collections;
using UnityEditor;


namespace PigtailGames
{

[CustomEditor(typeof(SplineParticles))]
public class SplineParticlesCustomInspector :   BezierSplineEditor {
	
	//The spline path
	public bool			spiral;

	private Quaternion	previousRotation;
	private Vector3 	previousScale;
	private float		previousParticleLifeTime;
	
	
	//Variables for normalize the curves between 0 and 1 to be able to see the curves correctly on the particle editor
	private float 		xVelocityCurveScale = 1;
	private float 		yVelocityCurveScale = 1;
	private float 		zVelocityCurveScale = 1;
	
	//For tangent calculations
	private Vector3		previousPoint;
	
	
	public override void OnSceneGUI()
	{
		base.OnSceneGUI();
		
		SplineParticles particlesFollowPath = target as SplineParticles;
				
		
		if (GUI.changed && particlesFollowPath.enableContinuosEditorUpdate)
			CreateAndSetVelocityCurves(particlesFollowPath);
	}
	
	public override void OnEnable()
	{
		base.OnEnable();
		
		SplineParticles particlesFollowPath = target as SplineParticles;
		
		//Get the original values to know if any change to the particleSystem has been made
		previousRotation = particlesFollowPath.transform.rotation;
		previousScale = particlesFollowPath.transform.localScale;
		previousParticleLifeTime = particlesFollowPath.GetComponent<ParticleSystem>().startLifetime;
	}
	
	public override void OnInspectorGUI ()
	{
		
		base.OnInspectorGUI();
		
		//EditorGUIUtility.LookLikeInspector();
		DrawDefaultInspector();
		
		SplineParticles particlesFollowPath = target as SplineParticles; //Get the target
		
		//Check if we have made any changes to the particle system that affect the sline
		if (particlesFollowPath.enableContinuosEditorUpdate && (particlesFollowPath.transform.rotation != previousRotation || particlesFollowPath.transform.localScale != previousScale || previousParticleLifeTime != particlesFollowPath.GetComponent<ParticleSystem>().startLifetime))
			CreateAndSetVelocityCurves(particlesFollowPath);
		
		previousScale 				= 	particlesFollowPath.transform.localScale;
		previousRotation			=	particlesFollowPath.transform.rotation;
		previousParticleLifeTime	=	particlesFollowPath.GetComponent<ParticleSystem>().startLifetime;
		
		
		//Parameters
		EditorGUILayout.Separator();
		particlesFollowPath.useSpiral = EditorGUILayout.Toggle("Is spiral", particlesFollowPath.useSpiral);
		if (particlesFollowPath.useSpiral)
		{
			particlesFollowPath.spiralLoops 	= EditorGUILayout.FloatField("Spiral loops", particlesFollowPath.spiralLoops);
			particlesFollowPath.spiralAmplitude = EditorGUILayout.FloatField( "Spiral Amplitude", particlesFollowPath.spiralAmplitude);
			particlesFollowPath.spiralAxis 		= (SplineParticles.SPIRAL_AXIS)EditorGUILayout.EnumPopup("Spiral Axis", particlesFollowPath.spiralAxis);
			EditorGUILayout.Separator();
		}
		
	
		
		GUILayout.BeginHorizontal(); 
		GUILayout.Label("   Path subdivisions " + "("+ particlesFollowPath.pathQuality.ToString("0.000") + ")" );
		particlesFollowPath.pathQuality = GUILayout.HorizontalSlider(particlesFollowPath.pathQuality,0.001f,0.25f);
		GUILayout.Label("   Path simplify margin" + "("+ particlesFollowPath.pathSimplifyError.ToString("0.00") + ")");
		particlesFollowPath.pathSimplifyError = GUILayout.HorizontalSlider(particlesFollowPath.pathSimplifyError,0,1);
		GUILayout.EndHorizontal();
		
		if(particlesFollowPath.Spline.WrapMode == BaseSpline.SplineWrapMode.Loop)
		{
			particlesFollowPath.loopNumber 	= EditorGUILayout.IntField("Loops", particlesFollowPath.loopNumber);
		}
		else
			particlesFollowPath.loopNumber = 1;
		
		//Buttons
		
		GUIStyle buttonStyleRed = new GUIStyle("button");
		buttonStyleRed.normal.textColor = Color.red;
		
		GUIStyle buttonStyleGreen = new GUIStyle("button");
		buttonStyleGreen.normal.textColor = Color.green;
		
		if (GUILayout.Button("Create velocity curve", buttonStyleGreen))
		{
			CreateAndSetVelocityCurves(particlesFollowPath);
			
		}
		
		if (GUILayout.Button("Set velocity curve"))
		{
			ChangeVelocityCurve(particlesFollowPath);
		}
		
		
		if (GUILayout.Button("Clear velocity curve", buttonStyleRed))
		{
			ClearCurves(particlesFollowPath);
		}
	
		
		if (GUI.changed && particlesFollowPath.enableContinuosEditorUpdate)
		{
			CreateAndSetVelocityCurves(particlesFollowPath);			
			EditorUtility.SetDirty (particlesFollowPath);
		}
		
		
		
	}
	
	/// <summary>
	/// Creates the veocity curves and Assign them to the particle system
	/// </summary>
	/// <param name='_target'>
	/// _target.
	/// </param>
	private void CreateAndSetVelocityCurves(SplineParticles _target)
	{
		ClearCurves(_target);
		CreateVelocityCurve(_target);
		
		if (_target.useSpiral)
			SetSpiralProperties(_target);
		
		NormelizeAndSimplify(_target);
		
		ChangeVelocityCurve(_target);
		
	}
	
	
	
	/// <summary>
	/// Creates the velocity curve.
	/// </summary>
	/// <param name='_target'>
	/// _target.
	/// </param>
	private void CreateVelocityCurve(SplineParticles _target)
	{
		
		if (_target.pathQuality <= 0)
		{
			Debug.LogWarning("Too much quality on particle follow path");
			return;	
		}
		
		BaseSpline.SplineIterator splineIterator;
		
		if (_target.splinePath == null)
			splineIterator = _target.GetComponent<SplineParticles>().Spline.GetIterator();

		else
			splineIterator = _target.splinePath.Spline.GetIterator();
		
		//Clear curve values
		_target.velocityCurveX = new AnimationCurve();
		_target.velocityCurveY = new AnimationCurve();
		_target.velocityCurveZ = new AnimationCurve();
		
		
		if (_target.loopNumber <= 1) //Set the curve points for each loop
		{
			SetCurvePoints(1, splineIterator, _target, 0);
		}
		else
		{
			for (int i = 0; i < _target.loopNumber; i++)
				SetCurvePoints(_target.loopNumber, splineIterator, _target, (float)i/(float)_target.loopNumber);
		}
		
		//Check if there is any key on the last keyframe, for precision problems
		if (1 - _target.velocityCurveX.keys[_target.velocityCurveX.keys.Length-1].time >= 0.0001f)
		{
				CalculatePoint(1, 1, splineIterator, _target, 0);
		}
		
	}
	
	/// <summary>
	/// Sets the curve points.
	/// </summary>
	private void SetCurvePoints(int _loopNumber, BaseSpline.SplineIterator _iterator, SplineParticles _target, float _offset)
	{
		previousPoint = Vector3.zero;
		
		for (float i = 0; i<=1; i += _target.pathQuality)
		{
			CalculatePoint(i, _loopNumber, _iterator, _target, _offset);
		}
		
	}
	
	/// <summary>
	/// Calculates the point each animation curve point. 
	/// </summary>
	private void CalculatePoint(float _point, int _loopNumber, BaseSpline.SplineIterator _iterator, SplineParticles _target, float _offset)
	{
		
			_iterator.SetOffsetPercent(_point);
			
			//Calc modifiers
			
			float lifeTimeModifier = _target.GetComponent<ParticleSystem>().startLifetime;
			float loopsModifier = _target.loopNumber;
			

			Vector3 currentPosition = _target.transform.TransformPoint(_iterator.GetPosition());
			Vector3 tangentAtPoint = (_iterator.GetTangent().normalized);

			Vector3 velocityAtPoint = Vector3.zero;
			
				
			if (_point == 0 && _target.Spline.WrapMode == BaseSpline.SplineWrapMode.Loop)
			{
				_iterator.SetOffsetPercent(1-_target.pathQuality);
				previousPoint = _target.transform.TransformPoint(_iterator.GetPosition()); 

			}
			else if (_point == 0 && _target.Spline.WrapMode != BaseSpline.SplineWrapMode.Loop)
			{
				_iterator.SetOffsetPercent(_point+_target.pathQuality);
				previousPoint = _target.transform.TransformPoint(_iterator.GetPosition()); 

			}
			
			velocityAtPoint = (currentPosition - previousPoint).magnitude * tangentAtPoint;

				
			previousPoint = currentPosition;
			
			velocityAtPoint *= loopsModifier*(1/_target.pathQuality)/lifeTimeModifier;
			_target.velocityCurveX.AddKey(_point/_loopNumber + _offset,velocityAtPoint.x);
			_target.velocityCurveY.AddKey(_point/_loopNumber + _offset,velocityAtPoint.y);
			_target.velocityCurveZ.AddKey(_point/_loopNumber + _offset,velocityAtPoint.z);
		
		
	}
	
	private void NormelizeAndSimplify(SplineParticles _target)
	{
		//Curve values normalization. To show correctly on the particle velocity curve editor
		
		xVelocityCurveScale = NormalizeCurve(ref _target.velocityCurveX);
		yVelocityCurveScale = NormalizeCurve(ref _target.velocityCurveY);
		zVelocityCurveScale = NormalizeCurve(ref _target.velocityCurveZ);
		
		//Simplify the number of key. This will help to reduce noise or to be easier to tweek the curves on the editor
		_target.velocityCurveX = SimplifyCurve(_target.velocityCurveX, _target);
		_target.velocityCurveY = SimplifyCurve(_target.velocityCurveY, _target);
		_target.velocityCurveZ = SimplifyCurve(_target.velocityCurveZ, _target);
		
	}
	
	/// <summary>
	/// Normalizes the curve to make it show ok in the editor
	/// </summary>

	private float NormalizeCurve(ref AnimationCurve _curveToNormalize)
	{
			
		float maxValue = 0;
		float minValue = 0;
		float normalizationValue = 1;
		
		foreach (Keyframe keyframeIterator in _curveToNormalize.keys)
		{
			if (keyframeIterator.value > maxValue)
				maxValue = keyframeIterator.value;
			if (keyframeIterator.value < minValue)
				minValue = keyframeIterator.value;
		}
		
		if (Mathf.Abs(maxValue) >= Mathf.Abs(minValue)) //Get the max or min value
			normalizationValue = Mathf.Abs(maxValue);
		else
			normalizationValue = Mathf.Abs(minValue);
		
		AnimationCurve tempAnimationCurve = new AnimationCurve();
		
		if (normalizationValue >= 1)
		{
			for (int i = 0; i<_curveToNormalize.keys.Length; i++)
			{
				Keyframe currentKey = _curveToNormalize.keys[i];
				
				tempAnimationCurve.AddKey(currentKey.time,currentKey.value/normalizationValue); 
			}
		
			_curveToNormalize = tempAnimationCurve;
			return normalizationValue;
		}
		else
			return	1;
		
	}
	
	/// <summary>
	/// Simplifies the curve, to reduce noise or to make it eaiser to tweak on the editor
	/// </summary>
	private AnimationCurve SimplifyCurve(AnimationCurve _curveToSimplify, SplineParticles _target)
	{
		AnimationCurve	simplifiedAnimationCurve =  new AnimationCurve();
		
	
		for (int i = 0; i<_curveToSimplify.keys.Length; i++)
		{
			if (i == 0 || i == _curveToSimplify.keys.Length-1)
				simplifiedAnimationCurve.AddKey(_curveToSimplify.keys[i]);
			else
			{
				Keyframe prevKey = _curveToSimplify.keys[i-1];
				Keyframe currentKey = _curveToSimplify.keys[i];
				float inLineTangent = (currentKey.value - prevKey.value)/(currentKey.time-prevKey.time);
				
				// We have to test not only the coordinates but also the tangents
				if (Mathf.Sign(Mathf.Round(inLineTangent*100)/100) != Mathf.Sign(Mathf.Round(currentKey.inTangent*100)/100) ||
					Mathf.Sign(Mathf.Round(inLineTangent*100)/100) != Mathf.Sign(Mathf.Round(prevKey.outTangent*100)/100) ||
					(
					(Mathf.Sign(inLineTangent) == Mathf.Sign(currentKey.inTangent) && Mathf.Abs(Mathf.Abs(inLineTangent)-Mathf.Abs(currentKey.inTangent)) > _target.pathSimplifyError) ||
					(Mathf.Sign(inLineTangent) == Mathf.Sign(currentKey.outTangent) && Mathf.Abs(Mathf.Abs(inLineTangent)-Mathf.Abs(currentKey.outTangent)) > _target.pathSimplifyError)					
					))
					simplifiedAnimationCurve.AddKey(_curveToSimplify.keys[i]);	
			}
		}
		
		return simplifiedAnimationCurve;
		
	}
	
	
	/// <summary>
	/// Sets the spiral properties yo the curves
	/// </summary>
	
	private void SetSpiralProperties(SplineParticles _target)
	{
		AnimationCurve 	tempCurveFirstAxis = new AnimationCurve();
		AnimationCurve 	tempCurveSecondAxis = new AnimationCurve();
		AnimationCurve	targetAnimationCurveFirstAxis 	= new AnimationCurve();
		AnimationCurve	targetAnimationCurveSecondAxis	= new AnimationCurve();
		
		switch (_target.spiralAxis)
		{
			case SplineParticles.SPIRAL_AXIS.X:
				targetAnimationCurveFirstAxis 	= 	_target.velocityCurveZ;
				targetAnimationCurveSecondAxis 	=	_target.velocityCurveY;
			break;
			case SplineParticles.SPIRAL_AXIS.Y:
				targetAnimationCurveFirstAxis 	= 	_target.velocityCurveX;
				targetAnimationCurveSecondAxis 	=	_target.velocityCurveZ;
			break;
			case SplineParticles.SPIRAL_AXIS.Z:
				targetAnimationCurveFirstAxis 	= 	_target.velocityCurveX;
				targetAnimationCurveSecondAxis 	=	_target.velocityCurveY;
			break;
		
		}
		
		
		
		for (int i = 0; i<targetAnimationCurveFirstAxis.keys.Length; i++)
		{
			Keyframe newKey = targetAnimationCurveFirstAxis.keys[i];
			newKey.value += Mathf.Sin(newKey.time*6*_target.spiralLoops)*10*_target.spiralAmplitude;
			tempCurveFirstAxis.AddKey(newKey);
		}
		
		
		for (int i = 0; i<targetAnimationCurveSecondAxis.keys.Length; i++)
		{
			Keyframe newKey = targetAnimationCurveSecondAxis.keys[i];
			newKey.value += Mathf.Cos(newKey.time*6*_target.spiralLoops)*10*_target.spiralAmplitude;
			tempCurveSecondAxis.AddKey(newKey);
			
		}
		
		//Assign curves
		switch (_target.spiralAxis)
		{
			case SplineParticles.SPIRAL_AXIS.X:
				_target.velocityCurveZ = tempCurveFirstAxis;	
				_target.velocityCurveY = tempCurveSecondAxis;
			break;
			case SplineParticles.SPIRAL_AXIS.Y:
				_target.velocityCurveX = tempCurveFirstAxis;	
				_target.velocityCurveZ = tempCurveSecondAxis;
			break;
			case SplineParticles.SPIRAL_AXIS.Z:
				_target.velocityCurveX = tempCurveFirstAxis;
				_target.velocityCurveY = tempCurveSecondAxis;
			break;
		}
		
		
	}
	
	private void ClearCurves(SplineParticles _target)
	{
		
		_target.velocityCurveX = AnimationCurve.Linear(0,0,1,0);
		_target.velocityCurveY = AnimationCurve.Linear(0,0,1,0);
		_target.velocityCurveZ = AnimationCurve.Linear(0,0,1,0);
		
		xVelocityCurveScale = 1;
		yVelocityCurveScale = 1;
		zVelocityCurveScale = 1;
		
		this.Repaint();

		
	}
	
	/// <summary>
	/// Apply the changes to the particle system
	/// </summary>
	
	private void ChangeVelocityCurve(SplineParticles _target)	
	{
		
		
		ParticleSystem myParticleSystem = _target.gameObject.GetComponent<ParticleSystem>();
		
		SerializedObject newSerializedObject = new SerializedObject(myParticleSystem);
		
		SerializedProperty enableVelocityModule = newSerializedObject.FindProperty("VelocityModule.enabled");
		
		SerializedProperty velocityModuleTypeX = newSerializedObject.FindProperty("VelocityModule.x.minMaxState");
		SerializedProperty velocityModuleTypeY = newSerializedObject.FindProperty("VelocityModule.y.minMaxState");
		SerializedProperty velocityModuleTypeZ = newSerializedObject.FindProperty("VelocityModule.z.minMaxState");
		
		SerializedProperty velocityModuleInWorldSpace = newSerializedObject.FindProperty("VelocityModule.inWorldSpace");
		
		
		SerializedProperty speedCurveX = newSerializedObject.FindProperty("VelocityModule.x.maxCurve");
		
		SerializedProperty scalarCurveX = newSerializedObject.FindProperty("VelocityModule.x.scalar");
		
		SerializedProperty speedCurveY = newSerializedObject.FindProperty("VelocityModule.y.maxCurve");
			
		SerializedProperty scalarCurveY = newSerializedObject.FindProperty("VelocityModule.y.scalar");
		
		SerializedProperty speedCurveZ = newSerializedObject.FindProperty("VelocityModule.z.maxCurve");
		
		SerializedProperty scalarCurveZ = newSerializedObject.FindProperty("VelocityModule.z.scalar");
		
		if (_target.autoEnableParticleVelocityCurves == true)
		{
			if (!_target.hasCreatedTheCurveOnce)
			{
				_target.GetComponent<ParticleSystem>().startSpeed = 0;
				_target.hasCreatedTheCurveOnce = true;
			}
			velocityModuleInWorldSpace.boolValue = false;
			enableVelocityModule.boolValue 	= _target.autoEnableParticleVelocityCurves;
			velocityModuleTypeX.intValue  	= 1;
			velocityModuleTypeY.intValue  	= 1;
			velocityModuleTypeZ.intValue  	= 1;
			_target.GetComponent<ParticleSystem>().startSpeed = 0;
		}
		
		
		scalarCurveX.floatValue = xVelocityCurveScale;
		scalarCurveY.floatValue = yVelocityCurveScale;
		scalarCurveZ.floatValue = zVelocityCurveScale;
		
		speedCurveX.animationCurveValue = _target.velocityCurveX;
		speedCurveY.animationCurveValue = _target.velocityCurveY;
		speedCurveZ.animationCurveValue = _target.velocityCurveZ;
		
		
		newSerializedObject.ApplyModifiedProperties();
	}
	
}
}

