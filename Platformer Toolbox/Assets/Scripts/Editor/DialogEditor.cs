using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Xml;					//Needed for XML functionality

public class DialogEditor : EditorWindow {

	public static Texture2D APLDPALDPALDPLADPLDPA = null;

	private static List <string> allXmlFileNames = new List <string> ();
	private static SceneDialogs currentlyLoadedSceneDialogs;
	private static string fileDirectory = "Resources/XML_Files/SceneDialogs";

	private static string selectedSceneName;
	private static int selectedSceneIndex = 0;
	private static string createNewFileName = "";

	private static Dialog selectedDialog;
	private static Vector2 scrollPosition = Vector2.zero;
	private static int dialogIDInput;
	private static string dialogDescriptionInput;

	private static Vector2 gridScrollPosition = Vector2.zero;
	private static Texture2D gridBackground;
	private static List<DialogGridNode> gridNodes = new List<DialogGridNode> ();
	private static List<Rect> windows = new List<Rect>();
	private static Rect startNode;
	private static Rect endNode;

	private static DialogLine selectedDialogLine;

	private static string speakerName;
	private static string lineText;
	private static int followUpLine;
	private static int responseCount;

	private static List<string> responseText = new List<string> ();
	private static List<int> responseFollowUpID = new List<int> ();


	[MenuItem ("Tools/DialogEditor")]
	public static void Init () {
		EditorWindow.GetWindow (typeof(DialogEditor));

		startNode = new Rect (10, 10, 50, 50);
		endNode = new Rect (500, 500, 50, 50);

		ReloadData ();
	}

	//--- Data / Loading Functions ---------------------------------------------------------------------------------------------------
	// Makes a list of all the currently existing XML files, and loads in the first file.
	private static void ReloadData () {
		LoadXmlFileList ();
		if (allXmlFileNames.Count != 0) {
			LoadXmlFileData (allXmlFileNames [0]);
			selectedSceneName = allXmlFileNames [0];
		}
	}

	private static void LoadXmlFileList () { 
		allXmlFileNames.Clear ();
		DirectoryInfo dir = new DirectoryInfo (Path.Combine ("Assets", fileDirectory));
		FileInfo[] info = dir.GetFiles ("*.xml");
		foreach (FileInfo f in info) {
			allXmlFileNames.Add (f.Name);
		}
	}

	// Reads and Serializes a specific XML file
	private static void LoadXmlFileData (string fileName) {
		string xmlPath = Path.Combine (fileDirectory, fileName);

		if (Path.GetExtension (xmlPath) == string.Empty) {
			xmlPath += ".xml";
		} else if (Path.GetExtension (xmlPath) != ".xml") {
			Path.ChangeExtension (xmlPath, ".xml");
		}

		if (File.Exists (Path.Combine (Application.dataPath, xmlPath))) {
			currentlyLoadedSceneDialogs = XML_Loader.Deserialize <SceneDialogs> ((Path.Combine (Application.dataPath, xmlPath)));
			if (currentlyLoadedSceneDialogs.allDialogsInScene [0] != null) {
				SelectDialogByID (0);
				if (selectedDialog.DialogLines.Count != 0) {
					SelectDialogLine (0);
				}
			}
		} else {
			Debug.LogError ("XML File: " + (Path.Combine (Application.dataPath, xmlPath)) + " is not found.");
		}
	}

	//--- XML-File Functions ---------------------------------------------------------------------------------------------------
	private static void CreateNewXMLFile (string name) {
		// Making sure the file directory exists
		if (!AssetDatabase.IsValidFolder (fileDirectory)) {
			Directory.CreateDirectory (Path.Combine (Application.dataPath, fileDirectory));
		}

		// Making sure the given file-name is legit
		string path = Path.Combine (Application.dataPath, Path.Combine (fileDirectory, name));
		if (Path.GetExtension (path) == string.Empty) {
			path += ".xml";
		}
		else if (Path.GetExtension (path) != ".xml") {
			Path.ChangeExtension (path, ".xml");
		}
		
		XmlDocument doc = new XmlDocument ();
		XmlNode rootNode = doc.CreateElement ("SceneDialogs");
		doc.AppendChild (rootNode);

		XmlNode dialogNode = doc.CreateElement ("Dialog");
		XmlAttribute IDattribute = doc.CreateAttribute ("ID");
		IDattribute.Value = "0";
		dialogNode.Attributes.Append (IDattribute);
		XmlAttribute bigSprites = doc.CreateAttribute ("BigSprites");
		bigSprites.Value = "true";
		dialogNode.Attributes.Append (bigSprites);
		rootNode.AppendChild (dialogNode);

		XmlNode description = doc.CreateElement ("Description");
		description.InnerText = "DialogDescription";
		dialogNode.AppendChild (description);

		doc.Save (path);

		ReloadData ();
	}

