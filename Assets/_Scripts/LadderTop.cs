using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class LadderTop : MonoBehaviour {

	void Start () {
		Physics2D.IgnoreLayerCollision(10,15,true);
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		Physics2D.IgnoreLayerCollision(10,15,false);
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if(CrossPlatformInputManager.GetAxisRaw("Vertical") < -0.15f)
		{
			Physics2D.IgnoreLayerCollision(10,15,true);
		}
		if(CrossPlatformInputManager.GetAxisRaw("Vertical") > 0.15f)
		{
			Physics2D.IgnoreLayerCollision(10,15,true);
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if(CrossPlatformInputManager.GetAxisRaw("Vertical") < -0.15f)
		{
			Physics2D.IgnoreLayerCollision(10,15,true);
		}
		 if(CrossPlatformInputManager.GetAxisRaw("Vertical") > 0.15f)
		{
			Physics2D.IgnoreLayerCollision(10,15,true);
		}

	}
}
