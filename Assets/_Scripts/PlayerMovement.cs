using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMovement : PlayerInput
{
    [SerializeField]
    private float _runMultiplier = 10f;
    [SerializeField]
    private float _fallMultiplier = 10f;
    [SerializeField]
    private float _defaultJumpSpeed = 10f;
    [SerializeField]
    private float _doubleJumpStrength = 8f;
    [SerializeField]
    private float _distanceToWallLengthCheck = 0.15f;
    [SerializeField]
    private Vector2 _wallJumpSpd = new Vector2(15f, 10f);

    private float _doubleJumpTimer = 0f;
    private const float _doubleJumpTimerOffset = 0.165f;

    private float _ignoreHorizontalInputTimer = 0f;
    private const float _ignoreHorizontalInputOffset = 0.45f;

    private const float _airXOffset = 0.85f;

    public int _doubleJumpCounter
    {
        get; set;
    }
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
        if (_IsPlayerRunning && !_IsPlayerSwinging)
        {
            if (_DataInstance._IsAboveSomething)
            {
                Run();
            }
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
        _IsPlayerJumping = false;
        _IsPlayerRunning = false;
        _IsPlayerSwinging = false;
    }

    private void Run()
    {
        // This code only runs if the player is above something / isgrounded
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        float playerX = xPos * _runMultiplier * Time.fixedDeltaTime;

        Vector2 playerVelocity = new Vector2(playerX, _RigidBody.velocity.y);
        _RigidBody.velocity = playerVelocity;

        bool isPlayerMoving = Mathf.Abs(_RigidBody.velocity.x) > Mathf.Epsilon;
        _Animator.SetBool("isRunning", isPlayerMoving);
    }

    private void AirControl()
    {
        // the If is for directly after a wall jump
        if (Time.timeSinceLevelLoad < _ignoreHorizontalInputTimer)
        {
            float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
            float playerX = xPos * _fallMultiplier;

            _RigidBody.AddForce(new Vector2(playerX, 0f), ForceMode2D.Force);

            bool isPlayerFalling = Mathf.Abs(_RigidBody.velocity.x) > Mathf.Epsilon;
            _Animator.SetBool("isRunning", isPlayerFalling);
        }
        else
        {
            //This else section is for adjusting airSpeed anytime other than after a walljump
            float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
            float playerX = xPos * (_runMultiplier * _airXOffset) * Time.fixedDeltaTime;

            Vector2 playerVelocity = new Vector2(playerX, _RigidBody.velocity.y);
            _RigidBody.velocity = playerVelocity;

            bool isPlayerMoving = Mathf.Abs(_RigidBody.velocity.x) > Mathf.Epsilon;
            _Animator.SetBool("isRunning", isPlayerMoving);
        }
    }

    private void Jump()
    {
        if (PlayerData._DataInstance._IsAboveSomething)
        {
            _doubleJumpCounter = 0;
        }
        if (_doubleJumpCounter == 0)
        {
            _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
            _doubleJumpCounter++;
            _RigidBody.AddForce(new Vector2(0f, _defaultJumpSpeed), ForceMode2D.Impulse);
        }
        else if (Time.timeSinceLevelLoad > _doubleJumpTimer)
        {
            if (_DataInstance._IsHorizontalToWall)
            {
                _doubleJumpCounter++;
                WallJump();
            }
            else if (_doubleJumpCounter == 1)
            {
                _doubleJumpCounter++;
                _RigidBody.AddForce(new Vector2(0f, _doubleJumpStrength), ForceMode2D.Impulse);
            }
        }
    }

    private void WallJump()
    {
        RaycastHit2D hit = _DataInstance._HorizontalWallRaycast;
        if (hit.collider)
        {
            _SpriteRenderer.flipX = !_SpriteRenderer.flipX;
            Vector2 vel = new Vector2(_wallJumpSpd.x * hit.normal.x, _wallJumpSpd.y);
            _RigidBody.velocity = new Vector2(0f, _RigidBody.velocity.y);
            _RigidBody.AddForce(vel, ForceMode2D.Impulse);

            //This will keep the player from running and jumping right away
            _ignoreHorizontalInputTimer = Time.timeSinceLevelLoad + _ignoreHorizontalInputOffset;
            _doubleJumpTimer = Time.timeSinceLevelLoad + _doubleJumpTimerOffset;
        }
    }

    private void ChangeDirection()
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
}
