using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{

    [SerializeField]
    private float _maxSpeed = 6f;
    [SerializeField]
    private float _jumpPower = 20f;
    [SerializeField]
    private float _jumpMultiplier = 20f;
    [SerializeField]
    private float _jumpOffset = 1f;
	[SerializeField]
	private float _doubleJumpStrength = 0.8f;
    private float _defaultJumpMultiplier;
    private float _defaultJumpOffset;
    private float _currentSpeed = 0f;

    private bool _isJumping;
    private bool _isHoldingJumpButton;
    private bool _isDoubleJumpReady;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private CapsuleCollider2D _collider2D;
    private int _doubleJumpCounter;

    public float jumpShortSpeed = 3f;   // Velocity for the lowest jump
    public float jumpSpeed = 6f;          // Velocity for the highest jump
    bool jump = false;
    bool jumpCancel = false;
    bool doubleJump = false;
    bool doubleJumpCancel = false;
    bool grounded = false;


    void Awake()
    {
        _collider2D = GetComponent<CapsuleCollider2D>();
        _defaultJumpMultiplier = _jumpMultiplier;
        _defaultJumpOffset = _jumpOffset;
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }


    void Start()
    {       
	    _isHoldingJumpButton = false;
        _isDoubleJumpReady = false;
        _isJumping = false;
        _doubleJumpCounter = 0;
    }

    void Update()
    {
        _currentSpeed = Input.GetAxis("Horizontal") * _maxSpeed;

        grounded = _collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));  // Check if player is grounded however you like

        if (_doubleJumpCounter < 2)
        {
            if (Input.GetButtonUp("Jump") && !grounded)
            {
                _doubleJumpCounter++;
                jumpCancel = true;
            }
            if (Input.GetButtonDown("Jump") && _isDoubleJumpReady)
            {
                doubleJump = true;
            }
        }
		else if(grounded)
		{
			_doubleJumpCounter = 0;
		}

    }

    void FixedUpdate()
    {
        Move();
        Jump();
    }

    void Move()
    {
        _rb.velocity = new Vector2(_currentSpeed, _rb.velocity.y);
        ChangeDirection();
    }

    void ChangeDirection()
    {
        if (_currentSpeed > 0f && _spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = false;
        }
        else if (_currentSpeed < 0 && !_spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = true;
        }
    }

    void Jump()
    {
        if (!_isDoubleJumpReady)
        {
            _isDoubleJumpReady = true;
        }

        if (_isDoubleJumpReady)
        {
            if (jumpCancel)
            {
                if (_rb.velocity.y > jumpShortSpeed)
                {
					if(_doubleJumpCounter < 1)
					{
						_rb.velocity = new Vector2(_rb.velocity.x, jumpShortSpeed);
					}
					else
					{
						_rb.velocity = new Vector2(_rb.velocity.x, jumpShortSpeed * _doubleJumpStrength);
					}
                }
                jumpCancel = false;
            }
            if (doubleJump)
            {
				if(_doubleJumpCounter < 1)
				{
					_rb.velocity = new Vector2(_rb.velocity.x, jumpSpeed);
				}
				else
				{
					_rb.velocity = new Vector2(_rb.velocity.x, jumpSpeed * _doubleJumpStrength);
				}
                doubleJump = false;
            }
        }

    }
}


//**************************************************************************** */
//*******************THE BELOW CODE WAS THE CRAPPY WAY I GOT DOUBLE JUMP WORKING */
    // void Update()
    // {
    //     _currentSpeed = Input.GetAxis("Horizontal") * _maxSpeed;

    //     grounded = _collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));  // Check if player is grounded however you like

    //     if (_doubleJumpCounter < 2)
    //     {
    //         // if (Input.GetButtonDown("Jump") && grounded)
    //         // {
    //         //     jump = true;
    //         // }
    //         if (Input.GetButtonUp("Jump") && !grounded)
    //         {
    //             _doubleJumpCounter++;
    //             Debug.Log(_doubleJumpCounter + " = JumpCtr");
    //             jumpCancel = true;
    //         }

    //         if (Input.GetButtonDown("Jump") && _isDoubleJumpReady)
    //         {
    //             doubleJump = true;
    //         }
    //         // if (Input.GetButtonUp("Jump") && !grounded && !_isDoubleJumpReady)
    //         // {
    //         //     doubleJumpCancel = true;
    //         // }
    //     }
	// 	else if(grounded)
	// 	{
	// 		_doubleJumpCounter = 0;
	// 	}

    // }

	//     void Jump()
    // {
    //     if (!_isDoubleJumpReady)
    //     {
    //         // if (jump)
    //         // {
    //         //     _rb.velocity = new Vector2(_rb.velocity.x, jumpSpeed);
    //         //     Debug.Log("Hit 'Jump'");
	// 		// 	jump = false;
    //         // }
    //         // Cancel the jump when the button is no longer pressed
    //         _isDoubleJumpReady = true;
    //     }

    //     if (_isDoubleJumpReady)
    //     {
    //         if (jumpCancel)
    //         {
    //             if (_rb.velocity.y > jumpShortSpeed)
    //             {
    //                 _rb.velocity = new Vector2(_rb.velocity.x, jumpShortSpeed);
	// 				Debug.Log("Hit 'JumpCancel'");
    //             }
    //             jumpCancel = false;
    //         }
    //         if (doubleJump)
    //         {
    //             Debug.Log("Hit 'DoubleJump'");
    //             _rb.velocity = new Vector2(_rb.velocity.x, jumpSpeed);
    //             doubleJump = false;
    //         }
    //         // Cancel the jump when the button is no longer pressed
    //        // if (doubleJumpCancel)
    //         //{
    //           //  Debug.Log("Hit 'DoubleJumpCANCEL'");

    //             //if (_rb.velocity.y > jumpShortSpeed)
    //            // {
    //                // _rb.velocity = new Vector2(_rb.velocity.x, jumpShortSpeed);
    //             //}
    //             //doubleJumpCancel = false;
    //             //_isDoubleJumpReady = false;
    //        // }
    //     }

    // }
