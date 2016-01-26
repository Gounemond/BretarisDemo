using UnityEngine;
using System.Collections;


namespace PigtailGames
{
public class ParticleSelector : MonoBehaviour {

	public GameObject[] splineParticleSystems;
	
	void OnGUI()
	{
		
		GUILayout.BeginScrollView(Vector2.zero);
		
		foreach(GameObject iteratorGameObject in splineParticleSystems)
		{
			if (GUILayout.Button(iteratorGameObject.name))
			{
				foreach(GameObject iteratorSecondGameObject in splineParticleSystems)
				{
					iteratorSecondGameObject.SetActive(false);
					iteratorSecondGameObject.GetComponent<ParticleSystem>().Clear();
				}
				iteratorGameObject.SetActive(true);
			}		
		}
		
		GUILayout.EndScrollView();
		
		
	}
}
}