	private static void SafeToXMLFile () {
		// Making sure the file directory exists
		if (!AssetDatabase.IsValidFolder (fileDirectory)) {
			Directory.CreateDirectory (Path.Combine (Application.dataPath, fileDirectory));
		}

		// Making sure the given file-name is legit
		string path = Path.Combine (Application.dataPath, Path.Combine (fileDirectory, selectedSceneName));
		if (Path.GetExtension (path) == string.Empty) {
			path += ".xml";
		}
		else if (Path.GetExtension (path) != ".xml") {
			Path.ChangeExtension (path, ".xml");
		}

		XML_Loader.Serialize (currentlyLoadedSceneDialogs, path);
	}

	private static void DeleteCurrentXMLFile () {
		string path = Path.Combine (Application.dataPath, Path.Combine (fileDirectory, allXmlFileNames [selectedSceneIndex]));
		File.Delete (path);
		File.Delete (path + ".meta");

		ReloadData ();
	}


	//--- Dialog Functions ---------------------------------------------------------------------------------------------------
	private static void CreateNewDialog () {
		Dialog newDialog = new Dialog (currentlyLoadedSceneDialogs.allDialogsInScene.Count);
		currentlyLoadedSceneDialogs.allDialogsInScene.Add (newDialog);
		// ZORG DAT DE NIEUWE DIALOG ID NIET EEN ANDER KAN OVERWRITEN
	}

	private static void RemoveSelectedDialog () {
		foreach (Dialog dialog in currentlyLoadedSceneDialogs.allDialogsInScene) {
			if (dialog.ID == selectedDialog.ID) {
				currentlyLoadedSceneDialogs.allDialogsInScene.Remove (dialog);
				break;
			}
		}
	}

	// Called from the Dialog-Selection-List Buttons; sets the selected Dialog.
	private static void SelectDialogByID (int id) {
		foreach (Dialog d in currentlyLoadedSceneDialogs.allDialogsInScene) {
			if (d.ID == id) {
				selectedDialog = d;

				dialogDescriptionInput = selectedDialog.dialogDescription;
				dialogIDInput = selectedDialog.ID;
			}
			gridNodes.Clear (); windows.Clear ();

			int i = 0;		// Creates the associated grid-windows for each DialogLine
			foreach (DialogLine line in selectedDialog.DialogLines) {
				Rect r = new Rect (10 + 100 * i, 70, 100, 100);
				windows.Add (r);
				gridNodes.Add (new DialogGridNode (line, r, SelectDialogLine));
				i++;
			}
		}
	}

	// Changes the selected Dialog's ID and description
	private static void AdjustDialogSettings () {
		if (dialogIDInput != selectedDialog.ID) {
			bool IDAlreadyExists = false;
			foreach (Dialog d in currentlyLoadedSceneDialogs.allDialogsInScene) {
				if (d.ID == dialogIDInput) {
					IDAlreadyExists = true;
				}
			}
			if (IDAlreadyExists) {
				Debug.Log ("ID EXISTS");
				// GEEF POPUP MET 'ID ALREADY EXISTS'.
				return;
			}
		}
		selectedDialog.ID = dialogIDInput;
		selectedDialog.dialogDescription = dialogDescriptionInput;
	}


	//--- DialogLine Functions ---------------------------------------------------------------------------------------------------
	private static void CreateNewDialogLine () {
		DialogLine newLine = new DialogLine (selectedDialog.DialogLines.Count);
		selectedDialog.DialogLines.Add (newLine);
		// ZORG DAT DE NIEUWE DIALOG ID NIET EEN ANDER KAN OVERWRITEN

		Rect r = new Rect (110, 250, 100, 100);
		windows.Add (r);
		gridNodes.Add (new DialogGridNode (newLine, r, SelectDialogLine));
	}

