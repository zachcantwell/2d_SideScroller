using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerV2 : MonoBehaviour
{
    [SerializeField]
    private float _speedForce = 50f;
    [SerializeField]
    private Transform _grounder;
    [SerializeField]
    private float _radiusS;
    [SerializeField]
    private LayerMask _groundMask;
    [SerializeField]
    private ParticleSystem _runParticles;
    [SerializeField]
    private Vector2 _jumpVec;

    public enum PlayerState { Idle, Running, Jumping, Swinging, Falling };

    public static PlayerState _PLAYERSTATE;
    private Vector3 _runParticlesRightRot = new Vector3(-125f, 90f, -90f);
    private Vector3 _runParticlesLeftRot = new Vector3(-55f, 90f, -90f);

    private bool _hasControlAlreadyBeenPressed;
    private float _walkSpeed;
    private float _moveSpeed;
    private float _runSpeed;
    private const float _hookTimerOffsetter = 1f;
    private float _theHookTimer;
    public static Rigidbody2D _rb;
    public SpriteRenderer _spriteRenderer;
    public Transform _playerTransform;
    private Animator _animator;
    public static GrapplingHookHolder _gh;
    public static bool _isGrounded;
    public static bool _hasPlayerEnteredHookHolderZone;

    public static PlayerControllerV2 _playerInstance;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _playerTransform = GetComponent<Transform>();

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
    // Use this for initialization
    void Start()
    {
        _hasControlAlreadyBeenPressed = false;
        _animator.speed = 1f;
        _theHookTimer = Time.timeSinceLevelLoad;
        _PLAYERSTATE = PlayerState.Idle;
        _hasPlayerEnteredHookHolderZone = false;
        _isGrounded = false;
        _walkSpeed = _speedForce;
        _runSpeed = _speedForce * 1.25f;

        if (_playerInstance != null && _playerInstance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _playerInstance = this;
        //   DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        InputHandler();
    }

    void OnDrawGizmos()
    {
        //Draw Ground Transform to see where we are making contact with ground 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_grounder.transform.position, _radiusS);

        // Draw Two Lines off to the sides of the player to check if we're colliding with the ground 
        Vector2 origin = new Vector2(transform.position.x + transform.localScale.x * 0.3f, transform.position.y - transform.localScale.y * .5f);
        Gizmos.DrawLine(origin, origin + Vector2.down * .5f);
        origin = new Vector2(transform.position.x - transform.localScale.x * 0.3f, transform.position.y - transform.localScale.y * .5f);
        Gizmos.DrawLine(origin, origin + Vector2.down * .5f);
        //  origin = new Vector2(transform.position.x - transform.localScale.x * 0.275f, transform.position.y - transform.localScale.y * .25f);
        //   Gizmos.DrawLine(origin, origin + Vector2.left * .5f);
        //  origin = new Vector2(transform.position.x + transform.localScale.x * 0.275f, transform.position.y - transform.localScale.y * .25f);
        //  Gizmos.DrawLine(origin, origin + Vector2.right * .5f);
    }

    void InputHandler()
    {
        IsPlayerGrounded();
        MovementControls();
        GrapplingHookControls();
        JumpingControls();
        AnimationStateControllerV2();
    }

    private void Sprinting()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && _isGrounded)
        {
            _speedForce = _runSpeed;
            _runParticles.Play();
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) && _isGrounded)
        {
            _speedForce = _walkSpeed;
            _runParticles.Stop();
        }
    }

    private void MovementControls()
    {
        // If you want sprinting, uncomment below code
        //SprintingControls();
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (_isGrounded && _PLAYERSTATE != PlayerState.Swinging)
            {
                _spriteRenderer.flipX = true;

                if (_PLAYERSTATE != PlayerState.Jumping || _PLAYERSTATE != PlayerState.Falling)
                {
                    _PLAYERSTATE = PlayerState.Running;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (_isGrounded && _PLAYERSTATE != PlayerState.Swinging)
            {
                _spriteRenderer.flipX = false;
                if (_PLAYERSTATE != PlayerState.Jumping || _PLAYERSTATE != PlayerState.Falling)
                {
                    _PLAYERSTATE = PlayerState.Running;
                }
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            if (_PLAYERSTATE != PlayerState.Swinging)
            {
                if (_spriteRenderer.flipX == false)
                {
                    _spriteRenderer.flipX = true;
                }
                _moveSpeed = _isGrounded ? _speedForce : _speedForce * 0.7f;
                _rb.velocity = new Vector2(-_moveSpeed, _rb.velocity.y);
                _runParticles.transform.rotation = Quaternion.Euler(_runParticlesLeftRot);

                if (_isGrounded)
                {
                    _PLAYERSTATE = PlayerState.Running;
                }
            }

        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (_PLAYERSTATE != PlayerState.Swinging)
            {
                if (_spriteRenderer.flipX == true)
                {
                    _spriteRenderer.flipX = false;
                }
                _moveSpeed = _isGrounded ? _speedForce : _speedForce * 0.7f;
                _rb.velocity = new Vector2(_moveSpeed, _rb.velocity.y);
                _runParticles.transform.rotation = Quaternion.Euler(_runParticlesRightRot);

                if (_isGrounded)
                {
                    _PLAYERSTATE = PlayerState.Running;
                }
            }
        }
        else // Player is not doing anything (NOT DOING ANYTHING)
        {
            PlayerIsIdle();
        }
    }

    private void PlayerIsIdle()
    {
        if (_isGrounded)
        {
            float yVel = _rb.velocity.y;
            _rb.velocity = new Vector2(0f, yVel);
            _PLAYERSTATE = PlayerState.Idle;
        }
        else
        {
            if (_PLAYERSTATE != PlayerState.Jumping && _PLAYERSTATE != PlayerState.Swinging)
            {
                _PLAYERSTATE = PlayerState.Falling;
            }
        }
    }

    private void GrapplingHookControls()
    {
        if (_hasPlayerEnteredHookHolderZone)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) && Time.timeSinceLevelLoad > _theHookTimer)
            {
                _PLAYERSTATE = PlayerState.Swinging;
            }
            else if (Input.GetKey(KeyCode.LeftControl) && _PLAYERSTATE == PlayerState.Swinging)
            {
                //Launch From Hook Here, this code will only be hit once!
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _PLAYERSTATE = PlayerState.Jumping;
                    _theHookTimer = Time.timeSinceLevelLoad + _hookTimerOffsetter;
                }
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) && _PLAYERSTATE == PlayerState.Swinging)
            {
                _theHookTimer = Time.timeSinceLevelLoad + _hookTimerOffsetter;
                _PLAYERSTATE = PlayerState.Jumping;
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) && _PLAYERSTATE == PlayerState.Swinging)
        {
            _theHookTimer = Time.timeSinceLevelLoad + _hookTimerOffsetter;
            _PLAYERSTATE = PlayerState.Jumping;
        }
        else if (_hasPlayerEnteredHookHolderZone == false && _PLAYERSTATE == PlayerState.Swinging)
        {
            _PLAYERSTATE = PlayerState.Jumping;
            _theHookTimer = Time.timeSinceLevelLoad + _hookTimerOffsetter;
        }
    }


    private void JumpingControls()
    {
        //Code For a Standard Jump
        if (Input.GetKeyDown(KeyCode.Space) && _PLAYERSTATE != PlayerState.Jumping)
        {
            if (_isGrounded)
            {
                _rb.AddForce(_jumpVec, ForceMode2D.Impulse);
                _PLAYERSTATE = PlayerState.Jumping;
            }
            else
            {
                _PLAYERSTATE = PlayerState.Jumping;
            }
        }
    }

    private void AnimationStateControllerV2()
    {
        if (_rb.velocity.x == 0)
        {
            //Idle
            _animator.SetBool("isRunning", false);
        }

        if (_rb.velocity.y == 0 || _PLAYERSTATE == PlayerState.Idle)
        {
            //idle part two
            _animator.SetBool("isJumping", false);
            _animator.SetBool("isFalling", false);
        }

        if (_isGrounded)
        {
            _hasControlAlreadyBeenPressed = false;
            _animator.SetBool("isSwinging", false);
            _animator.SetBool("isJumping", false);
            _animator.SetBool("isFalling", false);
        }

        if (Mathf.Abs(_rb.velocity.x) > 0  && _isGrounded) // _rb.velocity.y == 0)
        {
            //running part two
            _animator.SetBool("isRunning", true);
        }

        //Using Get Key will let you slide indefinitely
        if (Input.GetKeyDown(KeyCode.S) && Mathf.Abs(_rb.velocity.x) > 0)
        {
            _animator.SetBool("isSliding", true);
        }
        else
        {
            _animator.SetBool("isSliding", false);
        }

        // Swinging, Jumping and Falling
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKey(KeyCode.Space) || Input.GetKeyUp(KeyCode.LeftControl))
        {
            _animator.SetBool("isRunning", false);
            if (Input.GetKey(KeyCode.Space))
            {
                _animator.SetBool("isJumping", true);
                _animator.SetBool("isSwinging", false);
            }
            else if (Input.GetKeyDown(KeyCode.LeftControl) && _hasPlayerEnteredHookHolderZone)
            {
                // Keeps player from spamming the button
                if (_hasControlAlreadyBeenPressed == false)
                {
                    _hasControlAlreadyBeenPressed = true;
                    _animator.SetBool("isSwinging", true);
                    _animator.SetBool("isJumping", false);
                }

            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) && _hasPlayerEnteredHookHolderZone)
            {
                if (!_isGrounded)
                {
                    _animator.SetBool("isSwinging", false);
                    _animator.SetBool("isJumping", true);
                }
            }
        }//TODO Below Code will only be hit if the player isnt touching CTRL, resulting in a delay
        else if (!_isGrounded)
        {
            _animator.SetBool("isFalling", true);
        }
    }

    public void HasPlayerEnteredHolderZone(bool hasHe)
    {
        _hasPlayerEnteredHookHolderZone = hasHe;
    }

    public void GetGrapplingHookHolder(GrapplingHookHolder gh)
    {
        _gh = gh;
    }

    private bool IsPlayerTouchingWall()
    {
        float distance = 0.05f;
        float originPoint = 0.3f;
        float destinationPoint = 0.625f;

        Vector2 origin = new Vector2(transform.position.x - originPoint,
                                     transform.position.y - (transform.localScale.y * 0.25f));

        var hit = Physics2D.Linecast(origin, origin + (Vector2.left * destinationPoint));
        Debug.DrawLine(origin, origin + (Vector2.left * destinationPoint), Color.red, Time.deltaTime);

        if (hit.collider)
        {
            if (hit.transform.gameObject.tag == "Ground")
            {
                Debug.Log("Right Wall hit + " + hit.transform.gameObject.name);

                if (hit.distance < distance)
                {
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
                if (hit.transform.gameObject.tag == "Ground")
                {
                    Debug.Log("Left Wall hit + " + hit.transform.gameObject.name);
                    if (hit.distance < distance)
                    {
                        Debug.Log("Distance to left wall is " + hit.distance);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void IsPlayerGrounded()
    {
        Physics2D.queriesStartInColliders = false;
        bool isPlayerTouchingWall = IsPlayerTouchingWall();
        _isGrounded = Physics2D.OverlapCircle(_grounder.transform.position, _radiusS, _groundMask);

        //Double check if is grounded
        if (!_isGrounded && !isPlayerTouchingWall)
        {
            float _distance = 1f;
            Vector2 origin = new Vector2(transform.position.x + transform.localScale.x * 0.3f,
                                         transform.position.y - transform.localScale.y * 0.5f);

            var hit = Physics2D.Raycast(origin, origin + Vector2.down, _distance);

            //Checking right raycast first
            if (hit.collider)
            {
                Debug.Log(hit.transform.gameObject.name);
                if (hit.transform.gameObject.tag == "Ground")
                {
                    _isGrounded = true;
                }
                else
                {
                    _isGrounded = false;
                }
            }
            else
            {
                //Checking Left Raycast Here
                origin = new Vector2(transform.position.x - transform.localScale.x * 0.3f,
                                         transform.position.y - transform.localScale.y * .5f);

                hit = Physics2D.Raycast(origin, origin + Vector2.down, _distance);

                if (hit.collider)
                {
                    Debug.Log(hit.transform.gameObject.name);
                    if (hit.transform.gameObject.tag == "Ground")
                    {
                        _isGrounded = true;
                    }
                    else
                    {
                        _isGrounded = false;
                    }
                }
            }
        }


    }
}
