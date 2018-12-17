using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPad : MonoBehaviour {

	[SerializeField] float launchPower;

	AudioSource audio;

	// Use this for initialization
	void Start() {
		audio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update() {
		
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == 11)
		{
			Rigidbody rb = other.GetComponent<Rigidbody>();
			rb.AddForce(-launchPower * transform.forward, ForceMode.VelocityChange);
			audio.Play();
		}
	}
}
