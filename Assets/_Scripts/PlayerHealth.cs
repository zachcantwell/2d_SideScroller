using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {

	[SerializeField]
	private Vector2 _damageForce; 
	public static bool _WasPlayerHurt
	{
		get;  set; 
	}
	public static bool _WasPlayerKilled
	{
		get;  set; 
	}
	public static int _playerHealth
	{
		get; private set; 
	}
	private int _enemyLayer; 
	private float _ignoreDamageTimer = 0f; 
	private const float _ignoreDamageTimerOffset = 1f; 

	void Start () 
	{
		_enemyLayer = LayerMask.NameToLayer("Enemy");

		_playerHealth = 3;
		_ignoreDamageTimer = 0f; 

		_WasPlayerHurt = false;
		_WasPlayerKilled = false;  
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		int layer = other.GetComponent<Collider2D>().gameObject.layer;
		TakeDamage(layer, other);
	}

	void OnTriggerStay2D(Collider2D other)
	{
		int layer = other.GetComponent<Collider2D>().gameObject.layer;
		TakeDamage(layer, other);
	}

	void OnTriggerExit2D(Collider2D other)
	{
		int layer = other.GetComponent<Collider2D>().gameObject.layer;
		TakeDamage(layer, other);
	}

	private void TakeDamage(int layer, Collider2D other)
	{
		if(layer == _enemyLayer && Time.timeSinceLevelLoad > _ignoreDamageTimer)
		{
			_playerHealth--;
			Debug.LogWarning("Player hurt. Health = " + _playerHealth);
			if(_playerHealth != 0)
			{
				ApplyDamageForce(other);
				_ignoreDamageTimer = Time.timeSinceLevelLoad + _ignoreDamageTimerOffset; 
				_WasPlayerHurt = true; 
			}
			else
			{
				_WasPlayerKilled = true;
			}
		}
	}

	private void ApplyDamageForce(Collider2D other)
	{
		if(transform.localPosition.x < other.transform.localPosition.x)
		{
			GetComponent<Rigidbody2D>().velocity = new Vector2(-1f * _damageForce.x, 1f * _damageForce.y); 
																				
		}
		else
		{
			GetComponent<Rigidbody2D>().velocity = new Vector2(1f * _damageForce.x, 1f * _damageForce.y);  
		}
	}
}
