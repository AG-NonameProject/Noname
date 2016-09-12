using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour {

	private enum BALL_STATE {
		idle,
		hit,
		catching,
		caught
	}

	// Test trow vector
	public float x;
	public float y;
	public float z;

	public float lifeLenght = 10f;

	public float ballOpeningAngle = -20;

	public float aftershotSpeedValue = 100;

	public GameObject topSphere;

	// Scaling parameters
	public float scalingSpeed = 1f;
	public float targetScaleFactor = 0.1f;
	public float catchingSpeed = 10f;
	public float blastDuration = 5f;

	// Parameters
	private Vector3 _direction;
	private float _currentScale;
	private float _blastStartTime;
	private float _maxBlastIntensity;
	private float _targetScale;

	// Ball state
	private BALL_STATE state;

	// Object references
	private Rigidbody _rigidbody;
	private Light _halo;
	private Collision _other;

	// Ball position variables
	private float _lastY = 0;

	// Use this for initialization
	void Start () {
		init ();
	}

	// Update is called once per frame
	void Update () {
		switch (state) {
		case BALL_STATE.idle:
			// Check space pressed
			if (Input.GetKeyDown (KeyCode.Space)) {
				throwBall ();
			}

			// Aim ball
			aiming ();
			break;
		case BALL_STATE.hit:
			if (transform.position.y < _lastY) {
				startCatch ();
				state = BALL_STATE.catching;
			} else {
				_lastY = transform.position.y;
			}
			break;
		case BALL_STATE.catching:
			catchingAction ();
			break;
		case BALL_STATE.caught:
			coughtAction ();
			break;
		}
	}

	void OnCollisionEnter (Collision other) {
		// Check if collision with pokemon
		if (!(other.gameObject.layer == LayerMask.NameToLayer ("Pokemon"))) {
			return;
		}

		// Get pokemon object reference
		_other = other;

		// Set target scale
		_targetScale = _other.gameObject.transform.localScale.x * targetScaleFactor;

		state = BALL_STATE.hit;
		_lastY = transform.position.y;

		// Open ball
		topSphere.transform.eulerAngles = new Vector3(ballOpeningAngle, 0f, 0f);

		// Add vertical "bounce" speed
		_rigidbody.velocity = new Vector3(0f, aftershotSpeedValue, 0f);
	}

	private void init () {
		// Init object
		state = BALL_STATE.idle;

		_rigidbody = GetComponent<Rigidbody>();
		_halo = GetComponent<Light> ();

		_rigidbody.isKinematic = true;

		_other = null;
	}

	private void throwBall() {
		_rigidbody.isKinematic = false;

		// Set inital speed
		Vector3 initSpeed = new Vector3 (x, y, z);
		_rigidbody.velocity = initSpeed;

		// Set destroy after time
		Destroy(gameObject, lifeLenght);
	}

	private void aiming () {
		if (Input.GetKeyDown(KeyCode.W)) {
			y += 1;
		}
		if (Input.GetKeyDown(KeyCode.A)) {
			x += 1;
		}
		if (Input.GetKeyDown(KeyCode.S)) {
			y -= 1;
		}
		if (Input.GetKeyDown(KeyCode.D)) {
			x -= 1;
		}
	}

	private void startCatch() {
		// Stop ball and pokemon
		_rigidbody.useGravity = false;

		Rigidbody pokemonRigidBody = _other.gameObject.GetComponent<Rigidbody> ();
		pokemonRigidBody.isKinematic = true;
		pokemonRigidBody.isKinematic = false;

		// Calculate relative ball - pokemon vector
		_direction = (transform.position - _other.gameObject.transform.position) * scalingSpeed;
		_currentScale = _other.transform.localScale.x;

		// Disable collisions for pokemon object
		foreach (Collider c in _other.gameObject.GetComponents<Collider>()) {
			c.enabled = false;
		}

		// Set start time for light blast
		_blastStartTime = Time.time;
	}

	private void catchingAction () {
		// Set pokemon velocity
		Rigidbody pokemonRigidBody = _other.gameObject.GetComponent<Rigidbody> ();
		pokemonRigidBody.velocity = _direction * catchingSpeed;

		// Adjust light blast intensity
		float t = (Time.time - _blastStartTime) / blastDuration;
		_halo.intensity =  Mathf.SmoothStep (0f, 8f, t);

		// Scale pokemon
		// Check if scaling done
		if (_other.gameObject.transform.localScale.x <= _targetScale) {
			state = BALL_STATE.caught;
			_other.gameObject.SetActive (false);

			// Drop and close ball
			_rigidbody.useGravity = true;
			topSphere.transform.localEulerAngles = Vector3.zero;

			_blastStartTime = Time.time;
			_maxBlastIntensity = _halo.intensity;

			return;
		}

		float delta = (_currentScale - _targetScale)*scalingSpeed*_direction.magnitude;
		_other.gameObject.transform.localScale -= new Vector3(delta, delta, delta);
	}

	private void coughtAction () {
		float t = (Time.time - _blastStartTime) / blastDuration;
		_halo.intensity = Mathf.SmoothStep (_maxBlastIntensity, 0f, t);
	}
}
