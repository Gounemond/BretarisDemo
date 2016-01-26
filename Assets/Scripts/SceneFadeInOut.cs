using UnityEngine;
using System.Collections;

public class SceneFadeInOut : MonoBehaviour
{
	[Tooltip("The lower, the slower")]
	public float fadeSpeed = 1.5f;          // Speed that the screen fades to and from black.
	
	[Tooltip("0 for DeviceIntro, 1 for lungs show, 2 for particles emettitors")]
	public int sceneToLoad = 0;				
	
	[Tooltip("Delay in seconds for the scene start")]
	public float delay = 0;
	
	[HideInInspector]
	public bool startFadeIn = false;
	[HideInInspector]
	public bool startFadeOut = false;
	[HideInInspector]
	public bool loadNextScene = false;
	[HideInInspector]
	public bool restartLevel = false;
	[HideInInspector]
	public bool gameOver = false;
		
	void Awake()
	{
		StartCoroutine (StartFadeIn());
	}
	
	
	void Update ()
	{
		if (startFadeIn)
		{
			startFadeIn = false;
			StartCoroutine (StartFadeIn());
		}
		
		if (startFadeOut)
		{
			startFadeOut = false;
			StartCoroutine (StartFadeOut());
		}
		
		if (loadNextScene)
		{
			loadNextScene = false;
			NextLevel();
		}
		
		if (gameOver)
		{
			gameOver = false;
			GameOver();
		}
	}
	
	
	void FadeToClear ()
	{
		// Lerp the colour of the texture between itself and transparent.
		GetComponent<Renderer>().material.color = Color.Lerp(GetComponent<Renderer>().material.color, Color.clear, fadeSpeed * Time.deltaTime);
	}
	
	
	void FadeToBlack ()
	{
		// Lerp the colour of the texture between itself and black.
		GetComponent<Renderer>().material.color = Color.Lerp(GetComponent<Renderer>().material.color, Color.black, fadeSpeed * Time.deltaTime);
	}
	
	
	public IEnumerator StartFadeIn()
	{
		// Keep the texture black until delay has passed
		if (delay != 0)
		{
			yield return new WaitForSeconds(delay);
		}
		// Fade the texture to clear.
		while (GetComponent<Renderer>().material.color.a > 0.05f)
		{
			FadeToClear();
			yield return 0;
		}
		
		// If the texture is almost clear...
		if(GetComponent<Renderer>().material.color.a <= 0.05f)
		{
			// ... set the colour to clear and disable the GUITexture.
			GetComponent<Renderer>().material.color = Color.clear;
		}
	}
	
	public IEnumerator StartFadeOut()
	{
		
		// Start fading towards black.
		while (GetComponent<Renderer>().material.color.a < 0.95f)
		{
			FadeToBlack();
			yield return 0;
		}
		
		// If the screen is almost black...
		if(GetComponent<Renderer>().material.color.a >= 0.95f)
		{
			GetComponent<Renderer>().material.color = Color.black;
		}
	}
	
	public void NextLevel()
	{
		StartCoroutine(FadeAndLoadScene(sceneToLoad));
	}
	
	public void GameOver()
	{
		StartCoroutine (FadeAndLoadScene(0));
	}
	
	
	// Fade the texture to black.
	public IEnumerator FadeAndLoadScene(int level)
	{
		
		// Start fading towards black.
		while (GetComponent<Renderer>().material.color.a < 0.95f)
		{
			FadeToBlack();
			yield return 0;
		}
		
		// If the screen is almost black...
		if(GetComponent<Renderer>().material.color.a >= 0.95f)
		{
			GetComponent<Renderer>().material.color = Color.black;
			// ... load the level.
			Application.LoadLevel(level);
		}
	}
}