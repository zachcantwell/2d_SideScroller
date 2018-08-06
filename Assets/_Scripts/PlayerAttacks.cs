using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : PlayerInput
{
    [SerializeField]
    private float _resetSpeed = 100f;
    [SerializeField]
    private float _finalAttackDownSpeed = 10f;

    public static PlayerAttacks _playerAttacksInstance;
    public bool? _WerePlayerAttacksInitialized;
    private bool _wereJumpAttackValuesReset;
    private bool _isAirAttackBeingPerformed = false;
    public static bool _isAirAttackGrounded = false;
    private float _distanceOfRaycast = 0.3f;

    void Awake()
    {
        InitalizePlayerAttacks();
    }

    void Start()
    {
        _isAirAttackGrounded = false;
        _wereJumpAttackValuesReset = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_DataInstance._IsAboveSomething)
        {
            _isAirAttackBeingPerformed = CheckIfAirAttacking();
            if (_isAirAttackBeingPerformed)
            {
                _isAirAttackGrounded = CheckIfAirAttackIsGrounded();
                Debug.Log(_isAirAttackGrounded + " = isAirAttackGrounded");
            }
        }
        CheckOnDragAndGravity();
        CheckIfValuesReset();
    }

    void FixedUpdate()
    {
        if (_isAirAttackBeingPerformed)
        {
            CheckIfFinalAirAttack();
        }
    }

    private void InitalizePlayerAttacks()
    {
        if (_WasInitialized == null || _WasInitialized == false || _WasInitialized == null)
        {
            base.Initialize();
        }
        if (_InputInstance == null || _WasInputInitialized == false || _WasInputInitialized == null)
        {
            InitializeInput();
        }
        if (_playerAttacksInstance != null && _playerAttacksInstance != this)
        {
            Destroy(this);
            _WerePlayerAttacksInitialized = true;
        }
        else
        {
            _playerAttacksInstance = this;
            _WerePlayerAttacksInitialized = true;
        }
    }

    private bool CheckIfAirAttacking()
    {
        int atk = PlayerAnimations._GetAirAttackCounter;

        if ((_InputInstance._IsPlayerAirAttacking))
        {
            _wereJumpAttackValuesReset = true;
            if (!AnimationEventReciever._wasFinalAirAttackTriggererd)
            {
                if (PlayerAnimations._GetAirAttackCounter == 1 || PlayerAnimations._GetAirAttackCounter == 3)
                {
                    _DataInstance._RigidBody.gravityScale = 0f;
                    _DataInstance._RigidBody.drag = 10f;
                }
            }
            return true;
        }
        else if (atk == 1 || atk == 3 || atk == 4)
        {
            return true;
        }
        return false;
    }

    private void CheckIfFinalAirAttack()
    {
        if (AnimationEventReciever._wasFinalAirAttackTriggererd)
        {
            if ((!_DataInstance._IsAboveSomething || !_isAirAttackGrounded) && PlayerAnimations._GetAirAttackCounter == 4)
            {
                _DataInstance._RigidBody.velocity = Vector2.down * _finalAttackDownSpeed * Time.fixedDeltaTime;
            }
        }
    }

    private void CheckOnDragAndGravity()
    {
        if (_DataInstance._RigidBody.gravityScale != _DataInstance._DefaultGravityScale)
        {
            _DataInstance._RigidBody.gravityScale = Mathf.Lerp(_DataInstance._RigidBody.gravityScale,
                                                     _DataInstance._DefaultGravityScale, Time.deltaTime * _resetSpeed);
        }
        if (_DataInstance._RigidBody.drag != _DataInstance._DefaultDrag)
        {
            _DataInstance._RigidBody.drag = Mathf.Lerp(_DataInstance._RigidBody.drag, _DataInstance._DefaultDrag,
                                                        Time.deltaTime * _resetSpeed);
        }
    }

    private void CheckIfValuesReset()
    {
        if (_DataInstance._IsAboveSomething)
        {
            if (_wereJumpAttackValuesReset)
            {
                _wereJumpAttackValuesReset = false;
                _DataInstance._RigidBody.gravityScale = _DataInstance._DefaultGravityScale;
                _DataInstance._RigidBody.drag = _DataInstance._DefaultDrag;
            }
        }
    }

    private bool CheckIfAirAttackIsGrounded()
    {
        Vector2 origin = new Vector2(transform.position.x,
                                     transform.position.y + 0.1f - transform.localScale.y * 0.5f);

        var hit = Physics2D.LinecastAll(origin, origin + Vector2.down / 2);
        Debug.DrawLine(origin, origin + Vector2.down / 2, Color.yellow, Time.deltaTime);

        if (hit != null)
        {
            foreach (RaycastHit2D rayCenter in hit)
            {
                if (rayCenter.collider)
                {
                    if (rayCenter.collider.gameObject.tag == "Grabbable")
                    {
                        if (rayCenter.distance < _distanceOfRaycast)
                        {
                            return true;
                        }
                    }
                    else if (rayCenter.collider.gameObject.tag == "Corner")
                    {
                        if (rayCenter.distance < _distanceOfRaycast)
                        {
                            return true;
                        }
                    }
                    else if (rayCenter.collider.gameObject.tag == "Ground")
                    {
                        if (rayCenter.distance < _distanceOfRaycast)
                        {
                            return true;
                        }
                    }
                    else if (rayCenter.collider.gameObject.tag == "LadderTop" || rayCenter.collider.gameObject.tag == "Ladder")
                    {
                        if (rayCenter.distance < _distanceOfRaycast)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
}
