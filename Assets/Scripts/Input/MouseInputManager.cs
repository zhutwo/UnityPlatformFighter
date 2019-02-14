using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputManager : InputManager {

	Plane mousePlane;
	Vector3 mousePosition;
	Ray ray;

	public override AxesInfo spcAxes {
		get { return aimAxes; }
	}

	public override AxesInfo cStick {
		get { return aimAxes; }
	}

	void OnDrawGizmos() {
		mousePlane = new Plane(Vector3.back, transform.position);
		Vector3 hitPoint;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter = 0.0f;
		if (mousePlane.Raycast(ray, out enter))
		{
			hitPoint = ray.GetPoint(enter);
			Gizmos.DrawSphere(hitPoint, 0.1f);
		}
	}

	void Start() {
		mousePlane = new Plane(Vector3.back, transform.position);
	}

	protected override void UpdateAimAxes() {
		CheckMousePosition();
	}

	public override void UpdateAxes() {
		base.UpdateMoveAxes();
		UpdateAimAxes();
	}

	protected override void UpdateInfo(ref AxesInfo axes) {
		// to do
	}

	void CheckMousePosition() {
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter = 0.0f;
		if (mousePlane.Raycast(ray, out enter))
		{
			mousePosition = ray.GetPoint(enter);
		}
		mousePosition -= transform.position;
		mousePosition.z = 0.0f;
		mousePosition.Normalize();
		aimAxes.x = mousePosition.x;
		aimAxes.y = mousePosition.y;
	}
}
