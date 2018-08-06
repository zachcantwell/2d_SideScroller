using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackCollider : MonoBehaviour
{
    public bool _wasLevelColliderHit;
    private Vector2 _initPos;
    private Vector2 _lastPos;

    void Start()
    {
        _wasLevelColliderHit = false;
        _initPos = transform.localPosition;
        _lastPos = transform.localPosition;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Collider2D>().gameObject.tag == "Wall"
        || other.GetComponent<Collider2D>().gameObject.tag == "Corner")
        {
            _wasLevelColliderHit = true;
        }
        else
        {
            _wasLevelColliderHit = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Collider2D>().gameObject.tag == "Wall"
        || other.GetComponent<Collider2D>().gameObject.tag == "Corner")
        {
            _wasLevelColliderHit = false;
        }
    }
}
