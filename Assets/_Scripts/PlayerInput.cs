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

    public bool _IsPlayerSwordDrawn
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

    public bool _IsPlayerGrabbing
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
        _IsPlayerSwinging = false;
        _IsPlayerGrabbing = false;
        _IsPlayerClimbingUpLadder = false;
        _IsPlayerClimbingDownLadder = false;
        _IsPlayerWallSliding = false;
        _IsPlayerGroundSliding = false;
        _IsPlayerSwimming = false;
        _IsPlayerSwordDrawn = false; 
        _WasDirectionChanged = false;
        _WasInputInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        _IsPlayerRunning = IsPlayerRunning();
        _IsPlayerJumping = IsPlayerJumping();
        _IsPlayerSwimming = IsPlayerSwimming();
        _IsPlayerSwinging = IsPlayerSwinging();
        _IsPlayerGrabbing = IsPlayerGrabbing();
        _IsPlayerWallSliding = IsPlayerWallSliding();
        _IsPlayerClimbingUpLadder = IsPlayerClimbingUpLadder();
        _IsPlayerClimbingDownLadder = IsPlayerClimbingDownLadder();
        _IsPlayerJumpingOffLadder = IsPlayerJumpingOffLadder();
        _IsPlayerGroundSliding = IsPlayerGroundSliding();

        // Can ONLY trigger the sword drawing if the player is idle or running
        if(!_IsPlayerJumping && !_IsPlayerSwimming && !_IsPlayerSwinging && _DataInstance._IsAboveSomething
            && !_IsPlayerGrabbing && !_IsPlayerWallSliding && !_IsPlayerClimbingUpLadder 
            && !_IsPlayerClimbingDownLadder && !_IsPlayerJumpingOffLadder && !_IsPlayerGroundSliding)
            {
               DidPlayerDrawSword();
            }

        _GetHorizontalAxisValue = CrossPlatformInputManager.GetAxis("Horizontal");
        _GetVerticalAxisValue = CrossPlatformInputManager.GetAxis("Vertical");

        if ((_IsPlayerRunning || _IsPlayerJumping || _IsPlayerGrabbing || _IsPlayerWallSliding) && !_IsPlayerSwinging)
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

    private bool IsPlayerRunning()
    {
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        bool isPlayerMoving = Mathf.Abs(xPos) > Mathf.Epsilon;
        return isPlayerMoving;
    }

    private bool IsPlayerGroundSliding()
    {
        if (CrossPlatformInputManager.GetButtonDown("GroundSlide") && Mathf.Abs(_RigidBody.velocity.x) > 0
            && (_DataInstance._IsAboveCorner || _DataInstance._IsAboveGround || _DataInstance._IsAboveGrabbableObject))
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
            return isPlayerMoving;
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
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            return true;
        }
        return false;
    }

    private bool IsPlayerJumpingOffLadder()
    {
        if (CrossPlatformInputManager.GetButtonDown("Jump") && _DataInstance._IsTouchingLadder)
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

    private bool IsPlayerGrabbing()
    {
        // Cant perform this function if sword is drawn
        if(_IsPlayerSwordDrawn)
        {
            return false; 
        }
        if (CrossPlatformInputManager.GetButton("Grab"))
        {
            return true;
        }
        return false;
    }

    private bool IsPlayerClimbingUpLadder()
    {
        // Cant perform this function if sword is drawn
        if(_IsPlayerSwordDrawn)
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
        if(_IsPlayerSwordDrawn)
        {
            return false; 
        }
        float yPos = CrossPlatformInputManager.GetAxisRaw("Vertical");
        bool isPlayerPushingDown = yPos < -_climbThreshold;
        return isPlayerPushingDown;
    }

    private void DidPlayerDrawSword()
    {
        if(CrossPlatformInputManager.GetButtonDown("DrawSword"))
        {
            _IsPlayerSwordDrawn = !_IsPlayerSwordDrawn;
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
