using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput; 

public class PlayerController : MonoBehaviour {

	[SerializeField]
	private float _runMultiplier = 1f; 
	[SerializeField]
	private float _minJumpVelocity = 5f; 
	[SerializeField]
	private float  _climbMultiplier = 2f;
	[SerializeField]
	private float _holdJumpButtonMultiplier = 0.5f; 
	[SerializeField]
	private float _holdJumpButtonOffset = 1.75f;
	[SerializeField]
	private float _gravityScaleOffset = 1.75f; 

	private enum PlayerState{Idle, Running, Jumping, Climbing};
	private PlayerState _playerState;  

	private float _holdJumpButtonDefaultValue;
	private float _peakJumpHeight;
	private float _defaultGravityScale;  

	bool _isAlive; 
	bool _isPlayerRunning;
	bool _isPlayerJumping; 
	bool _isPlayerHoldingJumpButton;
	bool _isPlayerClimbing;
	bool _isPlayerTouchingLadder; 
	bool _wasMaxHeightJumped; 

	Rigidbody2D _rigidbody; 
	Animator _animator; 
	CapsuleCollider2D _collider2D; 

	void Start () {
		_rigidbody = GetComponent<Rigidbody2D>(); 
		_animator = GetComponent<Animator>(); 
		_collider2D = GetComponent<CapsuleCollider2D>();
		_holdJumpButtonDefaultValue = _holdJumpButtonMultiplier; 
		_defaultGravityScale = _rigidbody.gravityScale; 
		_isAlive = true; 
		_isPlayerHoldingJumpButton = false; 
		_isPlayerClimbing = false; 
		_isPlayerRunning = false; 
		_isPlayerJumping = false; 
		_isPlayerTouchingLadder = false; 
		_playerState = PlayerState.Idle;
	}
	
	// Update is called once per frame
	void Update () {
		_isPlayerRunning = IsPlayerRunning();
		_isPlayerTouchingLadder = IsPlayerTouchingLadder(); 
		_isPlayerClimbing = IsPlayerClimbingLadder(_isPlayerTouchingLadder);
		_isPlayerJumping = IsPlayerJumping(); 
		_isPlayerHoldingJumpButton =  IsJumpBeingHeld();


		if(!_isPlayerHoldingJumpButton)
		{
			// _peakJumpHeight = Mathf.NegativeInfinity;
			// _holdJumpButtonMultiplier = _holdJumpButtonDefaultValue; 
			// _rigidbody.gravityScale = _defaultGravityScale;
		}

 	}


	void FixedUpdate()
	{
		if(_isPlayerRunning)
		{
			FlipSprite(Run());
			_isPlayerRunning = false; 
		}

		if(_isPlayerClimbing)
		{
			_rigidbody.gravityScale = 0f; 
			Climbing();
		}

		if(_isPlayerJumping)
		{
			Jump(); 
			_isPlayerJumping = false; 
		}

		if(_isPlayerHoldingJumpButton) //  && !_isPlayerClimbing)
		{
			if( transform.position.y > _peakJumpHeight)
			{
				_holdJumpButtonMultiplier += Time.fixedDeltaTime * _holdJumpButtonOffset;
				_rigidbody.gravityScale -= Time.fixedDeltaTime * _gravityScaleOffset;
				_peakJumpHeight = transform.position.y; 
				_rigidbody.AddForce(new Vector2(0f, _rigidbody.velocity.y) * _holdJumpButtonMultiplier, ForceMode2D.Force);
			}
			else
			{
				_holdJumpButtonMultiplier = _holdJumpButtonDefaultValue;
				_rigidbody.gravityScale = _defaultGravityScale;
			}
			_isPlayerHoldingJumpButton = false; 
		}
		else 
		{
			_peakJumpHeight = Mathf.NegativeInfinity;
			_holdJumpButtonMultiplier = _holdJumpButtonDefaultValue; 

			//TODO This will always be shitty
			if(_isPlayerClimbing || _isPlayerTouchingLadder)
			{
				_rigidbody.gravityScale = 0f;
			}
			else
			{
				_rigidbody.gravityScale = _defaultGravityScale;
			}
		}
		_isPlayerClimbing = false;

	}

