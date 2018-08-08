using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{

    [SerializeField]
    private float _moveSpeed = 10f;
    private Rigidbody2D _enemyRigidbody;
    private SpriteRenderer _enemySpriteRenderer;

    // Use this for initialization
    void Start()
    {
        _enemyRigidbody = GetComponent<Rigidbody2D>();
        _enemySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (!IsEnemyGrounded())
        {
            _enemySpriteRenderer.flipX = !_enemySpriteRenderer.flipX;
            _moveSpeed *= -1f;
        }
    }

    void FixedUpdate()
    {
        _enemyRigidbody.velocity = Vector2.right * _moveSpeed * Time.fixedDeltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        string otherTag = other.GetComponent<Collider2D>().gameObject.tag;
        if (otherTag == "Wall" || otherTag == "Corner")
        {
            _enemySpriteRenderer.flipX = !_enemySpriteRenderer.flipX;
            _moveSpeed *= -1f;
        }
    }

    private bool IsEnemyGrounded()
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

}