	private static void SelectDialogLine (int id) {
		selectedDialogLine = selectedDialog.DialogLines [id];
		
		speakerName = selectedDialogLine.speakerData;
		lineText = selectedDialogLine.text;
		responseCount = selectedDialogLine.responses.Count;
		followUpLine = selectedDialogLine.followUpLine;

		responseText.Clear ();
		responseFollowUpID.Clear ();
		foreach (Response r in selectedDialogLine.responses) {
			responseText.Add (r.text);
			responseFollowUpID.Add (r.followupLine);
		}
	}

	// Adds/Removes from the response lists to make sure their length is equal to the responseCount
	private static void AdjustResponseLists () {
		if (Mathf.Sign (responseCount - responseText.Count) == 1) {
			while (responseCount != responseText.Count) {
				responseText.Add ("");
				responseFollowUpID.Add (-1);
			}
		} else {
			while (responseCount != responseText.Count) {
				responseText.RemoveAt (responseText.Count - 1);
				responseFollowUpID.RemoveAt (responseFollowUpID.Count - 1);
			}
		}
	}

	// Sets the Line/Response objects to be equal to the given values
	private static void ApplyDialogLineChanges () {
		selectedDialogLine.text = lineText;
		selectedDialogLine.followUpLine = followUpLine;
		selectedDialogLine.speakerData = speakerName;

		if (responseCount > 0) {
			for (int i = 0; i < responseCount; i++) {
				if (i > selectedDialogLine.responses.Count -1) {
					selectedDialogLine.responses.Add (new Response (i, responseText [i], responseFollowUpID [i]));
				} else {
					selectedDialogLine.responses [i].text = responseText [i];
					selectedDialogLine.responses [i].responseID = responseFollowUpID [i];
				}
			}
		}
		if (responseCount < selectedDialogLine.responses.Count) {
			while (responseCount != responseText.Count) {
				responseText.RemoveAt (responseText.Count - 1);
				responseFollowUpID.RemoveAt (responseFollowUpID.Count - 1);
			}
		}
	}

	//--- Grid/Map Functions ---------------------------------------------------------------------------------------------------
	private static void SetBackgroundTexture () {
		gridBackground = new Texture2D (1, 1, TextureFormat.RGBA32, false);
		gridBackground.SetPixel (0, 0, new Color (0.2f, 0.2f, 0.2f, 1f));
		gridBackground.Apply ();
	}

	// Draws the Nodes and Connection lines in the grid view
	private static void FillNodeGrid () {
		startNode = GUI.Window (-1, startNode, StartEndNodes, "Start");
		endNode = GUI.Window (-2, endNode, StartEndNodes, "End");

		if (selectedDialog.DialogLines.Count == 0 || windows.Count == 0)
			return;

		// Draws the windows in the grid
		int i = 0;
		foreach (Rect r in windows) {
			windows [i] = GUI.Window (i, windows [i], DrawNodeWindow, "Line" + i);
			i++;
		}

		// Draws the lines between the windows
		DrawNodeCurve (startNode, windows [0]);
		foreach (DialogLine line in selectedDialog.DialogLines) {
			if (line.responses.Count == 0) {
				DrawNodeCurve (windows [line.lineID], (line.followUpLine != -1) ? windows [line.followUpLine] : endNode);
			} else {
				foreach (Response r in line.responses) {
					DrawNodeCurve (windows [line.lineID], (r.followupLine < 0) ? endNode : windows [r.followupLine]);
				}
			}
		}
	}

	private static void StartEndNodes (int id) {
		if (id == -1)
			GUILayout.Label ("Start", EditorStyles.boldLabel);
		else
			GUILayout.Label ("End", EditorStyles.boldLabel);
		GUI.DragWindow ();
	}

	private static void DrawNodeWindow (int id) {
		DialogLine dL = gridNodes [id].dialogLine;

		GUILayout.Label ("Line ID: " + dL.lineID, EditorStyles.boldLabel);
		GUILayout.Label (dL.text, EditorStyles.label);
		GUILayout.Label ("#responses: " + dL.responses.Count, EditorStyles.label);
		if (GUILayout.Button ("Select", GUILayout.ExpandWidth (false))) {
			SelectDialogLine (dL.lineID);
		}

		GUI.DragWindow ();
	}

