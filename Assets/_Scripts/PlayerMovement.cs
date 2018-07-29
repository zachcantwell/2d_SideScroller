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

    private bool _wasWallSlidingDirectionChanged = false;
    private float _doubleJumpTimer = 0f;
    private float _ignoreHorizontalInputTimer = 0f;
    private const float _ignoreHorizontalInputOffset = 0.45f;
    private const float _doubleJumpTimerOffset = 0.15f;
    private const float _airXOffset = 0.85f;

    // Use this for initialization
    void Awake()
    {
        if (_WasInitialized == null || _WasInitialized == false)
        {
            base.Initialize();
        }
        if (_WasInputInitialized == null || _WasInputInitialized == false)
        {
            InitializeInput();
        }
        _doubleJumpCounter = 0;
    }

    void FixedUpdate()
    {
        BasicPlayerMovement();
    }

    private void BasicPlayerMovement()
    {
        if (_IsPlayerRunning && !_IsPlayerSwinging && !_DataInstance._IsTouchingWater)
        {
            if (_DataInstance._IsAboveSomething)
            {
                Run();
            }
        }
        if (_InputInstance._IsPlayerGroundSliding && !_DataInstance._IsTouchingWater)
        {
            GroundSlide();
        }
        if (_IsPlayerJumping)
        {
            Jump();
        }

        // AirControl has to happen after Jump() b/c of a timer.
        // It also depends on _IsPlayerRunning b/c it gets horizontal input of player.
        if (_IsPlayerRunning && !_IsPlayerSwinging)
        {
            if (!_DataInstance._IsAboveSomething)
            {
                AirControl();
            }
        }

        if (_WasDirectionChanged)
        {
            ChangeDirection();
            _WasDirectionChanged = !_WasDirectionChanged;
        }

        // Must set these to false or FixedUpdate will capture input too many times from Update 
        // Dont have to set _isTouchingLadder or _isTouchingWater b/c they're handled by OnCollisionEnter/Exit 
        _IsPlayerJumping = false;
        _IsPlayerRunning = false;
        _IsPlayerSwinging = false;
        _IsPlayerGroundSliding = false;
    }

    private void Run()
    {
        // This code only runs if the player is above something / isgrounded
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        float playerX = xPos * _runMultiplier * Time.fixedDeltaTime;

        Vector2 playerVelocity = new Vector2(playerX, _RigidBody.velocity.y);
        _RigidBody.velocity = playerVelocity;
    }

    private void AirControl()
    {
        // the If is for directly after a wall jump
        if (Time.timeSinceLevelLoad < _ignoreHorizontalInputTimer)
        {
            float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
            float playerX = xPos * _fallMultiplier;

            _RigidBody.AddForce(new Vector2(playerX, 0f), ForceMode2D.Force);
        }
        else
        {
            //This else section is for adjusting airSpeed anytime other than after a walljump
            float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
            float playerX = xPos * (_runMultiplier * _airXOffset) * Time.fixedDeltaTime;

            Vector2 playerVelocity = new Vector2(playerX, _RigidBody.velocity.y);
            _RigidBody.velocity = playerVelocity;
        }
    }

    private void Jump()
    {
        if (!_DataInstance._IsTouchingLadder && !_DataInstance._IsTouchingWater)
        {
            if (_DataInstance._IsAboveGrabbableObject || _DataInstance._IsAboveCorner || _DataInstance._IsAboveGround)
            {
                _doubleJumpCounter = 0;
            }
            if (_doubleJumpCounter == 0)
            {
                _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
                _doubleJumpCounter++;
                _DataInstance._RigidBody.AddForce(new Vector2(0f, _defaultJumpSpeed), ForceMode2D.Impulse);
            }
            else if (Time.timeSinceLevelLoad > _doubleJumpTimer)
            {
                if (_DataInstance._IsHorizontalToWall)
                {
                    WallJump();
                }
                else if (_doubleJumpCounter == 1)
                {
                    DoubleJump();
                }
            }
        }
        else
        {
            if (Time.timeSinceLevelLoad > _doubleJumpTimer && !_DataInstance._IsTouchingWater && _DataInstance._IsTouchingLadder)
            {
                //This is for a LadderJump
                _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
                _DataInstance._RigidBody.AddForce(new Vector2(0f, _defaultJumpSpeed), ForceMode2D.Impulse);
            }
            else if (Time.timeSinceLevelLoad > _doubleJumpTimer && _DataInstance._IsTouchingWater)
            {
                //This is weaker, for a waterjump
                _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
                float yStr = _waterJumpStrength * Time.fixedDeltaTime;
                _DataInstance._RigidBody.velocity = new Vector2(_DataInstance._RigidBody.velocity.x, yStr);
            }
        }

    }

    private void WallJump()
    {
        _doubleJumpCounter++;
        RaycastHit2D hit = _DataInstance._HorizontalWallRaycast;
        if (hit.collider)
        {
            _SpriteRenderer.flipX = !_SpriteRenderer.flipX;
            Vector2 vel = new Vector2(_wallJumpSpd.x * hit.normal.x, _wallJumpSpd.y);
            _RigidBody.velocity = vel;

            //This will keep the player from running and jumping right away
            _ignoreHorizontalInputTimer = Time.timeSinceLevelLoad + _ignoreHorizontalInputOffset;
            _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
        }
    }

    private void DoubleJump()
    {
        _doubleJumpCounter++;
        Vector2 vel = new Vector2(_DataInstance._RigidBody.velocity.x, _doubleJumpStrength);
        _DataInstance._RigidBody.velocity = vel;

        // Old Way is below
        // _DataInstance._RigidBody.AddForce(new Vector2(0f, _doubleJumpStrength), ForceMode2D.Impulse);

    }

    private void GroundSlide()
    {
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");

        if (!_DataInstance._IsTouchingWater)
        {
            if (!AnimationEventReciever._isItTheLastFrameOfGroundSliding)
            {
                //Performing a ground slide adds a force impulse, same code as below except
                // for the variable _groundSlideMultiplier
                float playerX = xPos * _groundSlideMultiplier * Time.fixedDeltaTime;
                Vector2 playerVelocity = new Vector2(playerX, _RigidBody.velocity.y);
                _RigidBody.AddForce(playerVelocity, ForceMode2D.Impulse);
            }
            else
            {
                // At the end of the animation, the speed is reverted to a slower speed.
                float playerX = xPos * _runMultiplier * Time.fixedDeltaTime;
                Vector2 playerVelocity = new Vector2(playerX, _RigidBody.velocity.y);
                _RigidBody.velocity = playerVelocity;
            }
        }

    }

    private void ChangeDirection()
    {
        if (!_InputInstance._IsPlayerWallSliding)
        {
            if (CrossPlatformInputManager.GetAxis("Horizontal") > 0 && _SpriteRenderer.flipX)
            {
                _SpriteRenderer.flipX = !_SpriteRenderer.flipX;
            }
            else if (CrossPlatformInputManager.GetAxis("Horizontal") < 0 && !_SpriteRenderer.flipX)
            {
                _SpriteRenderer.flipX = !_SpriteRenderer.flipX;
            }
        }
        else
        {
            Debug.LogWarning("Player Wall Sliding From ChangeDirection()");
            _SpriteRenderer.flipX = !_SpriteRenderer.flipX;
        }
    }


}
