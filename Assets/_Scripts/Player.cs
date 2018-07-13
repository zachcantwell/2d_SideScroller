using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class Player : MonoBehaviour
{
    [SerializeField]
    private Transform _grounder;
    [SerializeField]
    private float _runMultiplier = 310f;
    [SerializeField]
    private float _gravityScaleOffset = 6f;
    [SerializeField]
    private float _jumpOffset = 8f;
    [SerializeField]
    private Vector2 _wallJumpSpd = new Vector2(32.5f, 69f);
    [SerializeField]
    private float _doubleJumpStrength = 0.75f;
    [SerializeField]
    private float _defaultJumpSpeed = 66f;
    [SerializeField]
    private float _radiusS = 0.275f;
    [SerializeField]
    private LayerMask _groundMask;
    [SerializeField]
    private float _distanceToWallLengthCheck = 0.5f;
    [SerializeField]
    private float _xAirSpeed = 0.875f;
    [SerializeField]
    private float _distanceToCheck = 0.275f;
    [SerializeField]
    private float _directionXModifier = 65f;

    private float _holdJumpButtonDefaultValue;
    private float _peakJumpHeight;
    private float _defaultGravityScale;
    private float _currentSpeed = 0f;
    private float _ignoreHorizontalInputTimer = 0f;
    private const float _ignoreHorizontalInputOffset = 0.175f;
    private float _defaultDirectionXModifier;
    private int _doubleJumpCounter;

    private bool _isTouchingCorner;
    private bool _isWallSliding;
    private bool _isAlive;
    private bool _isPlayerRunning;
    private bool _isPlayerJumping;
    private bool _isPlayerHoldingJumpButton;
    private bool _wasMaxHeightJumped;
    private bool _isJumping;
    private bool _isHoldingJumpButton;
    private bool _isDoubleJumpReady;
    private bool _jump = false;
    private bool _hasPlayerJumped = false;
    private bool _doubleJumpCancel = false;
    private bool _isPlayerTouchingWall = false;
    private bool _hasChangedAerialDirection = false;
    public bool _isGrounded = false;
    public bool _HasPlayerChangedDirection
    {
        get; private set;
    }

    private Vector2 _lastPos;

    public static Player _instance;
    public static PlayerStatus _PLAYERSTATUS;
    public Rigidbody2D _rigidbody;
    public Animator _animator;
    public Collider2D[] _collider2D;
    public SpriteRenderer _spriteRenderer;

    void Awake()
    {
        _isTouchingCorner = false;
        _isAlive = true;
        _isPlayerHoldingJumpButton = false;
        _isPlayerRunning = false;
        _isPlayerJumping = false;
        _isWallSliding = false;

        _PLAYERSTATUS = PlayerStatus.Idle;

        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _collider2D = GetComponents<Collider2D>();

        _defaultGravityScale = _rigidbody.gravityScale;
        _defaultDirectionXModifier = _directionXModifier;

        _isHoldingJumpButton = false;
        _isDoubleJumpReady = true;
        _isJumping = false;

        _doubleJumpCounter = 0;
        _lastPos = Vector2.zero;

        if (_spriteRenderer == null)
        {
            SpriteRenderer[] sp = GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer sprite in sp)
            {
                if (sprite.gameObject.name == "Sprite")
                {
                    _spriteRenderer = sprite;
                }
            }
        }

        if (_animator == null)
        {
            Animator[] an = GetComponentsInChildren<Animator>();

            foreach (Animator anim in an)
            {
                if (anim.gameObject.name == "Sprite")
                {
                    _animator = anim;
                }
            }
        }
    }
    void Start()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    void Update()
    {
        _isTouchingCorner = IsPlayerTouchingCorner();
        _isPlayerTouchingWall = IsPlayerTouchingWall();
        _isGrounded = IsPlayerGrounded();
        _isPlayerRunning = IsPlayerRunning();
        _isPlayerJumping = IsPlayerJumping();
        WallSlide();

    }


    void FixedUpdate()
    {
        if (_isPlayerRunning && _isGrounded)
        {
            Vector2 run = Run();
            _HasPlayerChangedDirection = ChangeDirection(run);
            _isPlayerRunning = false;
        }
        
        if (_isPlayerJumping)
        {
            Jump();
            _isPlayerJumping = false;
        }
        if (!_isPlayerJumping && !_isPlayerRunning)
        {
            IsIdle();
        }
        _lastPos = transform.position;
    }

    private bool IsPlayerRunning()
    {
        float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal");
        float playerX = controlThrow * _runMultiplier * Time.deltaTime;

        Vector2 playerVelocity = new Vector2(playerX, _rigidbody.velocity.y);

        bool isPlayerMoving = Mathf.Abs(playerVelocity.x) > Mathf.Epsilon;
        return isPlayerMoving;
    }

    private Vector2 Run()
    {
        float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal");
        float playerX = controlThrow * _runMultiplier * Time.fixedDeltaTime;

        Vector2 playerVelocity = new Vector2(playerX, _rigidbody.velocity.y);

        // This is to keep the player from immediately changing directions after wall jumping
        if (Time.timeSinceLevelLoad > _ignoreHorizontalInputTimer)
        {
            if (_isGrounded )
            {
                _rigidbody.velocity = playerVelocity;
                _hasChangedAerialDirection = false;
                _directionXModifier = _defaultDirectionXModifier;
                Debug.Log("Changing Velocity from Run");

            }
            else 
            {
                ChangeAerialSpeed(playerVelocity, playerX);
                Debug.Log("Changing Velocity from Run");
            }
        }

        bool isPlayerMoving = Mathf.Abs(playerVelocity.x) > Mathf.Epsilon;
        _animator.SetBool("isRunning", isPlayerMoving);
        return playerVelocity;
    }

    private bool ChangeDirection(Vector2 move)
    {
        if (move.x > 0 && _spriteRenderer.flipX)
        {
            if (Time.timeSinceLevelLoad > _ignoreHorizontalInputTimer)
            {
                _hasChangedAerialDirection = true;
                _spriteRenderer.flipX = false;
                return true;
            }
        }
        else if (move.x < 0 && !_spriteRenderer.flipX)
        {
            if (Time.timeSinceLevelLoad > _ignoreHorizontalInputTimer)
            {
                _hasChangedAerialDirection = true;
                _spriteRenderer.flipX = true;
                return true;
            }
        }
        return false;
    }

    private bool IsPlayerJumping()
    {
        if (_doubleJumpCounter <= 2)
        {
            if (CrossPlatformInputManager.GetButtonUp("Jump") && !_isGrounded)
            {
                return true;
            }
            if (CrossPlatformInputManager.GetButtonDown("Jump") && _isDoubleJumpReady)
            {
                _hasPlayerJumped = true;
                return true;
            }
        }

        return false;
    }

    private void Jump()
    {
        if (_hasPlayerJumped)
        {
            if (_isGrounded)
            {
                _doubleJumpCounter = 0;
            }

            if (_isPlayerTouchingWall || _isTouchingCorner)
            {
                WallJump();
            }
            else
            {
                if (_doubleJumpCounter == 0)
                {
                    // First Jump here
                    _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _defaultJumpSpeed * Time.fixedDeltaTime * _jumpOffset) ;
                    _doubleJumpCounter++;
                    Debug.Log("Changing Velocity from DoubleJump == 0");

                }
                else if (_doubleJumpCounter == 1)
                {
                    Debug.Log("Changing Velocity from DoubleJump == 1");

                    // 2nd Jump Here, do this if/else to keep players from spamming double-jump right away
                    if (Time.timeSinceLevelLoad < _ignoreHorizontalInputTimer)
                    {
                        float _reducer = 0.5f;
                        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _defaultJumpSpeed * _doubleJumpStrength * _reducer * Time.fixedDeltaTime * _jumpOffset) ;
                    }
                    else
                    {
                        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _defaultJumpSpeed * _doubleJumpStrength * Time.fixedDeltaTime * _jumpOffset) ;
                    }
                    _doubleJumpCounter++;
                }
            }
            _hasPlayerJumped = false;
        }
    }

    private void WallJump()
    {
        if (_isGrounded)
        {
            _doubleJumpCounter = 1;
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _defaultJumpSpeed) * Time.fixedDeltaTime * _jumpOffset;
            Debug.Log("Changing Velocity from WallJump isGrounded");
        }
        else
        {
            _doubleJumpCounter = 1;
            _ignoreHorizontalInputTimer = Time.timeSinceLevelLoad + _ignoreHorizontalInputOffset;
            Physics2D.queriesStartInColliders = false;

            RaycastHit2D hit;
            if (_spriteRenderer.flipX == false)
            {
                hit = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x, _distanceToWallLengthCheck);
            }
            else
            {
                hit = Physics2D.Raycast(transform.position, Vector2.left * transform.localScale.x, _distanceToWallLengthCheck);
            }

            _spriteRenderer.flipX = !_spriteRenderer.flipX;
            _hasChangedAerialDirection = true;
            Vector2 vel = new Vector2(_wallJumpSpd.x * hit.normal.x, _wallJumpSpd.y);
            vel = vel * Time.fixedDeltaTime * _jumpOffset;

            if (_isWallSliding)
            {
                vel *= 1.3f;
            }
            _rigidbody.velocity = vel;
            Debug.Log("Changing Velocity from WallJump Else isGrounded");

        }
    }

    private void WallSlide()
    {
        if (Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) > 0 && !_isGrounded && _isPlayerTouchingWall)
        {
            if (!_isWallSliding && _lastPos.y > transform.position.y)
            {
                _rigidbody.gravityScale = 2.5f;
                _rigidbody.drag = 5f;
                _isWallSliding = true;
                _rigidbody.AddForce(new Vector2(0f, _rigidbody.velocity.magnitude), ForceMode2D.Impulse);
                Debug.Log("Changing Velocity from WallSlide");

            }
            else if (_isWallSliding && _lastPos.y > transform.position.y)
            {
                float _dragSpeed = 8f;
                _rigidbody.gravityScale = Mathf.Lerp(_rigidbody.gravityScale, 1.75f, Time.deltaTime * _dragSpeed);
                _rigidbody.drag = Mathf.Lerp(_rigidbody.drag, 8.25f, Time.deltaTime * _dragSpeed);
                Debug.Log("Changing Velocity from WallSlide");
            }
        }
        else if (Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) <= 0.1f && !_isGrounded && _isPlayerTouchingWall)
        {
            if (_isWallSliding)
            {
                _isWallSliding = false;
            }
            float _dragSpeed = 2f;
            _rigidbody.gravityScale = Mathf.Lerp(_rigidbody.gravityScale, _defaultGravityScale, Time.deltaTime * _dragSpeed);
            _rigidbody.drag = Mathf.Lerp(_rigidbody.drag, 0f, Time.deltaTime * _dragSpeed);
            Debug.Log("Changing Velocity from WallSlide");
        }
        else if (_isGrounded || _isWallSliding || _rigidbody.drag > 0 || _rigidbody.gravityScale != _defaultGravityScale)
        {
            {
                _isWallSliding = false;
                _rigidbody.drag = 0f;
                _rigidbody.gravityScale = _defaultGravityScale;
                Debug.Log("Changing Velocity from WallSlide");
            }
        }
    }

    private void ChangeAerialSpeed(Vector2 playerVelocity, float playerX)
    {
        if (_hasChangedAerialDirection)
        {
            float maxXspeed = 4.35f;

            playerVelocity = new Vector2(playerX * _xAirSpeed * _directionXModifier, _rigidbody.velocity.y);

            if (Mathf.Abs(_rigidbody.velocity.x) < maxXspeed)
            {
                _rigidbody.AddForce(playerVelocity, ForceMode2D.Force);

                if (_directionXModifier > 11f)
                {
                    _directionXModifier /= 1.45f;
                }
            }
            _hasChangedAerialDirection = false;
        }
        else if(!_isGrounded )
        {
            playerVelocity = new Vector2(playerX * _xAirSpeed, _rigidbody.velocity.y);
            _rigidbody.velocity = playerVelocity;
        }
    }

    private bool IsPlayerTouchingLadder()
    {
        return false;
    }

    private bool IsPlayerClimbingLadder(bool isTouchingLadder)
    {
        return false;
    }

    private void Climbing()
    {
        float controlThrowY = CrossPlatformInputManager.GetAxis("Vertical");
        float climbSpdY = controlThrowY * Time.fixedDeltaTime;

        float controlThrowX = CrossPlatformInputManager.GetAxis("Horizontal");
        float climbSpdX = controlThrowX * Time.fixedDeltaTime;

        Vector2 playerVelocity = new Vector2(climbSpdX, climbSpdY);
        _rigidbody.velocity = playerVelocity;
        bool isPlayerClimbingY = Mathf.Abs(_rigidbody.velocity.y) > Mathf.Epsilon;
        bool isPlayerClimbingX = Mathf.Abs(_rigidbody.velocity.x) > Mathf.Epsilon;

        if (isPlayerClimbingY || isPlayerClimbingX)
        {
            _animator.SetBool("isClimbing", true);
            _animator.speed = 1f;

        }
        else
        {
            _animator.speed = 0f;
        }
    }

    private bool IsPlayerTouchingCorner()
    {
        bool isTouchingCorner = false;
        foreach (Collider2D col in _collider2D)
        {
            isTouchingCorner = col.IsTouchingLayers(LayerMask.GetMask("Corner"));

            if (isTouchingCorner)
            {
                Debug.Log("Hitting corner ");
                return true;
            }
        }
        return false;
    }

    private bool IsPlayerTouchingWall()
    {
        bool isTouchingWall = false;
        foreach (Collider2D col in _collider2D)
        {
            isTouchingWall = col.IsTouchingLayers(LayerMask.GetMask("Wall"));

            if (isTouchingWall)
            {
                Debug.Log("Hitting wall");
                return true;
            }
        }

        float distance = 0.03f;
        float originPoint = 0.3f;
        float destinationPoint = 0.1f;

        Vector2 origin = new Vector2(transform.position.x - originPoint,
                                     transform.position.y - (transform.localScale.y * 0.25f));

        var hit = Physics2D.Linecast(origin, origin + (Vector2.left * destinationPoint));
        Debug.DrawLine(origin, origin + (Vector2.left * destinationPoint), Color.red, Time.deltaTime);

        if (hit.collider)
        {
            if (hit.transform.gameObject.tag == "Wall")
            {

                if (hit.distance < distance)
                {
                    Debug.Log("Right Wall hit + " + hit.transform.gameObject.name);
                    Debug.Log("Distance to right wall is " + hit.distance);
                    return true;
                }
            }
        }
        else
        {
            origin = new Vector2(transform.position.x + originPoint,
                                 transform.position.y - (transform.localScale.y * 0.25f)); ;

            hit = Physics2D.Linecast(origin, origin + (Vector2.right * destinationPoint));

            Debug.DrawLine(origin, origin + (Vector2.right * destinationPoint), Color.red, Time.deltaTime);
            if (hit.collider)
            {
                if (hit.transform.gameObject.tag == "Wall")
                {
                    if (hit.distance < distance)
                    {
                        Debug.Log("Left Wall hit + " + hit.transform.gameObject.name);
                        Debug.Log("Distance to left wall is " + hit.distance);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void IsIdle()
    {
        if (CrossPlatformInputManager.GetAxis("Horizontal") == 0 && CrossPlatformInputManager.GetAxis("Vertical") == 0)
        {
            if (!Input.anyKey)
            {
                if (_isGrounded)
                {
                    _rigidbody.velocity = Vector2.zero;
                }
            }
        }
    }

    private bool IsPlayerGrounded()
    {
        Physics2D.queriesStartInColliders = false;

        bool isGrounded = false;
        foreach (Collider2D col in _collider2D)
        {
            isGrounded = col.IsTouchingLayers(LayerMask.GetMask("Ground"));
            if (isGrounded)
            {
                return true;
            }

            isGrounded = col.IsTouchingLayers(LayerMask.GetMask("Corner"));
            if (isGrounded)
            {
                return true;
            }
        }
        if (isGrounded)
        {
            return true;
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(_grounder.transform.position, _radiusS, _groundMask);

            if (!isGrounded)
            {
                isGrounded = Physics2D.OverlapCircle(_grounder.transform.position, _radiusS, (LayerMask.GetMask("Corner")));
            }

            if (isGrounded)
            {
                return true;
            }
        }
        return false;
    }
}