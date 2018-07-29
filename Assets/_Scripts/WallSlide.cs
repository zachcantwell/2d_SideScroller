using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class WallSlide : PlayerInput
{
    [SerializeField]
    private float _resetYVelocity = 2f;
    [SerializeField]
    private float _setInitialDrag = 5f;
    [SerializeField]
    private float _setInitialGravityScale = 2.5f;
    [SerializeField]
    private float _activatedLerpSpeed = 9f;
    [SerializeField]
    private float _targetGravityScale = 1.65f;
    [SerializeField]
    private float _targetDragScale = 8.35f;

    public bool? _WasWallSlideInitialized
    {
        get; set;
    }

    public static WallSlide _WallSlideInstance
    {
        get; private set;
    }

    private bool _hasStartedWallSliding = false;

    void Awake()
    {
        InitializeWallSlide();
    }

    public void InitializeWallSlide()
    {
        if (_WasInitialized == null || _WasInitialized == false)
        {
            base.Initialize();
        }
        if (_WasInputInitialized == null || _WasInputInitialized == false)
        {
            InitializeInput();
        }

        if (_WasWallSlideInitialized == null || _WasWallSlideInitialized == false)
        {
            if (_WallSlideInstance != null && _WallSlideInstance != this)
            {
                if (_WallSlideInstance is WallSlide)
                {
                    Destroy(this);
                }
            }
            else
            {
                if (_WallSlideInstance is WallSlide)
                {
                    _WallSlideInstance = this;
                    _WasWallSlideInitialized = true;
                }
            }
        }
        _hasStartedWallSliding = false;
    }

    void FixedUpdate()
    {
        if (_InputInstance._IsPlayerWallSliding && !_InputInstance._IsPlayerGrabbing && !_InputInstance._IsPlayerSwinging && !_DataInstance._IsTouchingLadder)
        {
            if (_DataInstance._IsHorizontalToWall || _DataInstance._IsHorizontalToCorner)
            {
                if (_DataInstance._LastPlayerPosition.y > transform.position.y)
                {
                    if (!_DataInstance._IsAboveGround && !_DataInstance._IsTouchingWater && !_DataInstance._IsTouchingLadder)
                    {
                        PerformWallSlide();
                    }
                }
            }
        }
        else
        {
            CheckToResetValues();
        }
        _IsPlayerWallSliding = false;
    }

    private void PerformWallSlide()
    {
        if (Mathf.Abs(_InputInstance._GetHorizontalAxisValue) > 0.1f)
        {
            if (!_hasStartedWallSliding && _DataInstance._LastPlayerPosition.y > transform.position.y)
            {
                _hasStartedWallSliding = true;
                _DataInstance._RigidBody.gravityScale = _setInitialGravityScale;
                _DataInstance._RigidBody.drag = _setInitialDrag;
                _DataInstance._RigidBody.velocity = new Vector2(_RigidBody.velocity.x, _resetYVelocity);

            }
            else if (_hasStartedWallSliding && _DataInstance._LastPlayerPosition.y > transform.position.y)
            {

                _DataInstance._RigidBody.gravityScale = Mathf.Lerp(_DataInstance._RigidBody.gravityScale,
                                                _targetGravityScale, Time.fixedDeltaTime * _activatedLerpSpeed);

                _DataInstance._RigidBody.drag = Mathf.Lerp(_DataInstance._RigidBody.drag, _targetDragScale,
                                                                Time.fixedDeltaTime * _activatedLerpSpeed);
            }
        }
        else if (Mathf.Abs(_InputInstance._GetHorizontalAxisValue) <= 0.1f)
        {
            SetBackToDefaultValues();
        }
    }

    private void CheckToResetValues()
    {
        if (!_DataInstance._IsTouchingLadder)
        {
            if (_DataInstance._LastPlayerPosition.y < transform.position.y)
            {
                SetBackToDefaultValues();
            }
            else if (_InputInstance._IsPlayerJumping)
            {
                SetBackToDefaultValues();
            }
            else if (_DataInstance._IsAboveGround || _DataInstance._IsAboveCorner)
            {
                SetBackToDefaultValues();
            }
            else if (_InputInstance._GetHorizontalAxisValue <= 0.1f)
            {
                SetBackToDefaultValues();
            }
            else if(_DataInstance._IsTouchingWater || _DataInstance._IsTouchingLadder)
            {
                SetBackToDefaultValues();
            }
        }
    }

    private void SetBackToDefaultValues()
    {
        _hasStartedWallSliding = false;
        _DataInstance._RigidBody.drag = 0f;
        _DataInstance._RigidBody.gravityScale = _DataInstance._DefaultGravityScale;
    }


}
