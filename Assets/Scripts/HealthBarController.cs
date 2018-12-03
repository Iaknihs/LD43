using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour {

	private Slider healthBar;
	private GameObject entity;

	// Use this for initialization
	void Start () {
		healthBar = GetComponent<Slider> ();
		entity = gameObject.transform.parent.parent.gameObject;
		healthBar.maxValue = entity.GetComponent<EnemyCombat> ().maxHP;
	}
	
	// Update is called once per frame
	void Update () {
		healthBar.value = entity.GetComponent<EnemyCombat> ().HP;
	}
}
