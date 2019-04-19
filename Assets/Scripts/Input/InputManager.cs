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
		LEFT
	}

	public float x;
	public float y;
	public Direction direction;
	public Direction directionLast;
	public int tiltLevel;
	public bool isTapInput;
	public bool isBufferedTapInput;

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

	const int BUFFER_SIZE = 2;
	const float AXIS_TILT_THRESHOLD = 0.25f;

	[SerializeField] int playerIndex;
	[SerializeField] string[] inputAlias = new string[11];

	public AxesInfo leftAxes;
	public AxesInfo rightAxes;

	public virtual AxesInfo moveAxes {
		get { return leftAxes; }
	}

	public virtual AxesInfo aimAxes {
		get { return leftAxes; }
	}

	public virtual AxesInfo spcAxes {
		get { return leftAxes; }
	}

	public virtual AxesInfo cStick {
		get { return rightAxes; }
	}

	void Start() {
		SetAlias();
	}

	void SetAlias() {
		for (int i = 0; i < inputAlias.Length; i++)
		{
			inputAlias[i] = playerIndex.ToString() + inputAlias[i];
		}
	}

	protected void UpdateMoveAxes() {
		leftAxes.x = Input.GetAxis(inputAlias[0]);
		leftAxes.y = Input.GetAxis(inputAlias[1]);
	}

	protected virtual void UpdateAimAxes() {
		rightAxes.x = Input.GetAxis(inputAlias[2]);
		rightAxes.y = Input.GetAxis(inputAlias[3]);
	}

	protected virtual void UpdateInfo(ref AxesInfo axes) {
		// 2 frame window to tap input
		AxesInfo.Direction directionBeforeLast = axes.directionLast;
		axes.directionLast = axes.direction;
		axes.isBufferedTapInput = axes.isTapInput;
		axes.isTapInput = false;
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
		if (axes.tiltLevel == 2)
		{
			if (axes.direction != axes.directionLast || axes.direction != directionBeforeLast)
			{
				axes.isTapInput = true;
				axes.isBufferedTapInput = false;
				axes.directionLast = axes.direction; // lock out tap input for 1 frame
			}
		}
		else
		{
			axes.isBufferedTapInput = false;
		}
	}

	public virtual void UpdateAxes() {
		UpdateMoveAxes();
		UpdateAimAxes();
		UpdateInfo(ref leftAxes);
		UpdateInfo(ref rightAxes);
	}

	public virtual bool GetButtonDown(Button button) {
		switch (button)
		{
		case Button.ATTACK:
			return Input.GetButtonDown(inputAlias[4]);
		case Button.SHOOT:
			return Input.GetButtonDown(inputAlias[5]);
		case Button.DEFEND:
			return Input.GetButtonDown(inputAlias[6]);
		case Button.SPECIAL:
			return Input.GetButtonDown(inputAlias[7]);
		case Button.JUMP:
			return Input.GetButtonDown(inputAlias[8]);
		case Button.RELOAD:
			return Input.GetButtonDown(inputAlias[9]);
		case Button.SWAP:
			return Input.GetButtonDown(inputAlias[10]);
		case Button.TAUNT:
			//return Input.GetButtonDown(inputAlias[11]);
		default:
			return false;
		}
	}

	public virtual bool GetButtonHeld(Button button) {
		switch (button)
		{
		case Button.ATTACK:
			return Input.GetButton(inputAlias[4]);
		case Button.SHOOT:
			return Input.GetButton(inputAlias[5]);
		case Button.DEFEND:
			return Input.GetButton(inputAlias[6]);
		case Button.SPECIAL:
			return Input.GetButton(inputAlias[7]);
		case Button.JUMP:
			return Input.GetButton(inputAlias[8]);
		case Button.RELOAD:
			return Input.GetButton(inputAlias[9]);
		case Button.SWAP:
			return Input.GetButton(inputAlias[10]);
		case Button.TAUNT:
			//return Input.GetButton(inputAlias[11]);
		default:
			return false;
		}
	}

	public virtual bool GetButtonUp(Button button) {
		switch (button)
		{
		case Button.ATTACK:
			return Input.GetButtonUp(inputAlias[4]);
		case Button.SHOOT:
			return Input.GetButtonUp(inputAlias[5]);
		case Button.DEFEND:
			return Input.GetButtonUp(inputAlias[6]);
		case Button.SPECIAL:
			return Input.GetButtonUp(inputAlias[7]);
		case Button.JUMP:
			return Input.GetButtonUp(inputAlias[8]);
		case Button.RELOAD:
			return Input.GetButtonUp(inputAlias[9]);
		case Button.SWAP:
			return Input.GetButtonUp(inputAlias[10]);
		case Button.TAUNT:
			//return Input.GetButtonUp(inputAlias[11]);
		default:
			return false;
		}
	}
}
