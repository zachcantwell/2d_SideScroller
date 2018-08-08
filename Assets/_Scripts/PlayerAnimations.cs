using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerAnimations : PlayerInput
{
    public static PlayerAnimations _PlayerAnimationsInstance
    {
        get; private set;
    }
    public bool? _WerePlayerAnimationsInitialized
    {
        get; set;
    }
    public static int _GetAirAttackCounter
    {
        get; private set;
    }

    private bool _hasControlAlreadyBeenPressed = false;
    private bool _hasPlayerEnteredHookZone = false;
    private float _attackTimer = 0f;
    private float _airAttackTimer = 0f;
    private const float _attackTimerOffset = 0.25f;

    void Awake()
    {
        InitializePlayerAnimations();
    }

    void Update()
    {
        AnimationStateController();
    }

    private void AnimationStateController()
    {
        CheckForDodgingAnimations();
        CheckForSwordDrawnAnimation();
        CheckForIdleAnimation();
        CheckForFallingAnimation();
        CheckForClimbingAnimation();
        CheckForRunningAnimation();
        CheckForCrouchingAnimations();
        CheckForGroundSlideAnimation();
        CheckForGrabbingAnimation();
        CheckForCornerGrabAnimations();
        CheckForJumpAnimations();
        CheckForSwingingAnimation();
        CheckForWallSlideAnimation();
        CheckForSwimmingAnimations();
        CheckForSwordGroundAttackAnimation();
        CheckForSwordAirAttackAnimation();
        CheckForPlayerDamagedAnimations();
    }

    public void InitializePlayerAnimations()
    {
        if (_WerePlayerAnimationsInitialized == null || _WerePlayerAnimationsInitialized == false)
        {
            if (_PlayerAnimationsInstance != null && _PlayerAnimationsInstance != this)
            {
                if (_PlayerAnimationsInstance is PlayerAnimations)
                {
                    Debug.LogWarning("Destroying PlayerAnimations");
                    Destroy(this);
                }
            }
            else
            {
                if (_PlayerAnimationsInstance is PlayerAnimations)
                {
                    _PlayerAnimationsInstance = this;
                    _WerePlayerAnimationsInitialized = true;
                    Debug.LogWarning("Setting PlayerAnimations");
                }
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
        _hasControlAlreadyBeenPressed = false;
    }

    public void HasPlayerEnteredHookZone(bool hasHe)
    {
        _hasPlayerEnteredHookZone = hasHe;
    }

    private bool CheckForSwordDrawnAnimation()
    {
        if (_InputInstance._IsPlayerSwordDrawn)
        {
            _Animator.SetBool("isSwordDrawn", true);
            return true;
        }
        else
        {
            _Animator.SetBool("isSwordDrawn", false);
            return false;
        }
    }

    //********************************************/
    // All Sword Sheathed Animations Are Below Here!!! 
    //********************************************/

    private void CheckForIdleAnimation()
    {
        if (_RigidBody.velocity.x == 0 )
        {
            //Idle
            _Animator.SetBool("isWalkingRunningSprinting", false);
            _Animator.SetBool("isWalking", false);
            _Animator.SetBool("isRunning", false);
            _Animator.SetBool("isSprinting", false);
        }
        if (_RigidBody.velocity.y == 0)
        {
            //idle part two
            _Animator.SetBool("isJumping", false);
            _Animator.SetBool("isDoubleJumping", false);
            _Animator.SetBool("isGrounded", true);
            _Animator.SetBool("isFalling", false);
        }
    }

    private void CheckForFallingAnimation()
    {
        if (_DataInstance._IsAboveGround || _DataInstance._IsAboveGrabbableObject || _DataInstance._IsAboveCorner
            || _DataInstance._IsAboveWall || _DataInstance._IsAboveLadder) 
        {
            _hasControlAlreadyBeenPressed = false;
            _Animator.SetBool("isSwinging", false);
            _Animator.SetBool("isJumping", false);
            _Animator.SetBool("isDoubleJumping", false);
            _Animator.SetBool("isFalling", false);
            _Animator.SetBool("isGrounded", true);
        }
        else if (_DataInstance._IsTouchingWater)
        {
            _Animator.SetBool("isFalling", false);
        }
        else
        {
            _Animator.SetBool("isFalling", true);
        }
    }

    private void CheckForCrouchingAnimations()
    {
        if (_InputInstance._IsPlayerCrouching)
        {
            if (Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) > 0.1f)
            {
                _Animator.SetBool("isCrouchingMove", true);
                _Animator.SetBool("isCrouchingIdle", false);
            }
            else
            {
                _Animator.SetBool("isCrouchingMove", false);
                _Animator.SetBool("isCrouchingIdle", true);
            }
        }
        else
        {
            _Animator.SetBool("isCrouchingMove", false);
            _Animator.SetBool("isCrouchingIdle", false);
        }
    }

    private void CheckForRunningAnimation()
    {
        float xAxis = Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal"));

        if (xAxis > 0.05f)
        {
            _Animator.SetBool("isWalkingRunningSprinting", true);
            if (xAxis > 0.05f && xAxis < 0.40f && _DataInstance._IsAboveSomething && !_DataInstance._IsTouchingWater)
            {
                _Animator.SetBool("isWalking", true);
                _Animator.SetBool("isRunning", false);
                _Animator.SetBool("isSprinting", false);
                _Animator.SetBool("isGrounded", true);
            }
            else if(xAxis > 0.40f && xAxis < 0.95f && _DataInstance._IsAboveSomething && !_DataInstance._IsTouchingWater)
            {
                _Animator.SetBool("isWalking", false);
                _Animator.SetBool("isRunning", true);
                _Animator.SetBool("isSprinting", false);
                _Animator.SetBool("isGrounded", true);
            }
            else if(xAxis > 0.8f && _DataInstance._IsAboveSomething && !_DataInstance._IsTouchingWater)
            {
                _Animator.SetBool("isWalking", false);
                _Animator.SetBool("isRunning", false);
                _Animator.SetBool("isSprinting", true);
                _Animator.SetBool("isGrounded", true);
            }
        }
        else
        {
            _Animator.SetBool("isWalkingRunningSprinting", false);
            _Animator.SetBool("isWalking", false);
            _Animator.SetBool("isRunning", false);
            _Animator.SetBool("isSprinting", false);
        }
    }

    private void CheckForClimbingAnimation()
    {
        if (_DataInstance._IsTouchingLadder)
        {
            if (_DataInstance._IsAboveGround || _DataInstance._IsAboveGrabbableObject || _DataInstance._IsAboveCorner)
            {
                _Animator.SetBool("isGrounded", true);
            }
            else if (_InputInstance._IsPlayerClimbingUpLadder)
            {
                _Animator.SetBool("isClimbingLadder", true);
                _Animator.SetBool("isGrounded", false);
                _Animator.SetFloat("isReversed", 1.0f);
            }
            else if (_InputInstance._IsPlayerClimbingDownLadder)
            {
                if (!_DataInstance._IsAboveSomething)
                {
                    _Animator.SetBool("isClimbingLadder", true);
                    _Animator.SetFloat("isReversed", -1.0f);
                }
                else
                {
                    _Animator.SetBool("isGrounded", false);
                    _Animator.SetBool("isFalling", false);
                }
            }
            else
            {
                _Animator.SetFloat("isReversed", 0f);
            }
        }
        else if (!_DataInstance._IsTouchingLadder)
        {
            _Animator.SetBool("isClimbingLadder", false);
            _Animator.SetFloat("isReversed", 1f);
        }
    }

    private void CheckForGrabbingAnimation()
    {
        if (CrossPlatformInputManager.GetButton("Grab") &&
        (_DataInstance._IsHorizontalToGrabableObject || _DataInstance._grabbedObject != null))
        {
            _Animator.SetBool("isGrabbing", true);
        }
        else
        {
            _Animator.SetBool("isGrabbing", false);
        }
    }

    private void CheckForGroundSlideAnimation()
    {
        //Using Get Key will let you slide indefinitely
        if (!_DataInstance._IsTouchingWater)
        {
            if (CrossPlatformInputManager.GetButtonDown("GroundSlide") && Mathf.Abs(_RigidBody.velocity.x) > 0
             && (_DataInstance._IsAboveCorner || _DataInstance._IsAboveGround || _DataInstance._IsAboveGrabbableObject))
            {
                 Debug.LogWarning("Inside GroundSlideAnimation() == true");
                _Animator.SetBool("isSliding", true);
                _Animator.SetBool("isWalkingRunningSprinting", false);
                _Animator.SetBool("isRunning", false);
                _Animator.SetBool("isSprinting", false);
                _Animator.SetBool("isWalking", false);
            }
            if(AnimationEventReciever._isItTheLastFrameOfGroundSliding)
            {
                _Animator.SetBool("isSliding",false);
                AnimationEventReciever._isItTheLastFrameOfGroundSliding = false; 
            }
        }
    }

    private void CheckForJumpAnimations()
    {
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            _Animator.SetBool("isWalkingRunningSprinting", false);
            if (PlayerData._DataInstance._JumpState == JUMPSTATUS.NormalJump)
            {
                _Animator.SetBool("isJumping", true);
                _Animator.SetBool("isDoubleJumping", false);
                _Animator.SetBool("isSwinging", false);
                _Animator.SetBool("isClimbingLadder", false);
                _Animator.SetBool("isGrounded", false);
            }
            else if (PlayerData._DataInstance._JumpState == JUMPSTATUS.DoubleJump)
            {
                _Animator.SetBool("isDoubleJumping", true);
                _Animator.SetBool("isGrounded", false);
            }
        }
    }

    private void CheckForSwingingAnimation()
    {
        // Swinging, Jumping and Falling
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl))
        {
            _Animator.SetBool("isWalkingRunningSprinting", false);
            if (Input.GetKeyDown(KeyCode.LeftControl) && _hasPlayerEnteredHookZone)
            {
                // Keeps player from spamming the button
                if (_hasControlAlreadyBeenPressed == false)
                {
                    _hasControlAlreadyBeenPressed = true;
                    _Animator.SetBool("isSwinging", true);
                    _Animator.SetBool("isJumping", false);
                    _Animator.SetBool("isDoubleJumping", false);
                }
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) && _hasPlayerEnteredHookZone)
            {
                if (!_DataInstance._IsAboveSomething)
                {
                    _Animator.SetBool("isSwinging", false);
                    _Animator.SetBool("isJumping", false);
                    _Animator.SetBool("isGrounded", false);
                    _Animator.SetBool("isDoubleJumping", false);
                }
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Space))
            {
                _Animator.SetBool("isSwinging", false);
                _Animator.SetBool("isJumping", false);
                _Animator.SetBool("isGrounded", false);
                _Animator.SetBool("isDoubleJumping", false);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && !_hasPlayerEnteredHookZone)
            {
                _Animator.SetBool("isSwinging", false);
            }
        }

    }

    private void CheckForCornerGrabAnimations()
    {
        if (CornerGrab._CORNERSTATUS == CornerGrab.CornerGrabStatus.GrabCorner ||
            CornerGrab._CORNERSTATUS == CornerGrab.CornerGrabStatus.HangFromCorner)
        {
            _Animator.SetBool("isHanging", true);

        }
        else if (CornerGrab._CORNERSTATUS == CornerGrab.CornerGrabStatus.ClimbUpOnCorner)
        {
            _Animator.SetBool("isClimbingOntopOfCorner", true);
        }
        else
        {
            _Animator.SetBool("isClimbingOntopOfCorner", false);
            _Animator.SetBool("isHanging", false);
        }
    }

    private void CheckForWallSlideAnimation()
    {
        if (_InputInstance._IsPlayerWallSliding && !_InputInstance._IsPlayerGrabbingObject && !_InputInstance._IsPlayerSwinging)
        {
            if (_DataInstance._IsHorizontalToWall || _DataInstance._IsHorizontalToCorner)
            {
                if (_DataInstance._LastPlayerPosition.y > transform.position.y)
                {
                    if (!_DataInstance._IsAboveGround && !_DataInstance._IsTouchingWater && !_DataInstance._IsTouchingLadder)
                    {
                        _Animator.SetBool("isWallSliding", true);
                    }
                }
            }
        }
        else
        {
            if (_InputInstance._GetHorizontalAxisValue < 0.1f)
            {
                _Animator.SetBool("isWallSliding", false);
            }
            if (_DataInstance._LastPlayerPosition.y < transform.position.y)
            {
                _Animator.SetBool("isWallSliding", false);
            }
            else if (_InputInstance._IsPlayerJumping)
            {
                _Animator.SetBool("isWallSliding", false);
            }
            else if (_DataInstance._IsAboveGround || _DataInstance._IsAboveCorner)
            {
                _Animator.SetBool("isWallSliding", false);
            }
            else if (_DataInstance._IsTouchingWater || _DataInstance._IsTouchingLadder)
            {
                _Animator.SetBool("isWallSliding", false);
            }
        }
    }

    private void CheckForDodgingAnimations()
    {
        if (_InputInstance._IsPlayerDodging)
        {
            if (AnimationEventReciever._isItTheFirstFrameOfDodging)
            {
                AnimationEventReciever._isItTheFirstFrameOfDodging = false;
                _SpriteRenderer.flipX = !_SpriteRenderer.flipX;
            }
            _Animator.SetBool("isDodging", true);
        }
        else if (AnimationEventReciever._isItTheLastFrameOfDodging)
        {
            AnimationEventReciever._isItTheLastFrameOfDodging = false;
            _Animator.SetBool("isDodging", false);
        }
    }

    private void CheckForSwimmingAnimations()
    {
        if (_DataInstance._IsTouchingWater)
        {
            _Animator.SetBool("isInWater", true);
            _Animator.SetBool("isGrounded", false);
            _Animator.SetBool("isCrouchingMove", false);
            _Animator.SetBool("isCrouchingIdle", false);
            _Animator.SetBool("isSwordDrawn", false);

            if (Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) > 0.1f)
            {
                //This triggers the Swimming Animation
                _Animator.SetBool("isSwimming", true);
                _Animator.SetBool("isJumping", false);
            }
            else
            {
                //This triggers the IdleInWater Animation
                _Animator.SetBool("isSwimming", false);
                _Animator.SetBool("isJumping", false);
            }
        }
        else
        {
            _Animator.SetBool("isInWater", false);
            _Animator.SetBool("isSwimming", false);
        }
    }

    private void ResetNotDrawnBools()
    {
        _Animator.SetBool("isRunning", false);
        _Animator.SetBool("isSliding", false);
        _Animator.SetBool("isWallSliding", false);
        _Animator.SetBool("isFalling", false);
        _Animator.SetBool("isSwinging", false);
        _Animator.SetBool("isSwimming", false);
        _Animator.SetBool("isDucking", false);
        _Animator.SetBool("isInWater", false);
        _Animator.SetBool("isClimbingLadder", false);
        _Animator.SetBool("isWalkingRunningSprinting", false);


        bool jumping = _Animator.GetBool("isJumping");
        if (jumping == true)
        {
            _Animator.SetBool("isJumpingWithSword", true);
        }
        _Animator.SetBool("isDoubleJumping", false);
        _Animator.SetBool("isJumping", false);
    }


    private void CheckForSwordGroundAttackAnimation()
    {
        if (_InputInstance._IsPlayerAttacking)
        {
            if (AnimationEventReciever._attackCounter == 1)
            {
                if (Time.timeSinceLevelLoad > _attackTimer)
                {
                    _Animator.SetInteger("groundAttackCounter", 1);
                }
            }
            else
            {
                _attackTimer = Time.timeSinceLevelLoad + _attackTimerOffset;
                _Animator.SetInteger("groundAttackCounter", 0);
            }
        }
        else
        {
            if (AnimationEventReciever._attackCounter == 0)
            {
                _Animator.SetInteger("groundAttackCounter", 0);
            }
        }
    }


    private void CheckForSwordAirAttackAnimation()
    {
        if (_InputInstance._IsPlayerAirAttacking)
        {
            if (_DataInstance._IsAboveSomething)
            {
                _Animator.SetBool("isFalling", false);
            }
            else
            {
                if (AnimationEventReciever._aerialAttackCounter == 1)
                {
                    _Animator.SetTrigger("airAttackTrigger");
                    _Animator.SetInteger("airAttackCounter", 1);
                }
                else if (AnimationEventReciever._aerialAttackCounter == 3)
                {
                    _Animator.SetInteger("airAttackCounter", 3);
                }
                else if (AnimationEventReciever._aerialAttackCounter == 4)
                {
                    _Animator.SetInteger("airAttackCounter", 4);
                }
            }
            _GetAirAttackCounter = _Animator.GetInteger("airAttackCounter");
        }
        else
        {
            if (AnimationEventReciever._aerialAttackCounter == 0)
            {
                _Animator.SetInteger("airAttackCounter", 0);
            }
            else if (PlayerAttacks._isAirAttackGrounded)
            {
                _Animator.SetBool("isFalling", false);
            }
        }
    }

    private void CheckForPlayerDamagedAnimations()
    {
        if(PlayerHealth._WasPlayerHurt)
        {
            _Animator.SetTrigger("isHurting");
            PlayerHealth._WasPlayerHurt = false;
        }
        else if(PlayerHealth._WasPlayerKilled)
        {
            _Animator.SetTrigger("isDying");
            PlayerHealth._WasPlayerKilled = false; 
        }
    }
}
