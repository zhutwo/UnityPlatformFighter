using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : MonoBehaviour {

	GameObject owner;
	Rigidbody rb;
	float timer;
	[SerializeField] float lifetime;
	// Use this for initialization
	void Start() {
		timer = 0.0f;
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update() {
		timer += Time.deltaTime;
		if (timer >= lifetime)
		{
			rb.velocity = Vector3.zero;
			timer = 0.0f;
			this.gameObject.SetActive(false);
		}
	}

	public void SetOwner(GameObject owner) {
		this.owner = owner;
	}
}
