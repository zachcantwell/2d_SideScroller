using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class CornerGrab : PlayerInput
{
    public enum CornerGrabStatus
    {
        None,
        GrabCorner,
        HangFromCorner,
        ClimbUpOnCorner
    }

    [SerializeField]
    private float _moveToCornerSpeed = 100f;
    private float _initialDistance = 0f;
    private Vector2 _desiredPos;
    private Vector3 _cornerPosition;
    public static CornerGrabStatus _CORNERSTATUS;

    public bool? _WasCornerGrabInitialized
    {
        get; set;
    }

    public static CornerGrab _CornerGrabInstance
    {
        get; private set;
    }

    // Use this for initialization
    void Awake()
    {
        InitializeCornerGrab();
    }


    void Update()
    {
        if (_CORNERSTATUS == CornerGrabStatus.None)
        {
            bool grabWasPressed = Input.GetKey(KeyCode.E);
            bool grabIsHeld = Input.GetKeyUp(KeyCode.E);

            if (CheckIfHorizontalToCornerInWater() && (grabIsHeld || grabWasPressed))
            {
                _CORNERSTATUS = CornerGrabStatus.GrabCorner;
            }
        }
        else if (_CORNERSTATUS == CornerGrabStatus.GrabCorner)
        {
            SetCornerInformation();
        }
        else if (_CORNERSTATUS == CornerGrabStatus.HangFromCorner)
        {
            HangPlayerFromCorner();
        }
        else if (_CORNERSTATUS == CornerGrabStatus.ClimbUpOnCorner)
        {
            MovePlayerAboveCornerPosition();
        }
    }



    public void InitializeCornerGrab()
    {
        if (_WasInitialized == null || _WasInitialized == false)
        {
            base.Initialize();
        }
        if (_WasInputInitialized == null || _WasInputInitialized == false)
        {
            InitializeInput();
        }

        if (_WasCornerGrabInitialized == null || _WasCornerGrabInitialized == false)
        {
            if (_CornerGrabInstance != null && _CornerGrabInstance != this)
            {
                Destroy(this);
            }
            else
            {
                _CornerGrabInstance = this;
                _WasCornerGrabInitialized = true;
            }
        }
        _desiredPos = Vector2.zero;
        _cornerPosition = Vector3.zero;
        _CORNERSTATUS = CornerGrabStatus.None;
    }


    private void SetCornerInformation()
    {
        float normal = _DataInstance._HorizontalCornerRaycast.normal.x;
        Vector2 point = _DataInstance._HorizontalCornerRaycast.point;
        Vector3 scale = _DataInstance._HorizontalCornerRaycast.transform.localScale;
        Vector3 pos = _DataInstance._HorizontalCornerRaycast.transform.localPosition;

        //TODO: May want to use this to round to 0.5f!!!!
        //float xRounded = Mathf.Sign(point.x) * (Mathf.Abs((int)point.x) + 0.5f);
        //float yRounded = Mathf.Sign(point.y) * (Mathf.Abs((int)point.y) + 0.5f);

        float xRounded = Mathf.Round(point.x);
        float yRounded = Mathf.Round(point.y);

        // normal < 0 means you hit a negative normal. So, it would be a corner on the 
        // righthand side of the player 
        if (normal < 0)
        {
            Debug.LogWarning(_cornerPosition + " = IF cornerPosition");
            _cornerPosition = new Vector2(pos.x - scale.x/2, pos.y + scale.y/2);
            _desiredPos = new Vector2(point.x + Mathf.Abs(normal / 8), point.y + (scale.y * 2.15f));
        }
        else
        {
            Debug.LogWarning(_cornerPosition + " = ELSE cornerPosition");
            _cornerPosition = new Vector2(pos.x + scale.x/2, pos.y + scale.y/2);
            _desiredPos = new Vector2(point.x - (normal / 8), point.y + (scale.y * 2.15f));
        }
        _initialDistance = Vector2.Distance(transform.localPosition, _desiredPos);
        _CORNERSTATUS = CornerGrabStatus.HangFromCorner;
    }

    private void HangPlayerFromCorner()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Physics2D.IgnoreLayerCollision(10, 14, true);
            _CORNERSTATUS = CornerGrabStatus.HangFromCorner;
            _RigidBody.isKinematic = true;
            _DataInstance._RigidBody.MovePosition(_cornerPosition);
        }
        else if (Input.GetKey(KeyCode.E) && CrossPlatformInputManager.GetAxisRaw("Vertical") > 0.75f)
        {
            _CORNERSTATUS = CornerGrabStatus.ClimbUpOnCorner;
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            Physics2D.IgnoreLayerCollision(10, 14, false);
            _CORNERSTATUS = CornerGrabStatus.None;
            _RigidBody.isKinematic = false;

            _RigidBody.constraints = RigidbodyConstraints2D.None;
            _RigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        }
        else if (Input.GetKey(KeyCode.E))
        {
            _CORNERSTATUS = CornerGrabStatus.HangFromCorner;
            _RigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void MovePlayerAboveCornerPosition()
    {
        Physics2D.IgnoreLayerCollision(10, 14, true);
        _RigidBody.isKinematic = true;
        _InputInstance._IgnorePlayerInput = true;

        _moveToCornerSpeed = Mathf.Lerp(_moveToCornerSpeed, 14f, Time.deltaTime);
        Vector2 newPos = Vector2.MoveTowards(transform.localPosition, _desiredPos, Time.deltaTime * _moveToCornerSpeed);
        _RigidBody.MovePosition(newPos);

        float dist = Vector2.Distance(transform.localPosition, _desiredPos);
        if (dist / _initialDistance <= 0.3f)
        {
            _CORNERSTATUS = CornerGrabStatus.None;
            _RigidBody.constraints = RigidbodyConstraints2D.None;
            _RigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
            Physics2D.IgnoreLayerCollision(10, 14, false);

            _moveToCornerSpeed = 1.65f;
            _RigidBody.isKinematic = false;
            _InputInstance._IgnorePlayerInput = false;
        }
    }

    private bool CheckIfHorizontalToCornerInWater()
    {
        if (_DataInstance._IsHorizontalToCorner)
        {
            return true;
        }
        return false;
    }
}

