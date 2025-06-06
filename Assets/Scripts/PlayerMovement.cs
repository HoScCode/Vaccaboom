using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 movementDirection;
    public float minMoveSpeed = 2f;
    public float maxMoveSpeed = 15f;
    public float acceleration = 0.3f;
    private float movementSpeed;
    private bool move;
    private bool inputEnabled = true;

    public float delay = 0.5f;
    private float currentDelay;

    private bool rotate;
    public float rotationSpeed = 70f;

    public float recoilDistance = 0.15f;

    InputAction moveAction;

    private Rigidbody rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        rb = GetComponent<Rigidbody>();
        EnterRotateMode();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (moveAction.triggered && inputEnabled)
        {
            MovePressed();
        }
        
        if (move && inputEnabled)
        {
            MoveAhead();
        }

        if (rotate)
        {
            transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);

            currentDelay -= Time.deltaTime;
            if (currentDelay < 0)
            {
                currentDelay = 0;
            }
        }
    }

    void MovePressed()
    {
        if(rotate && currentDelay <= 0)
        {
            LeaveRotateMode();
        }

        if (move)
        {
            Accelerate();
        }
        
    }

    public void DisableMovement()
    {
        inputEnabled = false;
        move = false;
        rotate = false;
    }

    void MoveAhead()
    {
        transform.position += (movementDirection * Time.deltaTime * movementSpeed);     
    }

    void EnterRotateMode()
    {
        rb.constraints = RigidbodyConstraints.FreezePosition;
        StopMovement();
        currentDelay = delay;
        rotate = true;
    }

    void StopMovement()
    {
        move = false;
        movementSpeed = 0f;
    }

    void Accelerate()
    {
        movementSpeed += acceleration;

        if (movementSpeed > maxMoveSpeed)
        {
            movementSpeed = maxMoveSpeed;
        }
    }

    void LeaveRotateMode()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        movementDirection = transform.forward;
        rotate = false;
        movementSpeed = minMoveSpeed;
        move = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            ObstacleHit();
        }
    }

    void ObstacleHit()
    {
        StopMovement();

        transform.position -= movementDirection * recoilDistance;

        if (inputEnabled)
        {
            EnterRotateMode();
        }
    }
}
