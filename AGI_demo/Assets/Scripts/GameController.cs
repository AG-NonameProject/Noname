using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	public GameObject pokeball; 

	// Use this for initialization
	void Start () {
		createBall ();
	}
	
	// Update is called once per frame
	void Update () {
		// Create new ball
		if (Input.GetKeyDown(KeyCode.Return)) {
			createBall();
		}
	}

	private void createBall() {
		GameObject newBall = Instantiate (pokeball);
		newBall.SetActive (true);
	}
}
