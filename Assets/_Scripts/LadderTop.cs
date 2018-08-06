using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class LadderTop : MonoBehaviour
{
	private bool _didPlayerJump = false; 

    void Start()
    {
		_didPlayerJump = false;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Physics2D.IgnoreLayerCollision(10, 15, false);
		_didPlayerJump = false; 
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<Collider2D>().gameObject.tag == "Player")
        {
            if (CrossPlatformInputManager.GetButton("Jump") )
            {
				_didPlayerJump = true;
                Physics2D.IgnoreLayerCollision(10, 15, true);
            }
			else if( CrossPlatformInputManager.GetAxisRaw("Vertical") < -0.15f)
			{
				Physics2D.IgnoreLayerCollision(10, 15, true);
			}
			else if(_didPlayerJump)
			{
				Physics2D.IgnoreLayerCollision(10, 15, true);
			}
            else
            {
				_didPlayerJump = false;
                Physics2D.IgnoreLayerCollision(10, 15, false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (CrossPlatformInputManager.GetAxisRaw("Vertical") < -0.15f)
        {
            Physics2D.IgnoreLayerCollision(10, 15, true);
        }
        if (CrossPlatformInputManager.GetAxisRaw("Vertical") > 0.15f)
        {
            Physics2D.IgnoreLayerCollision(10, 15, true);
        }

    }
}
