using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerData : MonoBehaviour
{
    public bool? _WasInitialized
    {
        get; private set;
    }
    public bool _IsTouchingLadder
    {
        get; private set; 
    }
    public bool _IsTouchingWater
    {
        get; private set; 
    }
    public bool _IsTouchingGrabbable
    {
        get; private set; 
    }
    public bool _IsTouchingCorner
    {
        get; private set; 
    }
    //Horizontal Bools
    public bool _IsHorizontalToSomething
    {
        get; private set;
    }
    public bool _IsHorizontalToCorner
    {
        get; private set;
    }
    public bool _IsHorizontalToGround
    {
        get; private set;
    }
    public bool _IsHorizontalToWall
    {
        get; private set;
    }
    public bool _IsHorizontalToGrabableObject
    {
        get; private set;
    }
    public GameObject _grabbedObject
    {
        get;set; 
    }

    //Above Bools
    public bool _IsAboveSomething
    {
        get; private set;
    }
    public bool _IsAboveGround
    {
        get; private set;
    }
    public bool _IsAboveGrabbableObject
    {
        get; private set;
    }
    public bool _IsAboveCorner
    {
        get; private set;
    }
    public bool _IsAboveWall
    {
        get; private set;
    }
    public bool _IsAboveLadder
    {
        get; private set; 
    }

    //Horizontal Raycasts
    public RaycastHit2D _HorizontalWallRaycast
    {
        get; private set;
    }
    public RaycastHit2D _HorizontalCornerRaycast
    {
        get; private set;
    }
    public RaycastHit2D _HorizontalGrabbableRaycast
    {
        get; private set;
    }
    public RaycastHit2D _HorizontalGroundRaycast
    {
        get; private set;
    }

    //Above Raycasts
    public RaycastHit2D _AboveCornerRaycast
    {
        get; private set;
    }
    public RaycastHit2D _AboveGroundRaycast
    {
        get; private set;
    }
    public RaycastHit2D _AboveGrabbableRaycast
    {
        get; private set;
    }
    public RaycastHit2D _AboveWallRaycast
    {
        get; private set;
    }
    public RaycastHit2D _AboveLadderRaycast
    {
        get; private set; 
    }

    public Vector2 _LastPlayerPosition
    {
        get; private set;
    }
    public Vector2 _LastGroundedPosition
    {
        get; private set; 
    }
    public Vector2 _BoxColliderStandingDimensions
    {
        get; private set; 
    }
    public Vector2 _BoxColliderCrouchingDimensions
    {
        get; private set; 
    }
    public float _DefaultGravityScale
    {
        get; private set; 
    }
    public float _DefaultDrag
    {
        get; private set; 
    }
    public CircleCollider2D _PlayerCircleCollider
    {
        get; private set; 
    }
    public BoxCollider2D _PlayerBoxCollider
    {
        get; private set; 
    }

    public Collider2D _LadderCollider
    {
        get; private set; 
    }

    private const float _downDistanceOfRaycast = 0.1f;
    private const float _horiztonalDistanceOfRaycast = 0.1f;
    private const float _originPoint = 0.2f;
    private const float _destinationPoint = 0.2f;
    private const float _originOfXPosOffset = 0.235f; 

    public Rigidbody2D _RigidBody
    {
        get; private set;
    }
    public Animator _Animator
    {
        get; private set;
    }
    public Collider2D[] _Collider2D
    {
        get; private set;
    }
    public SpriteRenderer _SpriteRenderer
    {
        get; set;
    }
    public DistanceJoint2D _DistanceJoint2D
    {
        get; private set; 
    }

    public static PlayerData _DataInstance
    {
        get; private set;
    }

    public JUMPSTATUS _JumpState
    {
        get; set; 
    }


    void Awake()
    {
        if (_DataInstance != null && _DataInstance != this)
        {
            if(_DataInstance is PlayerData)
            {
                 Destroy(this);
            }
        }
        else
        {
            _DataInstance = this;
        }

        if(_WasInitialized == null)
        {
             Initialize();
        }
    }

    public void Initialize()
    {
        _LastGroundedPosition = Vector2.zero; 
        _LastPlayerPosition = Vector2.zero;
        _RigidBody = GetComponent<Rigidbody2D>();
        _DefaultDrag = _RigidBody.drag;
        _DefaultGravityScale = _RigidBody.gravityScale; 
        _Collider2D = GetComponents<Collider2D>();
        _PlayerBoxCollider = GetComponent<BoxCollider2D>();
        _PlayerCircleCollider = GetComponent<CircleCollider2D>();
        _DistanceJoint2D = GetComponent<DistanceJoint2D>();
        _BoxColliderStandingDimensions = _PlayerBoxCollider.size; 
        _BoxColliderCrouchingDimensions = new Vector2(_BoxColliderStandingDimensions.x/2.5f, _BoxColliderStandingDimensions.y);
        _LadderCollider = null;  
        _JumpState = JUMPSTATUS.None;
        SetBoolsToFalse();

        if (_SpriteRenderer == null)
        {
            SpriteRenderer[] sp = GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer sprite in sp)
            {
                if (sprite.gameObject.name == "Sprite")
                {
                    _SpriteRenderer = sprite;
                }
            }
        }

        if (_Animator == null)
        {
            Animator[] an = GetComponentsInChildren<Animator>();

            foreach (Animator anim in an)
            {
                if (anim.gameObject.name == "Sprite")
                {
                    _Animator = anim;
                }
            }
        }
        _WasInitialized = true;
    }

    public void SetBoolsToFalse()
    {
        _IsAboveCorner = false;
        _IsAboveGround = false;
        _IsAboveGrabbableObject = false;
        _IsAboveWall = false;
        _IsAboveSomething = false;
        _IsAboveLadder = false; 
        _IsHorizontalToCorner = false;
        _IsHorizontalToGrabableObject = false;
        _IsHorizontalToGround = false;
        _IsHorizontalToWall = false;
        _IsHorizontalToSomething = false;
    }

    void Update()
    {
        Physics2D.queriesStartInColliders = false;
        SetBoolsToFalse();

        _IsHorizontalToSomething = IsPlayerHorizontalToSomething();
        _IsAboveSomething = IsPlayerAboveSomething();
       
       // TODO: Uncomment this when needed (might use for crouching)
       // _IsBelowTopOfLadder = CheckAbovePlayersHead();

        if(_IsAboveSomething)
        {
            _LastGroundedPosition = transform.position;
        }
        _LastPlayerPosition = transform.position;
    }
    
    public bool IsPlayerHorizontalToSomething()
    {
        if (_SpriteRenderer.flipX == true)
        {
            bool left = CheckLeftOfPlayer();

            if (left == true)
            {
                return true;
            }
        }
        else if (_SpriteRenderer.flipX == false)
        {
            bool right = CheckRightOfPlayer();

            if (right == true)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPlayerAboveSomething()
    {
        // Only checking if the player is above the ground, corner or object. 
        // Not interested in being above a wall, it doesnt make sense. 
        if (CheckBelowCenterOfPlayer() || CheckBelowLeftOfPlayer() || CheckBelowRightOfPlayer())
        {
            return true;
        }
        return false;
    }

    public bool CheckRightOfPlayer()
    {
        Vector2 origin = new Vector2(transform.position.x + _originPoint,
                         transform.position.y - (transform.localScale.y * 0.25f)); ;

        var hit = Physics2D.LinecastAll(origin, origin + (Vector2.right * _destinationPoint));
        Debug.DrawLine(origin, origin + (Vector2.right * _destinationPoint), Color.yellow, Time.deltaTime);

        if (hit != null)
        {
            foreach (RaycastHit2D rayRight in hit)
            {
                if (rayRight.collider)
                {
                    if (rayRight.collider.gameObject.tag == "Wall")
                    {
                        if (rayRight.distance < _horiztonalDistanceOfRaycast)
                        {
                            Debug.Log("Right Wall hit");
                            _HorizontalWallRaycast = rayRight;
                            _IsHorizontalToWall = true;
                        }
                    }
                    else if (rayRight.collider.gameObject.tag == "Grabbable")
                    {
                        if (rayRight.distance < _horiztonalDistanceOfRaycast)
                        {
                            Debug.Log("Right Grabbable hit");
                            _HorizontalGrabbableRaycast = rayRight;
                            _IsHorizontalToGrabableObject = true;
                        }
                    }
                    else if (rayRight.collider.gameObject.tag == "Corner")
                    {
                        if (rayRight.distance < _horiztonalDistanceOfRaycast)
                        {
                            Debug.Log("Right Corner hit");
                            _HorizontalCornerRaycast = rayRight;
                            _IsHorizontalToCorner = true;
                        }
                    }
                    else if (rayRight.collider.gameObject.tag == "Ground")
                    {
                        if (rayRight.distance < _horiztonalDistanceOfRaycast)
                        {
                            Debug.Log("Right Ground hit");
                            _HorizontalGroundRaycast = rayRight;
                            _IsHorizontalToGround = true;
                        }
                    }
                }
            }
            if (_IsHorizontalToCorner || _IsHorizontalToGrabableObject || _IsHorizontalToGrabableObject || _IsHorizontalToWall)
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckLeftOfPlayer()
    {
        Vector2 origin = new Vector2(transform.position.x - _originPoint,
                                     transform.position.y - (transform.localScale.y * 0.25f));

        var hit = Physics2D.LinecastAll(origin, origin + (Vector2.left * _destinationPoint));
        Debug.DrawLine(origin, origin + (Vector2.left * _destinationPoint), Color.yellow, Time.deltaTime);

        if (hit != null)
        {
            foreach (RaycastHit2D rayLeft in hit)
            {
                if (rayLeft.collider)
                {
                    if (rayLeft.collider.gameObject.tag == "Wall")
                    {
                        if (rayLeft.distance < _horiztonalDistanceOfRaycast)
                        {
                            Debug.Log("Left Wall hit");
                            _HorizontalWallRaycast = rayLeft;
                            _IsHorizontalToWall = true;
                        }
                    }
                    else if (rayLeft.collider.gameObject.tag == "Grabbable")
                    {
                        if (rayLeft.distance < _horiztonalDistanceOfRaycast)
                        {
                            Debug.Log("Left Grabbable hit");
                            _HorizontalGrabbableRaycast = rayLeft;
                            _IsHorizontalToGrabableObject = true;
                        }
                    }
                    else if (rayLeft.collider.gameObject.tag == "Corner")
                    {
                        if (rayLeft.distance < _horiztonalDistanceOfRaycast)
                        {
                            Debug.Log("Left Corner hit");
                            _HorizontalCornerRaycast = rayLeft;
                            _IsHorizontalToCorner = true;
                        }
                    }
                    else if (rayLeft.collider.gameObject.tag == "Ground")
                    {
                        if (rayLeft.distance < _horiztonalDistanceOfRaycast)
                        {
                            Debug.Log("Left Corner hit");
                            _HorizontalGroundRaycast = rayLeft;
                            _IsHorizontalToGround = true;
                        }
                    }
                }
            }
            if (_IsHorizontalToCorner || _IsHorizontalToGrabableObject || _IsHorizontalToGrabableObject || _IsHorizontalToWall)
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckBelowCenterOfPlayer()
    {
        Vector2 origin = new Vector2(transform.position.x,
                                     transform.position.y + 0.1f - transform.localScale.y * 0.5f);

        var hit = Physics2D.LinecastAll(origin, origin + Vector2.down / 4);
        Debug.DrawLine(origin, origin + Vector2.down / 4, Color.cyan, Time.deltaTime);

        if (hit != null)
        {
            foreach (RaycastHit2D rayCenter in hit)
            {
                if (rayCenter.collider)
                {
                    if (rayCenter.collider.gameObject.tag == "Grabbable")
                    {
                        if (rayCenter.distance < _downDistanceOfRaycast)
                        {
                            Debug.Log("Above Grabbable hit");
                            _AboveGrabbableRaycast = rayCenter;
                            _IsAboveGrabbableObject = true;
                        }
                    }
                    else if (rayCenter.collider.gameObject.tag == "Corner")
                    {
                        if (rayCenter.distance < _downDistanceOfRaycast)
                        {
                            Debug.Log("Above Corner hit");
                            _AboveCornerRaycast = rayCenter;
                            _IsAboveCorner = true;
                        }
                    }
                    else if (rayCenter.collider.gameObject.tag == "Ground")
                    {
                        if (rayCenter.distance < _downDistanceOfRaycast)
                        {
                            Debug.Log("Above Ground hit");
                            _AboveGroundRaycast = rayCenter;
                            _IsAboveGround = true;
                        }
                    }
                    else if (rayCenter.collider.gameObject.tag == "LadderTop" || rayCenter.collider.gameObject.tag == "Ladder" )
                    {
                        if (rayCenter.distance < _downDistanceOfRaycast)
                        {
                            Debug.Log("Above Ladder hit");
                            _AboveLadderRaycast = rayCenter;
                            _IsAboveLadder = true;
                        }
                    }
                }
            }
            if (_IsAboveCorner || _IsAboveGround || _IsAboveGrabbableObject || _IsAboveLadder)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckBelowRightOfPlayer()
    {
        Vector2 origin = new Vector2(transform.position.x + transform.localScale.x * _originOfXPosOffset,
                                     transform.position.y + 0.1f - transform.localScale.y * 0.5f);

        var hit = Physics2D.LinecastAll(origin, origin + Vector2.down / 4);
        Debug.DrawLine(origin, origin + Vector2.down / 4, Color.cyan, Time.deltaTime);

        if (hit != null)
        {
            foreach (RaycastHit2D rayRight in hit)
            {
                if (rayRight.collider)
                {
                    if (rayRight.collider.gameObject.tag == "Grabbable")
                    {
                        if (rayRight.distance < _downDistanceOfRaycast)
                        {
                            Debug.Log("Above Grabbable hit");
                            _AboveGrabbableRaycast = rayRight;
                            _IsAboveGrabbableObject = true;
                        }
                    }
                    else if (rayRight.collider.gameObject.tag == "Corner")
                    {
                        if (rayRight.distance < _downDistanceOfRaycast)
                        {
                            Debug.Log("Above Corner hit");
                            _AboveCornerRaycast = rayRight;
                            _IsAboveCorner = true;
                        }
                    }
                    else if (rayRight.collider.gameObject.tag == "Ground")
                    {
                        if (rayRight.distance < _downDistanceOfRaycast)
                        {
                            Debug.Log("Above ground hit");
                            _AboveGroundRaycast = rayRight;
                            _IsAboveGround = true;
                        }
                    }
                }
            }
            if (_IsAboveCorner || _IsAboveGround || _IsAboveGrabbableObject)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckBelowLeftOfPlayer()
    {
        //Checking Left Raycast Here
        Vector2 origin = new Vector2(transform.position.x - transform.localScale.x * _originOfXPosOffset,
                                 transform.position.y + 0.1f - transform.localScale.y * .5f);

        var hit = Physics2D.LinecastAll(origin, origin + Vector2.down / 4);
        Debug.DrawLine(origin, origin + Vector2.down / 4, Color.cyan, Time.deltaTime);

        if (hit != null)
        {
            foreach (RaycastHit2D rayLeft in hit)
            {
                if (rayLeft.collider)
                {
                     if (rayLeft.collider.gameObject.tag == "Grabbable")
                    {
                        if (rayLeft.distance < _downDistanceOfRaycast)
                        {
                            Debug.Log("Above Grabbable hit");
                            _AboveGrabbableRaycast = rayLeft;
                            _IsAboveGrabbableObject = true;
                        }
                    }
                    else if (rayLeft.collider.gameObject.tag == "Corner")
                    {
                        if (rayLeft.distance < _downDistanceOfRaycast)
                        {
                            Debug.Log("Above Corner hit");
                            _AboveCornerRaycast = rayLeft;
                            _IsAboveCorner = true;

                        }
                    }
                    else if (rayLeft.collider.gameObject.tag == "Ground")
                    {
                        if (rayLeft.distance < _downDistanceOfRaycast)
                        {
                            Debug.Log("Above Ground hit");
                            _AboveGroundRaycast = rayLeft;
                            _IsAboveGround = true;
                        }
                    }
                }
            }
            if (_IsAboveCorner || _IsAboveGround || _IsAboveGrabbableObject)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckAbovePlayersHead()
    {
        Vector2 origin = new Vector2(transform.position.x,
                                     transform.position.y + 0.2f + transform.localScale.y * 0.5f);

        var hit = Physics2D.LinecastAll(origin, origin + Vector2.up/3);
        Debug.DrawLine(origin, origin + Vector2.up/3, Color.cyan, Time.deltaTime);

        if (hit != null)
        {
            foreach (RaycastHit2D rayAbove in hit)
            {
                if (rayAbove.collider)
                {
                    Debug.Log(rayAbove.collider.gameObject + " was hit above player");
                    if (rayAbove.collider.gameObject.tag == "Ladder")
                    {
                        Debug.Log("Ladder is Above Player Rayhit");
                        if (rayAbove.distance < _downDistanceOfRaycast)
                        {
                           // _IsBelowTopOfLadder = true; 
                        }
                    }
                    
                }
            }
           // if (_IsBelowTopOfLadder)
           // {
          //      return true;
           // }
        }
        return false;
    }

    void OnTriggerEnter2D(Collider2D other)
	{
		if(other.gameObject.tag == "Ladder")
		{
			_IsTouchingLadder = true; 
            _LadderCollider =  other.gameObject.GetComponent<Collider2D>();
		}
        if(other.gameObject.tag == "Water")
        {
            _IsTouchingWater = true; 
        }
        if(other.gameObject.tag == "Grabbable")
        {
            _IsTouchingGrabbable = true;
        }
        if(other.gameObject.tag == "Corner")
        {
            _IsTouchingCorner = true; 
        }
	}

    void OnTriggerStay2D(Collider2D other)
    {
        if(other.gameObject.tag == "Water")
        {
            _IsTouchingWater = true; 
        }
        if(other.gameObject.tag == "Grabbable")
        {
            _IsTouchingGrabbable = true;
        }
        if(other.gameObject.tag == "Corner")
        {
            _IsTouchingCorner = true; 
        }
    }

	void OnTriggerExit2D(Collider2D other)
	{
		if(other.gameObject.tag == "Ladder")
		{
			_IsTouchingLadder = false; 
            _LadderCollider = null; 
		}
        if(other.gameObject.tag == "Water")
        {
            _IsTouchingWater = false; 
        }
        if(other.gameObject.tag == "Grabbable")
        {
            _IsTouchingGrabbable = false;
        }
        if(other.gameObject.tag == "Corner")
        {
            _IsTouchingCorner = false; 
        }
	}
}
