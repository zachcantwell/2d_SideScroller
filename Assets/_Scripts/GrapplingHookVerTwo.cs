using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookVerTwo : PlayerControllerV2
{
    DistanceJoint2D _distanceJoint;
    public static GrapplingHookVerTwo _instance;
    private Vector2 _lastPos;
    private bool? _isLineRendererExtended;
    private Vector2 pos1;
    private Vector2 pos2;
    public static bool _isSwingingNow;

    [SerializeField]
    private float _swingMultiplier = 45f;
    [SerializeField]
    private float _releaseFromSwingMultiplier = 1.35f;
    [SerializeField]
    private float _maxSwingClamper = 1.35f;
    [SerializeField]
    private LineRenderer _lineRenderer;
    [SerializeField]
    private Vector2 _releaseVector;
    [SerializeField]
    private Vector2 _jumpReleaseVector;

    private bool _wasPlayerJustFlipped;
    private const float _hookTimerOffset = 1f;
    private float _hookTimer;
    private float _flipLineRendererTimer = 0f;
    private const float _flipLineRendererTimerOffset = 0.1f;
    private Vector2 _lineRendererHandlePos;
    void Start()
    {
        //_hookTimer = Time.timeSinceLevelLoad;
        _isLineRendererExtended = null;
        _lineRenderer.enabled = false;
        _distanceJoint = GetComponent<DistanceJoint2D>();
        _hasPlayerEnteredHookHolderZone = false;
        _isSwingingNow = false;
        _distanceJoint.enabled = false;
        _wasPlayerJustFlipped = false;
        _flipLineRendererTimer = 0f;
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
                        _lineRenderer.enabled = true;
                        _isLineRendererExtended = false;
                        _lineRenderer.SetPosition(0, pos1);
                        _lineRenderer.SetPosition(1, pos1);
                    }
                    else if (Input.GetKey(KeyCode.LeftControl) && _isLineRendererExtended == false)
                    {
                        SettingInitialHookPositions();
                    }
                    else if (Input.GetKey(KeyCode.LeftControl) && _isLineRendererExtended == true && _isSwingingNow)
                    {
                        UpdateHookPositions();
                    }
                }
                if ((Input.GetKeyUp(KeyCode.LeftControl) && (_isSwingingNow)))
                {
                    LaunchPlayer();
                }
            }
            if (_lineRenderer.enabled && (_isGrounded))
            {
                if (_isLineRendererExtended != null || _isSwingingNow)
                {
                    StartCoroutine("RetractLineRenderer");
                }
            }
        }
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
        if (_lastPos.x > _playerInstance.transform.position.x)
        {
            _rb.velocity = new Vector2(-_releaseVector.x, _releaseVector.y);
        }
        else
        {
            _rb.velocity = _releaseVector;
        }
    }

    private void ReleasePlayerWithExtremePower()
    {
        float magnitudeAdjuster = 2.35f;
        if (_lastPos.x < _playerInstance.transform.position.x && _lastPos.x < _gh.transform.position.x)
        {
            //          Debug.Log("Condition1");
            _rb.AddForce(_rb.velocity.magnitude / magnitudeAdjuster * _jumpReleaseVector, ForceMode2D.Impulse);
        }
        else if ((_lastPos.x < _playerInstance.transform.position.x && _lastPos.x >= _gh.transform.position.x))
        {
            //          Debug.Log("Condition2");
            _rb.AddForce(_rb.velocity.magnitude / magnitudeAdjuster * _jumpReleaseVector, ForceMode2D.Impulse);
        }
        else if (_lastPos.x >= _playerInstance.transform.position.x && _lastPos.x < _gh.transform.position.x)
        {
            //            Debug.Log("Condition3");
            _rb.AddForce(_rb.velocity.magnitude / magnitudeAdjuster * _jumpReleaseVector, ForceMode2D.Impulse);
        }
        else if (_lastPos.x >= _playerInstance.transform.position.x && _lastPos.x >= _gh.transform.position.x)
        {
            //           Debug.Log("Condition4");
            _rb.AddForce(_rb.velocity.magnitude / magnitudeAdjuster * new Vector2(-_jumpReleaseVector.x, _jumpReleaseVector.y), ForceMode2D.Impulse);
        }
    }

    private IEnumerator RetractLineRenderer()
    {
        _distanceJoint.enabled = false;
        _isSwingingNow = false;

        while (_lineRenderer.enabled)
        {
            _lineRenderer.SetPosition(0, pos1);

            Vector3 refVel = _rb.velocity;
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

    private void SettingInitialHookPositions()
    {
        _lineRenderer.SetPosition(0, pos1);
        Vector3 refVel = _rb.velocity;
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
            //TODO if you want the player to rotate, uncomment below code
            //_rb.freezeRotation = false;
            _isLineRendererExtended = true;
            _lineRenderer.SetPosition(1, pos2);
            _distanceJoint.distance = originalDistance;
            //TODO Clamp magnitude somewhere else, doing it here will cause it to fluctuate each time
            _rb.velocity = Vector2.ClampMagnitude(_rb.velocity, _rb.velocity.magnitude * _maxSwingClamper);
        }
    }

    private void UpdateHookPositions()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _lastPos = transform.position;
            ReleasePlayerWithExtremePower();
            StartCoroutine("RetractLineRenderer");
        }
        else
        {
            // _lineRenderer.SetPosition(0, pos1);
            // _lineRenderer.SetPosition(1, pos2);
            float angle = Vector2.Angle(pos1, pos2);

            if (angle < 1.75f)
            {
                _rb.AddForce(_rb.velocity * _swingMultiplier, ForceMode2D.Force);
            }
            FlipAndRotatePlayer();
            FlipLineRendererPosition();

            _lastPos = transform.position;

        }
    }


    private void FlipAndRotatePlayer()
    {
        //This Rotates the player with the angle of the linerenderer
        //_playerTransform.rotation = Quaternion.FromToRotation(pos1, pos2); 

        //This Flips the player from left to right when he gets to 
        // the highest point on either the left or right side of the hookything.
        if (_lastPos.x < transform.position.x && _lastPos.x < _gh.transform.position.x)
        {
            if (_spriteRenderer.flipX == true)
            {
                _wasPlayerJustFlipped = true;
                _spriteRenderer.flipX = false;
                _flipLineRendererTimer = Time.timeSinceLevelLoad + (_flipLineRendererTimerOffset / _rb.velocity.magnitude);
            }
        }
        else if (_lastPos.x > transform.position.x && _lastPos.x > _gh.transform.position.x)
        {
            if (_spriteRenderer.flipX == false)
            {
                _wasPlayerJustFlipped = true;
                _spriteRenderer.flipX = true;
                _flipLineRendererTimer = Time.timeSinceLevelLoad + (_flipLineRendererTimerOffset / _rb.velocity.magnitude);
            }
        }
    }

    private void FlipLineRendererPosition()
    {
        //This sets the linerenderers first position in the players hand
        if (_spriteRenderer.flipX == false)
        {
            pos1 = (Vector2)transform.position + (Vector2)transform.right * .625f;
        }
        else
        {
            pos1 = (Vector2)transform.position + -(Vector2)transform.right * .625f;
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
}







