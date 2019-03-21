using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

	[SerializeField] GameObject[] spawnPlatforms = new GameObject[2];
	[SerializeField] LayerMask ecbLayer;

	void Start() {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (1 << other.gameObject.layer == ecbLayer)
		{
			Avatar avatar = other.gameObject.GetComponentInParent<Avatar>();
			spawnPlatforms[avatar.playerID].SetActive(true);
			avatar.Respawn(spawnPlatforms[avatar.playerID].transform.position);
		}
	}
}
