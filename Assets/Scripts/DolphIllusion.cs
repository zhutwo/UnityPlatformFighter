using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DolphIllusion : MonoBehaviour {

	Vector3 lookAt;
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
			Destroy(this.gameObject);
		}
	}

	void LateUpdate() {
		spineRotationBone.transform.LookAt(lookAt);
	}

	public void SetLookVector(Vector3 look) {
		lookAt = look;
	}
}
