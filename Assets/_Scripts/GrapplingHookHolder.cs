using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookHolder : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            int layer = LayerMask.NameToLayer("Player");
            if (other.gameObject.layer == layer)
            {
                other.GetComponent<PlayerAnimations>().HasPlayerEnteredHookZone(true);
                other.GetComponent<GrapplingHook>().HasPlayerEnteredHookHolderZone(true);
                other.GetComponent<GrapplingHook>().SetGrapplingHookHolder(this);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            int layer = LayerMask.NameToLayer("Player");

            if (other.gameObject.layer == layer)
            {
                other.GetComponent<GrapplingHook>().HasPlayerEnteredHookHolderZone(false);
                other.GetComponent<PlayerAnimations>().HasPlayerEnteredHookZone(false);
            }
        }
    }

}
