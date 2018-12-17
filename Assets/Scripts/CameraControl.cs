using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraControl : MonoBehaviour {

	bool zoomed = false;
	// Use this for initialization
	void Start() {
		
	}
	
	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene("Menu");
		}
		var vec = transform.position;
		if (Input.GetKeyDown(KeyCode.Z))
		{
			if (zoomed)
			{
				vec.z -= 100.0f;
				zoomed = false;
			}
			else
			{
				vec.z += 100.0f;
				zoomed = true;
			}
		}
		if (Input.GetKey(KeyCode.A))
		{
			vec.x -= 40.0f * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.D))
		{
			vec.x += 40.0f * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.W))
		{
			vec.y += 40.0f * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.S))
		{
			vec.y -= 40.0f * Time.deltaTime;
		}
		transform.position = vec;
	}
}
