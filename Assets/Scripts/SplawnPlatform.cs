using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplawnPlatform : MonoBehaviour {

	[SerializeField] GameObject master;

	void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject == master)
		{
			this.gameObject.SetActive(false);
		}
	}
}
