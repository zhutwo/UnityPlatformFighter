using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoad : MonoBehaviour
{
	[SerializeField] string levelName;

	// Use this for initialization
	void Start()
	{
		
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}

	public void LoadScene()
	{
		SceneManager.LoadScene(levelName);
	}

	public void Exit()
	{
		Application.Quit();
	}
}
