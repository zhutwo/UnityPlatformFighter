using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Environmental Collision Box Manager
*/
public class ECBManager : MonoBehaviour {

	[SerializeField] PolygonCollider2D mainECB;
	[SerializeField] PolygonCollider2D triggerECB;
	[SerializeField] PolygonCollider2D platformECB;
	[SerializeField] PolygonCollider2D edgeECB;
	[SerializeField] PlatformEffector2D platformEffector;
	[SerializeField] LayerMask platformLayer;
	[SerializeField] LayerMask groundLayer;
	[SerializeField] float raycastLength;

	Collider2D currentPlatform;
	[SerializeField] Collider2D ignoredPlatform;
	Rigidbody2D rb;

	bool ignorePlatforms = false;

	public bool IgnorePlatforms {
		get { return ignorePlatforms; }
	}

	void OnDrawGizmos() {
		Gizmos.DrawLine(transform.position, transform.position + Vector3.down * raycastLength);
	}

	void Start() {
		rb = GetComponent<Rigidbody2D>();
	}

	void OnTriggerExit2D(Collider2D other) {
		//make platform valid for collision again
		if (other == ignoredPlatform)
		{
			Physics2D.IgnoreCollision(platformECB, ignoredPlatform, false);
			ignoredPlatform = null;
			currentPlatform = null;
		}
	}

	public void IgnoreCurrentPlatform() {
		RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, raycastLength, platformLayer);
		if (hit.collider != null)
		{
			currentPlatform = hit.collider;
			ignoredPlatform = currentPlatform;
			Physics2D.IgnoreCollision(platformECB, ignoredPlatform, true);
		}
	}

	public bool SetIgnorePlatforms(bool ignore) {
		platformEffector.useColliderMask = ignore;
		ignorePlatforms = ignore;
		if (ignore)
		{
			RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, raycastLength, platformLayer);
			if (hit.collider != null)
			{
				ignoredPlatform = hit.collider;
				Physics2D.IgnoreCollision(platformECB, ignoredPlatform, true);
				return true;
			}
		}
		return false;
	}

	public void ToggleEdgeECB(bool enabled) {
		edgeECB.enabled = enabled;
	}

	public bool GroundedRaycast() {
		if (ignorePlatforms)
		{
			return Physics2D.Raycast(rb.position, Vector2.down, raycastLength, groundLayer);
		}
		RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, raycastLength, groundLayer | platformLayer);
		if (hit.collider != null)
		{
			if (hit.collider != ignoredPlatform)
			{
				// required so player can finish falling through platform after releasing down
				return true;
			}
		}
		return false;
	}

	public float GetGroundPositionY() {
		RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, raycastLength, groundLayer | platformLayer);
		return hit.collider.transform.position.y;
	}

	public float GetEdgePositionX(float direction) {
		direction = Mathf.Sign(direction);
		RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, raycastLength, groundLayer | platformLayer);
		float x = hit.collider.transform.position.x + direction * hit.collider.transform.localScale.x / 2.0f;
		return x;
	}
}
