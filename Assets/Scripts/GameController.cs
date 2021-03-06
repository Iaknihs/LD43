using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameController : MonoBehaviour {

	public string loadScene;

	public GameObject[] party = new GameObject[3];
	private int activePlayer = 0;
	private bool initiated = false;
	private bool inBattle = false;

	private GameObject UI;

	public Battle battle = null;
	private Vector3 preBattleLocation;
	private GameObject initiator;

	enum Characters {
		steph,
		jeff,
		treav
	}

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (this.gameObject);

		// If starting scene isn't loaded yet, load it. This should avoid differences between edit mode in Unity, and finalised play-mode.
		bool sceneLoaded = false;
		for (int i = 0; i < SceneManager.sceneCount; i++){
			if (SceneManager.GetSceneAt (i).name == loadScene) {
				sceneLoaded = true;
			}
		}
		if (!sceneLoaded) 
			SceneManager.LoadScene (loadScene, LoadSceneMode.Additive);
	}
	
	// Update is called once per frame
	void Update () {
		// initiate player if none has been set. This should work once a first player is spawned in.
		if (!initiated) {
			if (party [0] != null) {
				party [0].SetActive (true);
				initiated = true;
				UI = GameObject.Find ("GUI");
			}
		} else {
			if (!inBattle) {
				if (Input.GetKeyDown (KeyCode.Tab)) {
					Vector3 position = party [activePlayer].transform.position;
					party [activePlayer].SetActive (false);
					while (true) {
						activePlayer = (activePlayer + 1) % party.Length;
						if (party [activePlayer] != null) {
							party [activePlayer].SetActive (true);
							party [activePlayer].transform.position = position;
							Debug.Log (party [activePlayer].name);
							break;
						}
					}
				}
			} else {
				if (battle.Update () /*if battle is over*/) {
					battle.arena.SetActive (false);
					battle = null;
					inBattle = false;

					party [activePlayer].SetActive (true);
					foreach (GameObject c in party) {
						c.GetComponent<PlayerController> ().combatVerion.SetActive (false);
					}
					initiated = false;

					Destroy (initiator);
				}
				// any possible actions in battle?
			}

			// Update GUI
			if (UI != null) {
				for (int i = 1; i <= 3; i++) {
					Transform hpph = UI.transform.FindChild ("HP_Placeholder" + i.ToString ());
					Transform bg = hpph.FindChild ("background");

					if (party [mod(activePlayer-i, 3)].GetComponent<PlayerController> ().HP < 0){
						party [mod (activePlayer - i, 3)].GetComponent<PlayerController> ().HP = 0;
					}
					bg.FindChild ("HP").gameObject.GetComponent<UnityEngine.UI.Text> ().text = party [mod(activePlayer-i, 3)].GetComponent<PlayerController> ().HP.ToString ();
					bg.FindChild ("MAXHP").gameObject.GetComponent<UnityEngine.UI.Text> ().text = party [mod(activePlayer-i, 3)].GetComponent<PlayerController> ().maxHP.ToString ();
					bg.FindChild ("HEAD").GetComponent<UnityEngine.UI.Image> ().sprite = party [mod(activePlayer - i, 3)].GetComponent<PlayerController> ().sprite;
				}
			}
		}
	}

	public void AddPartyMember(GameObject go){
		for (int i = 0; i < this.party.Length; i++) {
			if (this.party [i] == null) {
				this.party [i] = go;
				break;
			}
		}
	}

	public void enterBattle(GameObject enemy){
		string primaryTag = enemy.tag;
		inBattle = true;
		preBattleLocation = this.party [this.activePlayer].transform.position;
		initiator = enemy;
		this.party [this.activePlayer].SetActive (false);
		GameObject arena = GameObject.Find ("ArenaPlaceholder").transform.FindChild ("Arena").gameObject;
		arena.SetActive (true);

		this.battle = new Battle (this.party, this.activePlayer, enemy, arena, UI);
	}

	public static int mod(int x, int m) {
		int r = x % m;
		return r<0 ? r+m : r;
	}

	public class Battle{

		enum AttackState{
			selection,
			enemyselection,
			animation,
			damage
		}
		private int animationDuration = 60;
		private int animationTimer = 0;

		public GameObject[] party;
		public int activePlayer;
		public GameObject[] partyConfig;
		public GameObject[] enemiesConfig;

		private GameObject UI;

		private int turn = 0;
		private bool firstturn = true;
		private int selected = 0;
		private int selectedEnemy = 0;
		private AttackState atkstate = AttackState.selection;

		public GameObject arena;

		public GameObject initiator;

		private bool uiinited = false;
		private Transform hpph;
		private Transform atk1;
		private Transform atk2;

		public Battle(GameObject[] party, int activePlayer, GameObject initiator, GameObject arena, GameObject UI){
			this.initiator = initiator;
			this.arena = arena;
			this.UI = UI;
			this.party = party;
			this.activePlayer = activePlayer;

			this.partyConfig = new GameObject[3];
			if (party[activePlayer].GetComponent<PlayerController>().HP > 0){
				this.partyConfig[0] = party[activePlayer].GetComponent<PlayerController>().combatVerion;
				this.partyConfig[0].transform.position = arena.transform.FindChild("pos1").position;
				this.partyConfig[0].SetActive(true);
			}
			if (party[mod(activePlayer+1, 3)].GetComponent<PlayerController>().HP > 0){
				this.partyConfig[1] = party[mod(activePlayer+1, 3)].GetComponent<PlayerController>().combatVerion;
				this.partyConfig[1].transform.position = arena.transform.FindChild("pos3").position;
				this.partyConfig[1].SetActive(true);
			}
			if (party[mod(activePlayer+2, 3)].GetComponent<PlayerController>().HP > 0){
				this.partyConfig[2] = party[mod(activePlayer+2, 3)].GetComponent<PlayerController>().combatVerion;
				this.partyConfig[2].transform.position = arena.transform.FindChild("pos2").position;
				this.partyConfig[2].SetActive(true);
			}

			enemiesConfig = new GameObject[UnityEngine.Random.Range(1,5)];
			for(int i = 0; i < enemiesConfig.Length; i++){
				GameObject[] cs = initiator.GetComponent<EnemyController>().canSpawn;
				Vector3 pos = arena.transform.FindChild("pos"+(i+partyConfig.Length+1).ToString()).position;
				enemiesConfig[i] = Instantiate(cs[UnityEngine.Random.Range(0,cs.Length)], pos, Quaternion.identity);
			}
		}

		public bool Update(){
			bool allnull = true;
			foreach (GameObject p in partyConfig){
				if (p != null)
					allnull = false;
			}
			if (allnull) {
				
				SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
				return true;
			}
			allnull = true;
			foreach (GameObject e in enemiesConfig) {
				if (e != null)
					allnull = false;
			}
			if (allnull)
				return true;

			if (turn < partyConfig.Length) {
				if (partyConfig [turn] == null) {
					EndTurn ();
					return false;
				}

				// player interaction
				if (!uiinited) {
					hpph = UI.transform.FindChild ("HP_Placeholder" + (3 - turn).ToString ());
					atk1 = hpph.FindChild ("Attack1");
					atk2 = hpph.FindChild ("Attack2");
					atk1.gameObject.SetActive (true);
					atk2.gameObject.SetActive (true);
					PlayerCombatController scr = partyConfig [turn].GetComponent<PlayerCombatController> ();
					atk1.FindChild("Text").GetComponent<UnityEngine.UI.Text> ().text = scr.attackNames [0];
					atk2.FindChild("Text").GetComponent<UnityEngine.UI.Text> ().text = scr.attackNames [1];
					atk1.FindChild ("Text2").GetComponent<UnityEngine.UI.Text> ().text = scr.attackDMGs [0].ToString ();
					atk2.FindChild ("Text2").GetComponent<UnityEngine.UI.Text> ().text = scr.attackDMGs [1].ToString ();
				}


				switch (atkstate) {
				case AttackState.selection:

					selected = mod(selected + ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A)) ? 1 : 0) - ((Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown(KeyCode.D)) ? 1 : 0), 2);

					if (selected == 0) {
						atk1.gameObject.GetComponent<UnityEngine.UI.Image> ().color = Color.white;
						atk2.gameObject.GetComponent<UnityEngine.UI.Image> ().color = Color.grey;
					} else {
						atk1.gameObject.GetComponent<UnityEngine.UI.Image> ().color = Color.grey;
						atk2.gameObject.GetComponent<UnityEngine.UI.Image> ().color = Color.white;
					}

					UI.transform.FindChild ("Text_Placeholder").GetComponentInChildren<UnityEngine.UI.Text> ().text = 
						partyConfig [turn].GetComponent<PlayerCombatController> ().goodAttackDescriptions [selected];
					// select attack
					if(Input.GetKeyDown(KeyCode.E)){
						atkstate = AttackState.enemyselection;
						atk1.gameObject.GetComponent<UnityEngine.UI.Image> ().color = Color.white;
						atk2.gameObject.GetComponent<UnityEngine.UI.Image> ().color = Color.white;
					}
					break;
				case AttackState.enemyselection:
					int keySD = (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.D)) ? 1 : 0;
					int keyWA = (Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.W)) ? 1 : 0;
					if (keySD == 1 || keyWA == 1) {
						selectedEnemy = mod (selectedEnemy + keySD - keyWA, enemiesConfig.Length);
						int selstart = selectedEnemy;
						while (enemiesConfig [selectedEnemy] == null) {
							selectedEnemy = mod (selectedEnemy + keySD - keyWA, enemiesConfig.Length);
							if (selstart == selectedEnemy) {
								throw new InvalidOperationException ("endless loop. all enemy objects null");
								break;
							}
						}
					} else {
						int selstart = selectedEnemy;
						while (enemiesConfig [selectedEnemy] == null) {
							selectedEnemy = mod (selectedEnemy + 1, enemiesConfig.Length);
							if (selstart == selectedEnemy) {
								throw new InvalidOperationException ("endless loop. all enemy objects null");
								break;
							}
						}
					}

					for (int i = 0; i < enemiesConfig.Length; i++) {
						if (enemiesConfig [i] == null)
							continue;
						if (i == selectedEnemy) {
							enemiesConfig [i].GetComponent<UnityEngine.SpriteRenderer> ().color = Color.white;
						} else {
							enemiesConfig [i].GetComponent<UnityEngine.SpriteRenderer> ().color = Color.grey;
						}
					}

					UI.transform.FindChild ("Text_Placeholder").GetComponentInChildren<UnityEngine.UI.Text> ().text = 
						enemiesConfig [selectedEnemy].GetComponent<EnemyCombat> ().goodDescription;
					// select enemy
					if (Input.GetKeyDown (KeyCode.E)) {
						atkstate = AttackState.animation;
						partyConfig [turn].GetComponent<Animator> ().SetInteger ("animation", 2);
					}
					break;
				case AttackState.animation:
					// play animation

					animationTimer++;
					if (animationTimer >= animationDuration) {
						atkstate = AttackState.damage;
						animationTimer = 0;
						partyConfig[turn].GetComponent<Animator>().SetInteger("animation",0);
					}
					break;
				case AttackState.damage:
					// return to idle animation and deal damage
					int atktype = partyConfig [turn].GetComponent<PlayerCombatController> ().types [selected];
					switch (atktype) {
					case -1: 
						// analysation
						break;
					case 0:
						// normal aimed attack
						enemiesConfig [selectedEnemy].GetComponent<EnemyCombat> ().HP -= partyConfig [turn].GetComponent<PlayerCombatController> ().attackDMGs [selected];
						break;
					case 1:
						// spray attack, hit 3 enemies
						for (int i = selectedEnemy - 1; i < selectedEnemy + 3; i++) {
							try {
								enemiesConfig [i].GetComponent<EnemyCombat> ().HP -= partyConfig [turn].GetComponent<PlayerCombatController> ().attackDMGs [selected];
							} catch (Exception e) {
								// no damage~
							}
						}
						break;
					case 2:
						{
							// risky attack, hit 0-3 enemies
							int hit = UnityEngine.Random.Range (0, Mathf.Min (enemiesConfig.Length + 1, 4));
							for (int i = 0; i < hit; i++) {
								if (enemiesConfig [(i + selectedEnemy) % enemiesConfig.Length] == null)
									continue;
								enemiesConfig [(i + selectedEnemy) % enemiesConfig.Length].GetComponent<EnemyCombat> ().HP -= partyConfig [turn].GetComponent<PlayerCombatController> ().attackDMGs [selected];
							}
							break;
						}
					case 3:
						{
							//even riskier attack, hit 0-3 enemies, take half as much dmg as you deal.
							int hit = UnityEngine.Random.Range (0, Mathf.Min (enemiesConfig.Length + 1, 4));
							for (int i = 0; i < hit; i++) {
								enemiesConfig [(i + selectedEnemy) % enemiesConfig.Length].GetComponent<EnemyCombat> ().HP -= partyConfig [turn].GetComponent<PlayerCombatController> ().attackDMGs [selected];
								party [turn].GetComponent<PlayerController> ().HP -= Mathf.Abs (partyConfig [turn].GetComponent<PlayerCombatController> ().attackDMGs [selected] / 2);
							}
							break;
						}
					}



					if (firstturn) {
						// code for first attack
						firstturn = false;
						atkstate = AttackState.selection;
					} else {
						EndTurn ();
						atk1.gameObject.SetActive (false);
						atk2.gameObject.SetActive (false);
						uiinited = false;
						atkstate = AttackState.selection;
					}
					break;
				}
					
				if(Input.GetKeyDown(KeyCode.F10)){
					return true;
				}
			} else {
				// do enemy attack
				GameObject enm = enemiesConfig[turn - partyConfig.Length];
				if (enm == null) {
					EndTurn ();
					return false;
				}


				switch (atkstate) {
				case AttackState.selection:
					// prepare attack
					for (int i = 0; i < enemiesConfig.Length; i++) {
						if (enemiesConfig [i] == null)
							continue;
						enemiesConfig [i].GetComponent<UnityEngine.SpriteRenderer> ().color = Color.white;
					}
					atkstate = AttackState.animation;
					enemiesConfig[turn-partyConfig.Length].GetComponent<Animator>().SetInteger("animation",2);
					break;
				case AttackState.animation:
					// play animation

					animationTimer++;
					if (animationTimer >= animationDuration) {
						atkstate = AttackState.damage;
						animationTimer = 0;
						enemiesConfig[turn-partyConfig.Length].GetComponent<Animator>().SetInteger("animation",0);
					}
					break;
				case AttackState.damage:
					// return to idle animation and deal damage to RNG player
					int spray = UnityEngine.Random.Range(0, 2);
					if (spray == 0) {
						int pl = UnityEngine.Random.Range (0, partyConfig.Length);
						party [(pl + activePlayer) % 3].GetComponent<PlayerController> ().HP -= 5;
					} else {
						foreach (GameObject entity in party) {
							entity.GetComponent<PlayerController> ().HP -= UnityEngine.Random.Range (0, 5);
						}
					}

					EndTurn();
					atkstate = AttackState.selection;
					break;
				}

			}

			return false;
		}

		private void EndTurn(){

			// remove dead entities
			for (int i = 0; i < party.Length; i++) {
				if (partyConfig [i] == null)
					continue;
				if(party[i].GetComponent<PlayerController>().HP <= 0){
					this.partyConfig[mod(activePlayer+i, 3)].SetActive(false);
					this.partyConfig[mod(activePlayer+i, 3)] = null;
				}
			}
			for (int i = 0; i < enemiesConfig.Length; i++) {
				if (enemiesConfig [i] == null)
					continue;
				if (enemiesConfig [i].GetComponent<EnemyCombat> ().HP <= 0) {
					Destroy(enemiesConfig[i]);
					this.enemiesConfig [i] = null;
				}
			}

			int turnstart = turn;
			while (true) {
				this.turn = (this.turn + 1) % (partyConfig.Length + enemiesConfig.Length);
				if (this.turn < partyConfig.Length) {
					if (partyConfig [turn] != null) {
						break;
					}
				} else {
					if (enemiesConfig [turn - partyConfig.Length] != null) {
						break;
					}
				}
				if (turnstart == turn) {
					throw new System.InvalidOperationException ("No players or enemies left on the battlefield. ");
				}
			}
		}
	}
}
