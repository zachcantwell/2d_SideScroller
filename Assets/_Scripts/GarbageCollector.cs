using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageCollector : MonoBehaviour
{
    private Transform _transform;
    private List<GameObject> _childrenWithParticlesList;
    private List<GameObject> _childrenWithGhostSpritesList;
    // Use this for initialization
    void Start()
    {
        _transform = gameObject.transform;
        _childrenWithGhostSpritesList = new List<GameObject>();
        _childrenWithParticlesList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        ScanSceneForGhostSprites();
        SortChildrenIntoLists();
    }

    private void SortChildrenIntoLists()
    {
        if (_transform.childCount > 0)
        {
            SortParticleSystemChildren();
        }
        else
        {
            _childrenWithParticlesList.Clear();
        }
    }

    private void SortParticleSystemChildren()
    {
        for (int i = 0; i < _transform.childCount; i++)
        {
            GameObject child = _transform.GetChild(i).gameObject;
            if (child.GetComponent<ParticleSystem>() != null)
            {
                if (!_childrenWithParticlesList.Contains(child))
                {
                    _childrenWithParticlesList.Add(child);
                }
            }
        }
        DestroyChildrenWithParticleSystems();
    }

    private void DestroyChildrenWithParticleSystems()
    {
        if (_childrenWithParticlesList.Count > 0)
        {
            GameObject tempObj = null;
            foreach (GameObject gObj in _childrenWithParticlesList)
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
                _childrenWithParticlesList.Remove(tempObj);
                Debug.LogWarning("DestroyingChild");
                Destroy(tempObj.gameObject);
            }
        }
    }

    private void FadeOutGhostSprites(GameObject[] ghosts)
    {
       for(int i = 0; i < ghosts.Length; i++)
       {
           SpriteRenderer sp = ghosts[i].GetComponent<SpriteRenderer>();
           if(sp.color.a > 0)
           {
               sp.color = Color.Lerp(sp.color, Color.clear, Time.deltaTime * 5f); 
           }
       }
    }

    private void ScanSceneForGhostSprites()
    {
        GameObject[] ghostSprites = GameObject.FindGameObjectsWithTag("GhostSprite");

        foreach (GameObject gobj in ghostSprites)
        {
            if (gobj.transform.parent != transform)
            {
                gobj.transform.parent = transform;
            }
        }
        FadeOutGhostSprites(ghostSprites);
    }

}
