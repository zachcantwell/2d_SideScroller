using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dodge : PlayerInput
{
    [SerializeField]
    private float _dodgeSpeed = 2400f;
    private Vector2 _desiredDodgePos;
    private Vector2 _positionOfHitCollider;
    private float _initialDodgeDistance = 0f;
    private float _ignoreDodgeTimer = 0f;
    private float _ignoreDodgeTimerOffset = 0.3f;
    public bool? _WasDodgeInitialized;
    private bool? _WasXFlipped;
    private const float _originPt = 0.3f;
    private const float _destinationPt = 0.3f;
    private const float _horiztonalDistOfRaycast = 0.5f;
    private bool _isItTimeToMovePosition;
    private float _dodgeLength = 1.65f;
    private float _lerpSpd = 8.25f;

    public static Dodge _DodgeInstance
    {
        get; private set;
    }

    void Awake()
    {
        InitializeDodge();
    }

    void Start()
    {
        _desiredDodgePos = Vector2.zero;
        _isItTimeToMovePosition = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_InputInstance._IsPlayerDodging)
        {
            PerformDodge();
        }
    }

    void FixedUpdate()
    {
        if (_isItTimeToMovePosition)
        {
            Vector2 newPos = Vector2.Lerp(transform.localPosition, _desiredDodgePos, Time.fixedDeltaTime * _lerpSpd);
            _DataInstance._RigidBody.MovePosition(newPos);
        }
    }

    public void InitializeDodge()
    {
        if (_WasDodgeInitialized == null || _WasDodgeInitialized == false)
        {
            if (_DodgeInstance != null && _DodgeInstance != this)
            {
                Destroy(this);
            }
            else
            {
                _DodgeInstance = this;
                _WasDodgeInitialized = true;
            }
        }
        if (_WasInitialized == null || _WasInitialized == false)
        {
            base.Initialize();
        }
        if (_WasInputInitialized == null || _WasInputInitialized == false)
        {
            InitializeInput();
        }
    }

    public bool IsAnythingBehindPlayer()
    {
        if (_WasXFlipped == null)
        {
            _WasXFlipped = _SpriteRenderer.flipX;
        }
        if (_WasXFlipped == true)
        {
            bool right = CheckRight();

            if (right == true)
            {
                return true;
            }
        }
        else if (_WasXFlipped == false)
        {
            bool left = CheckLeft();

            if (left == true)
            {
                return true;
            }
        }
        return false;
    }

    private void PerformDodge()
    {
        _isItTimeToMovePosition = false;
        bool wasHit = IsAnythingBehindPlayer();
        bool flipx = _SpriteRenderer.flipX;

        if (Time.timeSinceLevelLoad < _ignoreDodgeTimer)
        {
            return;
        }

        float direction = flipx ? 1 : -1;

        if (!_InputInstance._IgnorePlayerInput)
        {
            _InputInstance._IgnorePlayerInput = true;
            _DataInstance._RigidBody.isKinematic = true;
            _desiredDodgePos = new Vector2(transform.localPosition.x + (direction * _dodgeLength), transform.localPosition.y);
            _initialDodgeDistance = Vector2.Distance(transform.localPosition, _desiredDodgePos);
        }

        float currentDistance = Vector2.Distance(transform.localPosition, _desiredDodgePos);

        if (currentDistance / _initialDodgeDistance < 0.035f || wasHit &&  _InputInstance._IgnorePlayerInput)
        {
            Debug.LogWarning(currentDistance / _initialDodgeDistance + " = currDist/initDist");
            _DataInstance._RigidBody.isKinematic = false;
            _InputInstance._IgnorePlayerInput = false;
            _InputInstance._IsPlayerDodging = false;
            _WasXFlipped = null;
            //TODO: This results in a choppy flip
             _SpriteRenderer.flipX = !_SpriteRenderer.flipX;
            _ignoreDodgeTimer = Time.timeSinceLevelLoad + _ignoreDodgeTimerOffset;
        }
        else
        {
            _isItTimeToMovePosition = true;
        }
    }

    public bool CheckRight()
    {
        Vector2 origin = new Vector2(transform.position.x + _originPt,
                         transform.position.y - (transform.localScale.y * 0.25f)); ;

        var hit = Physics2D.LinecastAll(origin, origin + (Vector2.right * _destinationPt));
        Debug.DrawLine(origin, origin + (Vector2.right * _destinationPt), Color.blue, Time.deltaTime * 10f);

        if (hit != null)
        {
            foreach (RaycastHit2D rayRight in hit)
            {
                if (rayRight.collider)
                {
                    if (rayRight.collider.gameObject.tag == "Wall")
                    {
                        if (rayRight.distance < _horiztonalDistOfRaycast)
                        {
                            _positionOfHitCollider = rayRight.transform.localPosition;
                            return true;
                        }
                    }
                    else if (rayRight.collider.gameObject.tag == "Corner")
                    {
                        if (rayRight.distance < _horiztonalDistOfRaycast)
                        {
                            _positionOfHitCollider = rayRight.transform.localPosition;
                            return true;
                        }
                    }
                    else if (rayRight.collider.gameObject.tag == "Ground")
                    {
                        if (rayRight.distance < _horiztonalDistOfRaycast)
                        {
                            _positionOfHitCollider = rayRight.transform.localPosition;
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool CheckLeft()
    {
        Vector2 origin = new Vector2(transform.position.x - _originPt,
                                     transform.position.y - (transform.localScale.y * 0.25f));

        var hit = Physics2D.LinecastAll(origin, origin + (Vector2.left * _destinationPt));
        Debug.DrawLine(origin, origin + (Vector2.left * _destinationPt), Color.blue, Time.deltaTime * 10f);

        if (hit != null)
        {
            foreach (RaycastHit2D rayLeft in hit)
            {
                if (rayLeft.collider)
                {
                    if (rayLeft.collider.gameObject.tag == "Wall")
                    {
                        if (rayLeft.distance < _horiztonalDistOfRaycast)
                        {
                            _positionOfHitCollider = rayLeft.transform.localPosition;
                            return true;
                        }
                    }
                    else if (rayLeft.collider.gameObject.tag == "Corner")
                    {
                        if (rayLeft.distance < _horiztonalDistOfRaycast)
                        {
                            _positionOfHitCollider = rayLeft.transform.localPosition;
                            return true;
                        }
                    }
                    else if (rayLeft.collider.gameObject.tag == "Ground")
                    {
                        if (rayLeft.distance < _horiztonalDistOfRaycast)
                        {
                            _positionOfHitCollider = rayLeft.transform.localPosition;
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
}
