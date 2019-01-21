using UnityEngine;
using Prime31;

public class DemoScene : MonoBehaviour {
	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;

	[HideInInspector]
	//private float normalizedHorizontalSpeed = 0;

	private CharacterController2D _controller;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;
	private InputManager _input;

	private void Start () {
		_controller = GetComponent<CharacterController2D> ();
		_input = GameManager.instance.inputManager;

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}

	#region Event Listeners
	private void onControllerCollider (RaycastHit2D hit) {
		// bail out on plain old ground hits cause they arent very interesting
		if (hit.normal.y == 1f)
			return;
	}

	private void onTriggerEnterEvent (Collider2D col) {
		Debug.Log ("onTriggerEnterEvent: " + col.gameObject.name);
	}

	private void onTriggerExitEvent (Collider2D col) {
		Debug.Log ("onTriggerExitEvent: " + col.gameObject.name);
	}
	#endregion

	// the Update loop contains a very simple example of moving the character around and controlling the animation
	private void Update () {
		if (_controller.isGrounded)
			_velocity.y = 0;

		if (_input.Current.DirectionalInput.x < 0) {
			if (transform.localScale.x < 0f)
				transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
		}
		else if (_input.Current.DirectionalInput.x > 0) {
			if (transform.localScale.x > 0f)
				transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
		}

		// we can only jump whilst grounded
		if (_controller.isGrounded && _input.Current.JumpInput) {
			_velocity.y = Mathf.Sqrt (2f * jumpHeight * -gravity);
		}

		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
		_velocity.x = Mathf.Lerp (_velocity.x, _input.Current.DirectionalInput.x * runSpeed, Time.deltaTime * smoothedMovementFactor);

		// apply gravity before moving
		_velocity.y += gravity * Time.deltaTime;

		// if holding down bump up our movement amount and turn off one way platform detection for a frame.
		// this lets us jump down through one way platforms
		if (_controller.isGrounded && _input.Current.DirectionalInput.y < 0) {
			_velocity.y *= 3f;
			_controller.ignoreOneWayPlatformsThisFrame = true;
		}

		_controller.move (_velocity * Time.deltaTime);

		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;
	}

}
