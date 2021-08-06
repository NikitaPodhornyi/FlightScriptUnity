using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flightControls : MonoBehaviour {
    //region of all variables
    #region Declarations  
    [SerializeField] float MaxEngineThrust = 300.0f;
	public float liftForce;
	float speed;
	float velocity;
	public float speedInKnots;
	public float altitude;
	[SerializeField]float rotEffect = 100f;
	[SerializeField]Rigidbody ship;
	public AnimationCurve test;
	public AnimationCurve rotationAffect;
	public AnimationCurve liftCo;
	public AnimationCurve airDense;
	public float throttle;
	float pitch,yaw,roll,throttleInput;
	public float inEditorShow;
	//for realistic lift formula
	float wingSurfaceArea = 78.8f;
	public float angleOfAttack;
	//Drag
	float startDrag;
	float flapsDrag = 0.005f; //for aech step, per Knot
	 float gearDrag = 0.01f; //prerKnot
	float breakDrag = 0.02f; //per Knot

	public bool gearUp = false;
	bool airBrakes = false;
    bool flaps = false;
	//fuel calculations
	public float totalFuel = 10000;
	public AnimationCurve fuelRation;
    #endregion 
    void Start () {
		startDrag = ship.drag;
	}
	
	void FixedUpdate () {

	   pitch = Input.GetAxis ("Vertical");
		yaw = Input.GetAxis ("Zax");
		roll = Input.GetAxis ("Horizontal");
		throttleInput = Input.GetAxis ("Thrust");

	    RealSpeed ();
		Lift ();
		Speed ();
		Rotation ();
	    AngleOfAttck ();
		Altitude ();
		DragAffect ();
	    FlapsAffect ();
		gearControls ();
	    //FuelConsumption ();

		ship = GetComponent<Rigidbody> ();

	}

	private void RealSpeed()
	{
		var locVel = transform.InverseTransformDirection(ship.velocity);
		velocity = Mathf.Max (0, locVel.z);
		speedInKnots = velocity * 1.94384449f;
	}

	private void Speed()
	{
		throttle = Mathf.Clamp01 (throttle + (throttleInput * Time.deltaTime * test.Evaluate (throttle))*0.2f);

		speed = throttle * MaxEngineThrust;
	}

	private void Lift()
	{
		var allForces = Vector3.zero;
		allForces += speed * transform.forward;
		liftForce = ((Mathf.Pow(velocity,1.6f) * wingSurfaceArea * airDense.Evaluate(altitude) * liftCo.Evaluate((-angleOfAttack*Mathf.Rad2Deg) / 180))/10000);
		allForces += liftForce * transform.up;
		ship.AddForce (allForces);
		inEditorShow = liftCo.Evaluate (angleOfAttack);
	}

	private void Rotation()
	{
		var rotation = Vector3.zero;
		rotation += pitch * transform.right*rotEffect;
		rotation += yaw * transform.up * (rotEffect);
		rotation += -roll * transform.forward * (rotEffect/10);
		ship.AddTorque (rotation*rotationAffect.Evaluate(throttle));
	}

	private void FlapsAffect()
	{
		if (Input.GetKeyDown (KeyCode.F)&&flaps == false) {
			flaps = true;
			wingSurfaceArea = wingSurfaceArea+10;
		}
		else if (Input.GetKeyDown (KeyCode.F) && flaps == true) {
			flaps = false;
			wingSurfaceArea -= 10f;
		}
	}

	private void gearControls()
	{
		if (Input.GetKeyDown (KeyCode.G) && gearUp == false) {
			gearUp = true;
		} else if (Input.GetKeyDown (KeyCode.G) && gearUp == true) {
			gearUp = false;
		}
	}

	private void DragAffect()
	{
		ship.angularDrag = 0.03f * speedInKnots;
		ship.drag = startDrag + (airBrakes ? (speedInKnots*breakDrag): 0 )+(gearUp ? 0 : (speedInKnots * gearDrag ))+(flaps ? (speedInKnots * flapsDrag): 0 );

	}

	private void AngleOfAttck()
	{
		var flatForward = transform.forward;
		flatForward.y = 0;
		flatForward.Normalize();
		var localFlatForward = transform.InverseTransformDirection(flatForward);
		angleOfAttack = (Mathf.Atan2(localFlatForward.y, localFlatForward.z));

	}

	private void Altitude()
	{
		var ray = new Ray(transform.position - Vector3.up*10, -Vector3.up);
		RaycastHit hit;
		altitude = Physics.Raycast(ray, out hit) ? hit.distance + 10 : transform.position.y;
	}

	private void FuelConsumption()
	{
		totalFuel -= fuelRation.Evaluate (throttle)*10;
	}
}
