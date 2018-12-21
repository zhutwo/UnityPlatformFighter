/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

[System.Serializable]
public class Hitbox : System.Object {

	[SerializeField] public MoveManager.Moves move;
	[SerializeField] static LayerMask avatarLayer;
	[SerializeField] bool hasFreezeFrames;
	[SerializeField] int baseDamage;
	[SerializeField] float baseAngle;
	[SerializeField] float baseKnockback;
	[SerializeField] float knockbackScaling;
	[SerializeField] int sweetSpotColliderIndex;
	[SerializeField] int sweetSpotDamage;
	[SerializeField] float sweetSpotAngle;
	[SerializeField] float sweetSpotKnockback;
	[SerializeField] Collider[] colliders;

	float direction;

	void Start() {
		knockbackScaling = knockbackScaling / 100.0f;
	}

	public void SetActive(bool active, float direction) {
		this.direction = direction;
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = active;
		}
	}

	Vector3 CalculateKnockback(int damage, float angle, float baseKb, float enemydamage, float enemyweight, out float stunTime) {
		float power = (((((enemydamage / 10.0f + enemydamage * (float)damage / 20.0f) * (200.0f / (enemyweight + 100.0f)) * 1.4f) + 18.0f) * knockbackScaling) + baseKb);
		stunTime = power * 0.4f / 60.0f;
		return Quaternion.Euler(0.0f, 0.0f, direction * (angle - 90.0f)) * (power * Vector3.up);
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == avatarLayer.value)
		{
			Avatar enemy = other.GetComponent<Avatar>();

			int damage = baseDamage;
			float angle = baseAngle;
			float kb = baseKnockback;
			if (colliders[sweetSpotColliderIndex].bounds.Intersects(other.bounds))
			{
				damage = sweetSpotDamage;
				angle = sweetSpotAngle;
				kb = sweetSpotKnockback;
			}
			float freezeTime = 0.0f;
			if (hasFreezeFrames)
			{
				freezeTime = (damage / 3.0f + 3.0f) / 60.0f;
				//parentAvatar.StartFreezeFrame(freezeTime);
			}
			float stunTime = 0.0f;
			Vector3 finalKb = CalculateKnockback(damage, angle, kb, (float)(enemy.MaxHealth - enemy.Health), enemy.Weight, out stunTime);
			enemy.TakeHit(damage, freezeTime, stunTime, finalKb);
		}
	}


}
*/