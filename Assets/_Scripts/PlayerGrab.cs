using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrab : PlayerInput
{
    [SerializeField]
    private float _grabDistance = 1f;
    [SerializeField]
    private Vector2 _throwForceVector = new Vector2(25f, 15f);

    private GameObject _hitObject;
    private RaycastHit2D _hit;

    private bool _wasGrabbed;
    private bool _wasDirectionChanged;

    private const float _baseOffset = 0.01f;
    private float _defaultObjectGravityScale;
    private float _offset;
    private Vector2 _hitPosition;
    private Rigidbody2D _grabbedRigidbody;
    
    private static PlayerGrab _GrabInstance
    {
        get; set;
    }


    void Awake()
    {
        if (_GrabInstance != null && _GrabInstance != this)
        {
            if (_GrabInstance is PlayerGrab)
            {
                Destroy(this);
            }
        }
        else
        {
            _GrabInstance = this;
        }
    }

    void Start()
    {
        InitializeGrab();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_wasGrabbed)
        {
            GrabObject();
        }
        else
        {
            bool checkLeft = false;
            bool checkRight = false;
            bool flipX = _DataInstance._SpriteRenderer.flipX;

            if (flipX)
            {
                checkLeft = CheckLeftOfBox();
            }
            else
            {
                checkRight = CheckRightOfBox();
            }

            Vector3 grabPos = GetGrabPosition(_DataInstance._SpriteRenderer.flipX);

            if (_hit.collider.IsTouchingLayers(LayerMask.GetMask("Player")))
            {
                TouchingPlayer(grabPos, checkLeft, checkRight);
            }
            else
            {
                NotTouchingPlayer(grabPos, checkLeft, checkRight);
            }
        }
    }

    void FixedUpdate()
    {
        if (_wasGrabbed)
        {
            ThrowObject();
        }
    }

    private void TouchingPlayer(Vector2 grabPos, bool checkLeft, bool checkRight)
    {
        if (!_hitObject)
        {
            _offset = _baseOffset;
        }
        Physics2D.IgnoreLayerCollision(12, 10, true);
        _grabbedRigidbody.gravityScale = 0f;

        if (checkLeft || checkRight)
        {
            if (checkLeft)
            {
                grabPos = _hitPosition + (Vector2)gameObject.transform.position;
            }
            else if (checkRight)
            {
                grabPos = _hitPosition - (Vector2)gameObject.transform.position;
            }

            _offset = _baseOffset; 
            if (_offset <= _baseOffset)
            {
                _offset = _baseOffset;
            }
        }
        else
        {
            _offset = Mathf.Lerp(_offset, 1, Time.deltaTime);
        }

         grabPos = Vector2.Lerp(_hitObject.transform.position, grabPos, _offset);
        _grabbedRigidbody.MovePosition(grabPos);
    }

    private void NotTouchingPlayer(Vector2 grabPos, bool checkLeft, bool checkRight)
    {
        if (_hitObject != null)
        {
            Physics2D.IgnoreLayerCollision(12, 10, true);
            _grabbedRigidbody.gravityScale = 0f;

            if (checkLeft || checkRight)
            {
                if (checkLeft)
                {
                    grabPos = _hitPosition + (Vector2)gameObject.transform.position;
                }
                else if (checkRight)
                {
                    grabPos = _hitPosition - (Vector2)gameObject.transform.position;
                }
                _offset = _baseOffset; 
                if (_offset <= _baseOffset)
                {
                    _offset = _baseOffset;
                }
            }
            else
            {
                _offset = Mathf.Lerp(_offset, 1, Time.deltaTime);
            }
            grabPos = Vector2.Lerp(_hitObject.transform.position ,grabPos,_offset);
                                                                                           
            _grabbedRigidbody.MovePosition(grabPos);

            if (Vector2.Distance(_hitObject.transform.position, grabPos) < 0.01f)
            {
                if (!checkLeft && !checkRight)
                {
                    Physics2D.IgnoreLayerCollision(12, 10, false);
                }
            }
        }
    }

    private void ThrowObject()
    {

        if ((!_InputInstance._IsPlayerGrabbing) && _hit.collider != null)
        {
            _wasGrabbed = false;
            _grabbedRigidbody.freezeRotation = false;

            // Input of button 'W' + releasing Grab will shoot the obj up vertically (Like in Mario World) 
            if (Input.GetKey(KeyCode.W))
            {
                _hit.rigidbody.velocity = new Vector2(_DataInstance._RigidBody.velocity.x,
                                                      _DataInstance._RigidBody.velocity.y + (_throwForceVector.y * 1.35f));
            }
            else
            {
                if (_DataInstance._SpriteRenderer.flipX == false)
                {
                    _hit.rigidbody.velocity = new Vector2(_DataInstance._RigidBody.velocity.x + _throwForceVector.x,
                                                          _DataInstance._RigidBody.velocity.y + _throwForceVector.y);
                }
                else
                {
                    _hit.rigidbody.velocity = new Vector2(_DataInstance._RigidBody.velocity.x - _throwForceVector.x,
                                                              _DataInstance._RigidBody.velocity.y + _throwForceVector.y);
                }
            }

            Physics2D.IgnoreLayerCollision(12, 10, false);
            _grabbedRigidbody.gravityScale = _defaultObjectGravityScale;
            _offset = _baseOffset; 
            _hit = new RaycastHit2D();
            _hitObject = null;
            _grabbedRigidbody = null;
        }
    }

    Vector3 GetGrabPosition(bool wasXFlipped)
    {
        Vector3 newPos = new Vector3();
        if (!wasXFlipped)
        {
            if (!_wasDirectionChanged)
            {
                _wasDirectionChanged = true;
                _offset = _baseOffset;
            }

            Vector2 pos = new Vector2(transform.position.x + 0.05f, transform.position.y + 0.2f);
            Vector2 line = Vector2.right * transform.localScale.x * _grabDistance;
            pos += line;
            newPos = new Vector3(pos.x, pos.y, transform.position.z);
        }
        else
        {
            if (_wasDirectionChanged)
            {
                _wasDirectionChanged = false;
                _offset = _baseOffset;
            }
            Vector2 pos = new Vector2(transform.position.x - 0.05f, transform.position.y + 0.2f);
            Vector2 line = Vector2.left * transform.localScale.x * _grabDistance;
            pos += line;
            newPos = new Vector3(pos.x, pos.y, transform.position.z);
        }
        return newPos;
    }

    private void GrabObject()
    {
        if (_InputInstance._IsPlayerGrabbing)
        {
            Physics2D.queriesStartInColliders = false;

            if (_DataInstance._IsHorizontalToGrabableObject)
            {
                _hit = _DataInstance._HorizontalGrabbableRaycast;
            }

            if (_hit.collider)
            {
                Debug.LogWarning("Grabbing Object");
                _wasGrabbed = true;
                _hitObject = _hit.transform.gameObject;
                _grabbedRigidbody = _hitObject.GetComponent<Rigidbody2D>();
                _defaultObjectGravityScale = _grabbedRigidbody.gravityScale;
                _grabbedRigidbody.freezeRotation = true;
                _offset = _baseOffset;
            }
        }
    }



    private void InitializeGrab()
    {
        if (base._WasInitialized == null)
        {
            base.Initialize();
        }
        if (_WasInputInitialized == null)
        {
            InitializeInput();
        }
        _wasDirectionChanged = false;
        _wasGrabbed = false;
   
        _offset = _baseOffset;
    }

    public bool CheckLeftOfBox()
    {
        Vector2 origin = new Vector2(transform.position.x - 0.15f,
                                     _hitObject.transform.position.y - (_hitObject.transform.localScale.y * 0.25f));

        Vector2 destination = new Vector2(_hitObject.transform.position.x - _hitObject.transform.localScale.x * 0.515f,
                                         _hitObject.transform.position.y - (_hitObject.transform.localScale.y * 0.25f));

        var hit = Physics2D.LinecastAll(origin, destination);
        Debug.DrawLine(origin, destination, Color.yellow, Time.deltaTime);

        if (hit != null)
        {
            foreach (RaycastHit2D rayLeft in hit)
            {
                if (rayLeft.collider)
                {
                    _hitPosition = rayLeft.point;
                    if (rayLeft.collider.gameObject.tag == "Wall")
                    {
                        _hitPosition = rayLeft.point;
                        return true;
                    }
                    else if (rayLeft.collider.gameObject.tag == "Corner")
                    {
                        _hitPosition = rayLeft.point;
                        return true;
                    }
                    else if (rayLeft.collider.gameObject.tag == "Ground")
                    {
                        _hitPosition = rayLeft.point;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool CheckRightOfBox()
    {
        Vector2 origin = new Vector2(transform.position.x + 0.15f,
                                     _hitObject.transform.position.y - (_hitObject.transform.localScale.y * 0.25f));

        Vector2 destination = new Vector2(_hitObject.transform.position.x + _hitObject.transform.localScale.x * 0.515f,
                                              _hitObject.transform.position.y - (_hitObject.transform.localScale.y * 0.25f));

        var hit = Physics2D.LinecastAll(origin, destination);
        Debug.DrawLine(origin, destination, Color.yellow, Time.deltaTime);

        if (hit != null)
        {
            foreach (RaycastHit2D rayLeft in hit)
            {
                if (rayLeft.collider)
                {
                    if (rayLeft.collider.gameObject.tag == "Wall")
                    {
                        _hitPosition = rayLeft.point;
                        return true;
                    }
                    else if (rayLeft.collider.gameObject.tag == "Corner")
                    {
                        _hitPosition = rayLeft.point;
                        return true;
                    }
                    else if (rayLeft.collider.gameObject.tag == "Ground")
                    {
                        _hitPosition = rayLeft.point;
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