	private bool IsPlayerRunning()
	{
		float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal"); 
		float playerX = controlThrow * _runMultiplier  * Time.deltaTime; 

        Vector2 playerVelocity = new Vector2(playerX, _rigidbody.velocity.y );

		bool isPlayerMoving = Mathf.Abs(playerVelocity.x) > Mathf.Epsilon; 
		return isPlayerMoving; 
	}
	

    private bool Run()
    {
        float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal"); 
		float playerX = controlThrow * _runMultiplier  * Time.fixedDeltaTime; 

        Vector2 playerVelocity = new Vector2(playerX, _rigidbody.velocity.y );
		_rigidbody.velocity = playerVelocity;  

		bool isPlayerMoving = Mathf.Abs(_rigidbody.velocity.x) > Mathf.Epsilon; 
		_animator.SetBool("isRunning", isPlayerMoving); 

		return isPlayerMoving; 
    }

	private void FlipSprite(bool isHeMoving)
	{
		if(isHeMoving)
		{
			transform.localScale = new Vector2(Mathf.Sign(_rigidbody.velocity.x), transform.localScale.y);
		}
	}

	private bool IsPlayerJumping()
	{
		if(!_collider2D.IsTouchingLayers(LayerMask.GetMask("Ground")))
		{
			return false;
		}
		if(CrossPlatformInputManager.GetButtonDown("Jump"))
		{
			return true; 
		}
		return false; 
	}

	private void Jump()
	{
		Vector2 jumpVelocityToAdd = new Vector2(0f, _minJumpVelocity * Time.fixedDeltaTime); 
		_rigidbody.AddForce(Vector2.up * jumpVelocityToAdd, ForceMode2D.Impulse);
	}

	private bool IsJumpBeingHeld()
	{
		if(CrossPlatformInputManager.GetButton("Jump"))
		{
			return true; 
		}
		else
		{
			_holdJumpButtonMultiplier = 0f; 
			return false; 
		}
	}

	private bool IsPlayerTouchingLadder()
	{
		bool isPlayerTouchingLadder = _collider2D.IsTouchingLayers(LayerMask.GetMask("Ladder"));

		if(!isPlayerTouchingLadder)
		{
			_animator.SetBool("isClimbing", false);
			return false;
		}
		else
		{
			return true; 
		}
	}

	private bool IsPlayerClimbingLadder(bool isTouchingLadder)
	{
		if(isTouchingLadder)
		{
			float controlThrow = CrossPlatformInputManager.GetAxis("Vertical"); 
			float playerY = controlThrow * _climbMultiplier  * Time.deltaTime; 
			bool isPlayerClimbing = Mathf.Abs(playerY) > Mathf.Epsilon; 
			bool isGrounded = _collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));

			if(isPlayerClimbing || (isTouchingLadder && !isGrounded))
			{
				return true;
			}
		}
		return false; 
	}

	private void Climbing()
	{
		float controlThrowY = CrossPlatformInputManager.GetAxis("Vertical"); 
		float climbSpdY = controlThrowY * _climbMultiplier  * Time.fixedDeltaTime; 

		float controlThrowX = CrossPlatformInputManager.GetAxis("Horizontal"); 
		float climbSpdX = controlThrowX * _climbMultiplier  * Time.fixedDeltaTime; 

        Vector2 playerVelocity = new Vector2(climbSpdX, climbSpdY);
		_rigidbody.velocity = playerVelocity;
		bool isPlayerClimbingY = Mathf.Abs(_rigidbody.velocity.y) > Mathf.Epsilon; 
		bool isPlayerClimbingX = Mathf.Abs(_rigidbody.velocity.x) > Mathf.Epsilon; 

		if(isPlayerClimbingY || isPlayerClimbingX)
		{
			_animator.SetBool("isClimbing", true);
			_animator.speed = 1f; 

		} 
		else
		{
			//_animator.SetBool("isClimbing", true);
			_animator.speed = 0f; 
		}
	}
	

}
