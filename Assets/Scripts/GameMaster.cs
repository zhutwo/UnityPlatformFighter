using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

	[SerializeField] Transform[] spawns = new Transform[2];
	[SerializeField] LayerMask ecbLayer;

	// Use this for initialization
	void Start() {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
	}
	
	// Update is called once per frame
	void Update() {
		
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (1 << other.gameObject.layer == ecbLayer)
		{
			Avatar avatar = other.gameObject.GetComponentInParent<Avatar>();
			avatar.Respawn(spawns[avatar.playerID].position);
		}
	}
}
