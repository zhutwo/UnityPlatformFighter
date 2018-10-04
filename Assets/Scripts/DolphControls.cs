using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DolphControls : MonoBehaviour
{

	KeyCode JUMP = KeyCode.Space;
	KeyCode RIGHT = KeyCode.D;
	KeyCode LEFT = KeyCode.A;
	KeyCode DOWN = KeyCode.S;

	Animator anim;
	Rigidbody rb;

	LayerMask groundLayer = 10;

	bool isGrounded = true;
	bool isHitstun;

	[Header("Jump Properties")]
	[SerializeField] float fullHopSpeed;
	[SerializeField] float shortHopSpeed;
	[SerializeField] float fallAccel;
	[SerializeField] float terminalFallSpeed;
	[SerializeField] float fastFallSpeed;

	// Use this for initialization
	void Start()
	{
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update()
	{
		CheckInput();
	}

	void FixedUpdate()
	{
		if (!isGrounded)
		{
			if (rb.velocity.y > -terminalFallSpeed)
			{
				rb.velocity -= transform.up * fallAccel * Time.fixedDeltaTime;
				if (rb.velocity.y < -terminalFallSpeed)
				{
					Vector3 temp = rb.velocity;
					temp.y = -terminalFallSpeed;
					rb.velocity = temp;
				}
			}
		}
	}

	void CheckInput()
	{
		if (Input.GetKeyDown(JUMP))
		{
			if (isGrounded)
			{
				anim.SetTrigger("JumpTrigger");
			}
		}
		if (Input.GetKeyDown(DOWN))
		{
			if (!isGrounded)
			{
				// input fast fall
				if (rb.velocity.y < 0 && rb.velocity.y > -fastFallSpeed)
				{
					Vector3 temp = rb.velocity;
					temp.y = -fastFallSpeed;
					rb.velocity = temp;
				}
			}
		}
	}

	void AddJumpForce()
	{
		// performs full hop if player is still holding jump at end of jump squat animation
		if (Input.GetKey(JUMP))
		{
			rb.velocity += transform.up * fullHopSpeed;
		}
		else
		{
			rb.velocity += transform.up * shortHopSpeed;
		}
		isGrounded = false;
	}

	void LandingCheck()
	{
		
	}
}
