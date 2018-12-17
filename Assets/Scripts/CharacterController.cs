using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
	KeyCode JUMP = KeyCode.Space;
	KeyCode RIGHT = KeyCode.D;
	KeyCode LEFT = KeyCode.A;
	KeyCode UP = KeyCode.W;
	KeyCode DOWN = KeyCode.S;

	Animator anim;
	Rigidbody rb;
	Vector3 tempVec;

	bool isGrounded = true;
	bool isHitstun = false;
	bool isActionable = true;
	bool isJumping = false;
	bool isCrouch = false;
	bool hasDoubleJump = true;

	[Header("MoveProperties")]
	[SerializeField] float runFwdSpeed;
	[SerializeField] float runBackSpeed;
	[SerializeField] float crouchWalkSpeed;
	[SerializeField] float airDriftSpeed;
	[SerializeField] float airDriftAccel;
	[SerializeField] float slideDampingFactor;

	[Header("JumpProperties")]
	[SerializeField] float fullHopSpeed;
	[SerializeField] float shortHopSpeed;
	[SerializeField] float doubleJumpSpeed;
	[SerializeField] float fallAccel;
	[SerializeField] float terminalFallSpeed;
	[SerializeField] float fastFallSpeed;
	[SerializeField] float hardLandThresholdSpeed;

	[Header("Grounding")]
	[SerializeField] LayerMask groundLayer;
	[SerializeField] Transform groundCheckPoint;
	[SerializeField] float groundCheckRadius;

	void OnDrawGizmos()
	{
		Gizmos.DrawSphere(groundCheckPoint.position, groundCheckRadius);
	}

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
			if (!isJumping)
			{
				anim.SetBool("isRunFwd", false);
				anim.SetBool("isRunBack", false);
				anim.SetTrigger("AirIdleTrigger");
				isJumping = true;
			}
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
			if (transform.forward.x < 0)
			{
				transform.eulerAngles = new Vector3(0, 90, 0);
			}
		}
		else
		{
			if (transform.forward.x > 0)
			{
				transform.eulerAngles = new Vector3(0, -90, 0);
			}
		}
		if (Input.GetKey(RIGHT) && Input.GetKey(LEFT))
		{
			if (isGrounded)
			{
				SlideToStop();
			}
		}
		else if (Input.GetKey(RIGHT))
		{
			if (isGrounded)
			{
				// facing right
				if (transform.rotation.y > 0)
				{
					anim.SetBool("isRunFwd", true);
					anim.SetBool("isRunBack", false);
					tempVec = rb.velocity;
					if (isCrouch)
					{
						tempVec.x = crouchWalkSpeed;
					}
					else
					{
						tempVec.x = runFwdSpeed;
					}
					rb.velocity = tempVec;
				}
				else
				{
					anim.SetBool("isRunFwd", false);
					anim.SetBool("isRunBack", true);
					tempVec = rb.velocity;
					if (isCrouch)
					{
						tempVec.x = crouchWalkSpeed;
					}
					else
					{
						tempVec.x = runBackSpeed;
					}
					rb.velocity = tempVec;
				}
			}
			else
			{
				if (rb.velocity.x < airDriftSpeed)
				{
					tempVec = rb.velocity;
					tempVec.x += airDriftAccel * Time.deltaTime;
					if (tempVec.x > airDriftSpeed)
					{
						tempVec.x = airDriftSpeed;
					}
					rb.velocity = tempVec;
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
					tempVec = rb.velocity;
					if (isCrouch)
					{
						tempVec.x = -crouchWalkSpeed;
					}
					else
					{
						tempVec.x = -runFwdSpeed;
					}
					rb.velocity = tempVec;
				}
				else
				{
					anim.SetBool("isRunFwd", false);
					anim.SetBool("isRunBack", true);
					tempVec = rb.velocity;
					if (isCrouch)
					{
						tempVec.x = -crouchWalkSpeed;
					}
					else
					{
						tempVec.x = -runBackSpeed;
					}
					rb.velocity = tempVec;
				}
			}
			else
			{
				if (rb.velocity.x > -airDriftSpeed)
				{
					tempVec = rb.velocity;
					tempVec.x -= airDriftAccel * Time.deltaTime;
					if (tempVec.x < -airDriftSpeed)
					{
						tempVec.x = -airDriftSpeed;
					}
					rb.velocity = tempVec;
				}
			}
		}
		else
		{
			if (isGrounded)
			{
				SlideToStop();
			}
		}
		if (Input.GetKeyDown(JUMP))
		{
			if (isGrounded || hasDoubleJump)
			{
				anim.SetTrigger("JumpTrigger");
			}
		}
		if (Input.GetKeyDown(DOWN))
		{
			isCrouch = true;
			anim.SetBool("isCrouch", true);
			if (!isGrounded)
			{
				// fast fall
				if (rb.velocity.y < 0 && rb.velocity.y > -fastFallSpeed)
				{
					tempVec = rb.velocity;
					tempVec.y = -fastFallSpeed;
					rb.velocity = tempVec;
				}
			}
		}
		if (Input.GetKeyUp(DOWN))
		{
			isCrouch = false;
			anim.SetBool("isCrouch", false);
		}
	}

	void SlideToStop()
	{
		anim.SetBool("isRunFwd", false);
		anim.SetBool("isRunBack", false);
		if (rb.velocity.x != 0)
		{
			tempVec = rb.velocity;
			tempVec.x *= Mathf.Pow(slideDampingFactor, Time.deltaTime);
			if (Mathf.Abs(tempVec.x) < 0.1)
			{
				tempVec.x = 0;
			}
			rb.velocity = tempVec;
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
			hasDoubleJump = true;
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
		anim.SetBool("isRunFwd", false);
		anim.SetBool("isRunBack", false);
	}

	void AddDoubleJumpForce()
	{
		tempVec = rb.velocity;
		tempVec.y = doubleJumpSpeed;
		// add aerial drift if player is holding direction
		if (Input.GetKey(RIGHT) && Input.GetKey(LEFT))
		{
			tempVec.x = 0;
		}
		else if (Input.GetKey(RIGHT))
		{
			tempVec.x = airDriftSpeed;
		}
		else if (Input.GetKey(LEFT))
		{
			tempVec.x = -airDriftSpeed;
		}
		else
		{
			tempVec.x = 0;
		}
		rb.velocity = tempVec;
		hasDoubleJump = false;
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
