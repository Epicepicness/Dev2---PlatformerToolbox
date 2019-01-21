using System;
using UnityEngine;

public class DialogGridNode {

	public Action<int> OnSelected;
	public DialogLine dialogLine;
	public Rect windowRect;
	public bool isSelected;

	public DialogGridNode (DialogLine line, Rect window, Action<int> selectAction) {
		dialogLine = line;
		windowRect = window;
		OnSelected = selectAction;
	}

	// Currently not used; was used for node/line selection
	public bool ProcessEvents (Event e) {
		if (e.type == EventType.MouseDown) {
			if (windowRect.Contains (e.mousePosition)) {
				isSelected = true;
				OnSelected (dialogLine.lineID);
			}
		}
		return true;
	}

}