using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avatar : MonoBehaviour {

	const float X_AXIS_TILT_THRESHOLD = 0.25f;
	const float Y_AXIS_TILT_THRESHOLD = 0.25f;
	const float TORSO_ROTATION_OFFSET = 35.0f;
	const float SHOT_MOMENTUM_TRANSFER_RATIO = 0.5f;
	const float SHOTGUN_VELOCITY_DRIFT_LIMIT = 0.8f;
	const float GROUND_STUN_THRESHOLD = 60.0f;
	const float TECH_WINDOW = 0.25f;
	const int MAX_METER = 3000;
	const int METER_CHARGE_RATE = 5;
	const int METER_DAMAGE_RATIO = 5;

	InputManager input;
	Animator anim;
	Rigidbody rb;
	AudioSource audio;
	GameObject[,] tracers;

	Vector2 moveAxes;
	Vector2 aimAxes;
	Vector2 specialAxes;
	Vector3 specialVector;
	Vector3 specialDestination;
	Vector3 tempVec;
	Vector3 oldVec;
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
	[SerializeField] float rangedSpeedMulti;
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
	[SerializeField] float groundCheckRadius;
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
		Gizmos.DrawSphere(groundCheckPoint.position, groundCheckRadius);
	}

	// Use this for initialization
	void Start() {
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
	
	// Update is called once per frame
	void Update() {
		if (control)
		{
			AxisInput();
			ComboLink();
			if (isActionable)
			{
				TurnAround();
				//AimRotation();
				ActionInput();
			}
		}
		if (!specialMovement)
		{
			ApplyPhysics();
			GroundCheck();
			if (!isHitstun)
			{
				Move();
			}
		}
		else // if special movement
		{
			//rb.velocity = specialVector * specialSpeed;
		}
		RunTimers();
	}

	void FixedUpdate() {
		if (specialMovement)
		{
			GameObject illusion = Instantiate(illusionPrefab, transform.position, transform.rotation);
			illusion.GetComponent<DolphIllusion>().SetLookVector(specialDestination);
		}
		else
		{
			//ApplyPhysics();
			//GroundCheck();
			//Move();
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
	}

	void LateUpdate() {
		if (control)
			LookAtCursor();
	}

	#region Input

	void AimRotation() {
		if (currentWeapon == Weapon.RANGED)
		{
			if (aimAxes.x == 0.0f && aimAxes.y == 0.0f)
			{
				lookDirection = transform.forward;
				lookRotation = Quaternion.LookRotation(lookDirection);
			}
			else
			{
				lookDirection = new Vector3(aimAxes.x, aimAxes.y, 0.0f);
				lookRotation = Quaternion.LookRotation(lookDirection);
			}
		}
	}

	void TurnAround() {
		float direction = transform.forward.x;
		switch (currentWeapon)
		{
		case Weapon.MELEE:
			direction = moveAxes.x;
			break;
		case Weapon.RANGED:
			direction = aimAxes.x;
			break;
		}
		if (direction > 0.0f)
		{
			transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
			anim.SetFloat("lookDirection", transform.forward.x);
		}
		else if (direction < 0.0f)
		{
			transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
			anim.SetFloat("lookDirection", transform.forward.x);
		}
	}

	void BufferTechInput() {
		if (!wasTechPressed && input.GetButtonDown(Button.DEFEND))
		{
			techTimer = TECH_WINDOW;
			wasTechPressed = true;
		}
	}

	void AxisInput() {
		moveAxes = input.GetMoveAxes();
		aimAxes = input.GetAimAxes();
		specialAxes = input.GetSpecialAxes();
		anim.SetFloat("xAxis", moveAxes.x);
		anim.SetFloat("yAxis", moveAxes.y);
	}

	void ActionInput() {
		if (Input.GetButtonDown("Jump"))
		{
			if (hasDoubleJump)
			{
				anim.SetTrigger("JumpTrigger");
			}
		}
		else if (Input.GetButtonDown("Attack"))
		{
			if (currentWeapon == Weapon.RANGED)
			{
				anim.SetTrigger("shotTrigger");
				FireWeapon();
			}
			else if (isGrounded)
			{
				switch (currentState)
				{
				case (State.RUNFWD):
					if (moveAxes.y > Y_AXIS_TILT_THRESHOLD)
					{
						anim.SetTrigger("uTiltTrigger");
					}
					else if (moveAxes.y < -Y_AXIS_TILT_THRESHOLD)
					{
						anim.SetTrigger("dTiltTrigger");
					}
					else
					{
						anim.SetTrigger("dashAttackTrigger");
					}
					break;
				case (State.RUNBACK):
					anim.SetTrigger("fTiltTrigger");
					break;
				case (State.CROUCH):
					anim.SetTrigger("dTiltTrigger");
					break;
				case (State.IDLE):
					if (moveAxes.y > Y_AXIS_TILT_THRESHOLD)
					{
						anim.SetTrigger("uTiltTrigger");
					}
					else
					{
						anim.SetTrigger("jabTrigger");
					}
					break;
				default:
					break;
				}
			}
			else // aerial attacks
			{
				if (moveAxes.y > Y_AXIS_TILT_THRESHOLD)
				{
					anim.SetTrigger("upairTrigger");
				}
				else if (moveAxes.y < -Y_AXIS_TILT_THRESHOLD)
				{
					anim.SetTrigger("dairTrigger");
				}
				else if (moveAxes.x * transform.forward.x > X_AXIS_TILT_THRESHOLD)
				{
					anim.SetTrigger("fairTrigger");
				}
				else
				{
					anim.SetTrigger("nairTrigger");
				}
			}
		}
		else if (Input.GetButtonDown("Defend"))
		{
			if (isGrounded && currentWeapon == Weapon.MELEE)
			{
				if (currentState == State.RUNFWD)
				{
					anim.SetTrigger("FwdRollTrigger");
				}
				else if (currentState == State.RUNBACK)
				{
					anim.SetTrigger("BackRollTrigger");
				}
				else
				{
					anim.SetTrigger("defendTrigger");
				}
			}
		}
		else if (Input.GetButtonDown("Special"))
		{
			if (currentWeapon == Weapon.MELEE && meter >= meterCost)
			{
				anim.SetTrigger("specialTrigger");
			}
		}
		else if (Input.GetButtonDown("Swap"))
		{
			ChangeWeapon();
		}
		else if (Input.GetButtonDown("Reload"))
		{
			Reload();
		}
	}

	void ComboLink() {
		if (currentState == State.COMBO)
		{
			if (Input.GetButtonDown("Attack"))
			{
				anim.SetBool("LinkCombo", true);
			}
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
			tempVec = rb.velocity;
			tempVec.x *= Mathf.Pow(slideDampingFactor, Time.deltaTime);
			if (Mathf.Abs(tempVec.x) < 0.001f)
			{
				tempVec.x = 0.0f;
			}
			rb.velocity = tempVec;
		}
	}

	void GroundCheck() {
		groundCheck = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
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
						anim.SetTrigger("TechTrigger");
					}
					else
					{
						if (true) //add rebound 
						{
							
						}
						isHitstun = false;
						anim.SetTrigger("MissedTechTrigger");
					}
				}
				else
				{
					
					if (currentState == State.AERIAL)
					{
						anim.SetTrigger("HardLandTrigger");
					}
					
					if (rb.velocity.y < -hardLandThresholdSpeed)
					{
						anim.SetTrigger("HardLandTrigger");
					}
					else
					{
						anim.SetTrigger("SoftLandTrigger");
					}
				}
			}
		}
		else
		{
			// if walked off platform
			if (isGrounded)
			{
				anim.SetTrigger("AirIdleTrigger");
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
		freezeTimer = time;
		isFreezeFrame = true;
		anim.speed = 0.0f;
		rb.constraints = RigidbodyConstraints.FreezeAll;
	}

	void EndFreezeFrame() {
		isFreezeFrame = false;
		anim.speed = 1.0f;
		rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
		if (isHitstun)
		{
			rb.velocity = knockbackToApply / 8.0f;

			techTimer = 0.0f;
			wasTechPressed = false;
		}
	}

	public void TakeHit(int damage, float freezeTime, float stunTime, Vector3 knockback) {
		//health -= damage;
		this.damage += damage;
		stunTimer = stunTime;
		isHitstun = true;
		knockbackToApply = knockback;
		if (knockback.y < GROUND_STUN_THRESHOLD)
		{
			stunTimer *= 0.5f;
			knockbackToApply = Vector3.zero;
			anim.SetTrigger("GroundStunTrigger");
		}
		else
		{
			ChangeState(State.TUMBLE);
			anim.SetTrigger("TumbleTrigger");
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
			tempVec = rb.velocity;
			if (isCrouch)
			{
				tempVec.x = moveAxes.x * crouchWalkSpeed;
			}
			else
			{
				if (transform.forward.x * moveAxes.x > 0.0f)
				{
					if (currentWeapon == Weapon.MELEE)
					{
						tempVec.x = moveAxes.x * runFwdSpeed;
						anim.SetFloat("runSpeed", 1.0f);
					}
					else
					{
						tempVec.x = moveAxes.x * runFwdSpeed * rangedSpeedMulti;
						anim.SetFloat("runSpeed", rangedSpeedMulti);
					}

				}
				else
				{
					tempVec.x = moveAxes.x * runBackSpeed;
				}
			}
			rb.velocity = tempVec;
		}
		else if (!isGrounded && currentState != State.TUMBLE) // airborne
		{
			// aerial drift
			tempVec = rb.velocity;
			if (tempVec.x < airDriftSpeed || tempVec.x > -airDriftSpeed)
			{
				tempVec.x += moveAxes.x * airDriftAccel * Time.deltaTime;
			}
			rb.velocity = tempVec;
			// fast fall
			if (moveAxes.y < -Y_AXIS_TILT_THRESHOLD)
			{
				if (rb.velocity.y < 0.0f && rb.velocity.y > -fastFallSpeed)
				{
					tempVec = rb.velocity;
					tempVec.y = -fastFallSpeed;
					rb.velocity = tempVec;
				}
			}
		}
	}

	// called by animation event at end of JumpSquat
	void AddJumpForce() {
		// performs full hop if player is still holding jump at end of jump squat animation
		if (Input.GetButton("Jump"))
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
		tempVec.x = moveAxes.x * airDriftSpeed;
		rb.velocity = tempVec;
		hasDoubleJump = false;
	}

	#endregion

	#region AnimTools

	void SetActionable(int i) {
		if (i == 0)
		{
			isActionable = false;
			anim.SetFloat("xAxis", 0.0f);
			anim.SetFloat("yAxis", 0.0f);
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
			if (newState == State.CROUCH)
			{
				isCrouch = true;
			}
			else
			{
				isCrouch = false;
			}

		}
		if (newState == State.ATTACK || newState == State.DASHATTACK || newState == State.SPECIAL || newState == State.DEFEND || newState == State.AERIAL || newState == State.LANDLAG || newState == State.TUMBLE || newState == State.GROUNDSTUN || newState == State.COMBO)
		{
			isActionable = false;
			/*
			if (newState != State.AERIAL || newState != State.TUMBLE)
			{
				anim.SetFloat("xAxis", 0.0f);
				anim.SetFloat("yAxis", 0.0f);
			}
			*/
			if (newState == State.DASHATTACK)
			{
				// change this to non force
				rb.AddForce(transform.forward * 20.0f, ForceMode.Impulse);
			}
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
		specialVector = new Vector3(specialAxes.x, specialAxes.y, 0.0f);
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
		groundCheck = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
		if (groundCheck)
		{
			rb.velocity = Vector3.zero;
			isGrounded = true;
			if (specialVector.y < -0.2f)
			{
				anim.SetTrigger("HardLandTrigger");
			}
			else
			{
				anim.SetTrigger("SoftLandTrigger");
			}
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
					anim.SetTrigger("AirIdleTrigger");
				}
				else
				{
					anim.SetTrigger("GroundStunEndTrigger");
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
		anim.SetTrigger("AirIdleTrigger");
	}

	#endregion
}
