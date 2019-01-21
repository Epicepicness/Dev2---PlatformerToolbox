using UnityEditor;
using System.Collections.Generic;

public class TileListWizard : ScriptableWizard {

	public List<ColorTiles> tilePerColour = new List<ColorTiles> ();

	public static void CreateWizard () {

	}

	private void OnEnable () {
		tilePerColour = EditorWindow.GetWindow<LevelSpawner> ().TilePerColour;
	}

	private void OnWizardCreate () {
		EditorWindow.GetWindow<LevelSpawner> ().TilePerColour = tilePerColour;
	}

	private void OnWizardUpdate () {
		helpString = "Selecteer je tiles";
	}

	private void OnWizardOtherButton () {
		EditorWindow.GetWindow<LevelSpawner> ().TilePerColour = tilePerColour;
	}

}
