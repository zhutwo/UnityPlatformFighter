using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	GameObject player;
	Vector3 velocity = Vector3.zero;
	[SerializeField] Vector3 offset;
	// Use this for initialization
	void Start() {
		player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		transform.position = Vector3.SmoothDamp(transform.position, player.transform.position + offset, ref velocity, 0.1f);
	}
}
