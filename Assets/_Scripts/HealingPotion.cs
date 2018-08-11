using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPotion : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _orangeHitParticles;
    [SerializeField]
    private ParticleSystem _heartParticles;
    [SerializeField]
    private int _HPValueOfPotion = 1;

    public int _HPRestoredByThisPotion
    {
        get; private set;
    }
    private CapsuleCollider2D _capsuleCollider;
    private SpriteRenderer _spriteRenderer;
    private bool _isItTimeToDestroyThisGameobject = false;
    // Use this for initialization
    void Start()
    {
        _HPRestoredByThisPotion = _HPValueOfPotion;
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isItTimeToDestroyThisGameobject = false;
        _orangeHitParticles.Stop();
        _heartParticles.Stop();
    }

    void Update()
    {
        if (_isItTimeToDestroyThisGameobject)
        {
            if (_orangeHitParticles != null)
            {
                if (_orangeHitParticles.isPlaying == false)
                {
                    _orangeHitParticles.gameObject.SetActive(false);
                    Destroy(_orangeHitParticles.gameObject);
                }
            }
            if (_heartParticles != null)
            {
                if (_heartParticles.isPlaying == false)
                {
                    _heartParticles.gameObject.SetActive(false);
                    Destroy(_heartParticles.gameObject);
                }
            }
            if (_orangeHitParticles == null && _heartParticles == null)
            {
                Destroy(this.gameObject);
            }
        }
    }

    // Update is called once per frame
    public void CheckOnPlayersCurrentHealth()
    {
        if (PlayerHealth._playerCurrentHP < PlayerHealth._playerMaxHP)
        {
            _isItTimeToDestroyThisGameobject = true;
            _spriteRenderer.enabled = false;
            _capsuleCollider.enabled = false;
            _orangeHitParticles.gameObject.SetActive(true);
            _heartParticles.gameObject.SetActive(true);
            _orangeHitParticles.Play();
            _heartParticles.Play();
        }
    }

}
