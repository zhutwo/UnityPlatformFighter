using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : MonoBehaviour {

	GameObject owner;
	Rigidbody rb;
	float timer;
	const float knockback = 80.0f;
	const int damage = 1;
	[SerializeField] float lifetime;
    [SerializeField] LayerMask hurtBoxLayer;
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

	void Recycle() {
		rb.velocity = Vector3.zero;
		timer = 0.0f;
		this.gameObject.SetActive(false);
	}

	public void SetOwner(GameObject owner) {
		this.owner = owner;
	}

    void OnTriggerEnter(Collider other) {
        if (1 << other.gameObject.layer == hurtBoxLayer)
        {
            Avatar avatar = other.gameObject.GetComponentInParent<Avatar>();
            avatar.TakeHit(damage, 0.1f, 0.2f, transform.forward * knockback);
        }
        Recycle();
    }
}
