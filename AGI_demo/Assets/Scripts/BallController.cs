using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour {

	// Test trow vector
	public float x;
	public float y;
	public float z;

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

	private Rigidbody _rigidbody;
	private Light _halo;

	// Collided object reference
	private Collision _other;

	// Ball position variables
	private float _lastY = 0;

	// Flags
	private bool _hit;
	private bool _caught;

	// Use this for initialization
	void Start () {
		// Init object
		_rigidbody = GetComponent<Rigidbody>();
		_halo = GetComponent<Light> ();

		_other = null;
		_hit = false;
		_caught = false;

		// Set inital speed
		Vector3 initSpeed = new Vector3 (x, y, z);
		_rigidbody.velocity = initSpeed;
	}
	
	// Update is called once per frame
	void Update () {
		if (_caught) {
			float t = (Time.time - _blastStartTime) / blastDuration;
			_halo.intensity =  Mathf.SmoothStep (_maxBlastIntensity, 0f, t);
		}

		if (_hit) {
			if (transform.position.y < _lastY) {
				_hit = false;
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
			} else {
				_lastY = transform.position.y;			
			}
		} else {
			// Check if hit occured
			if (_other == null || _caught) {
				return;
			}

			// Set pokemon velocity
			Rigidbody pokemonRigidBody = _other.gameObject.GetComponent<Rigidbody> ();
			pokemonRigidBody.velocity = _direction * catchingSpeed;

			// Adjust light blast intensity
			float t = (Time.time - _blastStartTime) / blastDuration;
			_halo.intensity =  Mathf.SmoothStep (0f, 8f, t);

			// Scale pokemon
			// Check if scaling done
			if (_other.gameObject.transform.localScale.x <= _targetScale) {
				_caught = true;
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

		_hit = true;
		_lastY = transform.position.y;

		// Open ball
		topSphere.transform.eulerAngles = new Vector3(ballOpeningAngle, 0f, 0f);

		// Add vertical "bounce" speed
		_rigidbody.velocity = new Vector3(0f, aftershotSpeedValue, 0f);
		
	}
}
