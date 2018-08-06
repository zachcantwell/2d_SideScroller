﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class AnimationEventReciever : MonoBehaviour {

	public static bool _isItTheLastFrameOfJumping;
	public static bool _isItTheLastFrameOfSwinging; 
	public static bool _isItTheLastFrameOfGroundSliding;
	public static bool _wasFinalAirAttackTriggererd; 
	public static bool _wasAirAttackJustPerformed; 
	public static bool _isItTheLastFrameOfDodging; 
	public static bool _isItTheFirstFrameOfDodging; 
	public static int _attackCounter; 
	public static int _aerialAttackCounter;

	private SpriteRenderer _playerSprite; 
	private Transform _swordHolder;
	private PlayerInput _playerInput; 
	private PlayerData _playerData; 
	private PlayerAnimations _playerAnimations; 
	

	// Use this for initialization
	void Start () {
		_isItTheLastFrameOfJumping = false; 
		_isItTheLastFrameOfGroundSliding = false; 
		_isItTheLastFrameOfSwinging = false; 
		_wasFinalAirAttackTriggererd = false; 
		_wasAirAttackJustPerformed = false; 
		_isItTheLastFrameOfDodging = false; 
		_isItTheFirstFrameOfDodging = false; 
		_attackCounter = 0;
		_aerialAttackCounter = 0;

		if(_playerSprite == null)
		{
			_playerSprite = GetComponent<SpriteRenderer>(); 
		}
		_swordHolder = transform.Find("SwordHolder");
		_playerInput = GetComponentInParent<PlayerInput>();
		_playerData = GetComponentInParent<PlayerData>();
		_playerAnimations = GetComponentInParent<PlayerAnimations>();
	}

	void Update()
	{
		if(CrossPlatformInputManager.GetButtonDown("Attack01") && _playerData._IsAboveSomething)
		{
			_attackCounter++;
		}
		else if(CrossPlatformInputManager.GetButtonDown("Attack01") && !_playerData._IsAboveSomething)
		{
			_aerialAttackCounter++;
		}
	}

	public void LastFrameOfJumpingReached()
	{
		_isItTheLastFrameOfJumping = true;
	}

	public void LastFrameOfSwingingReached()
	{
		_isItTheLastFrameOfSwinging = true;
	}

	public void IsItTheFirstFrameOfDodging()
	{
		_isItTheFirstFrameOfDodging = true;
		_isItTheLastFrameOfDodging = false;
	}

	public void LastFrameOfGroundSliding()
	{
		_isItTheLastFrameOfGroundSliding = true;
	}

	public void FirstFrameOfGroundSliding()
	{
		_isItTheLastFrameOfGroundSliding = false;
	}

	public void IsItTheLastFrameOfDodging()
	{
		_isItTheLastFrameOfDodging = true; 
		_isItTheFirstFrameOfDodging = false;
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
		if(Mathf.Abs(_playerData._RigidBody.velocity.y) > 0f)
		{
			StopDrawingTheSword();
		}
	}
	public void ResetAttackCounterToZero()
	{
		_attackCounter = 0;
	}

	public void ResetAirAttackCounterToZero()
	{
		_aerialAttackCounter = 0;
	}

	public void IncrementAirAttackAnimationCounter()
	{
		_aerialAttackCounter++;
		_wasAirAttackJustPerformed = true; 
	}

	public void SetFinalAirAttackToTrue()
	{
		_wasFinalAirAttackTriggererd = true; 
	}   

	public void SetFinalAirAttackToFalse()
	{
		_wasFinalAirAttackTriggererd = false; 
	}


}
