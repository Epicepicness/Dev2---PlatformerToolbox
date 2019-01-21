using UnityEngine;

public class DialogStarter : MonoBehaviour {

	[SerializeField] private int dialogID;

	public void OnCollisionEnter2D (Collision2D c) {
		Debug.Log ("Start Dialog");
		UIManager.instance.BeginDialog (dialogID);
	}

	private void OnDrawGizmos () {
		Vector3 gizmoPos = this.transform.position + new Vector3 (0, this.transform.localScale.y + 0, 0);
		Gizmos.DrawIcon (gizmoPos, "talkGizmo");
	}

}
