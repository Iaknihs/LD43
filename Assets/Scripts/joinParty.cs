using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class joinParty : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject controller = GameObject.Find ("GameController");
		controller.GetComponent<GameController> ().AddPartyMember (gameObject);
		gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
