using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	public GameObject pokeball; 

	// Use this for initialization
	void Start () {
		GameObject newBall = Instantiate (pokeball);
		newBall.SetActive (true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
