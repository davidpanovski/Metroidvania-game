using UnityEngine;

public class Crawler : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private float flipWaitTime;
    [SerializeField] private float ledgeCheckX;
    [SerializeField] private float ledgeCheckY;
    [SerializeField] private LayerMask whatIsGround;
    float timer;
    


    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 12f;
        
    }

   
    

    // Update is called once per frame



    protected override void UpdateEnemyStates()
    {
        if (health <= 0)
        {
            DeathMethod(0.05f);
        }

        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Crawler_Idle:
                //raycasting
                Vector3 ledgeCheckStart = transform.localScale.x > 0 ? new Vector3(ledgeCheckX, 0) : new Vector3(-ledgeCheckX, 0); //od kade kje pocne zrakot
                Vector2 wallCheckDir = transform.localScale.x > 0 ? transform.right : -transform.right; //vo koja nasoka kje odi zrakot

                if (!Physics2D.Raycast(transform.position + ledgeCheckStart, Vector2.down, ledgeCheckY, whatIsGround) ||
                    Physics2D.Raycast(transform.position, wallCheckDir, ledgeCheckX, whatIsGround))
                {
                    ChangeState(EnemyStates.Crawler_Flip);
                }
                if (transform.localScale.x > 0)
                {
                    rb.linearVelocity = new Vector2(speed,rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity=new Vector2 (-speed,rb.linearVelocity.y);
                }
                break;
            case EnemyStates.Crawler_Flip:
                timer += Time.deltaTime;
                
                if (timer > flipWaitTime)
                {
                    timer = 0;
                    
                    transform.localScale=new Vector2(transform.localScale.x*-1,transform.localScale.y);
                    ChangeState(EnemyStates.Crawler_Idle);

                  }
                break;
        }
    }

 
}
