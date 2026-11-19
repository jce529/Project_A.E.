using UnityEngine;

public class PatrolMovement : MonoBehaviour
{
    
    [Header("순찰 설정")]
    [SerializeField] private float moveSpeed;    
    [SerializeField] private float patrolDistance; 

    // === 내부에서 사용할 변수들 ===
    private Vector3 startPosition; 
    private int moveDirection = 1; 

    void Start()
    {
      
        startPosition = transform.position;
    }

    void Update()
    {
       

       
        transform.Translate(Vector2.right * moveDirection * moveSpeed * Time.deltaTime);

       
        if (transform.position.x >= startPosition.x + patrolDistance / 2)
        {
            
            moveDirection = -1;
            
            FlipEnemy(false);
        }
     
        else if (transform.position.x <= startPosition.x - patrolDistance / 2)
        {
        
            moveDirection = 1;
           
            FlipEnemy(true);
        }
    }

   
    private void FlipEnemy(bool movingRight)
    {
       
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            
            if (movingRight)
            {
                sr.flipX = false;
            }
           
            else
            {
                sr.flipX = true;
            }
        }
    }
}