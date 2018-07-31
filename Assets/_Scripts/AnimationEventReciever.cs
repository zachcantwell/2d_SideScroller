using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReciever : MonoBehaviour {

	public static bool _isItTheLastFrameOfJumping;
	public static bool _isItTheLastFrameOfSwinging; 
	public static bool _isItTheLastFrameOfGroundSliding;

	private SpriteRenderer _playerSprite; 
	private Transform _swordHolder;
	private PlayerInput _playerInput; 
	private PlayerData _playerData; 

	// Use this for initialization
	void Start () {
		_isItTheLastFrameOfJumping = false; 
		_isItTheLastFrameOfGroundSliding = false; 
		_isItTheLastFrameOfSwinging = false; 

		if(_playerSprite == null)
		{
			_playerSprite = GetComponent<SpriteRenderer>(); 
		}
		_swordHolder = transform.Find("SwordHolder");
		_playerInput = GetComponentInParent<PlayerInput>();
		_playerData = GetComponentInParent<PlayerData>();
	}
	
	public void LastFrameOfJumpingReached()
	{
		_isItTheLastFrameOfJumping = true;
	}

	public void LastFrameOfSwingingReached()
	{
		_isItTheLastFrameOfSwinging = true;
	}

	public void LastFrameOfGroundSliding()
	{
		_isItTheLastFrameOfGroundSliding = true;
	}

	public void FirstFrameOfGroundSliding()
	{
		_isItTheLastFrameOfGroundSliding = false;
	}

	public void CheckDirectionOfSwordSprites()
	{
		if(_playerSprite.flipX == true)
		{
			_swordHolder.localScale = new Vector3(-1,1,1);
		}
		else if(_playerSprite.flipX == false)
		{
			_swordHolder.localScale = new Vector3(1,1,1);
		}
	}

	public void StopDrawingTheSword()
	{
		_playerInput._IsPlayerSwordDrawn = false;
	}

	public void IsPlayerInAirWhileDrawingSword()
	{
		Debug.LogWarning(Mathf.Abs(_playerData._RigidBody.velocity.y) + "= yVelocity");

		if(Mathf.Abs(_playerData._RigidBody.velocity.y) > 0f)
		{
			StopDrawingTheSword();
		}
	}
}
