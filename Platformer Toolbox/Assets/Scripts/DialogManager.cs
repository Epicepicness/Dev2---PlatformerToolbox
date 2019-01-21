using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DialogManager : MonoBehaviour{

	private SceneDialogs sceneDialogs;
	private Dialog currentDialog;
	private DialogLine currentDialogLine;

	[SerializeField] private GameObject dialogCanvas;
	[SerializeField] private GameObject bigSpeakerObject;
	[SerializeField] private GameObject smallSpeakerObject;
	[SerializeField] private Image[] bigSpeakerImages = new Image[4];
	[SerializeField] private Text nameText;
	[SerializeField] private Text dialogText;
	[SerializeField] private GameObject pressToContinue;
	[SerializeField] private Button[] responseButtons = new Button[4];

	private int currentSpriteIndex = 0;


	// Loads the Dialogs out of an XML file from "Resources/XML_Files/SceneDialogs", and the passed file name.
	// This is called during OnNewLevelLoaded function in the GameManager
	public void LoadDialog (string fileName) {
		//Creates a path to an xml file, and only an xml file.
		string xmlPath = Path.Combine ("Resources/XML_Files/SceneDialogs", fileName);
		if (Path.GetExtension (xmlPath) == string.Empty) {
			xmlPath += ".xml";
		} else if (Path.GetExtension (xmlPath) != ".xml") {
			Path.ChangeExtension (xmlPath, ".xml");
		}

		if (File.Exists (Path.Combine (Application.dataPath, xmlPath))) {
			sceneDialogs = XML_Loader.Deserialize <SceneDialogs> ((Path.Combine (Application.dataPath, xmlPath)));
		} else {
			Debug.LogError ("XML File: " + (Path.Combine (Application.dataPath, xmlPath)) + " is not found.");
		}
	}

	///<summary>
	/// Opens the Dialogwindow, and starts the dialog with the passed ID.
	///</summary>
	public void StartDialog (int dialogID) {
		currentDialog = sceneDialogs.allDialogsInScene [dialogID];

		//Check for type of dialog, and to use big/small sprites.
		if (currentDialog.BigSprites == true) {
			bigSpeakerObject.SetActive (true);
			smallSpeakerObject.SetActive (false);
			for (int i = 0; i < 4; i++) {
				bigSpeakerImages [i].gameObject.SetActive (false);
			}
		} else {
			bigSpeakerObject.SetActive (false);
			smallSpeakerObject.SetActive (true);
		}
		dialogCanvas.SetActive (true);

		NextDialogLine (0);
	}

	public void EndDialog () {
		dialogCanvas.SetActive (false);
		UIManager.instance.EndDialog ();
	}

	public void NextDialogLine () {
		NextDialogLine (currentDialogLine.followUpLine);
	}

	private void NextDialogLine (int lineID) {
		//If given negative ID, end the dialog
		if (lineID < 0) {
			EndDialog ();
			return;
		}

		currentDialogLine = currentDialog.DialogLines [lineID];

		//Zoek Manier om PlayerDialogData te krijgen

		nameText.text = currentDialogLine.speakerData;
		dialogText.text = currentDialogLine.text;

		if (currentDialogLine.responses.Count == 0) {
			pressToContinue.SetActive (true);
			for (int i = 0; i <= 3; i++) {
				responseButtons [i].gameObject.SetActive (false);
			}
		} else {
			pressToContinue.SetActive (false);

			for (int i = 0; i <= 3; i++) {
				if (i <= currentDialogLine.responses.Count - 1) {
					responseButtons [i].onClick.RemoveAllListeners ();
					responseButtons [i].GetComponentInChildren<Text> ().text = currentDialogLine.responses [i].text;
					int local_i = i;
					responseButtons [i].onClick.AddListener (delegate {
						NextDialogLine (currentDialogLine.responses [local_i].followupLine);
					});
					responseButtons [i].gameObject.SetActive (true);
				} else {
					responseButtons [i].gameObject.SetActive (false);
				}
			}
		}

		//Setting up Sprites
		if (currentDialog.BigSprites) {
			if (bigSpeakerImages [currentSpriteIndex].IsActive ()) {
				currentSpriteIndex = (currentSpriteIndex == 3) ? 0 : currentSpriteIndex + 1;
			}
			bigSpeakerImages [currentSpriteIndex].gameObject.SetActive (true);
		}
	}

}
