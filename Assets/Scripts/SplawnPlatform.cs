using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplawnPlatform : MonoBehaviour {

	[SerializeField] GameObject master;

    void OnEnable()
    {
        Invoke("TimeOut", 3.0f);
    }

    void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject == master)
		{
            CancelInvoke("TimeOut");
			this.gameObject.SetActive(false);
		}
	}

    void TimeOut()
    {
        this.gameObject.SetActive(false);
    }
}
