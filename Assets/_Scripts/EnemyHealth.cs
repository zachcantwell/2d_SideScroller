using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{

    [SerializeField]
    private ParticleSystem _hitParticles;
    [SerializeField]
    private ParticleSystem _deathParticles;
    private GarbageCollector _garbageCollector;
    private int _enemyHealth;
    private int _playerSwordCollider;
    private Animator _enemyAnimator;
    private float _enemyInvulnerabilityTimer = 0f;
    private const float _enemyInvulnerabilityTimerOffset = 0.125f;

    // Use this for initialization
    void Start()
    {
        _enemyHealth = 3;
        _playerSwordCollider = LayerMask.NameToLayer("PlayerSword");
        _enemyAnimator = GetComponentInChildren<Animator>();
        _garbageCollector = FindObjectOfType<GarbageCollector>();
    }

    void OnDestroy()
    {
        Debug.LogWarning("EnemyDestroyed");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject sword = other.GetComponent<Collider2D>().gameObject;
        if (sword.layer == _playerSwordCollider)
        {
            DealDamageToEnemy();
        }
    }

    void DealDamageToEnemy()
    {
        if (Time.timeSinceLevelLoad > _enemyInvulnerabilityTimer)
        {
            _enemyInvulnerabilityTimer = Time.timeSinceLevelLoad + _enemyInvulnerabilityTimerOffset;
            _enemyHealth--;
            Debug.LogWarning("enemyHealth = " + _enemyHealth);

            if (_enemyHealth > 0)
            {
                _enemyAnimator.SetTrigger("isTakingDamage");
                _hitParticles.gameObject.SetActive(true);
            }
            else
            {
                _enemyAnimator.SetTrigger("isDying");
                _hitParticles.transform.SetParent(_garbageCollector.transform);
                _deathParticles.transform.SetParent(_garbageCollector.transform);
                _deathParticles.gameObject.SetActive(true);
                Destroy(gameObject);
            }
        }

    }


}
