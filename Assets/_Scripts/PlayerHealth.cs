using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private Vector2 _damageForce;
    [SerializeField]
    private PhysicsMaterial2D _defaultPlayerPhysicsMaterial;
    [SerializeField]
    private PhysicsMaterial2D _adjustedPlayerPhysicsMaterial;
    [SerializeField]
    private Image[] _UIPlayerHearts;

    public static bool _WasPlayerHurt
    {
        get; set;
    }
    public static bool _WasPlayerKilled
    {
        get; set;
    }
    public static int _playerCurrentHP
    {
        get; private set;
    }
    public static int _playerMaxHP
    {
        get; private set;
    }
    private bool _WasDamageDealt = false;
    private int _enemyLayer;
    private float _ignoreDamageTimer = 0f;
    private const float _ignoreDamageTimerOffset = 0.5f;
    private const float _defaultFriction = 0f;
    private const float _defaultBounciness = 0f;
    private float _newFriction = 5f;
    private float _newBounciness = 0.5f;

    private Collider2D _enemyCollider;
    public static PlayerHealthStatus _PLAYERHEALTHSTATUS;
    private Rigidbody2D _rb;
    private BoxCollider2D _boxCollider;
    private CircleCollider2D _circleCollider;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _circleCollider = GetComponent<CircleCollider2D>();
        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _PLAYERHEALTHSTATUS = PlayerHealthStatus.Default;
        _playerMaxHP = _UIPlayerHearts.Length * 2;
        _playerCurrentHP = _playerMaxHP;
        _ignoreDamageTimer = 0f;
        _WasPlayerHurt = false;
        _WasPlayerKilled = false;
        _WasDamageDealt = false;
        ResetPhysicsMaterialValues();
    }

    void Update()
    {
        if (_enemyCollider != null)
        {
            if (_WasDamageDealt)
            {
                TakeDamage(_enemyCollider);
            }
        }
    }

    void FixedUpdate()
    {
        if (_WasDamageDealt == false && (_WasPlayerHurt || _WasPlayerKilled))
        {
            if (_enemyCollider != null)
            {
                ApplyDamageForce(_enemyCollider);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject gObj = other.gameObject;
        if (gObj.layer == _enemyLayer && Time.timeSinceLevelLoad > _ignoreDamageTimer)
        {
            _WasDamageDealt = true;
            _enemyCollider = other;
        }
        if (gObj.layer == LayerMask.NameToLayer("Potion"))
        {
            if (gObj.tag == "HealingPotion")
            {
                HealPlayer(gObj);
            }
        }
    }

    private void HealPlayer(GameObject healObj)
    {
        HealingPotion hObj = healObj.GetComponent<HealingPotion>(); 
        if (_playerCurrentHP < _playerMaxHP)
        {
            hObj.CheckOnPlayersCurrentHealth();
            int hp = hObj._HPRestoredByThisPotion; 
            _playerCurrentHP += hp; 

            if (_playerCurrentHP > _playerMaxHP)
            {
                hp -= (_playerCurrentHP - _playerMaxHP);
                _playerCurrentHP = _playerMaxHP;
            }
            IncreaseUIHearts(hp);
        }
    }

    private void ReduceUIHearts(int damageDealt)
    {
        if (damageDealt > 0)
        {
            Debug.LogWarning(_playerCurrentHP + " = playerHealth");
            string loseBottomHalf = "loseBottomHalfHeart";
            string loseTopHalf = "loseTopHalfHeart";
            string gainBottomHalf = "gainBottomHalfHeart";
            string gainTopHalf = "gainTopHalfHeart";
            switch (_playerCurrentHP)
            {
                case 6:
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);

                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    break;

                case 5:
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);

                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    break;

                case 4:
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);

                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    break;

                case 3:
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);

                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    break;

                case 2:
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, true);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);

                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    break;

                case 1:
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, true);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);

                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainTopHalf, false);

                    break;

                case 0:
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, true);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, true);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, true);

                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    break;

                default:
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainTopHalf, false);
                    break;
            }
        }
    }

    private void IncreaseUIHearts(int healthDealt)
    {
        string gainBottomHalf = "gainBottomHalfHeart";
        string gainTopHalf = "gainTopHalfHeart";
        string loseBottomHalf = "loseBottomHalfHeart";
        string loseTopHalf = "loseTopHalfHeart";
        if (healthDealt > 0)
        {
            Debug.LogWarning(_playerCurrentHP + " = playerHealth");

            switch (_playerCurrentHP)
            {
                case 6:
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainTopHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainTopHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, true);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, true);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainBottomHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetTrigger("gainTopHalfHeartTrigger");
                    _UIPlayerHearts[1].GetComponent<Animator>().SetTrigger("gainTopHalfHeartTrigger");
                    _UIPlayerHearts[0].GetComponent<Animator>().SetTrigger("gainTopHalfHeartTrigger");
                    _UIPlayerHearts[2].GetComponent<Animator>().SetTrigger("gainBottomHalfHeartTrigger");
                    _UIPlayerHearts[1].GetComponent<Animator>().SetTrigger("gainBottomHalfHeartTrigger");
                    _UIPlayerHearts[0].GetComponent<Animator>().SetTrigger("gainBottomHalfHeartTrigger");

                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    break;

                case 5:
                    // This Section is a bit illogical but its the only way I could get the animator working
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetTrigger("gainBottomHalfHeartTrigger");

                    if(_UIPlayerHearts[0].GetComponent<Animator>().GetBool(gainTopHalf) == false 
                     || _UIPlayerHearts[0].GetComponent<Animator>().GetBool(gainBottomHalf) == false)
                    {
                       _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainBottomHalf, true);
                       _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, true);
                    }

                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    break;

                case 4:
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainTopHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetTrigger("gainBottomHalfHeartTrigger");
                    _UIPlayerHearts[2].GetComponent<Animator>().SetTrigger("gainTopHalfHeartTrigger");

                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    break;

                case 3:
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(gainBottomHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainTopHalf, true);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetTrigger("gainBottomHalfHeartTrigger");
                    _UIPlayerHearts[1].GetComponent<Animator>().SetTrigger("gainTopHalfHeartTrigger");

                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    break;

                case 2:
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainTopHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetTrigger("gainTopHalfHeartTrigger");
                    _UIPlayerHearts[1].GetComponent<Animator>().SetTrigger("gainBottomHalfHeartTrigger");

                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    break;

                case 1:
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, true);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetTrigger("gainTopHalfHeartTrigger");
                    _UIPlayerHearts[1].GetComponent<Animator>().SetTrigger("gainBottomHalfHeartTrigger");

                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    break;

                case 0:
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainBottomHalf, true);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, true);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetTrigger("gainTopHalfHeartTrigger");
                    _UIPlayerHearts[0].GetComponent<Animator>().SetTrigger("gainBottomHalfHeartTrigger");

                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    break;

                default:
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(gainTopHalf, true);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(gainBottomHalf, true);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetTrigger("gainTopHalfHeartTrigger");
                    _UIPlayerHearts[1].GetComponent<Animator>().SetTrigger("gainBottomHalfHeartTrigger");

                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[2].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[1].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseTopHalf, false);
                    _UIPlayerHearts[0].GetComponent<Animator>().SetBool(loseBottomHalf, false);
                    break;
            }
        }
    }

    private void TakeDamage(Collider2D other)
    {
        if (other.gameObject.layer == _enemyLayer && Time.timeSinceLevelLoad > _ignoreDamageTimer)
        {
            _WasDamageDealt = false;
            _ignoreDamageTimer = Time.timeSinceLevelLoad + _ignoreDamageTimerOffset;
            if (EnemyMovement._ENEMYMOVEMENTSTATUS == EnemyMovementStatus.IsAttackingPlayer)
            {
                int damage = 2;
                _playerCurrentHP -= damage;
                ReduceUIHearts(damage);
            }
            else if (EnemyMovement._ENEMYMOVEMENTSTATUS != EnemyMovementStatus.IsAttackingPlayer)
            {
                int damage = 1;
                _playerCurrentHP--;
                ReduceUIHearts(damage);
            }
            if (_playerCurrentHP > 0)
            {
                Invoke("SetNewPhysicsMaterialValues", 0.1f);
                Invoke("ResetPhysicsMaterialValues", 1.25f);
                _WasPlayerHurt = true;
                _PLAYERHEALTHSTATUS = PlayerHealthStatus.IsHurt;
            }
            else
            {
                KillPlayer();
            }
        }
    }

    private void KillPlayer()
    {
    Invoke("SetNewPhysicsMaterialValues", 0.1f);
                _WasPlayerKilled = true;
                _PLAYERHEALTHSTATUS = PlayerHealthStatus.IsDying;
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), 
                                               LayerMask.NameToLayer("Enemy"), true);
    }

    private void ApplyDamageForce(Collider2D other)
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        if (transform.position.x < other.transform.position.x)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(-1f * _damageForce.x, 1f * _damageForce.y);

        }
        else if (transform.position.x > other.transform.position.x)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(1f * _damageForce.x, 1f * _damageForce.y);
        }
        _enemyCollider = null;
    }

    private void SetNewPhysicsMaterialValues()
    {
        _boxCollider.sharedMaterial = _adjustedPlayerPhysicsMaterial;
        _circleCollider.sharedMaterial = _adjustedPlayerPhysicsMaterial;
    }

    private void ResetPhysicsMaterialValues()
    {
        _PLAYERHEALTHSTATUS = PlayerHealthStatus.Default;
        _boxCollider.sharedMaterial = _defaultPlayerPhysicsMaterial;
        _circleCollider.sharedMaterial = _defaultPlayerPhysicsMaterial;
    }
}
