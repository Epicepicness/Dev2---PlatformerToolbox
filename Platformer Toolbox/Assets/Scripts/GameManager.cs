using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;				//Singleton instance of GameManager.
	[HideInInspector] public DemoScene player;				//Reference to current Player script.
	[HideInInspector] public SmoothFollow mainCamera;		//Reference to current Camera.
	[HideInInspector] public InputManager inputManager;		//Reference to InputManager component.
	[HideInInspector] public UIManager uiManager;			//Reference to UIManager component.

	public static bool pauzeGameplay;


	private void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (this.gameObject);
		DontDestroyOnLoad (this.gameObject);

		SetupSceneComponents ();
	}

	//--- Game State Functions ---------------------------------------------------------------------------------------------------
	public void PauzeGameplay () {
		pauzeGameplay = true;
	}

	public void ResumeGameplay () {
		pauzeGameplay = false;
	}

	//--- Scene Functions ---------------------------------------------------------------------------------------------------
	private void SetupSceneComponents () {
		if (inputManager == null) {
			if (GetComponent<InputManager> ())
				inputManager = GetComponent<InputManager> ();
			else
				inputManager = this.gameObject.AddComponent<InputManager> ();
		}

		if (uiManager == null)
			uiManager = (GameObject.Find ("UIManager")) ? GameObject.Find ("UIManager").GetComponent<UIManager> () :
				((GameObject) Instantiate (Resources.Load ("Prefabs/UIManager"), Vector3.zero, Quaternion.identity)).GetComponent<UIManager> ();

		mainCamera = null; player = null;
		mainCamera = (GameObject.Find ("Main Camera")) ? GameObject.Find ("Main Camera").GetComponent<SmoothFollow> () :
			((GameObject) Instantiate (Resources.Load ("Prefabs/Camera"), Vector3.zero, Quaternion.identity)).GetComponent<SmoothFollow> ();
		player = (GameObject.Find ("Player")) ? GameObject.Find ("Player").GetComponent<DemoScene> () :
			((GameObject) Instantiate (Resources.Load ("Prefabs/Player"), Vector3.zero, Quaternion.identity)).GetComponent<DemoScene> ();
	}


}
