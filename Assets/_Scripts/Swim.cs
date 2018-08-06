using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Swim : PlayerInput
{
    [SerializeField]
    private float _swimMultiplier = 200f;

    public bool? _WasSwimInitialized
    {
        get; set;
    }
    public static Swim _SwimInstance
    {
        get; private set;
    }

    // Use this for initialization
    void Awake()
    {
        InitializeSwim();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_DataInstance._IsTouchingWater)
        {
            if (_InputInstance._IsPlayerSwordDrawn)
            {
                _InputInstance._IsPlayerSwordDrawn = false;
            }
            if (_InputInstance._IsPlayerSwimming)
            {
                StartSwimming();
            }
            else
            {
                IdleInWater();
            }
        }
        _InputInstance._IsPlayerSwimming = false;
    }

    public void InitializeSwim()
    {
        if (_WasInitialized == null || _WasInitialized == false)
        {
            base.Initialize();
        }
        if (_WasInputInitialized == null || _WasInputInitialized == false)
        {
            InitializeInput();
        }

        if (_WasSwimInitialized == null || _WasSwimInitialized == false)
        {
            if (_SwimInstance != null && _SwimInstance != this)
            {
                if (_SwimInstance is Swim)
                {
                    Destroy(this);
                }
            }
            else
            {
                if (_SwimInstance is Swim)
                {
                    _SwimInstance = this;
                    _WasSwimInitialized = true;
                }
            }
        }
    }

    private void StartSwimming()
    {
        float xPos = CrossPlatformInputManager.GetAxis("Horizontal");
        float playerX = xPos * _swimMultiplier * Time.fixedDeltaTime;

        Vector2 playerVelocity = new Vector2(playerX, _RigidBody.velocity.y);
        _RigidBody.velocity = playerVelocity;
    }

    private void IdleInWater()
    {
    }
}
