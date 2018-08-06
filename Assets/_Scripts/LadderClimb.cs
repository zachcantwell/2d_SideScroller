using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderClimb : PlayerInput
{
    private const float _ClimbMultiplier = 150f;

    public bool? _WasClimbLadderInitialized
    {
        get; set;
    }

    public static LadderClimb _LadderClimbInstance
    {
        get; private set;
    }

    private bool _IsPlayerClimbingUp;
    private bool _IsPlayerClimbingDown;
    private bool _didPlayerJumpFromLadder;
    private bool _wereLadderValuesReset;
    private float _ignoreClimbTimer;
    private const float _ignoreClimbOffset = 0.5f;

    private void InitializeLadderClimb()
    {
        if (_WasInitialized == null || _WasInitialized == false)
        {
            base.Initialize();
        }
        if (_WasInputInitialized == null || _WasInputInitialized == false)
        {
            InitializeInput();
        }

        if (_WasClimbLadderInitialized == null || _WasClimbLadderInitialized == false)
        {
            if (_LadderClimbInstance != null && _LadderClimbInstance != this)
            {
                if (_LadderClimbInstance is LadderClimb)
                {
                    Destroy(this);
                }
            }
            else
            {
                if (_LadderClimbInstance is LadderClimb)
                {
                    _LadderClimbInstance = this;
                    _WasClimbLadderInitialized = true;
                }
            }
        }
        _IsPlayerClimbingDown = false;
        _IsPlayerClimbingUp = false;
        _didPlayerJumpFromLadder = false;
        _wereLadderValuesReset = false;
        _ignoreClimbTimer = 0f;
        Physics2D.IgnoreLayerCollision(9, 10, true);
    }

    void Awake()
    {
        InitializeLadderClimb();
    }

    void Update()
    {
        CheckIfClimbingUp();
        CheckIfClimbingDown();
        CheckIfJumpingOff();
        CheckOnLadderCollisions();

        if (!_InputInstance._IsPlayerJumpingOffLadder)
        {
            if (_IsPlayerClimbingUp)
            {
                ClimbUpLadder();
            }
            else if (_IsPlayerClimbingDown)
            {
                ClimbDownLadder();
            }
            else
            {
                if (_DataInstance._IsTouchingLadder)
                {
                    //If the player is touching the ladder, but not pressing up/down, then Freeze The YPos.
                    HoldPositionOnLadder();
                }
                else
                {
                    //If not touching the ladder, reset the values that were changed.
                    ResetLadderValues();
                }
            }
        }
        else
        {
            JumpFromLadder();
        }
    }

    private void ClimbUpLadder()
    {
        _wereLadderValuesReset = false; 
        _RigidBody.constraints = RigidbodyConstraints2D.None;
        _RigidBody.freezeRotation = true;
        _RigidBody.gravityScale = 0f;

        if (!_DataInstance._IsAboveLadder)
        {
            _RigidBody.velocity = new Vector2(0f, _ClimbMultiplier * Time.fixedDeltaTime);
        }
        else
        {
            _RigidBody.velocity = Vector2.zero;
        }
    }

    private void ClimbDownLadder()
    {
        _wereLadderValuesReset = false; 
        _RigidBody.constraints = RigidbodyConstraints2D.None;
        _RigidBody.freezeRotation = true;
        _RigidBody.gravityScale = 0f;
        _RigidBody.velocity = new Vector2(0f, -_ClimbMultiplier * Time.fixedDeltaTime);
    }

    private void ResetLadderValues()
    {
        if (_wereLadderValuesReset == false)
        {
            _wereLadderValuesReset = true;
            _RigidBody.gravityScale = _DefaultGravityScale;
            _RigidBody.constraints = RigidbodyConstraints2D.None;
            _RigidBody.freezeRotation = true;
        }
    }

    private void HoldPositionOnLadder()
    {
        if (!_InputInstance._IsPlayerJumpingOffLadder)
        {
            _wereLadderValuesReset = false; 
            _RigidBody.constraints = RigidbodyConstraints2D.FreezePositionY;
            _RigidBody.freezeRotation = true;
            _RigidBody.gravityScale = 0f;
        }
    }

    private void JumpFromLadder()
    {
        _RigidBody.constraints = RigidbodyConstraints2D.None;
        _RigidBody.freezeRotation = true;
        _RigidBody.gravityScale = _DefaultGravityScale;
    }

    private void CheckIfClimbingUp()
    {
        if (_DataInstance._IsTouchingLadder && _InputInstance._IsPlayerClimbingUpLadder)
        {
            _DataInstance._LadderCollider.enabled = true;
            _IsPlayerClimbingUp = true;
        }
        else
        {
            _IsPlayerClimbingUp = false;
        }
    }

    private void CheckIfClimbingDown()
    {
        if (_DataInstance._IsTouchingLadder && _InputInstance._IsPlayerClimbingDownLadder)
        {
            _DataInstance._LadderCollider.enabled = true;
            _IsPlayerClimbingDown = true;
        }
        else
        {
            _IsPlayerClimbingDown = false;
        }
    }

    private void CheckIfJumpingOff()
    {
        if (_DataInstance._IsTouchingLadder && _InputInstance._IsPlayerJumping)
        {
            if (_InputInstance._IsPlayerJumpingOffLadder)
            {
                _ignoreClimbTimer = Time.timeSinceLevelLoad + _ignoreClimbOffset;
            }
        }
    }

    private void CheckOnLadderCollisions()
    {
        if (_InputInstance._IsPlayerJumpingOffLadder)
        {
            if (Time.timeSinceLevelLoad < _ignoreClimbTimer)
            {
                Physics2D.IgnoreLayerCollision(9, 10, true);
            }
        }
        else if (_InputInstance._IsPlayerClimbingDownLadder || _InputInstance._IsPlayerClimbingUpLadder)
        {
            if (!_InputInstance._IsPlayerJumpingOffLadder && Time.timeSinceLevelLoad > _ignoreClimbTimer)
            {
                Physics2D.IgnoreLayerCollision(9, 10, false);
            }
        }
    }
}
