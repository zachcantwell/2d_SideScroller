using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookHolder : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.GetComponent<PlayerAnimations>().HasPlayerEnteredHookZone(true);
            other.GetComponent<GrapplingHook>().HasPlayerEnteredHookHolderZone(true);
            other.GetComponent<GrapplingHook>().SetGrapplingHookHolder(this);
            //	PlayerControllerV2._playerInstance.HasPlayerEnteredHolderZone(true);
            //PlayerControllerV2._playerInstance.GetGrapplingHookHolder(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.GetComponent<GrapplingHook>().HasPlayerEnteredHookHolderZone(false);
            other.GetComponent<PlayerAnimations>().HasPlayerEnteredHookZone(false);
            // PlayerControllerV2._playerInstance.HasPlayerEnteredHolderZone(false);
        }
    }

}
