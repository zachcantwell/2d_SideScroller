using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallJump : PlayerControllerV2 {

	[SerializeField]
	private Vector2 _wallJumpSpd = new Vector2(3.5f,7f);
	private float _distance = 1f;
	private float _gravity;

	// Use this for initialization
	void Start () {
		_gravity =  _rb.gravityScale;
	}
	
	// Update is called once per frame
	void Update () {
		Physics2D.queriesStartInColliders = false; 

		RaycastHit2D hit;
		if(PlayerControllerV2._playerInstance._spriteRenderer.flipX == false)
		{
			 hit = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x, _distance);
		}
		else
		{
			 hit = Physics2D.Raycast(transform.position, Vector2.left * transform.localScale.x, _distance);
		}

		//WALL SLIDE CODE
		if((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && !_isGrounded && hit.collider != null)
		{
			 _rb.drag = 9f;
		}
		else
		{
			 _rb.drag = 0f;
		}

		//WALL JUMP CODE
		if(Input.GetKeyDown(KeyCode.Space) && !_isGrounded && hit.collider != null)
		{
			_rb.drag = 0f;
			PlayerControllerV2._playerInstance._spriteRenderer.flipX = !PlayerControllerV2._playerInstance._spriteRenderer.flipX;
			Vector2 vel =  _rb.velocity;
			vel = new Vector2(_wallJumpSpd.x  * hit.normal.x,  _wallJumpSpd.y);
			 _rb.velocity = vel;
		}

	}
}
