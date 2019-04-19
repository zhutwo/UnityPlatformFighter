using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

	[SerializeField] GameObject[] spawnPlatforms = new GameObject[2];
	[SerializeField] LayerMask ecbLayer;
    [SerializeField] GameObject ringOutPrefab;
    [SerializeField] Collider2D[] bounds = new Collider2D[4];

    AudioSource audio;

	void Start() {
		audio = GetComponent<AudioSource>();
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (1 << other.gameObject.layer == ecbLayer)
		{
            for (int i = 0; i < 4; i++)
            {
                if (other.IsTouching(bounds[i]))
                {
                    SpawnRingOutSplash(other.transform.position, bounds[i].transform.rotation);
                }
            }
            Avatar avatar = other.gameObject.GetComponentInParent<Avatar>();
			spawnPlatforms[avatar.playerID].SetActive(true);
			avatar.Respawn(spawnPlatforms[avatar.playerID].transform.position);
			audio.Play();
		}
	}

    void SpawnRingOutSplash(Vector3 position, Quaternion rotation)
    {
        var go = Instantiate(ringOutPrefab);
        go.transform.position = position;
        go.transform.rotation = rotation;
    }
}
