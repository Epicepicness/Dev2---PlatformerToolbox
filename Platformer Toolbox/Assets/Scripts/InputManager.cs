using UnityEngine;

public class InputManager : MonoBehaviour {

	public PlayerInput Current;

	void Start () {
		Current = new PlayerInput ();
	}

	void Update () {
		Vector3 directionalInput = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));

		bool jumpInput = Input.GetButtonDown ("Jump");
		bool sprintInput = Input.GetButton ("Sprint");

		Current = new PlayerInput () {
			DirectionalInput = directionalInput,
			JumpInput = jumpInput,
			SprintInput = sprintInput,
		};
	}
}

public struct PlayerInput {
	public Vector3 DirectionalInput;
	public bool JumpInput;
	public bool SprintInput;
}
