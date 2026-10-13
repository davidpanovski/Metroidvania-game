using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling=false;
    
    protected PlayerController player;
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;
    [SerializeField] protected GameObject blood;

    protected float recoilTimer;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator anim;

    protected enum EnemyStates
    {
        //Crawler states
        Crawler_Idle,
        Crawler_Flip,

        //Bat
        Bat_Idle,
        Bat_Chase,
        Bat_Stunned,
        Bat_Death,

        //
        Charger_Idle,
        Charger_Suprised,
        Charger_Charge

    }
    protected EnemyStates currentEnemyState;

    protected virtual void Start()
    {
        rb=GetComponent<Rigidbody2D>();
        
        player = PlayerController.Instance;
        sr=GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();    
    }



    // Update is called once per frame
    protected virtual void Update()
    {
        
        

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
        else
        {
            UpdateEnemyStates();
        }
        
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        if (!isRecoiling)
        {
            GameObject _Blood=Instantiate(blood, transform.position, Quaternion.identity);
            Destroy(_Blood, 5.5f);
            rb.linearVelocity = _hitForce * recoilFactor * _hitDirection;
        }
    }
    protected virtual void DeathMethod(float _destroyTime)
    {
        Destroy(gameObject,_destroyTime);

    }


    

   protected void OnCollisionStay2D(Collision2D _other) { 
   

        if (_other.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invincible && health >0)
        {
            
            Attack();
            PlayerController.Instance.HitStopTime(0, 5, 0.5f);
        }
    }


    protected virtual void UpdateEnemyStates()
    {

    }
    protected virtual void ChangeCurrentAnimation()
    {

    }

    protected virtual EnemyStates GetCurrentEnemyState
    {
        get { return currentEnemyState; }

        set{
            if (currentEnemyState!=value)
            {
                currentEnemyState = value;
                ChangeCurrentAnimation();
            }
        }
    }

    protected void ChangeState(EnemyStates _newState)
    {
        GetCurrentEnemyState = _newState;
    }


    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
    }
}
