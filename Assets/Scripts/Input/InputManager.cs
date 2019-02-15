using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Button {
	ATTACK,
	SHOOT,
	DEFEND,
	SPECIAL,
	JUMP,
	RELOAD,
	SWAP,
	TAUNT
}

public struct AxesInfo {

	public enum Direction {
		NONE,
		UP,
		DOWN,
		RIGHT,
		LEFT,
		NULL
	}

	public float x;
	public float y;
	public Direction direction;
	public int tiltLevel;

	public Vector2 asVector {
		get { return new Vector2(x, y); }
	}
}

// base input manager class
public class InputManager : MonoBehaviour {

	public enum InputScheme {
		CLASSIC,
		TWINSTICK,
		KBMOUSE
	}

	const float AXIS_TILT_THRESHOLD = 0.25f;

	public AxesInfo moveAxes;
	public AxesInfo aimAxes;

	public virtual AxesInfo spcAxes {
		get { return moveAxes; }
	}

	public virtual AxesInfo cStick {
		get { return aimAxes; }
	}

	protected void UpdateMoveAxes() {
		moveAxes.x = Input.GetAxis("Move X");
		moveAxes.y = Input.GetAxis("Move Y");
	}

	protected virtual void UpdateAimAxes() {
		aimAxes.x = Input.GetAxis("Aim X");
		aimAxes.y = Input.GetAxis("Aim Y");
	}

	protected virtual void UpdateInfo(ref AxesInfo axes) {
		axes.tiltLevel = 0;
		axes.direction = AxesInfo.Direction.NONE;
		if (axes.x != 0.0f || axes.y != 0.0f)
		{
			if (Mathf.Abs(axes.x) > Mathf.Abs(axes.y)) // x is dominant axis
			{
				if (Mathf.Abs(axes.x) > AXIS_TILT_THRESHOLD)
				{
					axes.tiltLevel = 2;
				}
				else
				{
					axes.tiltLevel = 1;
				}
				if (axes.x > 0.0f)
				{
					axes.direction = AxesInfo.Direction.RIGHT;
				}
				else
				{
					axes.direction = AxesInfo.Direction.LEFT;
				}
			}
			else // y is dominant axis
			{
				if (Mathf.Abs(axes.y) > AXIS_TILT_THRESHOLD)
				{
					axes.tiltLevel = 2;
				}
				else
				{
					axes.tiltLevel = 1;
				}
				if (axes.y > 0.0f)
				{
					axes.direction = AxesInfo.Direction.UP;
				}
				else
				{
					axes.direction = AxesInfo.Direction.DOWN;
				}
			}
		}
	}

	public virtual void UpdateAxes() {
		UpdateMoveAxes();
		UpdateAimAxes();
		UpdateInfo(ref moveAxes);
		UpdateInfo(ref aimAxes);
	}

	public virtual bool GetButtonDown(Button button) {
		switch (button)
		{
		case Button.ATTACK:
			return Input.GetButtonDown("Attack");
		case Button.SHOOT:
			return Input.GetButtonDown("Shoot");
		case Button.DEFEND:
			return Input.GetButtonDown("Defend");
		case Button.SPECIAL:
			return Input.GetButtonDown("Special");
		case Button.JUMP:
			return Input.GetButtonDown("Jump");
		case Button.RELOAD:
			return Input.GetButtonDown("Reload");
		case Button.SWAP:
			return Input.GetButtonDown("Swap");
		case Button.TAUNT:
			return Input.GetButtonDown("Taunt");
		default:
			return false;
		}
	}

	public virtual bool GetButtonHeld(Button button) {
		switch (button)
		{
		case Button.ATTACK:
			return Input.GetButton("Attack");
		case Button.SHOOT:
			return Input.GetButton("Shoot");
		case Button.DEFEND:
			return Input.GetButton("Defend");
		case Button.SPECIAL:
			return Input.GetButton("Special");
		case Button.JUMP:
			return Input.GetButton("Jump");
		case Button.RELOAD:
			return Input.GetButton("Reload");
		case Button.SWAP:
			return Input.GetButton("Swap");
		case Button.TAUNT:
			return Input.GetButton("Taunt");
		default:
			return false;
		}
	}

	public virtual bool GetButtonUp(Button button) {
		switch (button)
		{
		case Button.ATTACK:
			return Input.GetButtonUp("Attack");
		case Button.SHOOT:
			return Input.GetButtonUp("Shoot");
		case Button.DEFEND:
			return Input.GetButtonUp("Defend");
		case Button.SPECIAL:
			return Input.GetButtonUp("Special");
		case Button.JUMP:
			return Input.GetButtonUp("Jump");
		case Button.RELOAD:
			return Input.GetButtonUp("Reload");
		case Button.SWAP:
			return Input.GetButtonUp("Swap");
		case Button.TAUNT:
			return Input.GetButtonUp("Taunt");
		default:
			return false;
		}
	}
}
