using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMovement : PlayerInput
{
    [SerializeField]
    private float _runMultiplier = 10f;
    [SerializeField]
    private float _crouchMultiplier = 5f;
    [SerializeField]
    private float _groundSlideMultiplier = 15f;
    [SerializeField]
    private float _defaultJumpSpeed = 10f;
    [SerializeField]
    private float _doubleJumpStrength = 8f;
    [SerializeField]
    private float _pushSpeed = 1.5f;
    [SerializeField]
    private Vector2 _wallJumpSpd = new Vector2(15f, 10f);
    [SerializeField]
    private float _wallJumpSpeedOffset = 7.5f;

    public int _doubleJumpCounter
    {
        get; set;
    }
    public bool? _WasPlayerMovementInitialized
    {
        get; set;
    }
    public static PlayerMovement _PlayerMovementInstance
    {
        get; private set;
    }

    private RaycastHit2D _hit;
    private bool _WasFlipped = false; 
    private bool _WereJumpConditionsMet = false;
    private bool _WereRunConditionsMet = false;
    private bool _WereGroundSlideConditionsMet = false;
    private bool _WereAirControlConditionsMet = false;
    private bool _wasWallSlidingDirectionChanged = false;
    private bool _isWallJumping = false;
    private float _doubleJumpTimer = 0f;
    private float _ignoreHorizontalInputTimer = 0f;
    private float _maxXVelocity = 0f;
    private float _ignoreHorizontalInputOffsetOffsetter = 0f;

    private const float _ignoreHorizontalInputOffset = 0.5f;
    private const float _doubleJumpTimerOffset = 0.15f;
    private const float _airXOffset = 0.83f;

    // Use this for initialization
    void Awake()
    {
        InitializePlayerMovement();
    }

    void Start()
    {
        _WereJumpConditionsMet = false;
        _WereRunConditionsMet = false;
        _WereGroundSlideConditionsMet = false;
        _WereAirControlConditionsMet = false;
        _isWallJumping = false;
        _ignoreHorizontalInputOffsetOffsetter = _ignoreHorizontalInputOffset / 2.5f;
        _doubleJumpCounter = 0;
    }

    public void InitializePlayerMovement()
    {
        if (_WasPlayerMovementInitialized == null || _WasPlayerMovementInitialized == false)
        {
            if (_PlayerMovementInstance != null && _PlayerMovementInstance != this)
            {
                Destroy(this);
            }
            else
            {
                _PlayerMovementInstance = this;
                _WasPlayerMovementInitialized = true;
            }
        }
        if (_WasInitialized == null || _WasInitialized == false)
        {
            base.Initialize();
        }
        if (_WasInputInitialized == null || _WasInputInitialized == false)
        {
            InitializeInput();
        }
    }

    void Update()
    {
        if (_InputInstance._IsPlayerRunning && !_InputInstance._IgnorePlayerInput)
        {
            _WereRunConditionsMet = CheckRunConditions();
        }
        if (_InputInstance._IsPlayerGroundSliding && !_InputInstance._IgnorePlayerInput)
        {
            _WereGroundSlideConditionsMet = CheckGroundSlideConditions();
        }
        if (_InputInstance._IsPlayerJumping && !_InputInstance._IgnorePlayerInput)
        {
            _WereJumpConditionsMet = CheckJumpConditions();
        }
        if (!_DataInstance._IsAboveSomething && !_InputInstance._IgnorePlayerInput)
        {
            _WereAirControlConditionsMet = CheckAirControlConditions();
        }
        else
        {
            _WereAirControlConditionsMet = false;
        }
        if (_InputInstance._WasDirectionChanged && !_InputInstance._IgnorePlayerInput)
        {
            ChangeDirection();
            _InputInstance._WasDirectionChanged = !_InputInstance._WasDirectionChanged;
        }
    }

    void FixedUpdate()
    {
        BasicPlayerMovement();
    }

    private void BasicPlayerMovement()
    {
        if (_InputInstance._IsPlayerCrouching && !_InputInstance._IgnorePlayerInput)
        {
            Crouch();
        }
        else if (_WereRunConditionsMet && _InputInstance._IsPlayerRunning)
        {
            Run();
        }

        if (_WereGroundSlideConditionsMet && _InputInstance._IsPlayerGroundSliding)
        {
            GroundSlide();
        }
        if (_WereJumpConditionsMet && _InputInstance._IsPlayerJumping)
        {
            Jump();
        }

        // AirControl has to happen after Jump() b/c of a timer.
        // It also depends on _IsPlayerRunning b/c it gets horizontal input of player.
        if (_WereAirControlConditionsMet)
        {
            AirControl();
        }

        // Must set these to false or FixedUpdate will capture input too many times from Update 
        // Dont have to set _isTouchingLadder or _isTouchingWater b/c they're handled by OnCollisionEnter/Exit 
        _InputInstance._IsPlayerJumping = false;
        _InputInstance._IsPlayerRunning = false;
        _InputInstance._IsPlayerSwinging = false;
        _InputInstance._IsPlayerGroundSliding = false;
    }

    private bool CheckRunConditions()
    {
        if (_InputInstance._IsPlayerRunning && !_InputInstance._IsPlayerSwinging && !_DataInstance._IsTouchingWater)
        {
            if (_DataInstance._IsAboveSomething)
            {
                return true;
            }
        }
        return false;
    }

    private void Run()
    {
        // This code only runs if the player is above something / isgrounded
        _isWallJumping = false;
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        float playerX = xPos * _runMultiplier * Time.fixedDeltaTime;

        if (_DataInstance._IsHorizontalToGrabableObject)
        {
            playerX /= _pushSpeed;
        }

        Vector2 playerVelocity = new Vector2(playerX, _DataInstance._RigidBody.velocity.y);
        _DataInstance._RigidBody.velocity = playerVelocity;
    }

    private void Crouch()
    {
        _isWallJumping = false;
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        float playerX = xPos * _crouchMultiplier * Time.fixedDeltaTime;

        if (_DataInstance._IsHorizontalToGrabableObject)
        {
            playerX /= _pushSpeed;
        }

        Vector2 playerVelocity = new Vector2(playerX, _DataInstance._RigidBody.velocity.y);
        _DataInstance._RigidBody.velocity = playerVelocity;
    }


    private bool CheckAirControlConditions()
    {
        if (_InputInstance._IsPlayerRunning && !_InputInstance._IsPlayerSwinging && !_DataInstance._IsTouchingWater)
        {
            return true;
        }
        return false;
    }

    private void AirControl()
    {
        // the If is for directly after a wall jump
        if (Time.timeSinceLevelLoad < (_ignoreHorizontalInputTimer - _ignoreHorizontalInputOffsetOffsetter) && _isWallJumping)
        {
            //Dont want the player to do anything here
            _maxXVelocity = _RigidBody.velocity.x;
            return;
        }
        if (Time.timeSinceLevelLoad < _ignoreHorizontalInputTimer && _isWallJumping)
        {
            //TODO
            //WAnt the player to have reduced control over their jump here
    
        }
        else if (Time.timeSinceLevelLoad >= _ignoreHorizontalInputTimer && _isWallJumping)
        {
            //TODO
            //WAnt the player to have more control over their jump here
        }
        else
        {
            //This else section is for adjusting airSpeed anytime other than after a walljump
            float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
            float playerX = xPos * (_runMultiplier * _airXOffset) * Time.fixedDeltaTime;
            Vector2 playerVelocity = new Vector2(playerX, _RigidBody.velocity.y);
            _DataInstance._RigidBody.velocity = playerVelocity;
        }
    }

    private bool CheckJumpConditions()
    {
        if (!_DataInstance._IsTouchingLadder && !_DataInstance._IsTouchingWater)
        {
            if (_DataInstance._IsAboveSomething)
            {
                _doubleJumpCounter = 0;
            }
            if (_doubleJumpCounter == 0)
            {
                NormalJump();
                return true;
            }
            else if (Time.timeSinceLevelLoad > _doubleJumpTimer)
            {
                float x = 0f;
                float inputX = 0f;

                if (_DataInstance._IsHorizontalToWall || _DataInstance._IsTouchingCorner)
                {
                    _hit = _DataInstance._HorizontalWallRaycast;
                    x = _hit.normal.x;
                    inputX = _InputInstance._GetHorizontalAxisValue;

                    // TODO: Making it != will reverse the direction required to initiate the jump
                    if ((Mathf.Sign(x) != Mathf.Sign(inputX)) && inputX != 0)
                    {
                        WallJump();
                        return true;
                    }
                    else
                    {
                        //this is for a screwed up wall jump
                        _doubleJumpCounter++;
                        return false;
                    }
                }
                else if (_doubleJumpCounter == 1)
                {
                    DoubleJump();
                    return true;
                }
            }
        }
        else
        {
            if (Time.timeSinceLevelLoad > _doubleJumpTimer && !_DataInstance._IsTouchingWater && _DataInstance._IsTouchingLadder)
            {
                LadderJump();
                return true;
            }
        }
        return false;
    }

    private void Jump()
    {
        switch (_DataInstance._JumpState)
        {
            case JUMPSTATUS.NormalJump:
                _DataInstance._RigidBody.AddForce(new Vector2(0f, _defaultJumpSpeed), ForceMode2D.Impulse);
                break;

            case JUMPSTATUS.DoubleJump:
                Vector2 vel = new Vector2(_DataInstance._RigidBody.velocity.x, _doubleJumpStrength);
                _DataInstance._RigidBody.velocity = vel;
                break;

            case JUMPSTATUS.WallJump:
                RaycastHit2D hit = _DataInstance._HorizontalWallRaycast;
                if (hit.collider)
                {
                    _DataInstance._SpriteRenderer.flipX = !_DataInstance._SpriteRenderer.flipX;
                    Vector2 veloc = new Vector2(_wallJumpSpd.x * hit.normal.x, _wallJumpSpd.y);
                    _DataInstance._RigidBody.velocity = veloc;
                    _maxXVelocity = _DataInstance._RigidBody.velocity.x;
                }
                break;

            case JUMPSTATUS.LadderJump:
                _DataInstance._RigidBody.AddForce(new Vector2(0f, _defaultJumpSpeed), ForceMode2D.Impulse);
                break;

            default:
                break;
        }
    }

    private void LadderJump()
    {
        _isWallJumping = false;
        _DataInstance._JumpState = JUMPSTATUS.LadderJump;
        _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
    }

    private void NormalJump()
    {
        _isWallJumping = false;
        _doubleJumpCounter++;
        _DataInstance._JumpState = JUMPSTATUS.NormalJump;
        _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
    }

    private void WallJump()
    {
        _isWallJumping = true;
        _doubleJumpCounter++;
        _DataInstance._JumpState = JUMPSTATUS.WallJump;
        _ignoreHorizontalInputTimer = Time.timeSinceLevelLoad + _ignoreHorizontalInputOffset;
        _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
    }

    private void DoubleJump()
    {
        _isWallJumping = false;
        _doubleJumpCounter++;
        _DataInstance._JumpState = JUMPSTATUS.DoubleJump;
    }

    private bool CheckGroundSlideConditions()
    {
        if (!_DataInstance._IsTouchingWater && Mathf.Abs(_RigidBody.velocity.x) > 0 && !_InputInstance._IsPlayerCrouching
        && (_DataInstance._IsAboveGround || _DataInstance._IsAboveCorner || _DataInstance._IsAboveWall))
        {
            return true;
        }
        return false;
    }

    private void GroundSlide()
    {
        Debug.LogWarning("Inside GroundSlide()");
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        if (!AnimationEventReciever._isItTheLastFrameOfGroundSliding)
        {
            //Performing a ground slide adds a force impulse, same code as below except
            // for the variable _groundSlideMultiplier
            float playerX = xPos * _groundSlideMultiplier * Time.fixedDeltaTime;
            Vector2 playerVelocity = new Vector2(playerX, _DataInstance._RigidBody.velocity.y);
            _DataInstance._RigidBody.AddForce(playerVelocity, ForceMode2D.Impulse);
        }
        else
        {
            // At the end of the animation, the speed is reverted to a slower speed.
            float playerX = xPos * _runMultiplier * Time.fixedDeltaTime;
            Vector2 playerVelocity = new Vector2(playerX, _DataInstance._RigidBody.velocity.y);
            _DataInstance._RigidBody.velocity = playerVelocity;
        }
    }

    private void ChangeDirection()
    {
        if (!_InputInstance._IsPlayerWallSliding)
        {
            if (CrossPlatformInputManager.GetAxis("Horizontal") > 0 && _DataInstance._SpriteRenderer.flipX)
            {
                _DataInstance._SpriteRenderer.flipX = !_DataInstance._SpriteRenderer.flipX;
                _WasFlipped = true;
            }
            else if (CrossPlatformInputManager.GetAxis("Horizontal") < 0 && !_DataInstance._SpriteRenderer.flipX)
            {
                _DataInstance._SpriteRenderer.flipX = !_DataInstance._SpriteRenderer.flipX;
                _WasFlipped = true;
            }
        }
        else
        {
            Debug.LogWarning("Player Wall Sliding From ChangeDirection()");
            _DataInstance._SpriteRenderer.flipX = !_DataInstance._SpriteRenderer.flipX;
        }
    }
}
