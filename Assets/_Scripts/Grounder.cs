using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounder : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		Physics2D.queriesStartInColliders = false; 
		if(other.collider.gameObject.tag == "Ground" || other.collider.gameObject.tag == "Corner" || other.collider.gameObject.tag == "Grabbable")
		{
			Debug.LogWarning(other.gameObject.name + " colliding with grounder");
		}
		else
		{
			Debug.LogWarning(other);
		}
	}

	void OnCollisionStay2D(Collision2D other)
	{
		Debug.LogWarning(other);
	}

}
