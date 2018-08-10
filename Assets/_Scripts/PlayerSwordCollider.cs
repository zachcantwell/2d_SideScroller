using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordCollider : MonoBehaviour {

	private int _enemyLayer; 
	// Use this for initialization
	void Start () {
		_enemyLayer = LayerMask.NameToLayer("Enemy");
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		GameObject enemy = other.GetComponent<Collider2D>().gameObject;
		if(enemy.layer == _enemyLayer)
		{
			DealDamageToEnemy(enemy);
		}
	}

	void DealDamageToEnemy(GameObject enemy)
	{

	}
}
