using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    private Rigidbody2D rb;
    
    [Header("Horizontal movement settings")]
    [SerializeField] private float walkSpeed = 1; //brzina; [SerializeField se korsiti da ni izleze kako editble pole vo editorot] 
    private float xAxis;//nasoka
    private float gravity;


    [Header("Vertical movement setings")]
    [SerializeField] private float jumpForce = 45;
    [SerializeField] private int jumpBufferFrames; // kolku vreme inputot se chuva vo buffer
    private int jumpBufferCounter=0;
    private float coyoteTimeCounter=0;
    [SerializeField]private float coyoteTime;// kolku vreme posle napustanje na platforma sakame na igrachot da mu dademe vreme da skokne 
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

    Animator anim;
    private PlayerStatesList pState;
    public static PlayerController Instance;

    public void Awake()
    {
        if(Instance!=null && this != Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;    
        }
    }
     // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pState = GetComponent<PlayerStatesList>();// referencirame player states
        rb = GetComponent<Rigidbody2D>();
        anim=GetComponent<Animator>();
        gravity = rb.gravityScale;

    }

    // Update is called once per frame
    void Update()
    {

        GetInputs();// sakame inputsot da ni se updateiraat na sekoj frame
        UpdateJumpVariables();
        if (pState.dashing) return;
        Flip();
        Move();
        Jump();
        StartDash();
    }

    void GetInputs()
    {
        //nasoka
        xAxis = 0;

        if (Keyboard.current.aKey.isPressed)
        {
            xAxis = -1;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            xAxis = 1;
        }
    }

    void Flip()// vrtenje na igrachot na drugata strana so cel da nema moonwalking lol
    {
        // localscale go flipnuva objektot odnosno spriteot zavisi od znakot pred nego po obratnata oska koja se pristapuva
        if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
        else if (xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
        }
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(walkSpeed * xAxis, rb.linearVelocity.y);
        anim.SetBool("Walking", rb.linearVelocity.x!=0 && isGrounded()); // igracot ja koristi animacijata walking koga e na zemja i se dvizhi

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
        rb.gravityScale = 0; // ja trgame gravitacijata na strana se so cel da igrachot mozhe horizontalno da se dvizhi bez da pagja nadole
        rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        if (isGrounded()) Instantiate(dashEffect, transform);//?
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDAsh=true;
    }

    public bool isGrounded()//za da igrachot mozhe da skokne treba da proverime dali e prvo na zemja
    {
        if (Physics2D.Raycast(groundCheckPoint.position,Vector2.down,groundCheckY,whatisGround) 
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX,0,0), Vector2.down, groundCheckY, whatisGround) 
            //proveruvame vo slucaj igrachot da e na rabot dokolku mozhe da padne levo ili desno
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX,0,0), Vector2.down, groundCheckY, whatisGround))
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

        if (Keyboard.current.wKey.wasReleasedThisFrame && rb.linearVelocity.y > 0) //ako pustis kopce za skokanje dodeka se dvizhis nagore pagjas 
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            pState.jumping = false;// koga go pusta kopceto stanuva false
        }
        if (!pState.jumping) 
         
        {//ako ne skoka i imame seuste frames vo counterot toa znaci deka e kliknato kopce prethodno i mu davame da se ispolni vo frameot
         //koga igrachot povtorno pagja na zemja

            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0) // dokolku kliknes skokas
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
                pState.jumping = true;

            }
            else if (!isGrounded() && airJumpsCounter < maxAirJumps && Keyboard.current.wKey.wasPressedThisFrame)
            {
                pState.jumping = true;
                airJumpsCounter++;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
            }

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
            coyoteTimeCounter-=Time.deltaTime; //se namaluva bilo koga koga igrachot ne na platforma odnosno pagja, skoka etc
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


}