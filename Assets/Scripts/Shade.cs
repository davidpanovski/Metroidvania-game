using UnityEngine;

public class Shade : Enemy
{
    [SerializeField] private float chaseDistance;

    [SerializeField] float stunnedTimer;
    float timer;

    public static Shade Instance { get; private set; }
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        SaveData.Instance.SaveShadeData();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Shade_Idle);
    }
    protected override void Update()
    {
        base.Update();
        if (!PlayerController.Instance.pState.alive)
        {
            ChangeState(EnemyStates.Shade_Idle);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {

        Enemy otherEnemy = collision.collider.GetComponentInParent<Enemy>();

        if (otherEnemy != null && otherEnemy != this)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;


        }
    }

    protected override void UpdateEnemyStates()
    {

        float _dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Shade_Idle:
                if (_dist <= chaseDistance)
                {
                    ChangeState(EnemyStates.Shade_Chase);
                }
                break;

            case EnemyStates.Shade_Chase:
                rb.MovePosition(Vector2.MoveTowards(transform.position, PlayerController.Instance.transform.position, Time.deltaTime * speed));
                FlipShade();

                break;
            case EnemyStates.Shade_Stunned:

                timer += Time.deltaTime;
                if (timer > stunnedTimer)
                {
                    timer = 0;
                    ChangeState(EnemyStates.Shade_Idle);

                }
                break;
            case EnemyStates.Shade_Death:
                break;
        }
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
        if (health > 0)
        {
            ChangeState(EnemyStates.Shade_Stunned);
        }
        else
        {
            ChangeState(EnemyStates.Shade_Death);
        }
    }
    protected override void DeathMethod(float _destroyTime)
    {
        rb.gravityScale = 12f;
        base.DeathMethod(_destroyTime);
    }

    protected override void ChangeCurrentAnimation()
    {
        
        
        
        if (GetCurrentEnemyState == EnemyStates.Shade_Idle)
        {
            anim.Play("knight_idle");
        }
        anim.SetBool("Walking", GetCurrentEnemyState == EnemyStates.Shade_Chase);
        if (GetCurrentEnemyState == EnemyStates.Shade_Death)
        {
            PlayerController.Instance.RestoreMana();
            SaveData.Instance.SavePlayerData();
            anim.SetTrigger("Death");
            Destroy(gameObject);
        }


    }
    protected override void Attack()
    {
        anim.SetTrigger("Attacking");
        PlayerController.Instance.TakeDamage(damage);
    }

    void FlipShade()
    {
        if (PlayerController.Instance.transform.position.x < transform.position.x)
        {
            sr.flipX = true;
        }
        else
        {
            sr.flipX = false;
        }
    }
}
