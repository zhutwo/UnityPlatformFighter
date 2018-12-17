using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class receives callbacks from animator and activates hitboxes on appropriate frames
public class MoveManager : MonoBehaviour {

	[System.Serializable]
	public class Hitbox : System.Object {

		[SerializeField] public MoveManager.Move move;
		[SerializeField] public bool hasFreezeFrames = true;
		[SerializeField] public bool hasSweetSpot = false;
		[SerializeField] public int baseDamage;
		[SerializeField] public float baseAngle;
		[SerializeField] public float baseKnockback;
		[SerializeField] public float knockbackScaling;
		[SerializeField] public int sweetSpotColliderIndex;
		[SerializeField] public int sweetSpotDamage;
		[SerializeField] public float sweetSpotAngle;
		[SerializeField] public float sweetSpotKnockback;
		[SerializeField] public float sweetSpotScaling;
		[SerializeField] public Collider[] colliders;

		List<GameObject> noHitList;
		float direction;

		public float Direction { get { return direction; } }

		public void SetActive(bool active, float direction) {
			this.direction = direction;
			for (int i = 0; i < colliders.Length; i++)
			{
				colliders[i].enabled = active;
			}
			if (!active)
			{
				noHitList.Clear();
			}
		}

		public void InitNoHit(int players) {
			noHitList = new List<GameObject>();
			noHitList.Capacity = players;
		}

		public void AddNoHit(GameObject player) {
			noHitList.Add(player);
		}

		public bool CheckNoHit(GameObject player) {
			return noHitList.Contains(player);
		}
	}

	public enum Move {
		FTILT,
		DTILT,
		UTILT1,
		UTILT2,
		JAB1,
		JAB2,
		JAB3,
		DASHATK,
		NAIR,
		FAIR,
		UAIR,
		DAIR,
		SPECIAL
	}

	[SerializeField] LayerMask targetLayer;
	[SerializeField] Hitbox[] hitboxes = new Hitbox[13];

	Avatar avatar;
	Move activeMove;

	// to display correct name of hitboxes in array in inspector
	void OnDrawGizmos() {
		for (int i = 0; i < hitboxes.Length; i++)
		{
			hitboxes[i].move = (Move)i;
		}
	}

	// Use this for initialization
	void Start() {
		avatar = GetComponent<Avatar>();
		for (int i = 0; i < hitboxes.Length; i++)
		{
			hitboxes[i].InitNoHit(8);
			// later get max players value
		}
	}
	
	// Update is called once per frame
	void Update() {
		
	}

	public void ActivateHitbox(Move move) {
		activeMove = move;
		hitboxes[(int)move].SetActive(true, transform.forward.x);
	}

	public void DeactivateHitbox(Move move) {
		hitboxes[(int)move].SetActive(false, transform.forward.x);
	}

	Vector3 CalculateKnockback(int damage, float direction, float angle, float baseKb, float kBscale, float enemydamage, float enemyweight, out float stunTime) {
		float power = (((((enemydamage / 10.0f + enemydamage * (float)damage / 20.0f) * (200.0f / (enemyweight + 100.0f)) * 1.4f) + 18.0f) * kBscale / 100.0f) + baseKb);
		print(power);
		stunTime = power * 0.4f / 60.0f;
		return Quaternion.Euler(0.0f, 0.0f, direction * (angle - 90.0f)) * (power * Vector3.up);
	}

	void OnTriggerEnter(Collider other) {
		if (1 << other.gameObject.layer == targetLayer)
		{
			Hitbox hitbox = hitboxes[(int)activeMove];
			if (hitbox.CheckNoHit(other.gameObject))
			{
				return; // prevent more than one collider of an attack triggering
			}
			Avatar enemy = other.gameObject.GetComponent<Avatar>();
			int damage = hitbox.baseDamage;
			float angle = hitbox.baseAngle;
			float baseKb = hitbox.baseKnockback;
			float kBscale = hitbox.knockbackScaling;
			if (hitbox.hasSweetSpot && hitbox.colliders[hitbox.sweetSpotColliderIndex].enabled && hitbox.colliders[hitbox.sweetSpotColliderIndex].bounds.Intersects(other.bounds))
			{
				damage = hitbox.sweetSpotDamage;
				angle = hitbox.sweetSpotAngle;
				baseKb = hitbox.sweetSpotKnockback;
				kBscale = hitbox.sweetSpotScaling;
			}
			float freezeTime = 0.0f;
			if (hitbox.hasFreezeFrames)
			{
				freezeTime = (damage / 3.0f + 5.0f) / 60.0f;
				avatar.StartFreezeFrame(freezeTime);
			}
			float stunTime = 0.0f;
			float direction = hitbox.Direction;
			Vector3 finalKb = CalculateKnockback(damage, direction, angle, baseKb, kBscale, enemy.Damage, enemy.Weight, out stunTime);
			enemy.TakeHit(damage, freezeTime, stunTime, finalKb);
			avatar.AddMeter(damage);
			hitbox.AddNoHit(other.gameObject);
		}
	}
}
