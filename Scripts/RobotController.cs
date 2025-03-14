using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    public bool Animations = true;
    public float RotationSpeed = 90f;
    public float MoveSpeed = 2f;
    public int MaxMoveDistance = 1;


    [SerializeField] private bool hasCargo = false;


    private Quaternion targetRotation;
    private Vector3 targetPosition;
    private Animator animator;


    private bool isMoving = false;
    private bool isRotating = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        targetRotation = transform.rotation;
        targetPosition = transform.position;
        RotateTo(3);
    }

    void Update()
    {
        
        MoveTo(new Vector2(2, -5));
        MoveTo(new Vector2(3, 3));
    }



    public void MoveTo(Vector2 target) 
    {   
        if(isMoving || isRotating) 
        {
            return;
        }


        targetPosition = new Vector3(target.x, transform.position.y, target.y);

        if(targetPosition == transform.position) return;

        Vector3 direction = targetPosition - transform.position;
        if(direction.magnitude > MaxMoveDistance) 
        {
            targetPosition = transform.position + direction.normalized * MaxMoveDistance;
            direction = targetPosition - transform.position;
        }

        Debug.Log($"Current position: {transform.position}, Target: {targetPosition}");
        if(Animations)
        {
            animator.SetFloat("Move", direction.magnitude);
        }
        else
        {
            StartCoroutine(MoveObject()); 
        }
    }


    public void RotateTo(int direction)
    {
        if (isMoving || isRotating) return;
        if(direction < 0 || direction > 3)
        {
            Debug.Log("0 <= direction <= 3");
            return;
        }

        if(Animations)
        {
            animator.SetInteger("Rotate", direction);
        }
        else
        {
 
        

            // direction: 0 = вперед, 1 = вправо, 2 = назад, 3 = влево
            float angle = direction * 90f;
            Quaternion relativeRotation = Quaternion.Euler(0, angle, 0);
            targetRotation = relativeRotation * transform.rotation; 
            Debug.Log($"Current rotation: {transform.eulerAngles}, Target: {targetRotation.eulerAngles}");
            StartCoroutine(RotateObject());
        }
        


    }

    public IEnumerator MoveObject()
    {
        isMoving = true;
        float totalMovementTime = Vector3.Distance(transform.position, targetPosition) / MoveSpeed;
        float currentMovementTime = 0f;
        while (Vector3.Distance(transform.position, targetPosition) > 0)
        {
            currentMovementTime += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, targetPosition, currentMovementTime / totalMovementTime);
            yield return null;
        }
        transform.position = targetPosition;
        isMoving = false;
    }

    public IEnumerator RotateObject()
    {
        isRotating = true;
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, 
                    targetRotation, 
                    RotationSpeed * Time.deltaTime
                );
                yield return null;
            }
        transform.rotation = targetRotation;
        isRotating = false;
    }

    
    public void PickUpCargo()
    {
        if (!hasCargo)
        {
            hasCargo = true;
            if(Animations)
            {
                animator.SetTrigger("PickUpCargo");
            }
        }
    }

    public void DropCargo()
    {
        if (hasCargo)
        {
            hasCargo = false;
            if(Animations)
            {
                animator.SetTrigger("DropCargo");
            }
        }
    }

}