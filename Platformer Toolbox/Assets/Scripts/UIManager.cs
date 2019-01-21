using UnityEngine;

public class UIManager : MonoBehaviour {

	public static UIManager instance = null;				//Singleton instance of GameManager.

	//Components
	[HideInInspector] public DialogManager dialogManager;


	private void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (this.gameObject);
		DontDestroyOnLoad (this.gameObject);

		dialogManager = GetComponent<DialogManager> ();
	}

	public void BeginDialog (int dialogID) {
		GameManager.instance.PauzeGameplay ();

		dialogManager.StartDialog (dialogID);
	}

	public void EndDialog () {
		GameManager.instance.ResumeGameplay ();
	}

}
