using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DolphIllusion : MonoBehaviour {

	Quaternion lookRotation;
	float timer;
	[SerializeField] float timeout;
	[SerializeField] GameObject spineRotationBone;
	// Use this for initialization
	void Start() {
		
	}
		
	// Update is called once per frame
	void Update() {
		timer += Time.deltaTime;
		if (timer >= timeout)
		{
			timer = 0.0f;
			this.gameObject.SetActive(false);
		}
	}

	void LateUpdate() {
		spineRotationBone.transform.rotation = lookRotation;
	}

	public void SetRotation(Quaternion rotation) {
		lookRotation = rotation;
	}
}
