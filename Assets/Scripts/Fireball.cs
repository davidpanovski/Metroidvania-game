using UnityEngine;

public class Fireball : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] float damage;
    [SerializeField] float hitForce;
    [SerializeField] int speed;
    [SerializeField] float lifetime=1;
    
    
    void Start(){
        Destroy(gameObject,lifetime);
        
    }

    private void FixedUpdate()
    {
        transform.position += speed * transform.right;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        _other.GetComponent<Enemy>().EnemyHit(damage, (_other.transform.position - transform.position).normalized,
            -hitForce);
    }
}
