using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 movementDirection;
    public float minMoveSpeed = 2f;
    public float maxMoveSpeed = 8f;
    public float acceleration = 1f;
    private float movementSpeed;
    private bool move;

    private bool rotate;
    public float rotationSpeed = 70f;

    public float recoilDistance = 0.15f;

    InputAction moveAction;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        EnterRotateMode();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (moveAction.triggered)
        {
            MovePressed();
        }
        
        if (move)
        {
            MoveAhead();
        }

        if (rotate)
        {
            transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
        }
    }

    void MovePressed()
    {
        if(rotate)
        {
            LeaveRotateMode();
        }
    }

    void MoveAhead()
    {
        transform.position += (movementDirection * Time.deltaTime * movementSpeed);
        
        movementSpeed += (acceleration * Time.deltaTime);
        if (movementSpeed > maxMoveSpeed)
        {
            movementSpeed = maxMoveSpeed;
        }
    }

    void EnterRotateMode()
    {
        StopMovement();
        rotate = true;
    }

    void StopMovement()
    {
        move = false;
        movementSpeed = 0f;
    }

    void LeaveRotateMode()
    {
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

        EnterRotateMode();
    }
}
