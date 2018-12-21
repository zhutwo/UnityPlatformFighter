using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputManager : InputManager {

	InputScheme scheme = InputScheme.KBMOUSE;
	Plane mousePlane;
	Vector3 mousePosition;
	Ray ray;

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

	public override Vector2 GetAimAxes() {
		CheckMousePosition();
		return aimAxes;
	}

	public override Vector2 GetSpecialAxes() {
		return aimAxes;
	}

	void CheckMousePosition() {
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter = 0.0f;
		if (mousePlane.Raycast(ray, out enter))
		{
			mousePosition = ray.GetPoint(enter);
		}
		mousePosition -= transform.position;
		aimAxes.x = mousePosition.x;
		aimAxes.y = mousePosition.y;
		aimAxes.Normalize();
	}
}
