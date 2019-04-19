using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	GameObject[] players;
	Vector3 velocity = Vector3.zero;
    Vector3 min, max, center;
    [SerializeField] Camera camera;
    [SerializeField] float padding;
	[SerializeField] Vector3 offset;
    [SerializeField] Vector3 limits;
	// Use this for initialization
	void Start() {
		players = GameObject.FindGameObjectsWithTag("Player");
	}
	
	// Update is called once per frame
	void FixedUpdate() {
        FindBounds();
        FindPosition();
		transform.position = Vector3.SmoothDamp(transform.position, center + offset, ref velocity, 0.1f);
	}

    void FindPosition()
    {
        center = min + max;
        center *= 0.5f;
        float zx = (Mathf.Abs(max.x - center.x)) * Mathf.Atan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
        float zy = (Mathf.Abs(max.y - center.y)) * Mathf.Atan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
        center.z = zx > zy ? -zx : -zy;
        /*
        if (center.x > limits.x || center.x < -limits.x)
        {
            center.x = limits.x * Mathf.Sign(center.x);
        }
        if (center.y > limits.y || center.y < -limits.y)
        {
            center.y = limits.y * Mathf.Sign(center.y);
        }
        if (center.z > limits.z)
        {
            center.z = limits.z;
        }
        */
    }

    void FindBounds()
    {
        min = players[0].transform.position;
        max = players[0].transform.position;
        min.y = 2.0f;
        max.y = 2.0f;
        foreach (var player in players)
        {
            if (player.transform.position.x < min.x)
            {
                min.x = player.transform.position.x;
            }
            else if (player.transform.position.x > max.x)
            {
                max.x = player.transform.position.x;
            }
            if (player.transform.position.y < min.y)
            {
                min.y = player.transform.position.y;
            }
            else if (player.transform.position.y > max.y)
            {
                max.y = player.transform.position.y;
            }
        }
    }
}
