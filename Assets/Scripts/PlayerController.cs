using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public int maxHP = 20;
	public int HP = 20;
	public Sprite sprite;
	public GameObject combatVerion;

	public float spd_hori;
	public float spd_vert;
	public Animator animator;
	public float interactDist;

	private Rigidbody rb;
	private SpriteRenderer spr;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		spr = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		if (moveHorizontal > 0) {
			spr.flipX = false;
		} else if (moveHorizontal < 0) {
			spr.flipX = true;
		}

		if (Input.GetKeyDown(KeyCode.E)) {
			Debug.Log ("key pressed!");
			Transform tMin = null;
			float minDist = Mathf.Infinity;
			Vector3 curPos = transform.position;
			foreach (GameObject go in GameObject.FindGameObjectsWithTag("npc")) {
				Transform t = go.transform;
				float dist = Vector3.Distance (t.position, curPos);
				if (dist < minDist) {
					tMin = t;
					minDist = dist;
				}
			}
			Debug.Log (minDist);
			if (minDist < interactDist) {
				tMin.GetComponent<NPCController> ().interacted = true;
				Debug.Log ("interacted!");
			}
		}
	}

	void FixedUpdate(){
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		float moveVertical = Input.GetAxisRaw ("Vertical");

		if (moveHorizontal != 0.0f || moveVertical != 0.0f) {
			animator.SetInteger ("animation", 1);
		} else {
			animator.SetInteger ("animation", 0);
		}

		Vector3 movement = new Vector3 (moveHorizontal * spd_hori, rb.velocity.y, moveVertical * spd_vert);

		rb.velocity = movement;
	}

	private void OnTriggerEnter(Collider other){
		Debug.Log (other.name);
		if (other.gameObject.tag == "Enemy") {
			GameObject.Find ("GameController").GetComponent<GameController> ().enterBattle (other.gameObject);
		}
	}
}
