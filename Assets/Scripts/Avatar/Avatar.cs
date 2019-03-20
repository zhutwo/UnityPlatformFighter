using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avatar : MonoBehaviour {

	const float AXIS_TILT_THRESHOLD = 0.5f;
	const float TORSO_ROTATION_OFFSET = 35.0f;
	const float SHOT_MOMENTUM_TRANSFER_RATIO = 0.5f;
	const float SHOTGUN_VELOCITY_DRIFT_LIMIT = 0.8f;
	const float GROUND_STUN_THRESHOLD = 60.0f;
	const float TECH_WINDOW = 0.25f;
	const int MAX_METER = 3000;
	const int METER_CHARGE_RATE = 5;
	const int METER_DAMAGE_RATIO = 5;

	MoveManager movemgr;
	InputManager input;
	Animator anim;
	Rigidbody rb;
	AudioSource audio;
	GameObject[,] tracers;

	Vector3 specialVector;
	Vector3 specialDestination;
	Vector3 tempVec;
	Vector3 savedVelocity;
	Vector3 lookDirection;
	Vector3 knockbackToApply;
	Quaternion startRotation;
	Quaternion lookRotation;

	float comboTimer;
	float shotTimer;
	float reloadTimer;
	float techTimer;
	float stunTimer;
	float freezeTimer;

	bool groundCheck;
	bool applyGroundMotion = false;
	bool isGrounded = true;
	bool isHitstun = false;
	bool isActionable = true;
	bool isCrouch = false;
	bool hasDoubleJump = true;
	bool specialMovement = false;
	bool specialStartup = false;
	bool isFreezeFrame = false;
	bool wasTechPressed = false;
	bool isReloading = false;
	bool isCombo = false;
	bool isShotCooldown = false;

	[SerializeField] bool control = true;
	[SerializeField] State currentState;
	[SerializeField] Weapon currentWeapon;
	[SerializeField] UIManager ui;
	[SerializeField] GameObject trailPrefab;
	[SerializeField] public int playerID;

	[Header("Stats")]
	[SerializeField] float weight;
	[SerializeField] int maxhealth;
	[SerializeField] int health;
	[SerializeField] int clipSize;
	[SerializeField] int ammo;
	[SerializeField] int meter;
	[SerializeField] float damage = 0;

	[Header("Geometry")]
	[SerializeField] Material baseMaterial;
	[SerializeField] Transform rotationPoint;
	[SerializeField] GameObject hipRotationBone;
	[SerializeField] GameObject spineRotationBone;
	[SerializeField] GameObject spineTopRotationBone;
	[SerializeField] GameObject headRotationBone;

	[Header("MoveProperties")]
	[SerializeField] float dashSpeed;
	[SerializeField] float runSpeed;
	[SerializeField] float walkSpeed;
	[SerializeField] float crawlSpeed;
	[SerializeField] float airDriftSpeed;
	[SerializeField] float airDriftAccel;
	[SerializeField] float airFriction;
	[SerializeField] float groundTraction;

	[Header("JumpProperties")]
	[SerializeField] float fullHopSpeed;
	[SerializeField] float shortHopSpeed;
	[SerializeField] float doubleJumpSpeed;
	[SerializeField] float fallAccel;
	[SerializeField] float terminalFallSpeed;
	[SerializeField] float fastFallSpeed;
	[SerializeField] float hardLandThresholdSpeed;

	[Header("AttackProperties")]
	[SerializeField] GameObject gunModel;
	[SerializeField] GameObject bladeModelR;
	[SerializeField] GameObject bladeModelL;
	[SerializeField] GameObject muzzleFlash;
	[SerializeField] GameObject tracerPrefab;
	[SerializeField] int tracersPerShot;
	[SerializeField] float reloadTime;
	[SerializeField] float shotCooldown;
	[SerializeField] float weaponSpread;
	[SerializeField] float shotForce;

	[Header("SpecialProperties")]
	[SerializeField] Material illusionMaterial;
	[SerializeField] SkinnedMeshRenderer modelMesh;
	[SerializeField] GameObject illusionPrefab;
	[SerializeField] float specialRange;
	[SerializeField] float specialSpeed;
	[SerializeField] int meterCost;

	[Header("GroundCheck")]
	[SerializeField] LayerMask groundLayer;
	[SerializeField] Transform groundCheckPoint;
	[SerializeField] float groundCheckLength;
	[SerializeField] float maxSlopeAngle;

	public int Health {
		get { return health; }
	}

	public int MaxHealth {
		get { return maxhealth; }
	}

	public float Weight {
		get { return weight; }
	}

	public float Damage {
		get { return damage; }
	}

	void OnDrawGizmos() {
		Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckLength);
	}

	// Use this for initialization
	void Start() {
		movemgr = GetComponent<MoveManager>();
		input = GetComponent<InputManager>();
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
		audio = GetComponent<AudioSource>();
		currentState = State.IDLE;
		currentWeapon = Weapon.MELEE;
		// bullets preloaded and reused for better runtime performance
		tracers = new GameObject[clipSize, tracersPerShot];
		for (int i = 0; i < clipSize; i++)
		{
			for (int j = 0; j < tracersPerShot; j++)
			{
				tracers[i, j] = Instantiate(tracerPrefab);
				tracers[i, j].GetComponent<Tracer>().SetOwner(this.gameObject);
			}
		}
		ammo = clipSize;
		health = maxhealth;
		meter = MAX_METER;
		startRotation = hipRotationBone.transform.rotation;
	}

	void Update() {
		if (control)
		{
			input.UpdateAxes();
			SetAnimStickDirection(input.moveAxes.direction);
			SetAnimTiltLevel(input.moveAxes.tiltLevel);
			ComboLink();
			if (isActionable)
			{
				AimRotation();
				if (isGrounded)
				{
					MovementInput();
				}
				ActionInput();
			}
		}
		if (specialMovement)
		{
			GameObject illusion = Instantiate(illusionPrefab, transform.position, transform.rotation);
			illusion.GetComponent<DolphIllusion>().SetLookVector(specialDestination);
		}
		else
		{
			ApplyPhysics();
			GroundCheck();
			if (!isHitstun)
			{
				Move();
			}
		}
		if (meter < MAX_METER)
		{
			meter += METER_CHARGE_RATE;
			if (meter > MAX_METER)
			{
				meter = MAX_METER;
			}
		}
		if (currentState == State.TUMBLE)
		{
			Instantiate(trailPrefab, transform.position, transform.rotation);
		}
		UpdateUI();
		RunTimers();
	}

	void LateUpdate() {
		if (control)
			LookAtCursor();
	}

	#region Input

	void AimRotation() {
		if (currentWeapon == Weapon.RANGED)
		{
			if (input.aimAxes.x == 0.0f && input.aimAxes.y == 0.0f)
			{
				lookDirection = transform.forward;
				lookRotation = Quaternion.LookRotation(lookDirection);
			}
			else
			{
				lookDirection = new Vector3(input.aimAxes.x, input.aimAxes.y, 0.0f);
				lookRotation = Quaternion.LookRotation(lookDirection);
				if (input.aimAxes.x > 0.0f)
				{
					transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
					anim.SetFloat("LookDirection", transform.forward.x);
				}
				else if (input.aimAxes.x < 0.0f)
				{
					transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
					anim.SetFloat("LookDirection", transform.forward.x);
				}
			}
		}
	}

	void TurnAround() {
		if (transform.forward.x < 0.0f)
		{
			transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
		}
		else
		{
			transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
		}
		anim.SetFloat("LookDirection", transform.forward.x);
	}

	void ActionInput() {
		if (input.GetButtonDown(Button.JUMP))
		{
			if (hasDoubleJump)
			{
				TriggerOneFrame("JumpTrigger");
			}
		}
		if (input.cStick.tiltLevel == 2)
		{
			if (currentWeapon == Weapon.MELEE)
			{
				TriggerOneFrame("AttackTrigger", input.cStick.direction);
			}
		}
		else if (input.GetButtonDown(Button.ATTACK))
		{
			if (currentWeapon == Weapon.RANGED)
			{
				TriggerOneFrame("ShotTrigger");
				FireWeapon();
			}
			else
			{
				TriggerOneFrame("AttackTrigger", input.moveAxes.direction);
			}
		}
		else if (input.GetButtonDown(Button.DEFEND))
		{
			if (isGrounded && currentWeapon == Weapon.MELEE)
			{
				TriggerOneFrame("DefendTrigger", input.moveAxes.direction);
			}
		}
		else if (input.GetButtonDown(Button.SPECIAL))
		{
			if (currentWeapon == Weapon.MELEE && meter >= meterCost)
			{
				TriggerOneFrame("SpecialTrigger");
			}
		}
		else if (input.GetButtonDown(Button.SWAP))
		{
			ChangeWeapon();
		}
		else if (input.GetButtonDown(Button.RELOAD))
		{
			Reload();
		}
	}

	void MovementInput() {

		switch (currentState)
		{
		case (State.DASH):
			switch (input.moveAxes.direction)
			{
			case (AxesInfo.Direction.LEFT):
			case (AxesInfo.Direction.RIGHT):
				if (!DirectionSameAsInput(input.moveAxes))
				{
					TriggerOneFrame("IdleTrigger");
					TurnAround();
				}
				break;
			case (AxesInfo.Direction.UP):
				// jump if tap jump
				break;
			default:
				break;
			}
			break;
		case (State.RUN):
			switch (input.moveAxes.direction)
			{
			case (AxesInfo.Direction.LEFT):
			case (AxesInfo.Direction.RIGHT):
				if (!DirectionSameAsInput(input.moveAxes))
				{
					TriggerOneFrame("RunTurnTrigger");
				}
				break;
			case (AxesInfo.Direction.DOWN):
				if (input.moveAxes.tiltLevel == 2)
				{
					TriggerOneFrame("CrouchTrigger");
				}
				break;
			case (AxesInfo.Direction.UP):
				// jump if tap jump
				break;
			case (AxesInfo.Direction.NONE):
				TriggerOneFrame("RunStopTrigger");
				break;
			default:
				break;
			}
			break;
		case (State.WALKFWD):
			switch (input.moveAxes.direction)
			{
			case (AxesInfo.Direction.LEFT):
			case (AxesInfo.Direction.RIGHT):
				if (!DirectionSameAsInput(input.moveAxes))
				{
					TriggerOneFrame("IdleTrigger");
					TurnAround();
				}
				break;
			case (AxesInfo.Direction.DOWN):
				if (input.moveAxes.tiltLevel == 2)
				{
					TriggerOneFrame("CrouchTrigger");
				}
				break;
			case (AxesInfo.Direction.UP):
				// jump if tap jump
				break;
			case (AxesInfo.Direction.NONE):
				TriggerOneFrame("IdleTrigger");
				break;
			default:
				break;
			}
			break;
		case (State.CROUCH):
			switch (input.moveAxes.direction)
			{
			case (AxesInfo.Direction.LEFT):
			case (AxesInfo.Direction.RIGHT):
				if (DirectionSameAsInput(input.moveAxes))
				{
					if (input.moveAxes.tiltLevel == 2)
					{
						TriggerOneFrame("DashTrigger");
					}
					else
					{
						TriggerOneFrame("WalkTrigger");
					}
				}
				else
				{
					TriggerOneFrame("IdleTrigger");
					TurnAround();
				}
				break;
			case (AxesInfo.Direction.NONE):
				TriggerOneFrame("IdleTrigger");
				break;
			case (AxesInfo.Direction.UP):
				// jump if tap jump
				break;
			default:
				break;
			}
			break;
		case (State.IDLE):
			switch (input.moveAxes.direction)
			{
			case (AxesInfo.Direction.LEFT):
			case (AxesInfo.Direction.RIGHT):
				if (DirectionSameAsInput(input.moveAxes))
				{
					if (input.moveAxes.tiltLevel == 2)
					{
						TriggerOneFrame("DashTrigger");
					}
					else
					{
						TriggerOneFrame("WalkTrigger");
					}
				}
				else
				{
					TurnAround();
				}
				break;
			case (AxesInfo.Direction.DOWN):
				if (input.moveAxes.tiltLevel == 2)
				{
					TriggerOneFrame("CrouchTrigger");
				}
				break;
			case (AxesInfo.Direction.UP):
				// jump if tap jump
				break;
			default:
				break;
			}
			break;
		case (State.RUNSTOP):
		case (State.RUNTURN):
			break;
		default:
			switch (input.moveAxes.direction)
			{
			case (AxesInfo.Direction.LEFT):
			case (AxesInfo.Direction.RIGHT):
				if (DirectionSameAsInput(input.moveAxes))
				{
					if (input.moveAxes.tiltLevel == 2)
					{
						TriggerOneFrame("DashTrigger");
					}
					else
					{
						TriggerOneFrame("WalkTrigger");
					}
				}
				else
				{
					TriggerOneFrame("IdleTrigger");
					TurnAround();
				}
				break;
			case (AxesInfo.Direction.DOWN):
				if (input.moveAxes.tiltLevel == 2)
				{
					TriggerOneFrame("CrouchTrigger");
				}
				break;
			case (AxesInfo.Direction.UP):
				// jump if tap jump
				break;
			default:
				break;
			}
			break;
		}
	}

	void BufferTechInput() {
		if (!wasTechPressed && input.GetButtonDown(Button.DEFEND))
		{
			techTimer = TECH_WINDOW;
			wasTechPressed = true;
		}
	}

	void ComboLink() {
		if (currentState == State.COMBO)
		{
			if (input.GetButtonDown(Button.ATTACK))
			{
				anim.SetBool("LinkCombo", true);
			}
		}
	}

	bool DirectionSameAsInput(AxesInfo axes) {
		if (axes.x * transform.forward.x > 0.0f)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	#endregion

	#region Physics

	void ApplyPhysics() {
		if (!isFreezeFrame)
		{
			if (isGrounded)
			{
				AddFriction();
				FreeFall(); // todo: make ground clamp instead 
			}
			else
			{
				FreeFall();
			}
		}
	}

	void AddFriction() {
		if (rb.velocity.x != 0.0f)
		{
			if (Mathf.Abs(rb.velocity.x) - groundTraction * Time.deltaTime <= 0.0f)
			{
				tempVec = rb.velocity;
				tempVec.x = 0.0f;
				rb.velocity = tempVec;
			}
			else
			{
				rb.velocity -= groundTraction * Mathf.Sign(rb.velocity.x) * Vector3.right * Time.deltaTime;
			}
		}
	}

	void TransferLandingMomentum() {
		tempVec = rb.velocity;
		tempVec.y = 0.0f;
		rb.velocity = tempVec;
	}

	void GroundCheck() {
		groundCheck = Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckLength, groundLayer);
		if (groundCheck)
		{
			// if falling
			if (!isGrounded && rb.velocity.y < 0.0f)
			{
				if (isHitstun)
				{
					if (wasTechPressed && techTimer > 0.0f)
					{
						isHitstun = false;
						TriggerOneFrame("TechTrigger");
					}
					else
					{
						if (true) //add rebound 
						{
							
						}
						isHitstun = false;
						TriggerOneFrame("MissedTechTrigger");
					}
					rb.velocity = Vector3.zero;
				}
				else
				{
					
					if (currentState == State.AERIAL)
					{
						TriggerOneFrame("HardLandTrigger");
					}
					else if (rb.velocity.y < -hardLandThresholdSpeed)
					{
						TriggerOneFrame("HardLandTrigger");
					}
					else
					{
						TriggerOneFrame("SoftLandTrigger");
					}
					TransferLandingMomentum();
				}
			}
		}
		else
		{
			// if walked off platform
			if (isGrounded)
			{
				TriggerOneFrame("AirIdleTrigger");
			}
		}
	}

	void FreeFall() {
		if (rb.velocity.y > -terminalFallSpeed)
		{
			rb.velocity -= transform.up * fallAccel * Time.deltaTime;
			if (rb.velocity.y < -terminalFallSpeed)
			{
				tempVec = rb.velocity;
				tempVec.y = -terminalFallSpeed;
				rb.velocity = tempVec;
			}
		}
		if (rb.velocity.x > airDriftSpeed)
		{
			rb.velocity += Vector3.left * airDriftAccel * Time.deltaTime;
		}
		else if (rb.velocity.x < -airDriftSpeed)
		{
			rb.velocity += Vector3.right * airDriftAccel * Time.deltaTime;
		}
	}

	public void StartFreezeFrame(float time) {
		savedVelocity = rb.velocity;
		freezeTimer = time;
		isFreezeFrame = true;
		anim.speed = 0.0f;
		rb.constraints = RigidbodyConstraints.FreezeAll;
	}

	void EndFreezeFrame() {
		isFreezeFrame = false;
		anim.speed = 1.0f;
		rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ; //reset constraints to normal
		if (currentState == State.TUMBLE)
		{
			rb.velocity = knockbackToApply / 8.0f;
			techTimer = 0.0f;
			wasTechPressed = false;
		}
		else
		{
			rb.velocity = savedVelocity;
		}
	}

	public void TakeHit(int damage, float freezeTime, float stunTime, Vector3 knockback) {
		//health -= damage;
		this.damage += damage;
		stunTimer = stunTime;
		isHitstun = true;
		knockbackToApply = knockback;
		if (isGrounded && knockback.magnitude < GROUND_STUN_THRESHOLD)
		{
			ChangeState(State.GROUNDSTUN);
			//stunTimer *= 0.5f;
			TriggerOneFrame("GroundStunTrigger");
		}
		else
		{
			ChangeState(State.TUMBLE);
			TriggerOneFrame("TumbleTrigger");
			isGrounded = false;
			hipRotationBone.transform.rotation = Quaternion.LookRotation(-knockback, Vector3.up);
		}
		transform.eulerAngles = new Vector3(0.0f, Mathf.Sign(knockback.x) * -90.0f, 0.0f);
		StartFreezeFrame(freezeTime);
	}

	#endregion

	#region Movement

	void Move() {
		if (isGrounded && isActionable)
		{
			ApplyGroundMotion();
		}
		else if (!isGrounded && currentState != State.TUMBLE) // airborne
		{
			ApplyAirMotion();
		}
	}

	void ApplyGroundMotion() {
		
		switch (currentState)
		{
		case (State.DASH):
		case (State.DASHATTACK):
			rb.velocity = dashSpeed * transform.forward;
			break;
		case (State.RUN):
			rb.velocity = runSpeed * transform.forward;
			break;
		case (State.WALKFWD):
			rb.velocity = walkSpeed * transform.forward;
			break;
		case (State.WALKBACK):
			rb.velocity = -walkSpeed * transform.forward;
			break;
		default:
			break;
		}
	}

	void ApplyAirMotion() {
		// aerial drift
		tempVec = rb.velocity;
		if (tempVec.x < airDriftSpeed || tempVec.x > -airDriftSpeed)
		{
			tempVec.x += input.moveAxes.x * airDriftAccel * Time.deltaTime;
		}
		rb.velocity = tempVec;
		// fast fall
		if (input.moveAxes.y < -AXIS_TILT_THRESHOLD)
		{
			if (rb.velocity.y < 0.0f && rb.velocity.y > -fastFallSpeed)
			{
				tempVec = rb.velocity;
				tempVec.y = -fastFallSpeed;
				rb.velocity = tempVec;
			}
		}
	}

	// called by animation event at end of JumpSquat
	void AddJumpForce() {
		// performs full hop if player is still holding jump at end of jump squat animation
		if (input.GetButtonHeld(Button.JUMP))
		{
			rb.velocity += transform.up * fullHopSpeed;
		}
		else
		{
			rb.velocity += transform.up * shortHopSpeed;
		}
		isGrounded = false;
	}

	// called by animation event
	void AddDoubleJumpForce() {
		tempVec = rb.velocity;
		tempVec.y = doubleJumpSpeed;
		// add aerial drift if player is holding direction
		tempVec.x = input.moveAxes.x * airDriftSpeed;
		rb.velocity = tempVec;
		hasDoubleJump = false;
	}

	#endregion

	#region AnimTools

	void TriggerOneFrame(string trigger, AxesInfo.Direction direction = AxesInfo.Direction.NULL) {
		if (direction != AxesInfo.Direction.NULL)
		{
			SetAnimStickDirection(direction);
		}
		StartCoroutine(TriggerOneFrameCoroutine(trigger));
	}

	void SetAnimStickDirection(AxesInfo.Direction direction) {
		if (transform.forward.x < 0.0f)
		{
			if (direction == AxesInfo.Direction.RIGHT)
			{
				direction = AxesInfo.Direction.LEFT;
			}
			else if (direction == AxesInfo.Direction.LEFT)
			{
				direction = AxesInfo.Direction.RIGHT;
			}
		}
		anim.SetInteger("StickDirection", (int)direction);
	}

	void SetAnimTiltLevel(int tilt) {
		anim.SetInteger("TiltLevel", tilt);
	}

	IEnumerator TriggerOneFrameCoroutine(string trigger) {
		anim.SetTrigger(trigger);
		yield return null;
		if (anim != null)
		{
			anim.ResetTrigger(trigger);
		}
	}

	void SetActionable(int i) {
		if (i == 0)
		{
			isActionable = false;
		}
		else if (i == 1)
		{
			isActionable = true;
		}
		else
		{
			print("Animator passed invalid argument to PlayableCharacter::SetActionable (must be 0 or 1)");
		}
	}

	void ChangeState(State newState) {

		if (currentState == State.ATTACK || currentState == State.DASHATTACK || currentState == State.AERIAL || currentState == State.COMBO || currentState == State.SPECIAL)
		{
			movemgr.ResetHitboxes();
		}
		if (newState == State.SPECIAL)
		{
			specialStartup = true;
		}
		else if (newState == State.AIRIDLE || newState == State.AERIAL)
		{
			isGrounded = false;
		}
		else
		{
			// if changing from airborne to grounded state
			if (!isGrounded)
			{
				hasDoubleJump = true;
			}
			isGrounded = true;
		}
		if (newState == State.ATTACK || newState == State.DASHATTACK || newState == State.SPECIAL || newState == State.AERIAL || newState == State.LANDLAG || newState == State.TUMBLE || newState == State.GROUNDSTUN || newState == State.COMBO)
		{
			isActionable = false;
		}
		else
		{
			isActionable = true;
		}
		currentState = newState;
		anim.SetBool("LinkCombo", false);
	}

	void ChangeWeaponAnimCallback(int i) {
		isActionable = true;
		if (i == 0)
		{
			currentWeapon = Weapon.RANGED;
			gunModel.SetActive(true);
			bladeModelR.SetActive(false);
			bladeModelL.SetActive(false);
		}
		else if (i == 1)
		{
			currentWeapon = Weapon.MELEE;
			gunModel.SetActive(false);
			bladeModelR.SetActive(true);
			bladeModelL.SetActive(true);
		}
		else
		{
			print("Animator passed invalid argument to PlayableCharacter::ChangeWeaponAnimCallback (must be 0 or 1)");
		}
	}

	void ChangeWeapon() {
		isActionable = false;
		if (currentWeapon == Weapon.MELEE)
		{
			anim.SetBool("isMelee", false);
		}
		else
		{
			anim.SetBool("isMelee", true);
		}
	}

	void StartSpecial() {
		if (input.spcAxes.x == 0.0f && input.spcAxes.y == 0.0f)
		{
			specialVector = transform.forward;
		}
		else
		{
			if (Mathf.Sign(input.spcAxes.x) != Mathf.Sign(transform.forward.x))
			{
				TurnAround();
			}
			specialVector = new Vector3(input.spcAxes.x, input.spcAxes.y, 0.0f);
		}
		if (isGrounded && specialVector.y < 0.0f)
		{
			specialVector.y = 0.0f;
		}
		specialVector.Normalize();
		specialDestination = (specialVector * specialRange) + hipRotationBone.transform.position;
		specialMovement = true;
		specialStartup = false;
		modelMesh.material = illusionMaterial;
		rb.velocity = specialVector * specialSpeed;
		meter -= meterCost;
	}

	void ExitSpecial() {
		specialMovement = false;
		modelMesh.material = baseMaterial;
		groundCheck = Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckLength, groundLayer);
		if (groundCheck)
		{
			isGrounded = true;
			if (specialVector.y < -0.2f)
			{
				anim.SetTrigger("HardLandTrigger");
			}
			else
			{
				anim.SetTrigger("SoftLandTrigger");
			}
			//TransferLandingMomentum();
			rb.velocity = Vector3.zero;
		}
		else
		{
			rb.velocity = specialVector * (airDriftSpeed / 2.0f);
			isGrounded = false;
			anim.SetTrigger("AirIdleTrigger");
		}
	}

	void ChangeMaterial(int i) {
		if (i == 0)
		{
			modelMesh.material = baseMaterial;
		}
		else if (i == 1)
		{
			modelMesh.material = illusionMaterial;
		}
	}

	void Reload() {
		if (!isReloading)
		{
			reloadTimer = reloadTime;
			isReloading = true;
		}
	}

	void FireWeapon() {
		if (ammo > 0)
		{
			if (!isShotCooldown)
				ShotgunBlast();
		}
	}

	void ShotgunBlast() {
		float spread;
		float drift;
		ammo--;
		isShotCooldown = true;
		shotTimer = shotCooldown;
		muzzleFlash.SetActive(true);
		audio.Play();
		for (int i = 0; i < tracersPerShot; i++)
		{
			spread = Random.Range(-weaponSpread, weaponSpread);
			drift = Random.Range(SHOTGUN_VELOCITY_DRIFT_LIMIT, 1.0f);
			tracers[ammo, i].transform.position = muzzleFlash.transform.position;
			tracers[ammo, i].transform.rotation = lookRotation;
			tracers[ammo, i].transform.Rotate(spread, 0.0f, 0.0f);
			tracers[ammo, i].SetActive(true);
			tracers[ammo, i].GetComponent<Rigidbody>().velocity = (tracers[ammo, i].transform.forward * shotForce * drift) + (rb.velocity * SHOT_MOMENTUM_TRANSFER_RATIO);
		}
		if (ammo <= 0)
		{
			Reload();
		}

	}

	void LookAtCursor() {
		if (currentWeapon == Weapon.RANGED)
		{
			lookRotation = Quaternion.LookRotation(lookDirection);
			spineRotationBone.transform.rotation = lookRotation;
			tempVec = spineTopRotationBone.transform.localEulerAngles;
			tempVec.y += TORSO_ROTATION_OFFSET;
			spineTopRotationBone.transform.localEulerAngles = tempVec;
			headRotationBone.transform.rotation = lookRotation;
		}
		else if (currentState == State.SPECIAL)
		{
			lookRotation = Quaternion.LookRotation(specialVector);
			spineRotationBone.transform.rotation = lookRotation;
			//spineRotationBone.transform.LookAt(specialDestination);
		}
	}

	#endregion

	#region Misc

	public void AddMeter(int damage) {
		if (meter < MAX_METER)
		{
			meter += damage * METER_DAMAGE_RATIO;
			if (meter > MAX_METER)
			{
				meter = MAX_METER;
			}
		}
	}

	void RunTimers() {
		if (isFreezeFrame)
		{
			freezeTimer -= Time.deltaTime;
			if (freezeTimer <= 0.0f)
			{
				EndFreezeFrame();
			}
		}
		else if (isHitstun)
		{
			stunTimer -= Time.deltaTime;
			if (stunTimer <= 0.0f)
			{
				isHitstun = false;
				if (currentState == State.TUMBLE)
				{
					TriggerOneFrame("AirIdleTrigger");
				}
				else
				{
					TriggerOneFrame("GroundStunEndTrigger");
				}
			}
		}
		if (isShotCooldown)
		{
			shotTimer -= Time.deltaTime;
			if (shotTimer <= 0.0f)
			{
				isShotCooldown = false;
			}
		}
		if (isReloading)
		{
			reloadTimer -= Time.deltaTime;
			if (reloadTimer <= 0.0f)
			{
				ammo = clipSize;
				isReloading = false;
			}
		}

		if (wasTechPressed)
		{
			techTimer -= Time.deltaTime;
		}
	}

	void UpdateUI() {
		ui.SetAmmo(ammo, playerID);
		ui.SetMeter(meter, MAX_METER, playerID);
		ui.SetDamage((int)damage, playerID);
	}

	public void Respawn(Vector3 position) {
		ammo = clipSize;
		damage = 0.0f;
		meter = MAX_METER;
		rb.transform.position = position;
		currentState = State.IDLE;
		TriggerOneFrame("AirIdleTrigger");
	}

	#endregion
}
