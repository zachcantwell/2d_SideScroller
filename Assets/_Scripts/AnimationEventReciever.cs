using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReciever : MonoBehaviour {

	public static bool _isItTheLastFrameOfJumping;
	public static bool _isItTheLastFrameOfSwinging; 
	public static bool _isItTheLastFrameOfGroundSliding;
	// Use this for initialization
	void Start () {
		_isItTheLastFrameOfJumping = false; 
		_isItTheLastFrameOfGroundSliding = false; 
		_isItTheLastFrameOfSwinging = false; 
	}
	
	public void LastFrameOfJumpingReached()
	{
		_isItTheLastFrameOfJumping = true;
	}

	public void LastFrameOfSwingingReached()
	{
		_isItTheLastFrameOfSwinging = true;
	}

	public void LastFrameOfGroundSliding()
	{
		_isItTheLastFrameOfGroundSliding = true;
	}

	public void FirstFrameOfGroundSliding()
	{
		_isItTheLastFrameOfGroundSliding = false;
	}
}
