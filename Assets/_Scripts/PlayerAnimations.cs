using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    private void AnimationStateController()
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

        if (Mathf.Abs(_rb.velocity.x) > 0 && _isGrounded) // _rb.velocity.y == 0)
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

}
