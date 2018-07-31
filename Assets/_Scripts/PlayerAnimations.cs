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

    private bool _hasControlAlreadyBeenPressed = false;
    private bool _hasPlayerEnteredHookZone = false;

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
        if (CheckForSwordDrawnAnimation() == false)
        {
            // There are 2 animation states, (sword drawn vs not drawn).
            // both need to have the bools reset once the state changes
            ResetSwordDrawnBools();

            //Now check for all permitted animations in the 'not drawn' state.
            CheckForIdleAnimation();
            CheckForFallingAnimation();
            CheckForClimbingAnimation();
            CheckForRunningAnimation();
            CheckForGroundSlideAnimation();
            CheckForGrabbingAnimation();
            CheckForJumpAnimations();
            CheckForSwingingAnimation();
            CheckForWallSlideAnimation();
            CheckForSwimmingAnimations();
        }
        else
        {
            // There are 2 animation states, (sword drawn vs not drawn).
            // both need to have the bools reset once the state changes
            ResetNotDrawnBools();

            //Now check for all permitted animations in the 'sword drawn' state.
            CheckForIdleWithSwordAnimation();
            CheckForFallingWithSwordAnimation();
            CheckForRunningWithSwordAnimation();
            CheckForGroundSlidingWithSwordAnimation();
            CheckForJumpingWithSwordAnimation();
            CheckForWallSlidingWithSwordAnimation();
        }
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
        if (_RigidBody.velocity.x == 0 || Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) < 0.1f)
        {
            //Idle
            _Animator.SetBool("isRunning", false);
        }
        if (_RigidBody.velocity.y == 0)
        {
            //idle part two
            _Animator.SetBool("isJumping", false);
            _Animator.SetBool("isDoubleJumping", false);
            _Animator.SetBool("isFalling", false);
        }
    }

    private void CheckForFallingAnimation()
    {
        if (_DataInstance._IsAboveGround || _DataInstance._IsAboveGrabbableObject || _DataInstance._IsAboveCorner)
        {
            _hasControlAlreadyBeenPressed = false;
            _Animator.SetBool("isSwinging", false);
            _Animator.SetBool("isJumping", false);
            _Animator.SetBool("isDoubleJumping", false);
            _Animator.SetBool("isFalling", false);
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

    private void CheckForRunningAnimation()
    {
        if (Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) > 0.1f
            && _DataInstance._IsAboveSomething && !_DataInstance._IsTouchingWater)
        {
            _Animator.SetBool("isRunning", true);
            _Animator.SetBool("isGrounded", true);
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
        if (CrossPlatformInputManager.GetButton("Grab"))
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
                _Animator.SetBool("isSliding", true);
            }
            else
            {
                _Animator.SetBool("isSliding", false);
            }
        }
    }

    private void CheckForJumpAnimations()
    {
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            _Animator.SetBool("isRunning", false);
            Debug.LogWarning(PlayerData._DataInstance._JumpState + " = JumpState");
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
            }
        }
    }

    private void CheckForSwingingAnimation()
    {
        // Swinging, Jumping and Falling
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftControl))
        {
            _Animator.SetBool("isRunning", false);
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
                    _Animator.SetBool("isJumping", true);
                    _Animator.SetBool("isGrounded", false);
                    _Animator.SetBool("isDoubleJumping", false);
                }
            }
        }

    }

    private void CheckForWallSlideAnimation()
    {
        if (_InputInstance._IsPlayerWallSliding && !_InputInstance._IsPlayerGrabbing && !_InputInstance._IsPlayerSwinging)
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

    private void CheckForSwimmingAnimations()
    {
        if (_DataInstance._IsTouchingWater)
        {
            _Animator.SetBool("isInWater", true);

            if (Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) > 0.1f)
            {
                //This triggers the Swimming Animation
                _Animator.SetBool("isSwimming", true);
            }
            else
            {
                //This triggers the IdleInWater Animation
                _Animator.SetBool("isSwimming", false);
            }
        }
        else
        {
            _Animator.SetBool("isInWater", false);
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
        _Animator.SetBool("isGrounded", false);
        _Animator.SetBool("isInWater", false);
        _Animator.SetBool("isClimbingLadder", false);

        bool jumping = _Animator.GetBool("isJumping");
        if (jumping == true)
        {
            _Animator.SetBool("isJumpingWithSword", true);
        }
        _Animator.SetBool("isDoubleJumping", false);
        _Animator.SetBool("isJumping", false);

    }

    //********************************************/
    // All Sword Drawn Animations are below here!!! 
    //********************************************/

    private void CheckForFallingWithSwordAnimation()
    {
        if (_DataInstance._IsAboveGround || _DataInstance._IsAboveGrabbableObject || _DataInstance._IsAboveCorner)
        {
            _Animator.SetBool("isJumpingWithSword", false);
            _Animator.SetBool("isFallingWithSword", false);
        }
        else if (_DataInstance._IsTouchingWater)
        {
            _Animator.SetBool("isFallingWithSword", false);
        }
        else
        {
            _Animator.SetBool("isFallingWithSword", true);
        }
    }

    private void CheckForGroundSlidingWithSwordAnimation()
    {
        //Using Get Key will let you slide indefinitely
        if (!_DataInstance._IsTouchingWater)
        {
            if (CrossPlatformInputManager.GetButtonDown("GroundSlide") && Mathf.Abs(_RigidBody.velocity.x) > 0
             && (_DataInstance._IsAboveCorner || _DataInstance._IsAboveGround || _DataInstance._IsAboveGrabbableObject))
            {
                _Animator.SetBool("isGroundSlidingWithSword", true);
            }
            else
            {
                _Animator.SetBool("isGroundSlidingWithSword", false);
            }
        }
    }

    private void CheckForRunningWithSwordAnimation()
    {
        if (Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) > 0.1f
            && _DataInstance._IsAboveSomething && !_DataInstance._IsTouchingWater)
        {
            _Animator.SetBool("isRunningWithSword", true);
        }
    }

    private void CheckForWallSlidingWithSwordAnimation()
    {
        if (_InputInstance._IsPlayerWallSliding && !_InputInstance._IsPlayerGrabbing && !_InputInstance._IsPlayerSwinging)
        {
            if (_DataInstance._IsHorizontalToWall || _DataInstance._IsHorizontalToCorner)
            {
                if (_DataInstance._LastPlayerPosition.y > transform.position.y)
                {
                    if (!_DataInstance._IsAboveGround && !_DataInstance._IsTouchingWater && !_DataInstance._IsTouchingLadder)
                    {
                        _Animator.SetBool("isWallSlidingWithSword", true);
                    }
                }
            }
        }
        else
        {
            if (_InputInstance._GetHorizontalAxisValue < 0.1f)
            {
                _Animator.SetBool("isWallSlidingWithSword", false);
            }
            if (_DataInstance._LastPlayerPosition.y < transform.position.y)
            {
                _Animator.SetBool("isWallSlidingWithSword", false);
            }
            else if (_InputInstance._IsPlayerJumping)
            {
                _Animator.SetBool("isWallSlidingWithSword", false);
            }
            else if (_DataInstance._IsAboveGround || _DataInstance._IsAboveCorner)
            {
                _Animator.SetBool("isWallSlidingWithSword", false);
            }
            else if (_DataInstance._IsTouchingWater || _DataInstance._IsTouchingLadder)
            {
                _Animator.SetBool("isWallSlidingWithSword", false);
            }
        }
    }

    private void CheckForIdleWithSwordAnimation()
    {
        if (_RigidBody.velocity.x == 0)
        {
            //Idle
            _Animator.SetBool("isRunningWithSword", false);
        }

        if (_RigidBody.velocity.y == 0)
        {
            //idle part two
            _Animator.SetBool("isJumpingWithSword", false);
            _Animator.SetBool("isFallingWithSword", false);
        }
    }

    private void CheckForJumpingWithSwordAnimation()
    {
        // Swinging, Jumping and Falling
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _Animator.SetBool("isRunningWithSword", false);
            _Animator.SetBool("isJumpingWithSword", true);
        }
    }

    private void ResetSwordDrawnBools()
    {
        _Animator.SetBool("isRunningWithSword", false);
        _Animator.SetBool("isGroundSlidingWithSword", false);
        _Animator.SetBool("isWallSlidingWithSword", false);
        _Animator.SetBool("isFallingWithSword", false);

        bool jumpingWithSword = _Animator.GetBool("isJumpingWithSword");
        if (jumpingWithSword == true)
        {
            _Animator.SetBool("isJumping", true);
        }
        _Animator.SetBool("isJumpingWithSword", false);
    }
}
