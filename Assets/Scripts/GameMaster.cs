using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

	[SerializeField] Transform[] spawns = new Transform[2];

	// Use this for initialization
	void Start() {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
	}
	
	// Update is called once per frame
	void Update() {
		
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == 11)
		{
			Avatar avatar = other.gameObject.GetComponent<Avatar>();
			avatar.Respawn(spawns[avatar.playerID].position);
		}
	}
}
