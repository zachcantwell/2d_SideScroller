using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 10f;
    [SerializeField]
    private Vector2 _damageForce = new Vector2(8f, 12f);
    [SerializeField]
    private Vector2 _enemyAttackForce = new Vector2(30, 60);
    [SerializeField]
    private PhysicsMaterial2D _defaultPhysics;
    [SerializeField]
    private PhysicsMaterial2D _adjustedPhysics;
    private CircleCollider2D _enemyCircleCollider;
    private GameObject _player;
    private float _attackDistance = 1f;
    private Rigidbody2D _enemyRigidbody;
    private SpriteRenderer _enemySpriteRenderer;
    private Animator _enemyAnimator;
    private int _playerSwordLayer;
    public static EnemyMovementStatus _ENEMYMOVEMENTSTATUS = EnemyMovementStatus.IsMoving;

    // Use this for initialization
    void Start()
    {
        _enemyCircleCollider = GetComponent<CircleCollider2D>();
        _enemyCircleCollider.sharedMaterial = _adjustedPhysics;
        _enemyAnimator = GetComponentInChildren<Animator>();
        _player = FindObjectOfType<PlayerMovement>().gameObject;
        _enemyRigidbody = GetComponent<Rigidbody2D>();
        _enemySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _playerSwordLayer = LayerMask.NameToLayer("PlayerSword");
        EnemyMovementStatus _ENEMYMOVEMENTSTATUS = EnemyMovementStatus.IsMoving;
    }

    void Update()
    {
        // This code flips the enemy if he's at the edge of a platform and is about to fall
        if (IsPlayerInViewOfEnemy() && _ENEMYMOVEMENTSTATUS == EnemyMovementStatus.IsMoving)
        {
            _ENEMYMOVEMENTSTATUS = EnemyMovementStatus.IsChargingAttack;
        }
        else if (!IsEnemyGrounded() && _ENEMYMOVEMENTSTATUS == EnemyMovementStatus.IsMoving)
        {
            _enemySpriteRenderer.flipX = !_enemySpriteRenderer.flipX;
            _moveSpeed *= -1f;
        }

    }

    void FixedUpdate()
    {
        if (_ENEMYMOVEMENTSTATUS == EnemyMovementStatus.IsMoving)
        {
            _enemyRigidbody.velocity = Vector2.right * _moveSpeed * Time.fixedDeltaTime;
        }
        else if (_ENEMYMOVEMENTSTATUS == EnemyMovementStatus.IsChargingAttack)
        {
            _ENEMYMOVEMENTSTATUS = EnemyMovementStatus.IsAttackingPlayer;

            //Do One Last Check to make sure the player is still in view
            if (IsPlayerInViewOfEnemy())
            {
                _enemyAnimator.SetTrigger("isAttacking");
                Invoke("AttackPlayer", 1.0f);
            }
            else
            {
                _enemyAnimator.SetTrigger("isMoving");
                StartMovingEnemyAgain();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject otherObj = other.GetComponent<Collider2D>().gameObject;
        if (otherObj.tag == "Wall" || otherObj.tag == "Corner" || otherObj.tag == "Ground")
        {
            _enemySpriteRenderer.flipX = !_enemySpriteRenderer.flipX;
            _moveSpeed *= -1f;
        }
        if (otherObj.layer == _playerSwordLayer)
        {
            _ENEMYMOVEMENTSTATUS = EnemyMovementStatus.IsTakingDamage;
            ApplyDamageForces(otherObj);
        }
    }

    private void ApplyDamageForces(GameObject sword)
    {
        if (sword.transform.position.x < transform.position.x)
        {
            _enemyRigidbody.AddForce(_enemyAttackForce, ForceMode2D.Impulse);
        }
        else if (sword.transform.position.x > transform.position.x)
        {
            Vector2 dmg = new Vector2(-_enemyAttackForce.x, _enemyAttackForce.y);
            _enemyRigidbody.AddForce(dmg, ForceMode2D.Impulse);
        }
        Invoke("StartMovingEnemyAgain", 0.5f);
    }

    private void AttackPlayer()
    {
        if (_player != null && _ENEMYMOVEMENTSTATUS == EnemyMovementStatus.IsAttackingPlayer)
        {
            _enemyCircleCollider.sharedMaterial = _defaultPhysics;
            if (_player.transform.position.x < transform.position.x)
            {
                _enemyRigidbody.gravityScale = 3f; 
               // _ENEMYMOVEMENTSTATUS = EnemyMovementStatus.None;
                Vector2 dmg = new Vector2(-_damageForce.x, _damageForce.y);
                _enemyRigidbody.AddForce(dmg, ForceMode2D.Impulse);
            }
            else if (_player.transform.position.x > transform.position.x)
            {
                _enemyRigidbody.gravityScale = 3f; 
               // _ENEMYMOVEMENTSTATUS = EnemyMovementStatus.None;
                _enemyRigidbody.AddForce(_damageForce, ForceMode2D.Impulse);
            }
        }
        Invoke("StartMovingEnemyAgain", 0.5f);
    }

    private void StartMovingEnemyAgain()
    {
        _enemyCircleCollider.sharedMaterial = _adjustedPhysics;
        _enemyRigidbody.gravityScale = 9.81f; 
        _ENEMYMOVEMENTSTATUS = EnemyMovementStatus.IsMoving;
    }

    private bool IsEnemyGrounded()
    {
        if (_ENEMYMOVEMENTSTATUS != EnemyMovementStatus.IsChargingAttack
            || _ENEMYMOVEMENTSTATUS != EnemyMovementStatus.IsAttackingPlayer)
        {
            if (!_enemySpriteRenderer.flipX)
            {
                return CheckBottomRightOfEnemy();
            }
            else
            {
                return CheckBelowLeftOfEnemy();
            }
        }
        return true; 
    }

    private bool IsPlayerInViewOfEnemy()
    {
        if (!_enemySpriteRenderer.flipX)
        {
            return CheckIfPlayerIsRightOfEnemy();
        }
        else
        {
            return CheckIfPlayerIsLeftOfEnemy();
        }
    }

    private bool CheckBottomRightOfEnemy()
    {
        Vector2 origin = new Vector2(transform.position.x + transform.localScale.x * 0.25f,
                                         transform.position.y + 0.1f - transform.localScale.y * 0.5f);

        RaycastHit2D[] hit = Physics2D.LinecastAll(origin, origin + Vector2.down / 4);
        Debug.DrawLine(origin, origin + Vector2.down / 4, Color.cyan, Time.deltaTime);

        if (hit != null)
        {
            foreach (RaycastHit2D rayRight in hit)
            {
                if (rayRight.collider)
                {
                    if (rayRight.collider.gameObject.tag == "Corner"
                       || rayRight.collider.gameObject.tag == "Wall"
                       || (rayRight.collider.gameObject.tag == "Ground"))
                    {
                        if (rayRight.distance < 0.25f)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool CheckBelowLeftOfEnemy()
    {
        Vector2 origin = new Vector2(transform.position.x - transform.localScale.x * 0.25f,
                                         transform.position.y + 0.1f - transform.localScale.y * 0.5f);

        RaycastHit2D[] hit = Physics2D.LinecastAll(origin, origin + Vector2.down / 4);
        Debug.DrawLine(origin, origin + Vector2.down / 4, Color.cyan, Time.deltaTime);

        if (hit != null)
        {
            foreach (RaycastHit2D hitLeft in hit)
            {
                if (hitLeft.collider)
                {
                    if (hitLeft.collider.gameObject.tag == "Corner"
                    || hitLeft.collider.gameObject.tag == "Wall"
                    || hitLeft.collider.gameObject.tag == "Ground")
                    {
                        if (hitLeft.distance < 0.25f)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool CheckIfPlayerIsRightOfEnemy()
    {
        Vector2 origin = new Vector2(transform.position.x + transform.localScale.x / 2 + 0.1f, transform.position.y);
        Vector2 destination = origin + (Vector2.right * 1.5f);
        RaycastHit2D[] hits = Physics2D.LinecastAll(origin, destination);
        Debug.DrawLine(origin, destination, Color.cyan, Time.deltaTime);

        if (hits != null)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject.tag == "Player")
                {
                    if (hit.distance < _attackDistance)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool CheckIfPlayerIsLeftOfEnemy()
    {
        Vector2 origin = new Vector2(transform.position.x - transform.localScale.x / 2 - 0.1f, transform.position.y);
        Vector2 destination = origin + (Vector2.left * 1.5f);
        RaycastHit2D[] hits = Physics2D.LinecastAll(origin, destination);
        Debug.DrawLine(origin, destination, Color.cyan, Time.deltaTime);

        if (hits != null)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject.tag == "Player")
                {
                    if (hit.distance < _attackDistance)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
