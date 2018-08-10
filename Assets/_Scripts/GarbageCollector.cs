using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageCollector : MonoBehaviour
{

    private Transform _transform;
    private List<GameObject> _childrenList;
    // Use this for initialization
    void Start()
    {
        _transform = gameObject.transform;
        _childrenList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_transform.childCount > 0)
        {
            if (_transform.childCount == 1)
            {
                GameObject child = _transform.GetChild(0).gameObject;
                if (!_childrenList.Contains(child))
                {
                    _childrenList.Add(child);
                }
            }
            else
            {
                for (int i = 0; i < _transform.childCount; i++)
                {
                    GameObject child = _transform.GetChild(i).gameObject;
                    if (!_childrenList.Contains(child))
                    {
                        _childrenList.Add(child);
                    }
                }
            }
            DestroyChild();
        }
        else
        {
            _childrenList.Clear();
        }
    }

    private void DestroyChild()
    {
        GameObject tempObj = null; 
        foreach (GameObject gObj in _childrenList)
        {
            if (gObj.GetComponent<ParticleSystem>())
            {
                if (!gObj.GetComponent<ParticleSystem>().isPlaying)
                {
                    tempObj = gObj;
                }
            }
        }

        //Now its safe to remove Children from _childrenList
        if (tempObj != null)
        {
			_childrenList.Remove(tempObj);
			Debug.LogWarning("DestroyingChild"); 
			Destroy(tempObj.gameObject);
		}
    }


}
