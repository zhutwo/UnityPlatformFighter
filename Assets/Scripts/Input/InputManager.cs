using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base input manager class
public class InputManager : MonoBehaviour {

	protected Vector2 moveAxes;
	protected Vector2 aimAxes;

	public virtual Vector2 GetMoveAxes() {
		moveAxes.x = Input.GetAxis("Move X");
		moveAxes.y = Input.GetAxis("Move Y");
		return moveAxes;
	}

	public virtual Vector2 GetAimAxes() {
		aimAxes.x = Input.GetAxis("Aim X");
		aimAxes.y = Input.GetAxis("Aim Y");
		return aimAxes;
	}

	/// <summary>
	/// Must be called last after GetMoveAxes() and GetAimAxes()
	/// </summary>
	public virtual Vector2 GetSpecialAxes() {
		return moveAxes;
	}

	public virtual bool GetButtonDown(Button button) {
		switch (button)
		{
		case Button.ATTACK:
			return Input.GetKeyDown("Attack");
			break;
		case Button.SHOOT:
			return Input.GetKeyDown("Shoot");
			break;
		case Button.DEFEND:
			return Input.GetKeyDown("Defend");
			break;
		case Button.SPECIAL:
			return Input.GetKeyDown("Special");
			break;
		case Button.JUMP:
			return Input.GetKeyDown("Jump");
			break;
		case Button.RELOAD:
			return Input.GetKeyDown("Reload");
			break;
		case Button.SWAP:
			return Input.GetKeyDown("Swap");
			break;
		case Button.TAUNT:
			return Input.GetKeyDown("Taunt");
			break;
		}
		return false;
	}

	public virtual bool GetButtonHeld(Button button) {
		switch (button)
		{
		case Button.ATTACK:
			return Input.GetKey("Attack");
			break;
		case Button.SHOOT:
			return Input.GetKeyUp("Shoot");
			break;
		case Button.DEFEND:
			return Input.GetKey("Defend");
			break;
		case Button.SPECIAL:
			return Input.GetKey("Special");
			break;
		case Button.JUMP:
			return Input.GetKey("Jump");
			break;
		case Button.RELOAD:
			return Input.GetKey("Reload");
			break;
		case Button.SWAP:
			return Input.GetKey("Swap");
			break;
		case Button.TAUNT:
			return Input.GetKey("Taunt");
			break;
		}
		return false;
	}

	public virtual bool GetButtonUp(Button button) {
		switch (button)
		{
		case Button.ATTACK:
			return Input.GetKeyUp("Attack");
			break;
		case Button.SHOOT:
			return Input.GetKeyUp("Shoot");
			break;
		case Button.DEFEND:
			return Input.GetKeyUp("Defend");
			break;
		case Button.SPECIAL:
			return Input.GetKeyUp("Special");
			break;
		case Button.JUMP:
			return Input.GetKeyUp("Jump");
			break;
		case Button.RELOAD:
			return Input.GetKeyUp("Reload");
			break;
		case Button.SWAP:
			return Input.GetKeyUp("Swap");
			break;
		case Button.TAUNT:
			return Input.GetKeyUp("Taunt");
			break;
		}
		return false;
	}
}
