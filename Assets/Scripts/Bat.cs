using UnityEngine;

public class Bat : Enemy
{
    [SerializeField] private float chaseDistance;
   
    [SerializeField] float stunnedTimer;
    float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Bat_Idle);
    }
    protected override void Update()
    {
        base.Update();
        if (!PlayerController.Instance.pState.alive)
        {
            ChangeState(EnemyStates.Bat_Idle);
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

        float _dist= Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Bat_Idle:
                if (_dist <= chaseDistance)
                { 
                    ChangeState(EnemyStates.Bat_Chase); 
                }
                break;

            case EnemyStates.Bat_Chase:
                rb.MovePosition(Vector2.MoveTowards(transform.position, PlayerController.Instance.transform.position,Time.deltaTime*speed));
                FlipBat();

                break;
            case EnemyStates.Bat_Stunned:

                timer += Time.deltaTime;
                if (timer > stunnedTimer)
                {
                    timer = 0;
                    ChangeState(EnemyStates.Bat_Idle);

                }
                break;
            case EnemyStates.Bat_Death:
                break;
        }
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
        if (health > 0)
        {
            ChangeState(EnemyStates.Bat_Stunned);
        }
        else
        {
            ChangeState(EnemyStates.Bat_Death);
        }
    }
    protected override void DeathMethod(float _destroyTime)
    {
        rb.gravityScale = 12f;
        base.DeathMethod(_destroyTime);
    }

    protected override void ChangeCurrentAnimation() { 
        anim.SetBool("Idle", GetCurrentEnemyState==EnemyStates.Bat_Idle);
        anim.SetBool("Chase", GetCurrentEnemyState == EnemyStates.Bat_Chase);
        anim.SetBool("Stunned", GetCurrentEnemyState==EnemyStates.Bat_Stunned);
        
        if (GetCurrentEnemyState == EnemyStates.Bat_Death)
        {
            anim.SetTrigger("Death");
        }
    
    
    }

    void FlipBat()
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
    // Update is called once per frame

}
