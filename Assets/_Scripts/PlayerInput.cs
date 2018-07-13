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

    public bool _IsPlayerRunning
    {
        get; set;
    }

    public bool _IsPlayerJumping
    {
        get; set;
    }

    public bool _IsPlayerSwinging
    {
        get;  set; 
    }

    public bool _IsPlayerGrabbing
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

    public static PlayerInput _InputInstance
    {
        get; private set; 
    }
    
    // Use this for initialization
    void Awake()
    {
        if (_WasInitialized == null || _WasInitialized == false)
        {
            base.Initialize();
        }

        if (_InputInstance != null && _InputInstance != this)
        {
            if(_InputInstance is PlayerInput)
            {
                 Destroy(this);
            }
        }
        else
        {
            _InputInstance = this;
        }

        if(_WasInputInitialized == null)
        {
             Initialize();
        }
    }

    
    public void InitializeInput()
    {
        _IsPlayerJumping = false;
        _IsPlayerRunning = false;
        _IsPlayerSwinging = false; 
        _IsPlayerGrabbing = false; 
        _IsPlayerWallSliding = false; 
		_WasDirectionChanged = false; 
        _WasInputInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        _IsPlayerRunning = IsPlayerRunning();
        _IsPlayerJumping = IsPlayerJumping();
        _IsPlayerSwinging = IsPlayerSwinging();
        _IsPlayerGrabbing = IsPlayerGrabbing();
        _IsPlayerWallSliding = IsPlayerWallSliding();

		 if((_IsPlayerRunning || _IsPlayerJumping) && !_IsPlayerSwinging)
        {
            _WasDirectionChanged = HasChangedDirection();
        }
    }

    private bool IsPlayerRunning()
    {
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        bool isPlayerMoving = Mathf.Abs(xPos) > Mathf.Epsilon;
        return isPlayerMoving;
    }

    private bool IsPlayerWallSliding()
    {
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        bool isPlayerWallSliding = Mathf.Abs(xPos) > Mathf.Epsilon;
        return isPlayerWallSliding;
    }

    private bool IsPlayerJumping()
    {
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            return true;
        }
        return false;
    }

    private bool IsPlayerSwinging()
    {
        if(CrossPlatformInputManager.GetButton("Swing"))
        {
            return true; 
        }
        return false; 
    }

    private bool IsPlayerGrabbing()
    {
        if(CrossPlatformInputManager.GetButton("Grab"))
        {
            Debug.Log("Pressing Grab");
            return true; 
        }
        return false; 
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