	private static void DrawNodeCurve (Rect start, Rect end) {
		Vector3 startPos = new Vector3 (start.x + start.width, start.y + start.height / 2, 0);
		Vector3 endPos = new Vector3 (end.x, end.y + end.height / 2, 0);
		Vector3 startTan = startPos + Vector3.right * 50;
		Vector3 endTan = endPos + Vector3.left * 50;
		Color shadowCol = new Color (0, 0, 0, 0.06f);

		for (int i = 0; i < 3; i++) {
			Handles.DrawBezier (startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
		}

		Handles.DrawBezier (startPos, endPos, startTan, endTan, Color.black, null, 1);
	}

	private void ProcessNodeEvents (Event e) {	// Not in use atm
		if (gridNodes == null)					// Was used for dialogLine selection when clicking on windows
			return;

		for (int i = gridNodes.Count - 1; i >= 0; i--) {
			gridNodes [i].ProcessEvents (e);
			/*bool guiChanged = gridNodes [i].ProcessEvents (e);

			if (guiChanged) {
				GUI.changed = true;
			}*/
		}
	}


	//--- OnGUI ---------------------------------------------------------------------------------------------------
	private void OnGUI () {

		Debug.Log (DialogEditor.APLDPALDPALDPLADPLDPA);

		EditorGUIUtility.labelWidth = 75f;

		//--- The XML-File Manipulation Tool at the top ---------------------------------------------------------------------------------------------------
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ();
		GUILayout.Label ("Scene Settings", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal ();
		{
			// The XML-File Manipulation Tools
			if (GUILayout.Button ("Delete Current XMLFile", GUILayout.ExpandWidth (false))) {
				//GEEF DIT EEN CONFIRMATION BUTTON
				DeleteCurrentXMLFile ();
			}
			if (GUILayout.Button ("Save Current XMLFile", GUILayout.ExpandWidth (false))) {
				SafeToXMLFile ();
			}
			if (GUILayout.Button ("Create New XMLFile", GUILayout.ExpandWidth (false))) {
				if (createNewFileName != "") {
					//DOE HEIR EEN CHECK OF DAT HET BESTAND AL BESTAAT
					CreateNewXMLFile (createNewFileName);
				} else {
					//GEEF HIER 'NIET GELUKT' FEEDBACK POPUP
				}
			}
			createNewFileName = EditorGUILayout.TextField ("File Name: ", createNewFileName, GUILayout.Width (300));
		}
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		{
			//--- The XML-File selection Tool ---------------------------------------------------------------------------------------------------
			GUILayout.Label ("Select XML File", GUILayout.Width (100));
			if (allXmlFileNames.Count == 0) {
				GUILayout.Label ("Create an xml file above", EditorStyles.label);
			} else {
				int previouslySelected = selectedSceneIndex;
				selectedSceneIndex = EditorGUILayout.Popup (selectedSceneIndex, allXmlFileNames.ToArray (), GUILayout.Width (200));
				if (previouslySelected != selectedSceneIndex) {
					selectedSceneName = allXmlFileNames [selectedSceneIndex];
					LoadXmlFileData (allXmlFileNames [selectedSceneIndex]);
				}
				GUILayout.Label (selectedSceneName, EditorStyles.label, GUILayout.Width (150));
				GUILayout.Label ("Index: " + selectedSceneIndex.ToString (), EditorStyles.label, GUILayout.Width (75));
				GUILayout.Label ("Number of Dialogs: " + currentlyLoadedSceneDialogs.allDialogsInScene.Count, EditorStyles.label, GUILayout.Width (150));
			}
		}
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
		EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
		EditorGUILayout.Space ();

		if (allXmlFileNames.Count == 0)
			return;

		// Main Content Area
		GUILayout.BeginHorizontal ();
		{
			//--- Left Area, containing the Scene's Dialog Tree ---------------------------------------------------------------------------------------------------
			GUILayout.BeginVertical ();
			{
				//--- The buttons at the top ---
				GUILayout.Label ("Dialog Options", EditorStyles.boldLabel, GUILayout.Width (300));
				if (GUILayout.Button ("Create New Dialog", GUILayout.ExpandWidth (false))) {
					CreateNewDialog ();
				}
				if (GUILayout.Button ("Remove Selected Dialog", GUILayout.ExpandWidth (false))) {
					//GEEF DIT EEN CONFIRMATION BUTTON
					RemoveSelectedDialog ();
				}
				EditorGUILayout.Space ();

				//--- The Dialog Button List ---
				GUILayout.Label ("Dialogs in Scene", EditorStyles.boldLabel, GUILayout.Width (300));
				if (currentlyLoadedSceneDialogs != null) {
					int dialogTreeViewLength = currentlyLoadedSceneDialogs.allDialogsInScene.Count * 25;
					scrollPosition = GUI.BeginScrollView (new Rect (0, 100, 325, 1000), scrollPosition, new Rect (0, 95, 325, dialogTreeViewLength));
					{
						Color c = GUI.backgroundColor;
						foreach (Dialog dialog in currentlyLoadedSceneDialogs.allDialogsInScene) {
							if (dialog == selectedDialog) {
								GUI.backgroundColor = Color.grey;
							}
							if (GUILayout.Button (dialog.ID + ": " + dialog.dialogDescription, GUILayout.ExpandWidth (false), GUILayout.Width (300))) {
								SelectDialogByID (dialog.ID);
							}
							GUI.backgroundColor = c;
						}
					}
					GUI.EndScrollView ();
				}
			}
			GUILayout.EndVertical ();

			if (selectedDialog == null)
				return;
			
			//--- Middle Area, containing the Dialog Settings and Dialog Map ---------------------------------------------------------------------------------------------------
			GUILayout.BeginVertical (GUILayout.Width (1000));
			{
				//--- Buttons to adjust the Selected Dialog ---
				GUILayout.Label ("DialogSettings", EditorStyles.boldLabel);
				GUILayout.BeginHorizontal ();
				{
					dialogIDInput = EditorGUILayout.IntField ("ID:", dialogIDInput, GUILayout.Width (100));
					dialogDescriptionInput = EditorGUILayout.TextField ("Description:", dialogDescriptionInput, GUILayout.ExpandWidth (false), GUILayout.Width (350));
					if (GUILayout.Button ("Update Dialog", GUILayout.ExpandWidth (false), GUILayout.Width (200))) {
						AdjustDialogSettings ();
					}
				}
				GUILayout.EndHorizontal ();
				if (GUILayout.Button ("Create New Dialog Line", GUILayout.ExpandWidth (false), GUILayout.Width (300))) {
					CreateNewDialogLine ();
				}
				EditorGUILayout.Space ();


				//--- Grid to represent all the DialogLines in the selected Dialog ---
				if (gridBackground == null)
					SetBackgroundTexture ();

				//Pas de volgende scroll view groote aan op aantal dialoog dingen
				gridScrollPosition = GUI.BeginScrollView (new Rect (325, 175, position.width - 800, position.height - 175), gridScrollPosition, new Rect (0, 0, 300, 300), true, true);
				GUI.DrawTexture (new Rect (0, 0, position.width - 800, position.height - 150), gridBackground, ScaleMode.StretchToFill);

				BeginWindows ();
				FillNodeGrid ();	// Function that draws the node windows and lines.
				EndWindows ();

				GUI.EndScrollView ();
			}
			GUILayout.EndVertical ();

			if (selectedDialogLine == null)
				return;

			//--- The Right Area, containing the DialogLine Inspector ---------------------------------------------------------------------------------------------------
			GUILayout.BeginVertical ();
			{
				GUILayout.Label ("Dialog Line Inspector", EditorStyles.boldLabel);

				GUILayout.Label ("Line ID: " + selectedDialogLine.lineID, EditorStyles.label);
				speakerName = EditorGUILayout.TextField ("Speaker Name: ", speakerName, GUILayout.Width (300));
				lineText = EditorGUILayout.TextField ("Line Text: ", lineText);
				responseCount = EditorGUILayout.IntField ("Response Count: ", responseCount, GUILayout.Width (100));
				GUILayout.Label ("Current Response Count: " + selectedDialogLine.responses.Count, EditorStyles.label);

				if (responseCount != responseText.Count)
					AdjustResponseLists ();

				if (responseCount > 0) {
					EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);

					for (int i = 0; i < responseCount; i++) {
						GUILayout.Label ("Response ID: " + i, EditorStyles.label);

						responseText[i] = EditorGUILayout.TextField ("Response Text: ", responseText[i]);
						responseFollowUpID[i] = EditorGUILayout.IntField ("Followup ID: ", responseFollowUpID[i], GUILayout.Width (100));

						EditorGUILayout.Space ();
					}
				}

				if (GUILayout.Button ("Apply", GUILayout.ExpandWidth (false))) {
					ApplyDialogLineChanges ();
				}

			}
			GUILayout.EndVertical ();
		}
		GUILayout.EndHorizontal ();
	}

}
