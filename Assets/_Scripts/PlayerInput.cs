using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerInput : PlayerData
{
    public bool _IsPlayerWallSliding
    {
        get; set;
    }

    public bool _IsPlayerSwimming
    {
        get; set;
    }

    public bool _IsPlayerDodging
    {
        get; set; 
    }

    public bool _IsPlayerSwordDrawn
    {
        get; set;
    }
    public bool _IsPlayerAirAttacking
    {
        get; set;
    }
    public bool _IsPlayerGroundSliding
    {
        get; set;
    }

    public bool _IsPlayerRunning
    {
        get; set;
    }

    public bool _IsPlayerJumping
    {
        get; set;
    }

    public bool _IsPlayerJumpingOffLadder
    {
        get; set;
    }

    public bool _IsPlayerSwinging
    {
        get; set;
    }

    public bool _IsPlayerCrouching
    {
        get; set; 
    }

    public bool _IsPlayerGrabbingObject
    {
        get; set;
    }
    
    public bool _IsPlayerGrabbingCorner
    {
        get; set; 
    }

    public bool _IsPlayerClimbingUpLadder
    {
        get; set;
    }

    public bool _IsPlayerClimbingDownLadder
    {
        get; set;
    }

    public bool _WasDirectionChanged
    {
        get; set;
    }

    public bool? _WasInputInitialized
    {
        get; set;
    }

    public float _GetHorizontalAxisValue
    {
        get; private set;
    }
    public float _GetVerticalAxisValue
    {
        get; private set;
    }
    public static PlayerInput _InputInstance
    {
        get; private set;
    }
    public bool _IsPlayerAttacking
    {
        get; private set;
    }
    public bool _IgnorePlayerInput
    {
        get; set;
    }

    private const float _climbThreshold = 0.15f;

    // Use this for initialization
    void Awake()
    {
        if (_WasInitialized == null || _WasInitialized == false)
        {
            base.Initialize();
        }

        if (_InputInstance != null && _InputInstance != this)
        {
            if (_InputInstance is PlayerInput)
            {
                Destroy(this);
            }
        }
        else
        {
            _InputInstance = this;
        }

        if (_WasInputInitialized == null)
        {
            InitializeInput();
        }
    }


    public void InitializeInput()
    {
        _IsPlayerJumping = false;
        _IsPlayerRunning = false;
        _IsPlayerDodging = false; 
        _IsPlayerSwinging = false;
        _IsPlayerGrabbingObject = false;
        _IsPlayerGrabbingCorner = false;
        _IsPlayerClimbingUpLadder = false;
        _IsPlayerClimbingDownLadder = false;
        _IsPlayerWallSliding = false;
        _IsPlayerGroundSliding = false;
        _IsPlayerSwimming = false;
        _IsPlayerSwordDrawn = false;
        _IsPlayerCrouching = false; 
        _IsPlayerAttacking = false;
        _WasDirectionChanged = false;
        _WasInputInitialized = true;
        _IgnorePlayerInput = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_IgnorePlayerInput == false && PlayerHealth._PLAYERHEALTHSTATUS != PlayerHealthStatus.IsDying)
        {
            _IsPlayerRunning = IsPlayerRunning();
            _IsPlayerJumping = IsPlayerJumping();
            _IsPlayerSwimming = IsPlayerSwimming();
            _IsPlayerSwinging = IsPlayerSwinging();
            _IsPlayerGrabbingObject = IsPlayerGrabbingObject();
            _IsPlayerGrabbingCorner = IsPlayerGrabbingCorner();
            _IsPlayerWallSliding = IsPlayerWallSliding();
            _IsPlayerClimbingUpLadder = IsPlayerClimbingUpLadder();
            _IsPlayerClimbingDownLadder = IsPlayerClimbingDownLadder();
            _IsPlayerJumpingOffLadder = IsPlayerJumpingOffLadder();
            _IsPlayerGroundSliding = IsPlayerGroundSliding();


            // Can ONLY trigger the sword drawing if the player is idle or running
            if (!_IsPlayerSwimming && !_IsPlayerSwinging && !_IsPlayerGrabbingObject && !_IsPlayerJumpingOffLadder
                && !_IsPlayerWallSliding && !_IsPlayerClimbingUpLadder && !_IsPlayerClimbingDownLadder && !_IsPlayerGroundSliding)
            {
                DidPlayerDrawSword();
                DidPlayerCrouch();
                _IsPlayerDodging = IsPlayerDodging();
            }

            _IsPlayerAttacking = IsPlayerAttacking();
            _IsPlayerAirAttacking = IsPlayerAirAttacking();

            _GetHorizontalAxisValue = CrossPlatformInputManager.GetAxis("Horizontal");
            _GetVerticalAxisValue = CrossPlatformInputManager.GetAxis("Vertical");

            if ((_IsPlayerRunning || _IsPlayerJumping || _IsPlayerGrabbingObject || _IsPlayerWallSliding) && !_IsPlayerSwinging)
            {
                if (_IsPlayerWallSliding)
                {
                    _WasDirectionChanged = false;
                }
                else
                {
                    _WasDirectionChanged = HasChangedDirection();
                }
            }
        }
       
    }

    private bool IsPlayerDodging()
    {
        if(CrossPlatformInputManager.GetButtonDown("Dodge") && _DataInstance._IsAboveSomething)
        {
            return true;
        }
        return false; 
    }

    private void DidPlayerCrouch()
    {
        if(CrossPlatformInputManager.GetButtonDown("Crouch"))
        {
            _IsPlayerCrouching = !_IsPlayerCrouching; 
            _DataInstance._PlayerBoxCollider.size =  _DataInstance._BoxColliderCrouchingDimensions; 
        }
        else
        {
            _DataInstance._PlayerBoxCollider.size =  _DataInstance._BoxColliderStandingDimensions;
        }
    }

    private bool IsPlayerRunning()
    {
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        bool isPlayerMoving = Mathf.Abs(xPos) > Mathf.Epsilon;
        return isPlayerMoving;
    }

    private bool IsPlayerGroundSliding()
    {
        if (CrossPlatformInputManager.GetButtonDown("GroundSlide") && !_IsPlayerCrouching &&
            Mathf.Abs(_RigidBody.velocity.x) > 0 && (_DataInstance._IsAboveCorner || _DataInstance._IsAboveGround))
        {
            return true;
        }
        return false;
    }

    private bool IsPlayerSwimming()
    {
        if (_DataInstance._IsTouchingWater)
        {
            float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
            bool isPlayerMoving = Mathf.Abs(xPos) > Mathf.Epsilon;
            _IsPlayerCrouching = false; 
            return isPlayerMoving;
        }
        return false;
    }

    private bool IsPlayerAttacking()
    {
        if (_IsPlayerSwordDrawn && CrossPlatformInputManager.GetButtonDown("Attack01"))
        {
            return true;
        }
        return false;
    }

    private bool IsPlayerAirAttacking()
    {
        if (_IsPlayerAttacking && !_DataInstance._IsAboveSomething && !_DataInstance._IsTouchingWater && !_IsPlayerWallSliding)
        {
            return true;
        }
        return false;
    }

    private bool IsPlayerWallSliding()
    {
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        bool isPlayerWallSliding = Mathf.Abs(xPos) > Mathf.Epsilon;

        if (_DataInstance._IsHorizontalToWall || _DataInstance._IsHorizontalToCorner)
        {
            if (isPlayerWallSliding && !_IsPlayerJumping)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsPlayerJumping()
    {
        if (CrossPlatformInputManager.GetButtonDown("Jump") && !_IsPlayerCrouching)
        {
            return true;
        }
        return false;
    }

    private bool IsPlayerJumpingOffLadder()
    {
        if (CrossPlatformInputManager.GetButtonDown("Jump") && _DataInstance._IsTouchingLadder && !_IsPlayerCrouching)
        {
            return true;
        }
        return false;
    }

    private bool IsPlayerSwinging()
    {
        if (CrossPlatformInputManager.GetButton("Swing"))
        {
            return true;
        }
        return false;
    }

    private bool IsPlayerGrabbingObject()
    {
        // Cant perform this function if sword is drawn
        if (_IsPlayerSwordDrawn || _DataInstance._IsTouchingWater || _IsPlayerCrouching)
        {
            return false;
        }
        if (CrossPlatformInputManager.GetButton("Grab") && 
            (_DataInstance._IsHorizontalToGrabableObject || _DataInstance._grabbedObject != null))
        {
            return true;
        }
        return false;
    }

    private bool IsPlayerGrabbingCorner()
    {
        if(_IsPlayerSwordDrawn || _IsPlayerCrouching)
        {
            return false; 
        }
        if(CrossPlatformInputManager.GetButton("Grab") && _DataInstance._IsHorizontalToCorner)
        {
            return true; 
        }
        return false;
    }

    private bool IsPlayerClimbingUpLadder()
    {
        // Cant perform this function if sword is drawn
        if (_IsPlayerSwordDrawn || _IsPlayerCrouching)
        {
            return false;
        }
        float yPos = CrossPlatformInputManager.GetAxisRaw("Vertical");
        bool isPlayerPushingUp = yPos > _climbThreshold;
        return isPlayerPushingUp;
    }

    private bool IsPlayerClimbingDownLadder()
    {
        // Cant perform this function if sword is drawn
        if (_IsPlayerSwordDrawn || _IsPlayerCrouching)
        {
            return false;
        }
        float yPos = CrossPlatformInputManager.GetAxisRaw("Vertical");
        bool isPlayerPushingDown = yPos < -_climbThreshold;
        return isPlayerPushingDown;
    }

    private void DidPlayerDrawSword()
    {
        if (CrossPlatformInputManager.GetButtonDown("DrawSword"))
        {
            if (_DataInstance._IsTouchingWater || !_DataInstance._IsAboveSomething)
            {
                return;
            }
            else
            {
                _IsPlayerSwordDrawn = !_IsPlayerSwordDrawn;
            }
        }
    }


    private bool HasChangedDirection()
    {
        if (CrossPlatformInputManager.GetAxis("Horizontal") > 0 && _SpriteRenderer.flipX)
        {
            return true;
        }
        else if (CrossPlatformInputManager.GetAxis("Horizontal") < 0 && !_SpriteRenderer.flipX)
        {
            return true;
        }
        return false;
    }
}
