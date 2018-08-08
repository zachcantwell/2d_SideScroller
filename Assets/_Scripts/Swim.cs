using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Swim : PlayerInput
{
    [SerializeField]
    private float _swimMultiplier = 200f;
    private BuoyancyEffector2D _waterEffector;
    private GameObject _waterSurface;
    private EdgeCollider2D _waterSurfaceEdgeCollider;
    private BoxCollider2D _waterSurfaceBoxCollider;
    private float _waterDensity;

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

        float yPos = CrossPlatformInputManager.GetAxis("Vertical");
        float playerY = yPos * _swimMultiplier * Time.fixedDeltaTime;

        Vector2 playerVelocity = new Vector2(playerX, playerY);
        _RigidBody.velocity = playerVelocity;

        if (Mathf.Sign(yPos) < 0)
        {
            if(_waterEffector)
            {
                _waterEffector.density = 9f;
            }
        }
        else if(Mathf.Sign(yPos) > 0)
        {
            if(_waterEffector)
            {
               _waterEffector.density = _waterDensity;
            }
            if(_waterSurface)
            {
                if(transform.position.y >= _waterSurface.transform.position.y - 0.35f)
                {
                    _RigidBody.velocity = new Vector2(_RigidBody.velocity.x, 0f); 
                }
            }
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BuoyancyEffector2D>())
        {
            _waterEffector = other.GetComponent<BuoyancyEffector2D>();
            _waterDensity = _waterEffector.density;
        }
        if (other.GetComponent<Collider2D>().gameObject.tag == "WaterSurface")
        {
            _waterSurface = other.gameObject;
            _waterSurfaceBoxCollider = _waterSurface.GetComponent<BoxCollider2D>();
            _waterSurfaceEdgeCollider = _waterSurface.GetComponent<EdgeCollider2D>();
        }
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other == _waterEffector)
        {
            _waterEffector.density = _waterDensity;
            _waterEffector = null;
        }
        if (other == _waterSurface)
        {
            _waterSurface = null;
        }
    }
}
