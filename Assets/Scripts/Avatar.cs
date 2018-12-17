using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avatar : MonoBehaviour {

	public enum State {
		IDLE,
		RUNFWD,
		RUNBACK,
		CROUCH,
		AIRIDLE,
		ATTACK,
		DASHATTACK,
		AERIAL,
		TUMBLE,
		JUMPSQUAT,
		LANDLAG,
		SPECIAL,
		DEFEND
	}

	public enum Weapon {
		MELEE,
		RANGED
	}

	const float TORSO_ROTATION_OFFSET = 35.0f;
	const float SHOT_MOMENTUM_TRANSFER_RATIO = 0.5f;
	const float SHOTGUN_VELOCITY_DRIFT_LIMIT = 0.8f;

	KeyCode ATTACK = KeyCode.Mouse0;
	KeyCode DEFEND = KeyCode.Mouse1;
	KeyCode SPECIAL = KeyCode.LeftShift;
	KeyCode JUMP = KeyCode.Space;
	KeyCode RIGHT = KeyCode.D;
	KeyCode LEFT = KeyCode.A;
	KeyCode UP = KeyCode.W;
	KeyCode DOWN = KeyCode.S;

	GameObject[] tracers;
	Plane mousePlane;
	Camera mainCamera;
	Animator anim;
	Rigidbody rb;
	Ray ray;
	Vector3 mouseWorldPosition;
	Vector3 specialVector;
	Vector3 specialDestination;
	Vector3 tempVec;
	Vector3 oldVec;
	Vector3 lookDirection;
	Vector3 knockbackToApply;
	Quaternion startRotation;
	Quaternion lookRotation;

	float techTime;
	float stunTime;
	float freezeTime;
	float xAxisInput;
	float yAxisInput;
	float xAxisTiltThreshold = 0.0f;
	float yAxisTiltThreshold = 0.0f;

	bool groundCheck;
	bool isGrounded = true;
	bool isHitstun = false;
	bool isActionable = true;
	bool isCrouch = false;
	bool hasDoubleJump = true;
	bool specialMovement = false;
	bool specialStartup = false;
	bool freezeFrame = false;
	bool techPressed = false;

	[SerializeField] bool control = true;
	[SerializeField] State currentState;
	[SerializeField] Weapon currentWeapon;

	[Header("Stats")]
	[SerializeField] float weight;
	[SerializeField] int maxhealth;
	[SerializeField] int health;
	[SerializeField] int clipSize;
	[SerializeField] int ammo;

	[Header("Geometry")]
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
	[SerializeField] Material baseMaterial;
	[SerializeField] Material illusionMaterial;
	[SerializeField] SkinnedMeshRenderer modelMesh;
	[SerializeField] GameObject illusionPrefab;
	[SerializeField] float specialRange;
	[SerializeField] float specialSpeed;
	[SerializeField] float specialCooldown;

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

	void OnDrawGizmos() {
		Gizmos.DrawSphere(groundCheckPoint.position, groundCheckRadius);
		mainCamera = Camera.main;
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

	// Use this for initialization
	void Start() {
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
		mainCamera = Camera.main;
		mousePlane = new Plane(Vector3.back, transform.position);
		currentState = State.IDLE;
		currentWeapon = Weapon.MELEE;
		// bullets preloaded and reused for better runtime performance
		tracers = new GameObject[tracersPerShot * clipSize];
		for (int i = 0; i < tracers.Length; i++)
		{
			tracers[i] = Instantiate(tracerPrefab);
			tracers[i].GetComponent<Tracer>().SetOwner(this.gameObject);
		}
		ammo = clipSize;
		health = maxhealth;
		startRotation = hipRotationBone.transform.rotation;
	}
	
	// Update is called once per frame
	void Update() {
		if (control)
		{
			MovementInput();
			if (isActionable)
			{
				CheckMousePosition();
				ActionInput();
			}
		}
		if (!specialMovement)
		{
			ApplyPhysics();
			GroundCheck();
			if (currentState != State.TUMBLE)
			{
				Move();
			}
		}
		else // if special movement
		{
			//rb.velocity = specialVector * specialSpeed;
		}
		
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
	}

	void LateUpdate() {
		if (control)
			LookAtCursor();
	}

	#region Input

	void CheckMousePosition() {
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter = 0.0f;
		if (mousePlane.Raycast(ray, out enter))
		{
			mouseWorldPosition = ray.GetPoint(enter);
			mouseWorldPosition.z = 0.0f;
		}
		if (Input.mousePosition.x > (float)(Screen.width / 2))
		{
			if (transform.forward.x < 0.0f)
			{
				transform.eulerAngles = new Vector3(0, 90, 0);
				anim.SetFloat("lookDirection", transform.forward.x);
			}
		}
		else
		{
			if (transform.forward.x > 0.0f)
			{
				transform.eulerAngles = new Vector3(0, -90, 0);
				anim.SetFloat("lookDirection", transform.forward.x);
			}
		}
		tempVec = rotationPoint.position;
		tempVec.z = 0.0f;
		lookDirection = Vector3.Normalize(mouseWorldPosition - tempVec);
		lookRotation = Quaternion.LookRotation(lookDirection);
	}

	void BufferTechInput() {
		if (!techPressed && Input.GetKeyDown(DEFEND))
		{
			techTime = 0.5f;
		}
	}

	void MovementInput() {
		xAxisInput = Input.GetAxis("Horizontal");
		yAxisInput = Input.GetAxis("Vertical");
		if (Input.GetKeyDown(JUMP))
		{
			if (hasDoubleJump)
			{
				anim.SetTrigger("JumpTrigger");
			}
		}
		anim.SetFloat("xAxis", xAxisInput);
		anim.SetFloat("yAxis", yAxisInput);
	}

	void ActionInput() {
		if (Input.GetKeyDown(ATTACK))
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
					anim.SetTrigger("dashAttackTrigger");
					break;
				case (State.RUNBACK):
					anim.SetTrigger("fTiltTrigger");
					break;
				case (State.CROUCH):
					anim.SetTrigger("dTiltTrigger");
					break;
				case (State.IDLE):
					if (yAxisInput > yAxisTiltThreshold)
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
				if (yAxisInput > yAxisTiltThreshold)
				{
					anim.SetTrigger("upairTrigger");
				}
				else if (yAxisInput < -yAxisTiltThreshold)
				{
					anim.SetTrigger("dairTrigger");
				}
				else if (xAxisInput * transform.forward.x > xAxisTiltThreshold)
				{
					anim.SetTrigger("fairTrigger");
				}
				else
				{
					anim.SetTrigger("nairTrigger");
				}
			}
		}
		else if (Input.GetKeyDown(DEFEND))
		{
			if (isGrounded && currentWeapon == Weapon.MELEE)
				anim.SetTrigger("defendTrigger");
		}
		else if (Input.GetKeyDown(SPECIAL))
		{
			if (currentWeapon == Weapon.MELEE)
				anim.SetTrigger("specialTrigger");
		}
		else if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
		{
			ChangeWeapon();
		}
	}

	#endregion

	#region Physics

	void ApplyPhysics() {
		if (freezeFrame)
		{
			freezeTime -= Time.deltaTime;
			if (freezeTime <= 0.0f)
			{
				EndFreezeFrame();
			}
		}
		else
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
			if (Mathf.Abs(tempVec.x) < 0.1)
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
			if (!isGrounded && rb.velocity.y < 0)
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
		freezeTime = time;
		freezeFrame = true;
		anim.speed = 0.0f;
		rb.constraints = RigidbodyConstraints.FreezeAll;
	}

	void EndFreezeFrame() {
		freezeFrame = false;
		anim.speed = 1.0f;
		rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
		rb.velocity = knockbackToApply;
	}

	public void TakeHit(int damage, float freezeTime, float stunTime, Vector3 knockback) {
		health -= damage;
		ChangeState(State.TUMBLE);
		anim.SetTrigger("tumbleTrigger");
		StartFreezeFrame(freezeTime);
		knockbackToApply = knockback;
		this.stunTime = stunTime;
		techPressed = false;
		hipRotationBone.transform.rotation = Quaternion.LookRotation(-knockback, Vector3.up);
	}

	#endregion

	#region Movement

	void Move() {
		if (isGrounded && isActionable)
		{
			tempVec = rb.velocity;
			if (isCrouch)
			{
				tempVec.x = xAxisInput * crouchWalkSpeed;
			}
			else
			{
				if (transform.forward.x * xAxisInput > 0.0f)
				{
					if (currentWeapon == Weapon.MELEE)
					{
						tempVec.x = xAxisInput * runFwdSpeed;
						anim.SetFloat("runSpeed", 1.0f);
					}
					else
					{
						tempVec.x = xAxisInput * runFwdSpeed * rangedSpeedMulti;
						anim.SetFloat("runSpeed", rangedSpeedMulti);
					}

				}
				else
				{
					tempVec.x = xAxisInput * runBackSpeed;
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
				tempVec.x += xAxisInput * airDriftAccel * Time.deltaTime;
			}
			rb.velocity = tempVec;
			// fast fall
			if (yAxisInput < -yAxisTiltThreshold)
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

	// called by animation event
	void AddDoubleJumpForce() {
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
		if (newState == State.ATTACK || newState == State.DASHATTACK || newState == State.SPECIAL || newState == State.DEFEND || newState == State.AERIAL || newState == State.LANDLAG || newState == State.TUMBLE)
		{
			isActionable = false;
			if (newState != State.AERIAL || newState != State.TUMBLE)
			{
				anim.SetFloat("xAxis", 0.0f);
				anim.SetFloat("yAxis", 0.0f);
			}
			if (newState == State.DASHATTACK)
			{
				rb.AddForce(transform.forward * 20.0f, ForceMode.Impulse);
			}
		}
		else
		{
			isActionable = true;
		}
		currentState = newState;
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
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter = 0.0f;
		if (mousePlane.Raycast(ray, out enter))
		{
			mouseWorldPosition = ray.GetPoint(enter);
		}
		specialVector = mouseWorldPosition - hipRotationBone.transform.position;
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

	void FireWeapon() {
		if (ammo > 0)
		{
			ShotgunBlast();
		}
	}

	void ShotgunBlast() {
		float spread;
		float drift;
		muzzleFlash.SetActive(true);
		for (int i = 0; i < tracersPerShot; i++)
		{
			spread = Random.Range(-weaponSpread, weaponSpread);
			drift = Random.Range(SHOTGUN_VELOCITY_DRIFT_LIMIT, 1.0f);
			tracers[i].transform.position = muzzleFlash.transform.position;
			tracers[i].transform.rotation = lookRotation;
			tracers[i].transform.Rotate(spread, 0.0f, 0.0f);
			tracers[i].SetActive(true);
			tracers[i].GetComponent<Rigidbody>().velocity = (tracers[i].transform.forward * shotForce * drift) + (rb.velocity * SHOT_MOMENTUM_TRANSFER_RATIO);
		}
		ammo--;
	}

	void LookAtCursor() {
		if (currentWeapon == Weapon.RANGED)
		{
			spineRotationBone.transform.rotation = lookRotation;
			tempVec = spineTopRotationBone.transform.localEulerAngles;
			tempVec.y += TORSO_ROTATION_OFFSET;
			spineTopRotationBone.transform.localEulerAngles = tempVec;
			headRotationBone.transform.rotation = lookRotation;
		}
		else if (currentState == State.SPECIAL)
		{
			spineRotationBone.transform.rotation = lookRotation;
			//spineRotationBone.transform.LookAt(specialDestination);
		}
	}

	#endregion
}
