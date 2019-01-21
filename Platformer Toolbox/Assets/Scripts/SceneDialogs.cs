using System.Collections.Generic;   //Needed for Lists
using System.Xml;                   //Needed for XML functionality
using System.Xml.Serialization;     //Needed for XML Functionality

[XmlRoot ("SceneDialogs")]
public class SceneDialogs {
	[XmlElement ("Dialog")]
	public List<Dialog> allDialogsInScene { get; set; }
}

[XmlType ("Dialog")]
[System.Serializable]
public class Dialog {
	public Dialog () { }

	public Dialog (int id) {
		ID = id;
		BigSprites = false;
		dialogDescription = "New Dialog";
		DialogLines = new List<DialogLine> ();
	}

	public Dialog (int id, bool bigSprites, string description) {
		ID = id;
		BigSprites = false;
		dialogDescription = "New Dialog";
		DialogLines = new List<DialogLine> ();
	}

	[XmlAttribute ("ID")]
	public int ID { get; set; }
	[XmlAttribute ("BigSprites")]
	public bool BigSprites { get; set; }    // To be implemented
	[XmlElement ("Description")]
	public string dialogDescription { get; set; }

	[XmlElement ("DialogLine")]
	public List<DialogLine> DialogLines { get; set; }
}

[XmlType ("DialogLine")]
[System.Serializable]
public class DialogLine {
	public DialogLine () { }

	public DialogLine (int id) {
		lineID = id;
		followUpLine = -1;
		responses = new List<Response> ();
	}

	[XmlAttribute ("lineID")]
	public int lineID { get; set; }
	[XmlElement ("SpeakerData")]
	public string speakerData { get; set; }
	[XmlElement ("DialogText")]
	public string text { get; set; }
	[XmlElement ("FollowUpLine")]
	public int followUpLine;

	[XmlElement ("Response")]
	public List<Response> responses { get; set; }
}

[XmlType ("Response")]
[System.Serializable]
public class Response {
	public Response () { }

	public Response (int id, string t, int followup) {
		responseID = id;
		text = t;
		followupLine = followup;
	}

	[XmlAttribute ("responseID")]
	public int responseID { get; set; }
	[XmlElement ("ResponseText")]
	public string text { get; set; }
	[XmlElement ("FollowUpLine")]
	public int followupLine { get; set; }
}
