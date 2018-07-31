using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMovement : PlayerInput
{
    [SerializeField]
    private float _runMultiplier = 10f;
    [SerializeField]
    private float _groundSlideMultiplier = 15f;
    [SerializeField]
    private float _fallMultiplier = 10f;
    [SerializeField]
    private float _defaultJumpSpeed = 10f;
    [SerializeField]
    private float _doubleJumpStrength = 8f;
    [SerializeField]
    private float _waterJumpStrength = 9f;
    [SerializeField]
    private float _distanceToWallLengthCheck = 0.16f;
    [SerializeField]
    private Vector2 _wallJumpSpd = new Vector2(15f, 10f);

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

    private bool _WereJumpConditionsMet = false;
    private bool _WereRunConditionsMet = false;
    private bool _WereGroundSlideConditionsMet = false;
    private bool _WereAirControlConditionsMet = false;
    private bool _wasWallSlidingDirectionChanged = false;
    private float _doubleJumpTimer = 0f;
    private float _ignoreHorizontalInputTimer = 0f;
    private const float _ignoreHorizontalInputOffset = 0.45f;
    private const float _doubleJumpTimerOffset = 0.15f;
    private const float _airXOffset = 0.85f;

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
        if (_InputInstance._IsPlayerRunning)
        {
            _WereRunConditionsMet = CheckRunConditions();
        }
        if (_InputInstance._IsPlayerGroundSliding)
        {
            _WereGroundSlideConditionsMet = CheckGroundSlideConditions();
        }
        if (_InputInstance._IsPlayerJumping)
        {
            _WereJumpConditionsMet = CheckJumpConditions();
        }
        if(!_DataInstance._IsAboveSomething)
        {
            _WereAirControlConditionsMet = CheckAirControlConditions();
        }
        if (_InputInstance._WasDirectionChanged)
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
        if (_WereRunConditionsMet && _InputInstance._IsPlayerRunning)
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
        if(_WereAirControlConditionsMet)
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
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        float playerX = xPos * _runMultiplier * Time.fixedDeltaTime;

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
        if (Time.timeSinceLevelLoad < _ignoreHorizontalInputTimer)
        {
            float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
            float playerX = xPos * _fallMultiplier;
            _DataInstance._RigidBody.AddForce(new Vector2(playerX, 0f), ForceMode2D.Force);
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
                if (_DataInstance._IsHorizontalToWall)
                {
                    WallJump();
                    return true;
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
            else if (Time.timeSinceLevelLoad > _doubleJumpTimer && _DataInstance._IsTouchingWater)
            {
                WaterJump();
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
                }
                break;

            case JUMPSTATUS.LadderJump:
                _DataInstance._RigidBody.AddForce(new Vector2(0f, _defaultJumpSpeed), ForceMode2D.Impulse);
                break;

            case JUMPSTATUS.WaterJump:
                float yStr = _waterJumpStrength * Time.fixedDeltaTime;
                _DataInstance._RigidBody.velocity = new Vector2(_DataInstance._RigidBody.velocity.x, yStr);
                break;

            default:
                Debug.LogWarning("JumpState = " + _DataInstance._JumpState);
                break;
        }
    }

    private void LadderJump()
    {
        _DataInstance._JumpState = JUMPSTATUS.LadderJump;
        _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
    }

    private void WaterJump()
    {
        _DataInstance._JumpState = JUMPSTATUS.WaterJump;
        _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
    }

    private void NormalJump()
    {
        _doubleJumpCounter++;
        _DataInstance._JumpState = JUMPSTATUS.NormalJump;
        _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
    }

    private void WallJump()
    {
        _doubleJumpCounter++;
        _DataInstance._JumpState = JUMPSTATUS.WallJump;
        _ignoreHorizontalInputTimer = Time.timeSinceLevelLoad + _ignoreHorizontalInputOffset;
        _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
    }

    private void DoubleJump()
    {
        _doubleJumpCounter++;
        _DataInstance._JumpState = JUMPSTATUS.DoubleJump;
    }

    private bool CheckGroundSlideConditions()
    {
        if (!_DataInstance._IsTouchingWater &&  Mathf.Abs(_RigidBody.velocity.x) > 0 &&
        (_DataInstance._IsAboveGround || _DataInstance._IsAboveCorner || _DataInstance._IsAboveWall))
        {
            return true;
        }
        return false;
    }

    private void GroundSlide()
    {
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
            }
            else if (CrossPlatformInputManager.GetAxis("Horizontal") < 0 && !_DataInstance._SpriteRenderer.flipX)
            {
                _DataInstance._SpriteRenderer.flipX = !_DataInstance._SpriteRenderer.flipX;
            }
        }
        else
        {
            Debug.LogWarning("Player Wall Sliding From ChangeDirection()");
            _DataInstance._SpriteRenderer.flipX = !_DataInstance._SpriteRenderer.flipX;
        }
    }
}
