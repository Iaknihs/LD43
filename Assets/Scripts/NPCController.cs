using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour {

	public GameObject gameObj;
	public string[] texts;
	public int textid;
	public bool interacted;

	private GameObject textBox = null;

	// Use this for initialization
	void Start () {
		textid = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if (interacted) {
			interact ();
		}
	}


	void interact(){
		Debug.Log ("superInteracted!");
		if (textBox != null) {
			Destroy (textBox);
			textBox = null;
		}
		textBox = Instantiate (gameObj, transform.position+new Vector3(0,1.5f,0), Quaternion.identity);
		textBox.GetComponentInChildren<UnityEngine.UI.Text> ().text = texts [textid];
		if (textid+1 < texts.Length) {
			textid++;
		}
		interacted = false;
	}
}
