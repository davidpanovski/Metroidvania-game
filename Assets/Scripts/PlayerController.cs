using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    public Rigidbody2D rb;

    [Header("Horizontal movement settings")]
    [SerializeField] private float walkSpeed = 1; //brzina; [SerializeField se korsiti da ni izleze kako editble pole vo editorot] 
    private float xAxis, yAxis;//nasoka
    private float gravity;


    [Header("Vertical movement setings")]
    [SerializeField] private float jumpForce = 45;
    [SerializeField] private int jumpBufferFrames; // kolku vreme inputot se chuva vo buffer
    private int jumpBufferCounter = 0;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;// kolku vreme posle napustanje na platforma sakame na igrachot da mu dademe vreme da skokne 
    private int airJumpsCounter = 0;
    [SerializeField] private int maxAirJumps;



    [Header("Ground Check setings")]

    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatisGround;

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashTime;
    [SerializeField] float dashCooldown;
    [SerializeField] GameObject dashEffect;
    private bool canDAsh = true;
    private bool dashed = false;


    bool attack = false;
    [Header("Attacking")]
    [SerializeField] float damage;
    float timeBetweenAttack, timeSinceAttack;//timeBetweenAttack = attack cooldown
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] GameObject slashEffect;

    [Header("Recoil")]
    [SerializeField] float recoilXSteps = 5;
    [SerializeField] float recoilYSteps = 5;
    [SerializeField] float recoilxSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    int StepsXrecoiled, StepsYrecoiled;

    [Header("Health")]
    [SerializeField] public int health;
    [SerializeField] public int maxHealth;
    [SerializeField] GameObject BloodSpurt;
    [SerializeField] float hitFlashSpeed;
    private SpriteRenderer sr;
    public delegate void OnHealthChangeDelegate();
    [HideInInspector]public OnHealthChangeDelegate onHealthChangedCallback;


    float healTimer;
    [Header("Healing")]
    [SerializeField] float timeToHeal;


    [Header("Mana Settings")]
    [SerializeField] Image manaStorage;
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;

    [Header("Spell Settings")]
    [SerializeField] float manaSpellCost = 0.3f;
    [SerializeField] float timeBetweenCast = 0.5f;
    float timeSinceCast;
    [SerializeField] GameObject Fireball;
    
    
    bool restoreTime;
    float restoreTimeSpeed;






    Animator anim;
    [HideInInspector]public PlayerStatesList pState;
    public static PlayerController Instance;

    public void Awake()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);  
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        pState = GetComponent<PlayerStatesList>();// referencirame player states
        rb = GetComponent<Rigidbody2D>(); 
        
        
        anim = GetComponent<Animator>();
        gravity = rb.gravityScale;
        sr=GetComponent<SpriteRenderer>();

        Health = maxHealth ;
        Mana = mana ;

    }

    // Update is called once per frame
    void Update()
    {
        if (pState.cutscene) return;
        GetInputs();// sakame inputsot da ni se updateiraat na sekoj frame
        UpdateJumpVariables();
        RestoreTimeScale();
        if (pState.dashing) return;
        Flip();
        Move();
        Jump();
        StartDash();
        Attack();
        
        FlashWhileInvincble();
        Heal();
        CAstSpell();
    }

    private void FixedUpdate()
    {
        if (pState.dashing) return;
        Recoil();
    }



    public IEnumerator WalkIntoNewScene(Vector2 _exitDir, float _delay)
    {
        //if i want to make it upwards
        if (_exitDir.y>0)
        {
            rb.linearVelocity = jumpForce * _exitDir;
        }

        //if exit dir requires horizntal movement
        if (_exitDir.x != 0)
        {
            xAxis = _exitDir.x > 0 ? 1 : -1;

            Move();
        }
        Flip();
        yield return new WaitForSeconds(_delay);
        pState.cutscene = false;

    }
    void CAstSpell()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame && timeSinceCast >= -timeBetweenCast && Mana >= manaSpellCost)
        {
            pState.casting = true;
            timeSinceCast = 0;
            StartCoroutine(CastCoroutine());

        }
        else
        {
            timeSinceCast += Time.deltaTime;
        } 
    }

    IEnumerator CastCoroutine()
    {
        anim.SetBool("Casting", true);
        yield return new WaitForSeconds(0.1f);

        if (yAxis ==0 || (yAxis<0 && isGrounded()))
        {
            GameObject _fireball= Instantiate(Fireball,SideAttackTransform.position,Quaternion.identity);

            //flip the fireball if looking in another direction
            if (pState.lookingRight)
            {
                _fireball.transform.eulerAngles = Vector3.zero;// ako gleda desno keep as normal
            }
            else
            {
                _fireball.transform.eulerAngles = new Vector2(_fireball.transform.eulerAngles.x, 180);//ako ne gleda desno rotiraj za 180
            }
            pState.recoilingX = true;
        }

        Mana -= manaSpellCost;
        yield return new WaitForSeconds(0.35f);
        anim.SetBool("Casting", false);
        pState.casting = false;
    }
    


    
    
    float Mana
    {
        get { return mana; }

        set { if (mana != value) { 
                mana=Mathf.Clamp(value, 0, 1);
                if (manaStorage!=null) {
                manaStorage.fillAmount = Mana;}
            } 
        }
    }

    public void BindManaUI(Image newManaStorage)
    {
        manaStorage = newManaStorage;

        if (manaStorage != null)
        {
            manaStorage.fillAmount = mana;
        }
    }

    void Heal()
    {
        if (Mouse.current.rightButton.isPressed && maxHealth > Health && Mana>0 && !pState.jumping && !pState.dashing)
        {
            pState.healing = true;
            anim.SetBool("Healing", true);
            //healing
            healTimer += Time.deltaTime;
            if (healTimer >= timeToHeal)
            {
                Health++;
                healTimer = 0;
            }
            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else
        {
            pState.healing= false;
            anim.SetBool("Healing", false);
            healTimer = 0;
        }

    }

    public void RestoreTimeScale()
    {
        if (restoreTime) {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.deltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        
        
        }
    }

    public void HitStopTime(float _newTimeScale,int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        Time.timeScale = _newTimeScale;

        if (_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));

        }
        else restoreTime = true;
    }

    IEnumerator StartTimeAgain(float delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(delay);
    }

    void GetInputs()
    {
        //nasoka
        xAxis = 0;
        yAxis = 0;

        if (Keyboard.current.aKey.isPressed)
        {
            xAxis = -1;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            xAxis = 1;
        }
        if (Keyboard.current.wKey.isPressed)
        {
            yAxis = 1;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            yAxis = -1;
        }
        attack = Mouse.current.leftButton.wasPressedThisFrame;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    void Flip()// vrtenje na igrachot na drugata strana so cel da nema moonwalking lol
    {
        // localscale go flipnuva objektot odnosno spriteot zavisi od znakot pred nego po obratnata oska koja se pristapuva
        if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
            pState.lookingRight = true;
        }
        else if (xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
            pState.lookingRight = false;
        }
    }
    public void TakeDamage(float damage)
    {
        Health -= Mathf.RoundToInt(damage);
        StartCoroutine(StopTakingDamage());
        
    }

    public int Health
    {
        get { return health; }

        set { if (health != value) { 
                
                health = Mathf.Clamp(value, 0, maxHealth); 

                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            
            
            }
            
       
                
                    
        }
    }
    

    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        GameObject _bloodSpurtParticles = Instantiate(BloodSpurt,transform.position,Quaternion.identity);
        Destroy(_bloodSpurtParticles,1.5f);
        anim.SetTrigger("TakeDamage");
       
        yield return new WaitForSeconds(1);
        pState.invincible = false;
    }

    public void FlashWhileInvincble()
    {
        sr.material.color= pState.invincible ? Color.Lerp(Color.white,Color.black,Mathf.PingPong(Time.time*hitFlashSpeed,1.0f)):Color.white;
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(walkSpeed * xAxis, rb.linearVelocity.y);
        anim.SetBool("Walking", rb.linearVelocity.x != 0 && isGrounded()); // igracot ja koristi animacijata walking koga e na zemja i se dvizhi

    }

    void StartDash()
    {
        if (Keyboard.current.cKey.isPressed && canDAsh && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;

        }

        if (isGrounded())
        {
            dashed = false;
        }
    }

    IEnumerator Dash()
    {
        canDAsh = false;
        pState.dashing = true;
        anim.SetTrigger("Dashing");
        int _dir = pState.lookingRight ? 1 : -1;
        rb.gravityScale = 0; // ja trgame gravitacijata na strana se so cel da igrachot mozhe horizontalno da se dvizhi bez da pagja nadole
        rb.linearVelocity = new Vector2(_dir * dashSpeed, 0);
        if (isGrounded()) Instantiate(dashEffect, transform);//?
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDAsh = true;
    }

    public bool isGrounded()//za da igrachot mozhe da skokne treba da proverime dali e prvo na zemja
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatisGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatisGround)
            //proveruvame vo slucaj igrachot da e na rabot dokolku mozhe da padne levo ili desno
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatisGround))
        //od kade trgnuva(dolniot del na karakterot),nasoka vo koja se dvizhi,dolzhina na zrakot(kolkudaleku kje patuva),shto bara da dopre
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Jump()
    {

        if (Keyboard.current.wKey.wasReleasedThisFrame && rb.linearVelocity.y > 3) //ako pustis kopce za skokanje dodeka se dvizhis nagore pagjas 
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            pState.jumping = false;// koga go pusta kopceto stanuva false
        }
        //ako ne skoka i imame seuste frames vo counterot toa znaci deka e kliknato kopce prethodno i mu davame da se ispolni vo frameot
        //koga igrachot povtorno pagja na zemja
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !pState.jumping) // dokolku kliknes skokas
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
            pState.jumping = true;

        }
        if (!isGrounded() && airJumpsCounter < maxAirJumps && Keyboard.current.wKey.wasPressedThisFrame)
        {
            pState.jumping = true;
            airJumpsCounter++;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
        }
        anim.SetBool("Jumping", !isGrounded()); // sekogash koga chovekot e vo vozduh
    }
    void UpdateJumpVariables()
    {
        if (isGrounded())//ako e vekje na zemja go stavam false zs kje treba za vo jump funkcijata
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpsCounter = maxAirJumps;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; //se namaluva bilo koga koga igrachot ne na platforma odnosno pagja, skoka etc
        }

        //koga kje se klikne kopceto se stava vo buffer i sekoj frame odzema eden
        //ova dava moznosta na igrachot da ne e sekogash tocen da mozhe porano da klikne i pak da mu se zacuva
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferFrames;

        }
        else
        {
            jumpBufferCounter--;
        }
    }

    private void Hit(Transform _attackTransform, Vector2 _attackArea,ref bool _recoilBool, Vector2 _recoilDir ,float _recoilStrenght)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);
        if (objectsToHit.Length > 0)
        {
            _recoilBool = true;
  
        }
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().EnemyHit(damage, _recoilDir,_recoilStrenght);
                if (objectsToHit[i].CompareTag("Enemy"))
                {
                    Mana += manaGain;
                }
            }


        }
    }

    void SlashEffectAtAngle(GameObject _slashEffect,int _effectAngle, Transform _attackTransform)
    {
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);   
    }

    void Attack()
        {
            timeSinceAttack += Time.deltaTime;
            if (attack && timeSinceAttack >= timeBetweenAttack)
            {
                timeSinceAttack = 0;
                anim.SetTrigger("Attacking");

                if (yAxis == 0 || yAxis < 0 && isGrounded())
                {
                int _side = pState.lookingRight ? 1 : -1;//koja strana da e recoilot 
                
                    Hit(SideAttackTransform, SideAttackArea,ref pState.recoilingX,Vector2.right*_side, recoilYSpeed);//avtomtski pravi side attack ako e na zemja ili idle
                    Instantiate(slashEffect, SideAttackTransform);

                }
                else if (yAxis > 0)
                {
                    Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY,Vector2.up, recoilYSpeed); //ako igrachot drzhi W
                    SlashEffectAtAngle(slashEffect, 90, UpAttackTransform);
                }
                else if (yAxis < 0 && !isGrounded())
               {
                    Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY,Vector2.down, recoilYSpeed); //ako igrachot drzhi S
                    SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);
                }
            }
        }

    void StopRecoilX()
    {
        StepsXrecoiled = 0;
        pState.recoilingX = false;
    }

    void StopRecoilY()
    {
        StepsYrecoiled = 0;
        pState.recoilingY = false;
    }

    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.linearVelocity = new Vector2(-recoilxSpeed, 0);

            }
            else
            {
                rb.linearVelocity = new Vector2(recoilxSpeed, 0);
            }
        }
        if (pState.recoilingY)
        {
            if (yAxis < 0)
            {
                rb.gravityScale = 0;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, recoilYSpeed);
            }
            else
            {
                rb.linearVelocity=new Vector2(rb.linearVelocity.x,-recoilYSpeed); 
                rb.gravityScale = 0;
            }
            airJumpsCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }
        
        
        //koga da prestane recoilot 
        if (pState.recoilingX && StepsXrecoiled < recoilXSteps)
        {
            StepsXrecoiled ++;
        }
        else
        {
            StopRecoilX();
        }

        if (pState.recoilingY && StepsYrecoiled < recoilYSteps)
        {
            StepsYrecoiled++;
        }
        else
        {
            StopRecoilY();
        }
        if (isGrounded())
        {
            StopRecoilY();  
        }



    }


    }
