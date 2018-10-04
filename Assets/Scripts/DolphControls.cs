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
	Vector3 tempVec;

	bool isGrounded = true;
	bool isHitstun = false;
	bool isActionable = true;
	bool isJumping = false;

	[Header("MoveProperties")]
	[SerializeField] float runFwdSpeed;
	[SerializeField] float runBackSpeed;
	[SerializeField] float airDriftSpeed;
	[SerializeField] float runFwdAccel;
	[SerializeField] float runBackAccel;
	[SerializeField] float airDriftAccel;

	[Header("Jump Properties")]
	[SerializeField] float fullHopSpeed;
	[SerializeField] float shortHopSpeed;
	[SerializeField] float fallAccel;
	[SerializeField] float terminalFallSpeed;
	[SerializeField] float fastFallSpeed;
	[SerializeField] float hardLandThresholdSpeed;

	[Header("Grounding")]
	[SerializeField] LayerMask groundLayer;
	[SerializeField] Transform groundCheckPoint;
	[SerializeField] float groundCheckRadius;

	// Use this for initialization
	void Start()
	{
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update()
	{
		if (isActionable)
		{
			CheckActiveInput();
		}
	}

	void FixedUpdate()
	{
		if (isJumping && rb.velocity.y > 0)
		{
			
		}
		else
		{
			// replace with non-discrete system
			isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
		}
		if (!isGrounded)
		{
			if (rb.velocity.y > -terminalFallSpeed)
			{
				rb.velocity -= transform.up * fallAccel * Time.fixedDeltaTime;
				if (rb.velocity.y < -terminalFallSpeed)
				{
					tempVec = rb.velocity;
					tempVec.y = -terminalFallSpeed;
					rb.velocity = tempVec;
				}
			}
		}
		else
		{
			LandingCheck();
		}
	}

	// for directional influence, teching, etc.
	void CheckPassiveInput()
	{
		
	}

	// for actions
	void CheckActiveInput()
	{
		if (Input.mousePosition.x > (float)(Screen.width / 2))
		{
			transform.eulerAngles = new Vector3(0, 90, 0);
		}
		else
		{
			transform.eulerAngles = new Vector3(0, -90, 0);
		}
		if (Input.GetKey(RIGHT))
		{
			if (isGrounded)
			{
				// facing right
				if (transform.rotation.y > 0)
				{
					anim.SetBool("isRunFwd", true);
					anim.SetBool("isRunBack", false);
					if (rb.velocity.x < runFwdSpeed)
					{
						rb.velocity += transform.forward * runFwdAccel * Time.fixedDeltaTime;
						if (rb.velocity.x > runFwdSpeed)
						{
							tempVec = rb.velocity;
							tempVec.x = runFwdSpeed;
							rb.velocity = tempVec;
						}
					}
				}
				else
				{
					anim.SetBool("isRunFwd", false);
					anim.SetBool("isRunBack", true);
				}
			}
		}
		else if (Input.GetKey(LEFT))
		{
			if (isGrounded)
			{
				// facing left
				if (transform.rotation.y < 0)
				{
					anim.SetBool("isRunFwd", true);
					anim.SetBool("isRunBack", false);
					if (rb.velocity.x > -runFwdSpeed)
					{
						rb.velocity += transform.forward * runFwdAccel * Time.fixedDeltaTime;
						if (rb.velocity.x < -runFwdSpeed)
						{
							tempVec = rb.velocity;
							tempVec.x = -runFwdSpeed;
							rb.velocity = tempVec;
						}
					}
				}
				else
				{
					anim.SetBool("isRunFwd", false);
					anim.SetBool("isRunBack", true);
				}
			}
		}
		else
		{
			anim.SetBool("isRunFwd", false);
			anim.SetBool("isRunBack", false);

		}
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
					tempVec = rb.velocity;
					tempVec.y = -fastFallSpeed;
					rb.velocity = tempVec;
				}
			}
		}
	}

	void LandingCheck()
	{
		if (isJumping && rb.velocity.y < 0)
		{
			if (rb.velocity.y < -hardLandThresholdSpeed)
			{
				anim.SetTrigger("HardLandTrigger");
				isActionable = false;
			}
			else
			{
				anim.SetTrigger("SoftLandTrigger");
			}
			isJumping = false;
		}
	}

	// called by animation event at end of JumpSquat
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
		isJumping = true;
	}
		
	// called by animation events
	void ChangeState(string id)
	{
		switch (id)
		{
		case "Idle":
			isActionable = true;
			break;
		default:
			print("Invalid ChangeState(string id) argument: " + id);
			break;
		}
	}
}
