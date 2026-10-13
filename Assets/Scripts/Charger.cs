using UnityEngine;

public class Charger : Enemy
{
    [SerializeField] private float ledgeCheckX;
    [SerializeField] private float ledgeCheckY;
    [SerializeField] private float chargeSpeedMultiplier;
    [SerializeField] private float chargeDuration;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask whatIsGround;

    float timer;

    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Charger_Idle);
        rb.gravityScale = 12f;
    }

    protected override void UpdateEnemyStates()
    {
        if (health <= 0)
        {
            DeathMethod(0.05f);
        }

        Vector3 ledgeCheckStart = transform.localScale.x > 0
            ? new Vector3(ledgeCheckX, 0)
            : new Vector3(-ledgeCheckX, 0);

        Vector2 wallCheckDir = transform.localScale.x > 0
            ? transform.right
            : -transform.right;

        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Charger_Idle:
                bool grounded = rb.IsTouchingLayers(whatIsGround);

                bool groundAhead = Physics2D.Raycast(
                    transform.position + ledgeCheckStart,
                    Vector2.down,
                    ledgeCheckY,
                    whatIsGround);

                bool wallAhead = Physics2D.Raycast(
                    transform.position,
                    wallCheckDir,
                    ledgeCheckX,
                    whatIsGround);

                if (grounded && (!groundAhead || wallAhead))
                {
                    transform.localScale = new Vector3(
                        -transform.localScale.x,
                        transform.localScale.y,
                        transform.localScale.z);
                }


                float direction = transform.localScale.x > 0 ? 1f : -1f;

                bool playerInFront =
                    (player.transform.position.x - transform.position.x) * direction > 0f;

                RaycastHit2D _hit = Physics2D.Raycast(
                    transform.position + Vector3.right * direction * ledgeCheckX,
                    Vector2.right * direction,
                    ledgeCheckX * 10f
                );

                if (playerInFront &&
                    _hit.collider != null &&
                    _hit.collider.CompareTag("Player"))
                {
                    ChangeState(EnemyStates.Charger_Suprised);
                }

                if (transform.localScale.x > 0)
                {
                    rb.linearVelocity = new Vector2(
                        speed, rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(
                        -speed, rb.linearVelocity.y);
                }

                break;

            case EnemyStates.Charger_Suprised:
                rb.linearVelocity = new Vector2(0, jumpForce);
                ChangeState(EnemyStates.Charger_Charge);

                break;

            case EnemyStates.Charger_Charge:
                timer += Time.deltaTime;

                if (timer < chargeDuration)
                {
                    if (rb.IsTouchingLayers(whatIsGround))
                    {
                        if (transform.localScale.x > 0)
                        {
                            rb.linearVelocity = new Vector2(
                                speed * chargeSpeedMultiplier,
                                rb.linearVelocity.y);
                        }
                        else
                        {
                            rb.linearVelocity = new Vector2(
                                -speed * chargeSpeedMultiplier,
                                rb.linearVelocity.y);
                        }
                    }
                    else
                    {
                        rb.linearVelocity = new Vector2(
                            0, rb.linearVelocity.y);
                    }
                }
                else
                {
                    timer = 0;
                    ChangeState(EnemyStates.Charger_Idle);
                }

                break;
        }
    }

    protected override void ChangeCurrentAnimation()
    {
        if (GetCurrentEnemyState == EnemyStates.Charger_Idle)
        {
            anim.speed = 1;
        }

        if (GetCurrentEnemyState == EnemyStates.Charger_Charge)
        {
            anim.speed = chargeSpeedMultiplier;
        }
    }
}