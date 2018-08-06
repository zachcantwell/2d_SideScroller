using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GifPlayer : MonoBehaviour
{

    [SerializeField]
    private Sprite[] _sprites;
    [SerializeField]
    private float _timeOffset = 0.2f;
    private float _timer;
    private SpriteRenderer _spriteRenderer;
    private int _arraySize;
    private int _spriteCounter;
    private Camera _mainCam;
    private Transform _target;

    // Use this for initialization
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _arraySize = _sprites.Length;
        _timer = Time.timeSinceLevelLoad + _timeOffset;
        _mainCam = Camera.main; 
        _target = gameObject.transform;
        _spriteCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 viewPt = _mainCam.WorldToViewportPoint(_target.position);
        if (viewPt.x >= 0 && viewPt.x <= 1)
        {
            if (Time.timeSinceLevelLoad > _timer)
            {
                _timer = Time.timeSinceLevelLoad + _timeOffset;
                _spriteRenderer.sprite = _sprites[_spriteCounter];
                _spriteCounter++;

                if (_spriteCounter >= _arraySize)
                {
                    _spriteCounter = 0;
                }
            }
        }


    }
}
