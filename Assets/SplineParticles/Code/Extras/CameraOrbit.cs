/// <summary>
/// Author: Cesar Rios (Pigtail Games) 2013
/// </summary>
/// 
using UnityEngine;
using System.Collections;
 
namespace PigtailGames
{
public class CameraOrbit : MonoBehaviour
{
    public Transform target;
   
	public float	xRotationSpeed = 1;
	public float	yRotationSpeed = 1;
	
	private Vector3 lastMousePosition;
	
	void Awake()
	{
		transform.LookAt(target.position,Vector3.up);
	}
	
	
	
	
	private void LateUpdate()
	{
		Vector3 mousePositionDelta = Vector3.zero;
		
		if (Input.GetMouseButton(0))
		{
			mousePositionDelta = Input.mousePosition - lastMousePosition;
		
			transform.RotateAround(target.position,Vector3.up, mousePositionDelta.x*Time.deltaTime*xRotationSpeed);		
			transform.RotateAround(target.position,transform.right, -mousePositionDelta.y*Time.deltaTime*yRotationSpeed);		
		}
	
		
		lastMousePosition = Input.mousePosition;
		
		
	}
		
	
}
}