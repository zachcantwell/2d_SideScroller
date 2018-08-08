using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class GrapplingHook : PlayerInput
{
    [SerializeField]
    private LineRenderer _lineRenderer;
    [SerializeField]
    private Vector2 _releaseVector;
    [SerializeField]
    private Vector2 _jumpReleaseVector;

    private DistanceJoint2D _distanceJoint;

    private Vector2 pos1;
    private Vector2 pos2;
    private Vector2 _lastPosition;

    private static GrapplingHook _ghInstance;
    private GrapplingHookHolder _gh;

    private bool? _isLineRendererExtended;
    private bool _hasPlayerEnteredHookHolderZone = false;
    private bool _isSwingingNow = false;
    private bool _wasPlayerJustFlipped = false;
    private bool? _isDistanceJointDistanceSet = false;

    private float _hookTimer = 0f;
    private float _flipLineRendererTimer = 0f;
    private float _lastDistanceVal = 0f;
    private float _startingDistanceVal = 0f;
    private int _index = 0;
    private List<float> _distanceJointPositions;

    private const float _hookTimerOffset = 1f;
    private const float _flipLineRendererTimerOffset = 0.1f;
    private const float _swingMultiplier = 10f;
    private const float _angleToBeat = 1.70f;
    private const float _handPosition = 0.625f;
    private const float _magnitudeAdjuster = 2.35f;
    private const float _minYDistance = 1.8f;
    private const float _maxYDistance = 4.2f;


    void Awake()
    {
        InitializeBase();

    }
    void Start()
    {
        InitializeHook();
    }

    private void InitializeBase()
    {
        if (_DataInstance._WasInitialized == null)
        {
            base.Initialize();
        }
    }

    private void InitializeHook()
    {
        _flipLineRendererTimer = 0f;
        _hookTimer = 0f;
        _isLineRendererExtended = null;
        _lineRenderer.enabled = false;
        _hasPlayerEnteredHookHolderZone = false;
        _isSwingingNow = false;
        _wasPlayerJustFlipped = false;
        _hasPlayerEnteredHookHolderZone = false;
        _isDistanceJointDistanceSet = false;
        _hookTimer = Time.timeSinceLevelLoad;
        _distanceJoint = _DataInstance._DistanceJoint2D;
        _isDistanceJointDistanceSet = null;
        _distanceJointPositions = new List<float>();
        _distanceJoint.enabled = false;

        if (_ghInstance != null && _ghInstance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _ghInstance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        // Can ONLY Swing if sword isnt drawn
        if (!_InputInstance._IsPlayerSwordDrawn)
        {
            PerformSwing();
        }
    }

    private void PerformSwing()
    {
        // hook timer is used to control the frequency that the player spams the controls
        if (Time.timeSinceLevelLoad > _hookTimer + _hookTimerOffset)
        {
            if (_hasPlayerEnteredHookHolderZone)
            {
                if (_gh != null)
                {
                    if (_isLineRendererExtended != true)
                    {
                        FlipLineRendererPosition();
                    }
                    if (Input.GetKeyDown(KeyCode.LeftControl) && _isLineRendererExtended == null)
                    {
                        EnableLineRenderer();
                    }
                    else if (Input.GetKey(KeyCode.LeftControl) && _isLineRendererExtended == false)
                    {
                        SettingInitialHookPositions();
                    }
                    else if (Input.GetKey(KeyCode.LeftControl) && _isLineRendererExtended == true && _isSwingingNow)
                    {
                        SetYPos();
                        SetXVeloc();
                        UpdateHookPositions();
                    }
                }
                if ((Input.GetKeyUp(KeyCode.LeftControl) && (_isSwingingNow)))
                {
                    LaunchPlayer();
                }

                //If the player is grounded and its enabled, retract the hook.
                if (_lineRenderer.enabled && _DataInstance._IsAboveSomething)
                {
                    if (_isLineRendererExtended != null || _isSwingingNow)
                    {
                        StartCoroutine("RetractLineRenderer");
                    }
                }
            }
            else
            {
                //If the player is outside the swing zone, retract the hook
                if (_lineRenderer.enabled)
                {
                    if (_isLineRendererExtended != null || _isSwingingNow)
                    {
                        StartCoroutine("RetractLineRenderer");
                    }
                }
            }
        }
    }

    private void SetXVeloc()
    {
        float hor = CrossPlatformInputManager.GetAxis("Horizontal");
        float swingSpeed = 45f;
        float maxXVelocity = 13f;
        Debug.LogWarning(_DataInstance._RigidBody.velocity);
        if (CrossPlatformInputManager.GetAxis("Horizontal") > 0.1f && _lastPosition.x < transform.position.x)
        {
            if (Mathf.Abs(_DataInstance._RigidBody.velocity.x) < maxXVelocity)
            {
                _DataInstance._RigidBody.AddForce(Vector2.right * swingSpeed * hor, ForceMode2D.Force);
            }
        }
        else if (CrossPlatformInputManager.GetAxis("Horizontal") < -0.1f && _lastPosition.x > transform.position.x)
        {
            if (Mathf.Abs(_DataInstance._RigidBody.velocity.x) < maxXVelocity)
            {
                _DataInstance._RigidBody.AddForce(Vector2.left * swingSpeed * Mathf.Abs(hor), ForceMode2D.Force);
            }
        }
    }

    private void SetYPos()
    {
        float vert = CrossPlatformInputManager.GetAxis("Vertical");

        if (_isDistanceJointDistanceSet == null)
        {
            _distanceJointPositions.Clear();
            float pos = _minYDistance;
            while (pos < _maxYDistance)
            {
                pos += 0.05f;
                _distanceJointPositions.Add(pos);
            }

            for (int i = 0; i < _distanceJointPositions.Count; i++)
            {
                if (_distanceJoint.distance > _distanceJointPositions[i])
                {
                    _lastDistanceVal = _distanceJointPositions[i];
                }
                else
                {
                    _startingDistanceVal = _distanceJointPositions[i];
                    break;
                }
            }
            _index = _distanceJointPositions.IndexOf(_startingDistanceVal);
            _distanceJoint.distance = _distanceJointPositions[_index];
            _isDistanceJointDistanceSet = false;
        }

        if (vert > 0)
        {
            if (_index > 0 && _index < _distanceJointPositions.Count)
            {
                _index--;
                _distanceJoint.distance = _distanceJointPositions[_index];
                Debug.LogWarning("Index = " + _index + " Value of index = " + _distanceJointPositions[_index]);
            }
        }
        if (vert < 0)
        {
            if (_index >= 0 && _index < _distanceJointPositions.Count - 1)
            {
                _index++;
                _distanceJoint.distance = _distanceJointPositions[_index];
                Debug.LogWarning("Index = " + _index + " Value of index = " + _distanceJointPositions[_index]);
            }
        }
    }


    private void EnableLineRenderer()
    {
        _lineRenderer.enabled = true;
        _isLineRendererExtended = false;
        _lineRenderer.SetPosition(0, pos1);
        _lineRenderer.SetPosition(1, pos1);
    }

    private void SettingInitialHookPositions()
    {
        _lineRenderer.SetPosition(0, pos1);
        Vector3 refVel = _DataInstance._RigidBody.velocity;
        Vector3 currentPos = _lineRenderer.GetPosition(1);

        currentPos = Vector3.SmoothDamp(currentPos, pos2, ref refVel, Time.deltaTime * 0.5f, 60f);
        currentPos = Vector3.Lerp((Vector2)currentPos, pos2, Time.deltaTime * 6);
        _lineRenderer.SetPosition(1, currentPos);

        float originalDistance = Vector2.Distance(pos1, pos2);
        float currentDistance = Vector2.Distance(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(1));

        if (currentDistance / originalDistance > 0.75f && _distanceJoint.enabled == false)
        {
            _distanceJoint.enabled = true;
            _distanceJoint.connectedAnchor = _gh.transform.position;
            _distanceJoint.distance = originalDistance;
            _isSwingingNow = true;
        }
        if ((Vector2)_lineRenderer.GetPosition(1) == pos2 || currentDistance / originalDistance > 0.99f)
        {
            _isLineRendererExtended = true;
            _lineRenderer.SetPosition(1, pos2);
            _distanceJoint.distance = originalDistance;
        }
    }

    private void UpdateHookPositions()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _lastPosition = transform.position;
            ReleasePlayerWithExtremePower();
            StartCoroutine("RetractLineRenderer");
        }
        else
        {
            float angle = Vector2.Angle(pos1, pos2);

            if (angle < _angleToBeat)
            {
                _DataInstance._RigidBody.AddForce(_DataInstance._RigidBody.velocity * _swingMultiplier,
                                                     ForceMode2D.Force);
            }
            FlipAndRotatePlayer();
            FlipLineRendererPosition();

            _lastPosition = transform.position;
        }
    }


    private void FlipAndRotatePlayer()
    {
        //This Flips the player from left to right when he gets to 
        // the highest point on either the left or right side of the hookything.
        if (_lastPosition.x < transform.position.x && _lastPosition.x < _gh.transform.position.x)
        {
            if (_DataInstance._SpriteRenderer.flipX == true)
            {
                _wasPlayerJustFlipped = true;
                _DataInstance._SpriteRenderer.flipX = false;
                _flipLineRendererTimer = Time.timeSinceLevelLoad + (_flipLineRendererTimerOffset /
                                                                    _DataInstance._RigidBody.velocity.magnitude);
            }
        }
        else if (_lastPosition.x > transform.position.x && _lastPosition.x > _gh.transform.position.x)
        {
            if (_DataInstance._SpriteRenderer.flipX == false)
            {
                _wasPlayerJustFlipped = true;
                _DataInstance._SpriteRenderer.flipX = true;
                _flipLineRendererTimer = Time.timeSinceLevelLoad + (_flipLineRendererTimerOffset /
                                                                    _DataInstance._RigidBody.velocity.magnitude);
            }
        }
    }

    private void FlipLineRendererPosition()
    {
        //This sets the linerenderers first position in the players hand
        if (_DataInstance._SpriteRenderer.flipX == false)
        {
            pos1 = (Vector2)transform.position + (Vector2)transform.right * _handPosition;
        }
        else
        {
            pos1 = (Vector2)transform.position + -(Vector2)transform.right * _handPosition;
        }
        pos2 = _gh.transform.position;

        Interpolater();
    }

    private void Interpolater()
    {
        float speed = 1f;
        Vector2 lineRendererHandlePos = _lineRenderer.GetPosition(0);

        if (_wasPlayerJustFlipped)
        {
            if (Time.timeSinceLevelLoad < _flipLineRendererTimer)
            {
                speed = 15f * Time.deltaTime;
            }
            else
            {
                speed = 1f;
                _wasPlayerJustFlipped = false;
            }
        }
        lineRendererHandlePos = Vector3.Lerp(lineRendererHandlePos, pos1, speed);
        _lineRenderer.SetPosition(0, lineRendererHandlePos);
    }

    private void LaunchPlayer()
    {
        if (_isLineRendererExtended == true || _isLineRendererExtended == false)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ReleasePlayerWithExtremePower();
                StartCoroutine("RetractLineRenderer");
            }
            else
            {
                ReleasePlayerGently();
                StartCoroutine("RetractLineRenderer");
            }
        }
    }

    private void ReleasePlayerGently()
    {
        if (_lastPosition.x > transform.position.x)
        {
            _DataInstance._RigidBody.velocity = new Vector2(-_releaseVector.x, _releaseVector.y);
        }
        else
        {
            _DataInstance._RigidBody.velocity = _releaseVector;
        }
    }

    private void ReleasePlayerWithExtremePower()
    {
        if (_lastPosition.x < transform.position.x && _lastPosition.x < _gh.transform.position.x)
        {
            _DataInstance._RigidBody.AddForce(_DataInstance._RigidBody.velocity.magnitude / _magnitudeAdjuster
                                              * new Vector2(-_jumpReleaseVector.x, _jumpReleaseVector.y), ForceMode2D.Impulse);
        }
        else if ((_lastPosition.x < transform.position.x && _lastPosition.x >= _gh.transform.position.x))
        {
            _DataInstance._RigidBody.AddForce(_DataInstance._RigidBody.velocity.magnitude / _magnitudeAdjuster
                                            * new Vector2(-_jumpReleaseVector.x, _jumpReleaseVector.y), ForceMode2D.Impulse);
        }
        else if (_lastPosition.x >= transform.position.x && _lastPosition.x < _gh.transform.position.x)
        {
            _DataInstance._RigidBody.AddForce(_DataInstance._RigidBody.velocity.magnitude / _magnitudeAdjuster
                                            * new Vector2(-_jumpReleaseVector.x, _jumpReleaseVector.y), ForceMode2D.Impulse);
        }
        else if (_lastPosition.x >= transform.position.x && _lastPosition.x >= _gh.transform.position.x)
        {
            _DataInstance._RigidBody.AddForce(_DataInstance._RigidBody.velocity.magnitude / _magnitudeAdjuster
                                            * new Vector2(-_jumpReleaseVector.x, _jumpReleaseVector.y), ForceMode2D.Impulse);
        }
    }

    public void HasPlayerEnteredHookHolderZone(bool hook)
    {
        _hasPlayerEnteredHookHolderZone = hook;
    }

    public void SetGrapplingHookHolder(GrapplingHookHolder gh)
    {
        _gh = gh;
    }

    private IEnumerator RetractLineRenderer()
    {
        _distanceJointPositions.Clear();
        _distanceJoint.enabled = false;
        _isSwingingNow = false;
        _isDistanceJointDistanceSet = null;
        _lastDistanceVal = 0f;
        _startingDistanceVal = 0f;
        _index = 0;

        while (_lineRenderer.enabled)
        {
            _lineRenderer.SetPosition(0, pos1);

            Vector3 refVel = _DataInstance._RigidBody.velocity;
            Vector3 currentPos = _lineRenderer.GetPosition(1);

            currentPos = Vector3.SmoothDamp(currentPos, pos1, ref refVel, Time.deltaTime * 0.725f, 60f);
            currentPos = Vector3.Lerp((Vector2)currentPos, pos1, Time.deltaTime * 2.25f);
            _lineRenderer.SetPosition(1, currentPos);

            float originalDistance = Vector2.Distance(pos1, pos2);
            float currentDistance = Vector2.Distance(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(1));

            if (currentDistance / originalDistance < 0.01f)
            {
                _lineRenderer.enabled = false;
                _isLineRendererExtended = null;
                _hookTimer = Time.timeSinceLevelLoad;
                break;
            }
            yield return null;
        }
        yield return null;
        StopCoroutine("RetractLineRenderer");
    }

}
