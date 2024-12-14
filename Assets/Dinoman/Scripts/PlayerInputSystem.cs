using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    public Transform Head, Spine0, Spine1, Spine2, Neck0, Neck1, Neck2, Tail2, Tail3, Tail4, Tail5, Tail6, Left_Hips, Right_Hips, Left_Leg, Right_Leg, Left_Foot0, Right_Foot0;
    public AudioClip Waterflush, Hit_jaw, Hit_head, Hit_tail, Bigstep, Largesplash, Largestep, Idlecarn, Bite, Swallow, Sniff1, Rex1, Rex2, Rex3, Rex4, Rex5;

    private Rigidbody rigidBody;
    private PlayerControls playerInputActions;
    private Animator anm;

    [HideInInspector] public Quaternion angTGT = Quaternion.identity, normAng = Quaternion.identity;
    [HideInInspector] public AnimatorStateInfo OnAnm;
    [HideInInspector] public float posY, waterY, withersSize, size, speed;
    [HideInInspector] public float currframe, lastframe, lastHit;
    [HideInInspector] public bool isActive, isVisible, isDead, isOnGround, isOnWater, isInWater, isConstrained, isOnLevitation;
    [HideInInspector] public bool onAttack, onJump, onCrouch, onReset, onInvert, onHeadMove, onAutoLook, onTailAttack;
    [HideInInspector] public float crouch, spineX, spineY, headX, headY, pitch, roll, reverse;
    [HideInInspector] public Vector3 headPos, posCOL = Vector3.zero, posTGT = Vector3.zero, lookTGT = Vector3.zero, boxscale = Vector3.zero, normal = Vector3.zero;
    [HideInInspector] public float behaviorCount, distTGT, delta, actionDist, angleAdd, avoidDelta, avoidAdd;
    [HideInInspector] public string behavior, specie;
    [HideInInspector] public int rndX, rndY, rndMove, rndIdle, loop;
	[HideInInspector] public GameObject objTGT=null, objCOL=null;
    [HideInInspector] public Manager main = null;

    public float baseMass = 1, ang_T = 0.025f, crouch_Max = 0, yaw_Max = 0, pitch_Max = 0;
    [Range(0.0f,2.0f)] public float animSpeed=1.0f;
    public bool herbivorous, canAttack, canHeadAttack, canTailAttack, canWalk, canJump, canFly, canSwim, lowAltitude, canInvertBody;
    public AudioSource[] source;

   
	//IK TYPES
	public enum IkType { None, Convex, Quad, Flying, SmBiped, LgBiped }
	// IK goal position
	Vector3 FR_HIT, FL_HIT, BR_HIT, BL_HIT;
	// Terrain normals
	Vector3 FR_Norm=Vector3.up, FL_Norm=Vector3.up, BR_Norm=Vector3.up, BL_Norm=Vector3.up;
	//Back Legs
	float BR1, BR2, BR3, BR_Add; //Right
	float BL1, BL2, BL3, BL_Add; //Left
	float alt1, alt2, a1, a2, b1, b2, c1, c2;
	//Front Legs
	float FR1, FR2, FR3, FR_Add; //Right
	float FL1, FL2, FL3, FL_Add; //Left
	float alt3, alt4, a3, a4, b3, b4, c3, c4;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        anm = GetComponent<Animator>();
        playerInputActions = new PlayerControls();
        playerInputActions.Enable();
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
    }

    private void FixedUpdate()
    {
        // TODO: Apply run func
        // bool run = Input.GetKey(KeyCode.LeftShift) ? true : false;
        bool run = false;

        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();

        if (inputVector.x != 0 || inputVector.y != 0)
        {

            if (inputVector.y > 0 && !run)
                anm.SetInteger("Move", 1); // Forward walk
            else if (inputVector.y > 0)
                anm.SetInteger("Move", 2); // Run
            else if (inputVector.y < 0)
                anm.SetInteger("Move", -1); // Backward


            else if (inputVector.x > 0)
                anm.SetInteger("Move", 10); // Strafe right
            else if (inputVector.x < 0)
                anm.SetInteger("Move", -10); // Strafe left

            // TODO: Right stick look movement
            // anm.SetFloat("Turn", transform.eulerAngles.y + Input.GetAxis("Mouse X") * 22.5f); // Mouse turn
        }

        //Stopped
        if (OnAnm.IsName("Rex|Idle1A") | OnAnm.IsName("Rex|Idle2A") | OnAnm.IsName("Rex|Die1") | OnAnm.IsName("Rex|Die2"))
        {
            Move(Vector3.zero);
            if (OnAnm.IsName("Rex|Die1")) { onReset = true; if (!isDead) { PlaySound("Atk", 2); PlaySound("Die", 12); } }
            else if (OnAnm.IsName("Rex|Die2")) { onReset = true; if (!isDead) { PlaySound("Atk", 2); PlaySound("Die", 10); } }
        }

        //End Forward
        else if (OnAnm.normalizedTime > 0.5 && (OnAnm.IsName("Rex|Step1+") | OnAnm.IsName("Rex|Step2+") |
                OnAnm.IsName("Rex|ToIdle1C") | OnAnm.IsName("Rex|ToIdle2B") | OnAnm.IsName("Rex|ToIdle2D") | OnAnm.IsName("Rex|ToEatA") |
                OnAnm.IsName("Rex|ToEatC") | OnAnm.IsName("Rex|StepAtk1") | OnAnm.IsName("Rex|StepAtk2")))
            PlaySound("Step", 9);

        //Forward
        else if (OnAnm.IsName("Rex|Walk") | OnAnm.IsName("Rex|WalkGrowl") | (OnAnm.normalizedTime < 0.5 &&
           (OnAnm.IsName("Rex|Step1+") | OnAnm.IsName("Rex|Step2+") | OnAnm.IsName("Rex|ToIdle2B") |
           OnAnm.IsName("Rex|ToIdle1C") | OnAnm.IsName("Rex|ToIdle2D") | OnAnm.IsName("Rex|ToEatA") | OnAnm.IsName("Rex|ToEatC"))))
        {
            Move(transform.forward, 50);
            if (OnAnm.IsName("Rex|WalkGrowl")) { PlaySound("Growl", 1); PlaySound("Step", 6); PlaySound("Step", 13); }
            else if (OnAnm.IsName("Rex|Walk")) { PlaySound("Step", 6); PlaySound("Step", 13); }
            else { PlaySound("Step", 8); PlaySound("Step", 12); }
        }

        //Run
        else if (OnAnm.IsName("Rex|Run") | OnAnm.IsName("Rex|RunGrowl") | OnAnm.IsName("Rex|WalkAtk1") | OnAnm.IsName("Rex|WalkAtk2") |
           (OnAnm.normalizedTime < 0.6 && (OnAnm.IsName("Rex|StepAtk1") | OnAnm.IsName("Rex|StepAtk2"))))
        {
            roll = Mathf.Clamp(Mathf.Lerp(roll, spineX * 5.0f, 0.05f), -20f, 20f);
            Move(transform.forward, 128);
            if (OnAnm.IsName("Rex|RunGrowl")) { PlaySound("Growl", 1); PlaySound("Step", 6); PlaySound("Step", 13); }
            else if (OnAnm.IsName("Rex|Run")) { PlaySound("Step", 6); PlaySound("Step", 13); }
            else if (OnAnm.IsName("Rex|StepAtk1") | OnAnm.IsName("Rex|StepAtk2")) { onAttack = true; PlaySound("Atk", 2); PlaySound("Bite", 5); }
            else { onAttack = true; PlaySound("Atk", 2); PlaySound("Step", 6); PlaySound("Bite", 9); PlaySound("Step", 13); }
        }

        //Backward
        else if ((OnAnm.normalizedTime > 0.4 && OnAnm.normalizedTime < 0.8) && (OnAnm.IsName("Rex|Step1-") | OnAnm.IsName("Rex|Step2-") | OnAnm.IsName("Rex|ToSleep2")))
        {
            Move(-transform.forward, 50);
            PlaySound("Step", 12);
        }

        //Strafe/Turn right
        else if (OnAnm.IsName("Rex|Strafe1-") | OnAnm.IsName("Rex|Strafe2+"))
        {
            Move(transform.right, 25);
            PlaySound("Step", 6); PlaySound("Step", 13);
        }

        //Strafe/Turn left
        else if (OnAnm.IsName("Rex|Strafe1+") | OnAnm.IsName("Rex|Strafe2-"))
        {
            Move(-transform.right, 25);
            PlaySound("Step", 6); PlaySound("Step", 13);
        }
    }

    void OnCollisionStay(Collision col)
    {
        int rndPainsnd = Random.Range(0, 3);
        AudioClip painSnd = null;
        switch (rndPainsnd) { case 0: painSnd = Rex2; break; case 1: painSnd = Rex3; break; case 2: painSnd = Rex4; break; }
    }
    void PlaySound(string name, int time)
    {
        if (time == currframe && lastframe != currframe)
        {
            switch (name)
            {
                case "Step":
                    source[1].pitch = Random.Range(0.75f, 1.25f);
                    if (isInWater) source[1].PlayOneShot(Waterflush, Random.Range(0.25f, 0.5f));
                    else if (isOnWater) source[1].PlayOneShot(Largesplash, Random.Range(0.25f, 0.5f));
                    else if (isOnGround) source[1].PlayOneShot(Bigstep, Random.Range(0.25f, 0.5f));
                    lastframe = currframe; break;
                case "Bite":
                    source[1].pitch = Random.Range(0.5f, 0.75f); source[1].PlayOneShot(Bite, 2.0f);
                    lastframe = currframe; break;
                case "Die":
                    source[1].pitch = Random.Range(1.0f, 1.25f); source[1].PlayOneShot(isOnWater | isInWater ? Largesplash : Largestep, 1.0f);
                    lastframe = currframe; isDead = true; break;
                case "Food":
                    source[0].pitch = Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Swallow, 0.5f);
                    lastframe = currframe; break;
                case "Sniff":
                    source[0].pitch = Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Sniff1, 0.5f);
                    lastframe = currframe; break;
                case "Repose":
                    source[0].pitch = Random.Range(0.75f, 1.25f); source[0].PlayOneShot(Idlecarn, 0.25f);
                    lastframe = currframe; break;
                case "Atk":
                    int rnd1 = Random.Range(0, 2); source[0].pitch = Random.Range(0.75f, 1.75f);
                    if (rnd1 == 0) source[0].PlayOneShot(Rex3, 0.5f);
                    else source[0].PlayOneShot(Rex4, 0.5f);
                    lastframe = currframe; break;
                case "Growl":
                    int rnd2 = Random.Range(0, 3); source[0].pitch = Random.Range(1.0f, 1.25f);
                    if (rnd2 == 0) source[0].PlayOneShot(Rex1, 1.0f);
                    else if (rnd2 == 1) source[0].PlayOneShot(Rex2, 1.0f);
                    else source[0].PlayOneShot(Rex5, 1.0f);
                    lastframe = currframe; break;
            }
        }
    }

    void LateUpdate()
    {
        if (!isActive) return; headPos = Head.GetChild(0).GetChild(0).position;

        Spine0.rotation *= Quaternion.AngleAxis(headX, Vector3.forward) * Quaternion.AngleAxis(-headY, Vector3.right);
        Spine2.rotation *= Quaternion.AngleAxis(headX, Vector3.forward) * Quaternion.AngleAxis(-headY, Vector3.right);
        Neck0.rotation *= Quaternion.AngleAxis(headX, Vector3.forward) * Quaternion.AngleAxis(-headY, Vector3.right);
        Neck1.rotation *= Quaternion.AngleAxis(headX, Vector3.forward) * Quaternion.AngleAxis(-headY, Vector3.right);
        Neck2.rotation *= Quaternion.AngleAxis(headX, Vector3.forward) * Quaternion.AngleAxis(-headY, Vector3.right);
        Head.rotation *= Quaternion.AngleAxis(headX, Vector3.forward) * Quaternion.AngleAxis(-headY, Vector3.right);
        Tail2.rotation *= Quaternion.AngleAxis(-spineX, Vector3.forward);
        Tail3.rotation *= Quaternion.AngleAxis(-spineX, Vector3.forward);
        Tail4.rotation *= Quaternion.AngleAxis(-spineX, Vector3.forward);
        Tail5.rotation *= Quaternion.AngleAxis(-spineX, Vector3.forward);
        Tail6.rotation *= Quaternion.AngleAxis(-spineX, Vector3.forward);

        Right_Hips.rotation *= Quaternion.Euler(-roll, 0, 0);
        Left_Hips.rotation *= Quaternion.Euler(-roll, 0, 0);
        if (!isDead) Head.GetChild(0).transform.rotation *= Quaternion.Euler(lastHit, 0, 0);
        //Check for ground layer
        // TODO: Apply Ground Pos
        GetGroundPos(IkType.LgBiped, Right_Hips, Right_Leg, Right_Foot0, Left_Hips, Left_Leg, Left_Foot0);
    }

    	#region PHYSICAL FORCES
	public void ApplyGravity(float multiplier=1.0f)
	{
		rigidBody.AddForce((Vector3.up*size)*(rigidBody.velocity.y>0 ? -20*rigidBody.drag : -50*rigidBody.drag)*multiplier,ForceMode.Acceleration);
	}
	public void ApplyYPos()
	{
		if(isOnGround&&(Mathf.Abs(normal.x)>main.MaxSlope|Mathf.Abs(normal.z)>main.MaxSlope))
		{ rigidBody.AddForce(new Vector3(normal.x,-normal.y,normal.z)*64,ForceMode.Acceleration); behaviorCount=0; }
		rigidBody.AddForce(Vector3.up*Mathf.Clamp(posY-transform.position.y,-size,size),ForceMode.VelocityChange);
	}
	public void Move(Vector3 dir,float force=0,bool jump=false)
	{
		if(canAttack&&anm.GetBool("Attack").Equals(true))
		{
			force*=1.5f; transform.rotation=Quaternion.Lerp(transform.rotation,normAng,ang_T*2);
		}
		else transform.rotation=Quaternion.Lerp(transform.rotation,normAng,ang_T);

		if(dir!=Vector3.zero)
		{
			if(!canSwim&&!isOnGround)
			{
				if(isInWater|isOnWater) force/=8;
				else if(!canFly&&!onJump) force/=8;
				else force/=(4/rigidBody.drag);
			}
			else force/=(4/rigidBody.drag);

			rigidBody.AddForce(dir*force*speed,jump ? ForceMode.VelocityChange : ForceMode.Acceleration);
		}
	}
	#endregion
	#region LERP SKELETON ROTATION
	public void RotateBone(IkType ikType,float maxX,float maxY=0,bool CanMoveHead=true,float t=0.5f)
	{
		//Freeze all
		if(animSpeed==0.0f) return;

		//Slowdown on turning
		if(!onAttack&&!onJump)
		{ speed=size*anm.speed*(1.0f-Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y,anm.GetFloat("Turn")))/135f); }

		//Lerp feet position
		if(main.useIK&&ikType!=IkType.None)
		{
			float s;
			switch(ikType)
			{
				case IkType.Convex:
				s=0.1f;
				if(!isConstrained&&!isDead&&isOnGround&&!isInWater)
				{
					FR1=Mathf.Lerp(FR1,Mathf.Clamp(-alt1,-55,0),s); FR2=Mathf.Lerp(FR2,b1,s); FR3=Mathf.Lerp(FR3,c1,s);
					FL1=Mathf.Lerp(FL1,Mathf.Clamp(-alt2,-55,0),s); FL2=Mathf.Lerp(FL2,b2,s); FL3=Mathf.Lerp(FL3,c2,s);
					BR1=Mathf.Lerp(BR1,Mathf.Clamp(-alt3,-55,0),s); BR2=Mathf.Lerp(BR2,b3,s); BR3=Mathf.Lerp(BR3,c3,s);
					BL1=Mathf.Lerp(BL1,Mathf.Clamp(-alt4,-55,0),s); BL2=Mathf.Lerp(BL2,b4,s); BL3=Mathf.Lerp(BL3,c4,s);
				}
				else
				{
					FR_Add=Mathf.Lerp(FR_Add,0,s); FR1=Mathf.Lerp(FR1,0,s); FR2=Mathf.Lerp(FR2,0,s); FR3=Mathf.Lerp(FR3,0,s);
					FL_Add=Mathf.Lerp(FL_Add,0,s); FL1=Mathf.Lerp(FL1,0,s); FL2=Mathf.Lerp(FL2,0,s); FL3=Mathf.Lerp(FL3,0,s);
					BR_Add=Mathf.Lerp(BR_Add,0,s); BR1=Mathf.Lerp(BR1,0,s); BR2=Mathf.Lerp(BR2,0,s); BR3=Mathf.Lerp(BR3,0,s);
					BL_Add=Mathf.Lerp(BL_Add,0,s); BL1=Mathf.Lerp(BL1,0,s); BL2=Mathf.Lerp(BL2,0,s); BL3=Mathf.Lerp(BL3,0,s);
				}
				break;
				case IkType.Quad:
				s=0.1f;
				if(!isConstrained&&!isDead&&isOnGround)
				{

					FR1=Mathf.Lerp(FR1,Mathf.Clamp(-alt1,-50,0),s); FR2=Mathf.Lerp(FR2,b1,s); FR3=Mathf.Lerp(FR3,c1,s);
					FL1=Mathf.Lerp(FL1,Mathf.Clamp(-alt2,-50,0),s); FL2=Mathf.Lerp(FL2,b2,s); FL3=Mathf.Lerp(FL3,c2,s);
					BR1=Mathf.Lerp(BR1,Mathf.Clamp(-alt3,-50,0),s); BR2=Mathf.Lerp(BR2,b3,s); BR3=Mathf.Lerp(BR3,c3,s);
					BL1=Mathf.Lerp(BL1,Mathf.Clamp(-alt4,-50,0),s); BL2=Mathf.Lerp(BL2,b4,s); BL3=Mathf.Lerp(BL3,c4,s);
				}
				else
				{
					FR_Add=Mathf.Lerp(FR_Add,0,s); FR1=Mathf.Lerp(FR1,0,s); FR2=Mathf.Lerp(FR2,0,s); FR3=Mathf.Lerp(FR3,0,s);
					FL_Add=Mathf.Lerp(FL_Add,0,s); FL1=Mathf.Lerp(FL1,0,s); FL2=Mathf.Lerp(FL2,0,s); FL3=Mathf.Lerp(FL3,0,s);
					BR_Add=Mathf.Lerp(BR_Add,0,s); BR1=Mathf.Lerp(BR1,0,s); BR2=Mathf.Lerp(BR2,0,s); BR3=Mathf.Lerp(BR3,0,s);
					BL_Add=Mathf.Lerp(BL_Add,0,s); BL1=Mathf.Lerp(BL1,0,s); BL2=Mathf.Lerp(BL2,0,s); BL3=Mathf.Lerp(BL3,0,s);
				}
				break;
				case IkType.Flying:
				s=0.25f;
				if(!isConstrained&&!isDead&&isOnGround&&!isOnLevitation)
				{
					FR1=Mathf.Lerp(FR1,Mathf.Clamp(-alt1,-100,0),s); FR2=Mathf.Lerp(FR2,b1,s); FR3=Mathf.Lerp(FR3,c1,s);
					FL1=Mathf.Lerp(FL1,Mathf.Clamp(-alt2,-100,0),s); FL2=Mathf.Lerp(FL2,b2,s); FL3=Mathf.Lerp(FL3,c2,s);
					BR1=Mathf.Lerp(BR1,Mathf.Clamp(-alt3,-60,0),s); BR2=Mathf.Lerp(BR2,b3,s); BR3=Mathf.Lerp(BR3,c3,s);
					BL1=Mathf.Lerp(BL1,Mathf.Clamp(-alt4,-60,0),s); BL2=Mathf.Lerp(BL2,b4,s); BL3=Mathf.Lerp(BL3,c4,s);
				}
				else
				{
					FR_Add=Mathf.Lerp(FR_Add,0,s); FR1=Mathf.Lerp(FR1,0,s); FR2=Mathf.Lerp(FR2,0,s); FR3=Mathf.Lerp(FR3,0,s);
					FL_Add=Mathf.Lerp(FL_Add,0,s); FL1=Mathf.Lerp(FL1,0,s); FL2=Mathf.Lerp(FL2,0,s); FL3=Mathf.Lerp(FL3,0,s);
					BR_Add=Mathf.Lerp(BR_Add,0,s); BR1=Mathf.Lerp(BR1,0,s); BR2=Mathf.Lerp(BR2,0,s); BR3=Mathf.Lerp(BR3,0,s);
					BL_Add=Mathf.Lerp(BL_Add,0,s); BL1=Mathf.Lerp(BL1,0,s); BL2=Mathf.Lerp(BL2,0,s); BL3=Mathf.Lerp(BL3,0,s);
				}
				break;
				case IkType.SmBiped:
				s=0.25f;
				if(!isConstrained&&!isDead&&isOnGround)
				{
					BR1=Mathf.Lerp(BR1,Mathf.Clamp(-alt1,-60,0),s); BR2=Mathf.Lerp(BR2,b1,s); BR3=Mathf.Lerp(BR3,c1,s);
					BL1=Mathf.Lerp(BL1,Mathf.Clamp(-alt2,-60,0),s); BL2=Mathf.Lerp(BL2,b2,s); BL3=Mathf.Lerp(BL3,c2,s);
				}
				else
				{
					BR_Add=Mathf.Lerp(BR_Add,0,s); BR1=Mathf.Lerp(BR1,0,s); BR2=Mathf.Lerp(BR2,0,s); BR3=Mathf.Lerp(BR3,0,s);
					BL_Add=Mathf.Lerp(BL_Add,0,s); BL1=Mathf.Lerp(BL1,0,s); BL2=Mathf.Lerp(BL2,0,s); BL3=Mathf.Lerp(BL3,0,s);
				}
				break;
				case IkType.LgBiped:
				s=0.25f;
				if(!isDead&&isOnGround)
				{
					BR1=Mathf.Lerp(BR1,Mathf.Clamp(-alt1,-55,0),s); BR2=Mathf.Lerp(BR2,b1,s); BR3=Mathf.Lerp(BR3,c1,s);
					BL1=Mathf.Lerp(BL1,Mathf.Clamp(-alt2,-55,0),s); BL2=Mathf.Lerp(BL2,b2,s); BL3=Mathf.Lerp(BL3,c2,s);
				}
				else
				{
					BR_Add=Mathf.Lerp(BR_Add,0,s); BR1=Mathf.Lerp(BR1,0,s); BR2=Mathf.Lerp(BR2,0,s); BR3=Mathf.Lerp(BR3,0,s);
					BL_Add=Mathf.Lerp(BL_Add,0,s); BL1=Mathf.Lerp(BL1,0,s); BL2=Mathf.Lerp(BL2,0,s); BL3=Mathf.Lerp(BL3,0,s);
				}
				break;
			}
		}

		//Take damages animation
		if(lastHit!=0) { if(!isDead&&canWalk) crouch=Mathf.Lerp(crouch,(crouch_Max*size)/2,1.0f); lastHit--; }

		//Reset skeleton rotations
		if(onReset)
		{
			pitch=Mathf.Lerp(pitch,0.0f,t/10f);
			roll=Mathf.Lerp(roll,0.0f,t/10f);
			headX=Mathf.LerpAngle(headX,0.0f,t/10f);
			headY=Mathf.LerpAngle(headY,0.0f,t/10f);
			crouch=Mathf.Lerp(crouch,0.0f,t/10f);
			spineX=Mathf.LerpAngle(spineX,0.0f,t/10f);
			spineY=Mathf.LerpAngle(spineY,0.0f,t/10f);
			return;
		}

		//Smooth avoiding angle
		if(avoidDelta!=0)
		{
			if(Mathf.Abs(avoidAdd)>90) avoidDelta=0;
			avoidAdd=Mathf.MoveTowardsAngle(avoidAdd,avoidDelta>0.0f ? 135f : -135f,t);
		}
		else avoidAdd=Mathf.MoveTowardsAngle(avoidAdd,0.0f,t);

		//Setup Look target position
		if(objTGT)
		{
			if(behavior.EndsWith("Hunt")|behavior.Equals("Battle")|behavior.EndsWith("Contest")) lookTGT=objTGT.transform.position;
			else if(herbivorous&&behavior.Equals("Food")) lookTGT=posTGT;
			else if(loop==0) lookTGT=Vector3.zero;
		}
		else if(loop==0) lookTGT=Vector3.zero;

		//Lerp all skeleton parts
		if(CanMoveHead)
		{
			if(!onTailAttack&&!anm.GetInteger("Move").Equals(0))
			{
				spineX=Mathf.MoveTowardsAngle(spineX,(Mathf.DeltaAngle(anm.GetFloat("Turn"),transform.eulerAngles.y)/360f)*maxX,t);
				spineY=Mathf.LerpAngle(spineY,0.0f,t/10f);
			}
			else
			{
				spineX=Mathf.MoveTowardsAngle(spineX,0.0f,t/10f);
				spineY=Mathf.LerpAngle(spineY,0.0f,t/10f);
			}

			if((!canFly&&!canSwim&&anm.GetInteger("Move")!=2)|!isOnGround) roll=Mathf.Lerp(roll,0.0f,ang_T);
			crouch=Mathf.Lerp(crouch,0.0f,t/10f);

			if(onHeadMove) return;

			if(lookTGT!=Vector3.zero&&(lookTGT-transform.position).magnitude>boxscale.z)
			{
				Quaternion dir;
				if(objTGT&&objTGT.tag.Equals("Creature")) dir=Quaternion.LookRotation(objTGT.GetComponent<Rigidbody>().worldCenterOfMass-headPos);
				else dir=Quaternion.LookRotation(lookTGT-headPos);

				headX=Mathf.MoveTowardsAngle(headX,(Mathf.DeltaAngle(dir.eulerAngles.y,transform.eulerAngles.y)/(180f-yaw_Max))*yaw_Max,t);
				headY=Mathf.MoveTowardsAngle(headY,(Mathf.DeltaAngle(dir.eulerAngles.x,transform.eulerAngles.x)/(90f-pitch_Max))*pitch_Max,t);
			}
			else
			{
				if(Mathf.RoundToInt(anm.GetFloat("Turn"))==Mathf.RoundToInt(transform.eulerAngles.y))
				{
					if(loop==0&&Mathf.RoundToInt(headX*100)==Mathf.RoundToInt(rndX*100)&&Mathf.RoundToInt(headY*100)==Mathf.RoundToInt(rndY*100))
					{
						rndX=Random.Range((int)-yaw_Max/2,(int)yaw_Max/2);
						rndY=Random.Range((int)-pitch_Max/2,(int)pitch_Max/2);
					}
					headX=Mathf.LerpAngle(headX,rndX,t/10f);
					headY=Mathf.LerpAngle(headY,rndY,t/10f);
				}
				else
				{
					headX=Mathf.LerpAngle(headX,spineX,t/10f);
					headY=Mathf.LerpAngle(headY,0.0f,t/10f);
				}
			}
		}
		else
		{
			spineX=Mathf.LerpAngle(spineX,(Mathf.DeltaAngle(anm.GetFloat("Turn"),transform.eulerAngles.y)/360f)*maxX,ang_T);
			if(isOnGround&&!isInWater) { spineY=Mathf.LerpAngle(spineY,0.0f,t/10f); roll=Mathf.LerpAngle(roll,0.0f,t/10f); pitch=Mathf.Lerp(pitch,0.0f,t/10f); }
			else if(canFly)
			{
				if(anm.GetInteger("Move")>=2&&anm.GetInteger("Move")<3)
					spineY=Mathf.LerpAngle(spineY,(Mathf.DeltaAngle(anm.GetFloat("Pitch")*90f,pitch)/180f)*maxY,ang_T);
				roll=Mathf.LerpAngle(roll,-spineX,t/10f);
			}
			else { spineY=Mathf.LerpAngle(spineY,(Mathf.DeltaAngle(anm.GetFloat("Pitch")*90f,pitch)/180f)*maxY,ang_T); roll=Mathf.LerpAngle(roll,-spineX,t/10f); }
			headX=Mathf.LerpAngle(headX,spineX,t);
			headY=Mathf.LerpAngle(headY,spineY,t);
		}

	}
	#endregion

    public void GetGroundPos(IkType ikType, Transform RLeg1 = null, Transform RLeg2 = null, Transform RLeg3 = null, Transform LLeg1 = null, Transform LLeg2 = null, Transform LLeg3 = null,
                                         Transform RArm1 = null, Transform RArm2 = null, Transform RArm3 = null, Transform LArm1 = null, Transform LArm2 = null, Transform LArm3 = null, float FeetOffset = 0.0f)
    {
        posY = -transform.position.y;

        //* FOR BIPED CREATURE
        if (ikType == IkType.None | isDead | isInWater | !isOnGround)
        {
            if (Physics.Raycast(transform.position + Vector3.up * withersSize, -Vector3.up, out RaycastHit hit, withersSize * 1.5f, 1 << 0))
            { posY = hit.point.y; normal = hit.normal; isOnGround = true; }
            else isOnGround = false;
        }
        else
        {
            if (Physics.Raycast((transform.position + transform.forward * 2) + Vector3.up, -Vector3.up, out RaycastHit hit, withersSize * 2.0f, 1 << 0))
            { posY = hit.point.y; normal = hit.normal; }
            if (Physics.Raycast(RLeg3.position + Vector3.up * withersSize, -Vector3.up, out RaycastHit BR, withersSize * 2.0f, 1 << 0))
            { isOnGround = true; BR_HIT = BR.point; BR_Norm = BR.normal; }
            else BR_HIT.y = -transform.position.y;
            if (Physics.Raycast(LLeg3.position + Vector3.up * withersSize, -Vector3.up, out RaycastHit BL, withersSize * 2.0f, 1 << 0))
            { isOnGround = true; BL_HIT = BL.point; BL_Norm = BL.normal; }
            else BL_HIT.y = -transform.position.y;

            if (posY > BL_HIT.y && posY > BR_HIT.y) posY = Mathf.Max(BL_HIT.y, BR_HIT.y); else posY = Mathf.Min(BL_HIT.y, BR_HIT.y);
            normal = (BL_Norm + BR_Norm + normal) / 3;
        }

        if (ikType == IkType.None | isDead | isInWater | !isOnGround)
        {
            float x = ((transform.position.x - main.t.transform.position.x) / main.t.terrainData.size.x) * main.tres;
            float y = ((transform.position.z - main.t.transform.position.z) / main.t.terrainData.size.z) * main.tres;
            normal = main.t.terrainData.GetInterpolatedNormal(x / main.tres, y / main.tres);
            posY = main.t.SampleHeight(transform.position) + main.t.GetPosition().y;
        }
        else if (ikType >= IkType.SmBiped) // Biped
        {
            BR_HIT = new Vector3(RLeg3.position.x, main.t.SampleHeight(RLeg3.position) + main.tpos.y, RLeg3.position.z);
            float x = ((RLeg3.position.x - main.tpos.x) / main.tdata.size.x) * main.tres, y = ((RLeg3.position.z - main.tpos.z) / main.tdata.size.z) * main.tres;
            BR_Norm = main.tdata.GetInterpolatedNormal(x / main.tres, y / main.tres);
            BL_HIT = new Vector3(LLeg3.position.x, main.t.SampleHeight(LLeg3.position) + main.tpos.y, LLeg3.position.z);
            x = ((LLeg3.position.x - main.tpos.x) / main.tdata.size.x) * main.tres; y = ((LLeg3.position.z - main.tpos.z) / main.tdata.size.z) * main.tres;
            BL_Norm = main.tdata.GetInterpolatedNormal(x / main.tres, y / main.tres);

            if (posY > BL_HIT.y && posY > BR_HIT.y) posY = Mathf.Max(BL_HIT.y, BR_HIT.y); else posY = Mathf.Min(BL_HIT.y, BR_HIT.y);
            normal = (BL_Norm + BR_Norm + normal) / 3;
        }

        #region Set status
		//Set status
		if((transform.position.y-size)<=posY) isOnGround=true; else isOnGround=false; //On ground?
		waterY=main.waterAlt-crouch; //Check for water altitude
		if((transform.position.y)<waterY&&rigidBody.worldCenterOfMass.y>waterY) isOnWater=true; else isOnWater=false; //On water ?
		if(rigidBody.worldCenterOfMass.y<waterY) isInWater=true; else isInWater=false; // In water ?

		//Setup Rigidbody
		if(isDead)
		{
			rigidBody.maxDepenetrationVelocity=0.25f;
			rigidBody.constraints=RigidbodyConstraints.None;
		}
		else if(isConstrained)
		{
			rigidBody.maxDepenetrationVelocity=0.0f; crouch=0.0f;
			rigidBody.constraints=RigidbodyConstraints.FreezeRotation|RigidbodyConstraints.FreezePositionX|RigidbodyConstraints.FreezePositionZ;
		}
		else
		{
			rigidBody.maxDepenetrationVelocity=5.0f;
			if(lastHit==0) rigidBody.constraints=RigidbodyConstraints.FreezeRotationZ;
			else rigidBody.constraints=RigidbodyConstraints.None;
		}

		//Setup Y position and rotation
		if(isOnGround&&!isInWater) //On Ground outside water
		{
			Quaternion n=Quaternion.LookRotation(Vector3.Cross(transform.right,normal),normal);
			if(!canFly)
			{
				float rx=Mathf.DeltaAngle(n.eulerAngles.x,0.0f), rz=Mathf.DeltaAngle(n.eulerAngles.z,0.0f);
				float pitch=Mathf.Clamp(rx,-45f,45f), roll=Mathf.Clamp(rz,-10f,10f);
				normAng=Quaternion.Euler(-pitch,anm.GetFloat("Turn"),-roll);
			}
			else normAng=Quaternion.Euler(n.eulerAngles.x,anm.GetFloat("Turn"),n.eulerAngles.z); posY-=crouch;
		}
		else if(isInWater|isOnWater) //On Water or In water
		{ normAng=Quaternion.Euler(0,anm.GetFloat("Turn"),0); posY=waterY-rigidBody.centerOfMass.y; }
		else //In Air
		{ normAng=Quaternion.Euler(0,anm.GetFloat("Turn"),0); posY=-transform.position.y; }

		if(!isVisible|!main.useIK) return;
		switch(ikType)
		{
			case IkType.None: break;
			case IkType.Convex: Convex(RLeg1,RLeg2,RLeg3,LLeg1,LLeg2,LLeg3,RArm1,RArm2,RArm3,LArm1,LArm2,LArm3); break;
			case IkType.Quad: Quad(RLeg1,RLeg2,RLeg3,LLeg1,LLeg2,LLeg3,RArm1,RArm2,RArm3,LArm1,LArm2,LArm3,FeetOffset); break;
			case IkType.Flying: Flying(RLeg1,RLeg2,RLeg3,LLeg1,LLeg2,LLeg3,RArm1,RArm2,RArm3,LArm1,LArm2,LArm3); break;
			case IkType.SmBiped: SmBiped(RLeg1,RLeg2,RLeg3,LLeg1,LLeg2,LLeg3); break;
			case IkType.LgBiped: LgBiped(RLeg1,RLeg2,RLeg3,LLeg1,LLeg2,LLeg3); break;
		}
		#endregion
    }

    //* WTF IS THISS??
    #region FEET INVERSE KINEMATICS
	//QUADRUPED
	void Quad(Transform RLeg1,Transform RLeg2,Transform RLeg3,Transform LLeg1,Transform LLeg2,Transform LLeg3,
						Transform RArm1,Transform RArm2,Transform RArm3,Transform LArm1,Transform LArm2,Transform LArm3,float FeetOffset)
	{
		//Right arm
		float offset=(RArm3.position-RArm3.GetChild(0).GetChild(0).position).magnitude+FeetOffset;
		Vector3 va1=RArm3.position-transform.up*offset;

		RArm1.rotation*=Quaternion.Euler(0,-FR1+(FR1+FR_Add),0);
		a1=Vector3.Angle(RArm1.position-RArm2.position,RArm1.position-RArm3.position);
		RArm2.rotation*=Quaternion.Euler(0,(FR1*2f)-FR_Add,0);
		b1=Vector3.Angle(FR_Norm,RArm3.right)-100f;
		c1=Vector3.Angle(-FR_Norm,RArm3.up)-90;
		RArm3.rotation*=Quaternion.Euler(FR3,FR2,0);

		Vector3 va3=FR_HIT+(FR_HIT-RArm3.position)+transform.up*offset;
		Vector3 va2=new Vector3(va1.x,va1.y-(va1.y-RArm1.position.y)-(va1.y-FR_HIT.y),va1.z);
		alt1=((va1-va2).magnitude-(va3-va2).magnitude)*(100/(va1-va2).magnitude);
		//Left arm
		offset=(LArm3.position-LArm3.GetChild(0).GetChild(0).position).magnitude+FeetOffset;
		Vector3 vb1=LArm3.position-transform.up*offset;

		LArm1.rotation*=Quaternion.Euler(-FL1+(FL1+FL_Add),0,0);
		a2=Vector3.Angle(LArm1.position-LArm2.position,LArm1.position-LArm3.position);
		LArm2.rotation*=Quaternion.Euler((FL1*2f)-FL_Add,0,0);
		b2=Vector3.Angle(FL_Norm,LArm3.right)-90f;
		c2=Vector3.Angle(-FL_Norm,LArm3.up)-100f;
		LArm3.rotation*=Quaternion.Euler(FL3,FL2,0);

		Vector3 vb3=FL_HIT+(FL_HIT-LArm3.position)+transform.up*offset;
		Vector3 vb2=new Vector3(vb1.x,vb1.y-(vb1.y-LArm1.position.y)-(vb1.y-FL_HIT.y),vb1.z);
		alt2=((vb1-vb2).magnitude-(vb3-vb2).magnitude)*(100/(vb1-vb2).magnitude);
		//Right leg
		offset=(RLeg3.position-RLeg3.GetChild(0).GetChild(0).position).magnitude+FeetOffset;
		Vector3 vc1=RLeg3.position-transform.up*offset;

		RLeg1.rotation*=Quaternion.Euler(0,BR1-(BR1+BR_Add),0);
		a3=Vector3.Angle(RLeg1.position-RLeg2.position,RLeg1.position-RLeg3.position);
		RLeg2.rotation*=Quaternion.Euler(0,(-BR1*2f)+BR_Add,0);
		b3=Vector3.Angle(BR_Norm,RLeg3.right)-90f;
		c3=Vector3.Angle(-BR_Norm,RLeg3.up)-90f;
		RLeg3.rotation*=Quaternion.Euler(BR3,BR2,0);

		Vector3 vc3=BR_HIT+(BR_HIT-RLeg3.position)+transform.up*offset;
		Vector3 vc2=new Vector3(vc1.x,vc1.y-(vc1.y-RLeg1.position.y)-(vc1.y-BR_HIT.y),vc1.z);
		alt3=((vc1-vc2).magnitude-(vc3-vc2).magnitude)*(100/(vc1-vc2).magnitude);
		//Left leg
		offset=(LLeg3.position-LLeg3.GetChild(0).GetChild(0).position).magnitude+FeetOffset;
		Vector3 vd1=LLeg3.position-transform.up*offset;

		LLeg1.rotation*=Quaternion.Euler(0,BL1-(BL1+BL_Add),0);
		a4=Vector3.Angle(LLeg1.position-LLeg2.position,LLeg1.position-LLeg3.position);
		LLeg2.rotation*=Quaternion.Euler(0,(-BL1*2f)+BL_Add,0);
		b4=Vector3.Angle(BL_Norm,LLeg3.right)-90f;
		c4=Vector3.Angle(-BL_Norm,LLeg3.up)-90f;
		LLeg3.rotation*=Quaternion.Euler(BL3,BL2,0);

		Vector3 vd3=BL_HIT+(BL_HIT-LLeg3.position)+transform.up*offset;
		Vector3 vd2=new Vector3(vd1.x,vd1.y-(vd1.y-LLeg1.position.y)-(vd1.y-BL_HIT.y),vd1.z);
		alt4=((vd1-vd2).magnitude-(vd3-vd2).magnitude)*(100/(vd1-vd2).magnitude);

		//Add rotations
		if(!isConstrained&&!isDead&&isOnGround)
		{
			FR_Add=Vector3.Angle(RArm1.position-RArm2.position,RArm1.position-RArm3.position)-a1;
			FL_Add=Vector3.Angle(LArm1.position-LArm2.position,LArm1.position-LArm3.position)-a2;
			BR_Add=Vector3.Angle(RLeg1.position-RLeg2.position,RLeg1.position-RLeg3.position)-a3;
			BL_Add=Vector3.Angle(LLeg1.position-LLeg2.position,LLeg1.position-LLeg3.position)-a4;
		}
	}

	//SMALL BIPED
	void SmBiped(Transform RLeg1,Transform RLeg2,Transform RLeg3,Transform LLeg1,Transform LLeg2,Transform LLeg3)
	{
		Transform RLeg4=RLeg3.GetChild(0);
		//Right leg
		float offset1=(RLeg4.position-RLeg4.GetChild(0).position).magnitude;
		Vector3 va1=RLeg4.position-transform.up*offset1;
		float inv1=Mathf.Clamp(Vector3.Cross(RLeg4.position-transform.position,RLeg1.position-transform.position).y,-1.0f,1.0f);

		RLeg1.rotation*=Quaternion.Euler(0,BR1-(BR1+BR_Add),0);
		a1=Vector3.Angle(RLeg1.position-RLeg2.position,RLeg1.position-RLeg3.position);
		RLeg2.rotation*=Quaternion.Euler(0,-BR1*2f,0);
		RLeg3.rotation*=Quaternion.Euler(0,BR1-BR_Add*inv1,0);
		b1=Vector3.Angle(-BR_Norm,RLeg4.GetChild(0).right)-90f;
		c1=Vector3.Angle(-BR_Norm,RLeg4.up)-90f;
		RLeg4.rotation*=Quaternion.Euler(BR3,0,0);
		RLeg4.GetChild(0).rotation*=Quaternion.Euler(0,-BR2,0);

		Vector3 va3=BR_HIT+(BR_HIT-RLeg4.GetChild(0).position)+transform.up*offset1;
		Vector3 va2=(va1+transform.up*(va1-RLeg1.position).magnitude);
		alt1=((va1-va2).magnitude-(va3-va2).magnitude)*(100/(va1-va2).magnitude);

		Transform LLeg4=LLeg3.GetChild(0);
		//Left Leg
		float offset2=(LLeg4.position-LLeg4.GetChild(0).position).magnitude;
		Vector3 vb1=LLeg4.position-transform.up*offset2;
		float inv2=Mathf.Clamp(Vector3.Cross(LLeg4.position-transform.position,LLeg1.position-transform.position).y,-1.0f,1.0f);

		LLeg1.rotation*=Quaternion.Euler(BL1-(BL1+BL_Add),0,0);
		a2=Vector3.Angle(LLeg1.position-LLeg2.position,LLeg1.position-LLeg3.position);
		LLeg2.rotation*=Quaternion.Euler(-BL1*2f,0,0);
		LLeg3.rotation*=Quaternion.Euler(BL1+BL_Add*inv2,0,0);

		b2=Vector3.Angle(-BL_Norm,-LLeg4.GetChild(0).up)-90f;
		c2=Vector3.Angle(-BL_Norm,LLeg4.up)-90f;
		LLeg4.rotation*=Quaternion.Euler(BL3,0,0);
		LLeg4.GetChild(0).rotation*=Quaternion.Euler(0,0,BL2);


		Vector3 vb3=BL_HIT+(BL_HIT-LLeg4.GetChild(0).position)+transform.up*offset2;
		Vector3 vb2=(vb1+transform.up*(vb1-LLeg1.position).magnitude);
		alt2=((vb1-vb2).magnitude-(vb3-vb2).magnitude)*(100/(vb1-vb2).magnitude);

		//Add rotations
		if(!isConstrained&&!isDead&&isOnGround)
		{
			BR_Add=Vector3.Angle(RLeg1.position-RLeg2.position,RLeg1.position-RLeg3.position)-a1;
			BL_Add=Vector3.Angle(LLeg1.position-LLeg2.position,LLeg1.position-LLeg3.position)-a2;
		}


	}

	//LARGE BIPED
	public void LgBiped(Transform RLeg1,Transform RLeg2,Transform RLeg3,Transform LLeg1,Transform LLeg2,Transform LLeg3)
	{
		//Right leg
		Transform RLeg4=RLeg3.GetChild(0);
		float offset1=(RLeg4.position-RLeg4.GetChild(1).position).magnitude;
		Vector3 va1=RLeg4.position-transform.up*offset1;
		float inv1=Mathf.Clamp(Vector3.Cross(RLeg4.position-transform.position,RLeg1.position-transform.position).y,-1.0f,1.0f);

		RLeg1.rotation*=Quaternion.Euler(0,BR1-(BR1+BR_Add),0);
		a1=Vector3.Angle(RLeg1.position-RLeg2.position,RLeg1.position-RLeg3.position);
		RLeg2.rotation*=Quaternion.Euler(0,-BR1*2f,0);
		RLeg3.rotation*=Quaternion.Euler(0,BR1-BR_Add*inv1,0);
		b1=Vector3.Angle(-BR_Norm,RLeg4.GetChild(1).right)-90f;
		c1=Vector3.Angle(-BR_Norm,RLeg4.up)-90f;
		RLeg4.rotation*=Quaternion.Euler(BR3,0,0);
		RLeg4.GetChild(0).rotation*=Quaternion.Euler(0,-BR2,0);
		RLeg4.GetChild(1).rotation*=Quaternion.Euler(0,-BR2,0);
		RLeg4.GetChild(2).rotation*=Quaternion.Euler(0,-BR2,0);

		Vector3 va3=BR_HIT+(BR_HIT-RLeg4.position)+transform.up*offset1;
		Vector3 va2=(va1+transform.up*(va1-RLeg1.position).magnitude);
		alt1=((va1-va2).magnitude-(va3-va2).magnitude)*(100/(va1-va2).magnitude);

		//Left Leg
		Transform LLeg4=LLeg3.GetChild(0);
		float offset2=(LLeg4.position-LLeg4.GetChild(1).position).magnitude;
		Vector3 vb1=LLeg4.position-transform.up*offset2;
		float inv2=Mathf.Clamp(Vector3.Cross(LLeg4.position-transform.position,LLeg1.position-transform.position).y,-1.0f,1.0f);

		LLeg1.rotation*=Quaternion.Euler(0,BL1-(BL1+BL_Add),0);
		a2=Vector3.Angle(LLeg1.position-LLeg2.position,LLeg1.position-LLeg3.position);
		LLeg2.rotation*=Quaternion.Euler(0,-BL1*2f,0);
		LLeg3.rotation*=Quaternion.Euler(0,BL1+BL_Add*inv2,0);

		b2=Vector3.Angle(-BL_Norm,LLeg4.GetChild(1).up)-90f;
		c2=Vector3.Angle(-BL_Norm,LLeg4.up)-90f;
		LLeg4.rotation*=Quaternion.Euler(BL3,0,0);
		LLeg4.GetChild(0).rotation*=Quaternion.Euler(0,BL2,0);
		LLeg4.GetChild(1).rotation*=Quaternion.Euler(BL2,0,0);
		LLeg4.GetChild(2).rotation*=Quaternion.Euler(0,BL2,0);

		Vector3 vb3=BL_HIT+(BL_HIT-LLeg4.position)+transform.up*offset2;
		Vector3 vb2=(vb1+transform.up*(vb1-LLeg1.position).magnitude);
		alt2=((vb1-vb2).magnitude-(vb3-vb2).magnitude)*(100/(vb1-vb2).magnitude);

		//Add rotations
		if(!isDead&&isOnGround)
		{
			BR_Add=Vector3.Angle(RLeg1.position-RLeg2.position,RLeg1.position-RLeg3.position)-a1;
			BL_Add=Vector3.Angle(LLeg1.position-LLeg2.position,LLeg1.position-LLeg3.position)-a2;
		}
	}

	//CONVEX QUADRUPED
	void Convex(Transform RLeg1,Transform RLeg2,Transform RLeg3,Transform LLeg1,Transform LLeg2,Transform LLeg3,
										Transform RArm1,Transform RArm2,Transform RArm3,Transform LArm1,Transform LArm2,Transform LArm3)
	{
		//Right arm
		float offset1=(RArm3.position-RArm3.GetChild(0).position).magnitude;
		Vector3 va1=RArm3.position-transform.up*offset1;

		RArm1.rotation*=Quaternion.Euler(FR1-(FR1+FR_Add),0,0);
		a1=Vector3.Angle(RArm1.position-RArm2.position,RArm1.position-RArm3.GetChild(0).GetChild(0).position);
		RArm2.rotation*=Quaternion.Euler(0,FR1-FR_Add,0);
		b1=Vector3.Angle(FR_Norm,RArm3.GetChild(0).right)-90f;
		c1=Vector3.Angle(FR_Norm,-RArm3.GetChild(0).up)-90f;
		RArm3.rotation*=Quaternion.Euler(-FR3/2,-FR2/2,0);
		RArm3.GetChild(0).rotation*=Quaternion.Euler(-FR3/2,-FR2/2,0);

		Vector3 va3=FR_HIT+(FR_HIT-RArm3.GetChild(0).GetChild(0).position)+transform.up*offset1;
		Vector3 va2=new Vector3(va1.x,va1.y-(va1.y-RArm1.position.y)-(va1.y-FR_HIT.y),va1.z);
		alt1=((va1-va2).magnitude-(va3-va2).magnitude)*(100/(va1-va2).magnitude);

		//Left arm
		float offset2=(LArm3.position-LArm3.GetChild(0).position).magnitude;
		Vector3 vb1=LArm3.position-transform.up*offset2;

		LArm1.rotation*=Quaternion.Euler(FL1-(FL1+FL_Add),0,0);
		a2=Vector3.Angle(LArm1.position-LArm2.position,LArm1.position-LArm3.GetChild(0).GetChild(0).position);
		LArm2.rotation*=Quaternion.Euler(-FL1+FL_Add,0,0);
		b2=Vector3.Angle(FL_Norm,-LArm3.GetChild(0).up)-90f;
		c2=Vector3.Angle(FL_Norm,LArm3.GetChild(0).right)-90f;
		LArm3.rotation*=Quaternion.Euler(-FL2/2,-FL3/2,0);
		LArm3.GetChild(0).rotation*=Quaternion.Euler(-FL2/2,-FL3/2,0);

		Vector3 vb3=FL_HIT+(FL_HIT-LArm3.GetChild(0).GetChild(0).position)+transform.up*offset2;
		Vector3 vb2=new Vector3(vb1.x,vb1.y-(vb1.y-LArm1.position.y)-(vb1.y-FL_HIT.y),vb1.z);
		alt2=((vb1-vb2).magnitude-(vb3-vb2).magnitude)*(100/(vb1-vb2).magnitude);

		//Right leg
		float offset3=(RLeg3.position-RLeg3.GetChild(0).GetChild(0).position).magnitude;
		Vector3 vc1=RLeg3.position-transform.up*offset3;

		RLeg1.rotation*=Quaternion.Euler(0,-(BR1+(BR1+BR_Add)),0);
		a3=Vector3.Angle(RLeg1.position-RLeg2.position,RLeg1.position-RLeg3.position);
		RLeg2.rotation*=Quaternion.Euler(0,(BR1*2f)-BR_Add,0);
		b3=Vector3.Angle(BR_Norm,RLeg3.GetChild(0).right)-90f;
		c3=Vector3.Angle(-BR_Norm,RLeg3.GetChild(0).up)-90f;
		RLeg3.rotation*=Quaternion.Euler(-BR3/2,-BR2/2,0);
		RLeg3.GetChild(0).rotation*=Quaternion.Euler(-BR3/2,-BR2/2,0);

		Vector3 vc3=BR_HIT+(BR_HIT-RLeg3.position)+transform.up*offset3;
		Vector3 vc2=new Vector3(vc1.x,vc1.y-(vc1.y-RLeg1.position.y)-(vc1.y-BR_HIT.y),vc1.z);
		alt3=((vc1-vc2).magnitude-(vc3-vc2).magnitude)*(100/(vc1-vc2).magnitude);

		//Left leg
		float offset4=(LLeg3.position-LLeg3.GetChild(0).GetChild(0).position).magnitude;
		Vector3 vd1=LLeg3.position-transform.up*offset4;

		LLeg1.rotation*=Quaternion.Euler(BL1+(BL1+BL_Add),0,0);
		a4=Vector3.Angle(LLeg1.position-LLeg2.position,LLeg1.position-LLeg3.position);
		LLeg2.rotation*=Quaternion.Euler(-(BL1*2f)+BL_Add,0,0);
		b4=Vector3.Angle(BL_Norm,LLeg3.GetChild(0).right)-90f;
		c4=Vector3.Angle(-BL_Norm,LLeg3.GetChild(0).up)-90f;
		LLeg3.rotation*=Quaternion.Euler(-BL3/2,-BL2/2,0);
		LLeg3.GetChild(0).rotation*=Quaternion.Euler(-BL3/2,-BL2/2,0);

		Vector3 vd3=BL_HIT+(BL_HIT-LLeg3.position)+transform.up*offset4;
		Vector3 vd2=new Vector3(vd1.x,vd1.y-(vd1.y-LLeg1.position.y)-(vd1.y-BL_HIT.y),vd1.z);
		alt4=((vd1-vd2).magnitude-(vd3-vd2).magnitude)*(100/(vd1-vd2).magnitude);

		if(!isConstrained&&!isDead&&isOnGround&&!isInWater)
		{
			FR_Add=Vector3.Angle(RArm1.position-RArm2.position,RArm1.position-RArm3.GetChild(0).GetChild(0).position)-a1;
			FL_Add=Vector3.Angle(LArm1.position-LArm2.position,LArm1.position-LArm3.GetChild(0).GetChild(0).position)-a2;
			BR_Add=Vector3.Angle(RLeg1.position-RLeg2.position,RLeg1.position-RLeg3.position)-a3;
			BL_Add=Vector3.Angle(LLeg1.position-LLeg2.position,LLeg1.position-LLeg3.position)-a4;
		}
	}

	//FLYING
	void Flying(Transform RLeg1,Transform RLeg2,Transform RLeg3,Transform LLeg1,Transform LLeg2,Transform LLeg3,
								Transform RArm1,Transform RArm2,Transform RArm3,Transform LArm1,Transform LArm2,Transform LArm3)
	{
		//Right wing
		Vector3 va1=RArm3.GetChild(1).position;

		RArm1.rotation*=Quaternion.Euler(FR1,FR1-(FR1-FR_Add),FR1);
		a1=Vector3.Angle(RArm1.position-RArm2.position,RArm1.position-RArm3.GetChild(1).position);
		RArm2.rotation*=Quaternion.Euler(0,0,(-FR1*2.4f)-FR_Add);
		b1=Vector3.Angle(FR_Norm,RArm3.right)-90f;
		c1=Vector3.Angle(-FR_Norm,RArm3.up)-90f;
		RArm3.rotation*=Quaternion.Euler(FR3,FR2,0);

		Vector3 va3=FR_HIT+(FR_HIT-RArm3.GetChild(1).position);
		Vector3 va2=new Vector3(va1.x,va1.y-(va1.y-RArm1.position.y)-(va1.y-FR_HIT.y),va1.z);
		alt1=((va1-va2).magnitude-(va3-va2).magnitude)*(100/(va1-va2).magnitude);

		//Left Wing
		Vector3 vb1=LArm3.GetChild(1).position;

		LArm1.rotation*=Quaternion.Euler(-FL1,FL1-(FL1-FL_Add),-FL1);
		a2=Vector3.Angle(LArm1.position-LArm2.position,LArm1.position-LArm3.GetChild(1).position);
		LArm2.rotation*=Quaternion.Euler(0,0,(FL1*2.4f)+FL_Add);
		b2=Vector3.Angle(FL_Norm,LArm3.right)-90f;
		c2=Vector3.Angle(-FL_Norm,LArm3.up)-90f;
		LArm3.rotation*=Quaternion.Euler(FL3,FL2,0);

		Vector3 vb3=FL_HIT+(FL_HIT-LArm3.GetChild(1).position);
		Vector3 vb2=new Vector3(vb1.x,vb1.y-(vb1.y-LArm1.position.y)-(vb1.y-FL_HIT.y),vb1.z);
		alt2=((vb1-vb2).magnitude-(vb3-vb2).magnitude)*(100/(vb1-vb2).magnitude);

		//Right leg
		float offset1=(RLeg3.position-RLeg3.GetChild(2).position).magnitude/1.5f;
		Vector3 vc1=RLeg3.position-transform.up*offset1;
		float inv1=Mathf.Clamp(Vector3.Cross(RLeg3.GetChild(2).position-transform.position,RLeg1.position-transform.position).y,-1.0f,1.0f);

		RLeg1.rotation*=Quaternion.Euler(0,-BR1+(BR1-BR_Add),0);
		a3=Vector3.Angle(RLeg1.position-RLeg2.position,RLeg1.position-RLeg3.GetChild(2).position);
		RLeg2.rotation*=Quaternion.Euler(0,-BR1*2,0);
		c3=Vector3.Angle(BR_Norm,RLeg3.GetChild(2).up)-90f;
		RLeg3.rotation*=Quaternion.Euler(0,BR1-BR_Add*inv1,BR3);
		b3=Vector3.Angle(BR_Norm,RLeg3.GetChild(2).right)-90f;
		RLeg3.GetChild(0).rotation*=Quaternion.Euler(0,-BR2,0);
		RLeg3.GetChild(1).rotation*=Quaternion.Euler(0,-BR2,0);
		RLeg3.GetChild(2).rotation*=Quaternion.Euler(0,-BR2,0);
		RLeg3.GetChild(3).rotation*=Quaternion.Euler(0,-BR2,0);

		Vector3 vc3=BR_HIT+(BR_HIT-RLeg3.GetChild(2).position)+transform.up*offset1;
		Vector3 vc2=(vc1+transform.up*(vc1-RLeg1.position).magnitude);
		alt3=((vc1-vc2).magnitude-(vc3-vc2).magnitude)*(100/(vc1-vc2).magnitude);

		//Left leg
		float offset2=(LLeg3.position-LLeg3.GetChild(2).position).magnitude/1.5f;
		Vector3 vd1=LLeg3.position-transform.up*offset2;
		float inv2=Mathf.Clamp(Vector3.Cross(LLeg3.GetChild(2).position-transform.position,LLeg1.position-transform.position).y,-1.0f,1.0f);

		LLeg1.rotation*=Quaternion.Euler(0,-BL1+(BL1-BL_Add),0);
		a4=Vector3.Angle(LLeg1.position-LLeg2.position,LLeg1.position-LLeg3.GetChild(2).position);
		LLeg2.rotation*=Quaternion.Euler(0,-BL1*2,0);
		c4=Vector3.Angle(BL_Norm,LLeg3.GetChild(2).up)-90f;
		LLeg3.rotation*=Quaternion.Euler(0,BL1-BL_Add*inv2,BL3);
		b4=Vector3.Angle(BL_Norm,LLeg3.GetChild(2).right)-90f;
		LLeg3.GetChild(0).rotation*=Quaternion.Euler(0,-BL2,0);
		LLeg3.GetChild(1).rotation*=Quaternion.Euler(0,-BL2,0);
		LLeg3.GetChild(2).rotation*=Quaternion.Euler(0,-BL2,0);
		LLeg3.GetChild(3).rotation*=Quaternion.Euler(0,-BL2,0);

		Vector3 vd3=BL_HIT+(BL_HIT-LLeg3.GetChild(2).position)+transform.up*offset2;
		Vector3 vd2=(vd1+transform.up*(vd1-LLeg1.position).magnitude);
		alt4=((vd1-vd2).magnitude-(vd3-vd2).magnitude)*(100/(vd1-vd2).magnitude);

		//Add rotations
		if(!isConstrained&&!isDead&&isOnGround&&!isOnLevitation)
		{
			FR_Add=Vector3.Angle(RArm1.position-RArm2.position,LArm1.position-RArm3.GetChild(1).position)-a1;
			FL_Add=Vector3.Angle(LArm1.position-LArm2.position,LArm1.position-LArm3.GetChild(1).position)-a2;
			BR_Add=Vector3.Angle(RLeg1.position-RLeg2.position,RLeg1.position-RLeg3.GetChild(2).position)-a3;
			BL_Add=Vector3.Angle(LLeg1.position-LLeg2.position,LLeg1.position-LLeg3.GetChild(2).position)-a4;
		}
	}
	#endregion

}
