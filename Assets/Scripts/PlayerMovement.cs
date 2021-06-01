using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { set; get; }

    //CONSTANTS
    private const float LANE_DISTANCE = 2.0f;
    private const float TURN_SPEED = 0.5f;
    private const float MAGNET_TIME = 15f;
    private const float POWERUP_TIME = 15f;

    //
    public bool hasPowerUp = false;
    public bool isRunning = false;
    public bool isImmortal = false;

    //ANIMATION
    private Animator anim;

    //MOVEMENT VARIABLES
    private CharacterController playerController;
    private float jumpForce = 5.0f;
    private float gravity = 12.0f;
    private float verticalVelocity;

    public float Lane = 1;  // 0 = left, 1 = middle, 2 = right

    //SWIPE VARIABLES
    public bool swipeUp, swipeDown;
    private float startSwipeTime;
    private Vector2 startSwipePosition;
    private float swipeTime;
    private float swipeDistance;

    private float endSwipeTime;
    private Vector2 endSwipePosition;

    public float maxSwipeTime;
    public float minSwipeDistance;

    //SPEED MODIFIER
    private float originalSpeed = 7.0f;
    public float speed;
    public float speedSave;
    private float speedIncreasedLastTick;
    private float speedIncreasedTime = 2.5f;
    private float speedIncreaseAmount = 0.1f;

    //POWER UP
    private GameObject magnetCollider;

    void Start()
    {
        playerController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        magnetCollider = GameObject.FindWithTag("MagnetCollider");
        magnetCollider.SetActive(false);
        speed = originalSpeed;
    }

    void Update()
    {
        if (GameManager.Instance.isDead)
            return;

        if (!isRunning)
            return;

        if (!GameManager.Instance.standOff && Time.time-speedIncreasedLastTick>speedIncreasedTime && speed >= speedSave)
        {
            speedIncreasedLastTick = Time.time;
            speed += speedIncreaseAmount;
            speedSave = speed;
            GameManager.Instance.UpdateModifier(speed - originalSpeed);
        }

        CheckSwipe();

        //Calculate future position
        Vector3 targetPosition = transform.position.z * Vector3.forward;
        if (Lane == 0)
            targetPosition += Vector3.left * LANE_DISTANCE;
        else if (Lane == 2)
            targetPosition += Vector3.right * LANE_DISTANCE;

        bool isGrounded = IsGrounded();
        anim.SetBool("Grounded", isGrounded);

        //Calculate Y
        if (isGrounded) //if Grounded
        {
            verticalVelocity = -0.1f;
            if(swipeUp)
            {
                anim.SetTrigger("Jump");
                if(hasPowerUp)
                    verticalVelocity = 1.3f*jumpForce;
                else
                    verticalVelocity = jumpForce;
            }
            if(swipeDown)
            {
                anim.SetTrigger("Slide");
                SlideStart();
                Invoke("SlideStop", 1f);
            }
        }
        else
        {
            verticalVelocity -= (gravity * Time.deltaTime);

            if(swipeDown)
            {
                verticalVelocity = -jumpForce;
            }
        }

        //calculate move delta
        Vector3 moveVector = Vector3.zero;
        moveVector.x = (targetPosition - transform.position).normalized.x * speed;
        moveVector.y = verticalVelocity;
        moveVector.z = speed;

        //Move player
        playerController.Move(moveVector * Time.deltaTime);

        //Rotate player towards direction
        Vector3 dir = playerController.velocity;
        dir.y = 0;
        transform.forward = Vector3.Lerp(transform.forward, dir, TURN_SPEED);

        //Default
    }

    private void CheckSwipe()
    {
        if(Input.touchCount>0)
        {
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began)
            {
                startSwipePosition = touch.position;
                startSwipeTime = Time.time;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                endSwipePosition = touch.position;
                endSwipeTime = Time.time;
                swipeTime = endSwipeTime - startSwipeTime;
                swipeDistance = (endSwipePosition - startSwipePosition).magnitude;
                if (swipeTime < maxSwipeTime && swipeDistance > minSwipeDistance)
                {
                    //swiped = true;
                    //Debug.Log(swiped);
                    SwipeControl();
                }
            }
        }
        else
        {
            swipeUp = false;
            swipeDown = false;
        }
    }

    private void SwipeControl()
    {
        Vector2 Distance = endSwipePosition - startSwipePosition;
        float xDistance = Mathf.Abs(Distance.x);
        float yDistance = Mathf.Abs(Distance.y);

        if(xDistance>yDistance)
        {
            if (Distance.x > 0)
            {
                MoveLane(false);
            }
            else if (Distance.x < 0)
            {
                MoveLane(true);
            }
            swipeUp = false;
            swipeDown = false;
        }
        else if (xDistance < yDistance)
        {
            if (Distance.y > 0)
            {
                swipeUp = true;
            }
            if (Distance.y < 0)
            {
                swipeDown = true;
            }
        }
    }

    private void SlideStart()
    {
        playerController.height /= 2;
        playerController.center = (new Vector3(0f, 0.71f, 0f));
    }

    private void SlideStop()
    {
        playerController.height *= 2;
        playerController.center = (new Vector3(0f, 0.91f, 0f));
    }

    private void MoveLane(bool goingRight)
    {
        Lane += (goingRight) ? -1 : 1;
        Lane = Mathf.Clamp(Lane, 0, 2);
    }

    private bool IsGrounded()
    {
        Ray groundRay = new Ray(new Vector3(playerController.bounds.center.x, (playerController.bounds.center.y - playerController.bounds.extents.y) + 0.2f, playerController.bounds.center.z), Vector3.down);
        return Physics.Raycast(groundRay,0.2f+0.1f);
    }

    public void StartRunning()
    {
        isRunning = true;
    }

    private void Crash(GameObject obstacle)
    {
        //Debug.Log(obstacle.name);
        if (isImmortal)
            obstacle.SetActive(false);
        else 
        {
            playerController.height /= 4;
            playerController.center = (new Vector3(0f, 0.71f, 0f));
            anim.SetTrigger("isDead");
            //isRunning = false;
            GameManager.Instance.OnDead();
        }
    }

    private void CoinCollection()
    {
        GameManager.Instance.GetCoin();
    }

    private void InitiateStandOff()
    {
        GameManager.Instance.OnStandOffEnter();
    }

    private void OnPowerUPTaken(PowerUPType type)
    {
        switch (type)
        {
            case PowerUPType.magnet:
                StartCoroutine("MagnetTimeDelay", MAGNET_TIME);
                break;
            case PowerUPType.powerJump:
                StartCoroutine("PowerUpTime", POWERUP_TIME);
                break;
            case PowerUPType.immortal:
                isImmortal = true;
                StartCoroutine("PowerUpTime", POWERUP_TIME);
                break;
            case PowerUPType.fly:
                StartCoroutine("PowerUpTime", POWERUP_TIME);
                break;
        }
    }

    IEnumerator MagnetTimeDelay(float delay)
    {
        magnetCollider.SetActive(true);
        yield return new WaitForSeconds(delay);
        magnetCollider.SetActive(false);
    }
    IEnumerator PowerUpTime(float delay)
    {
        hasPowerUp = true;
        yield return new WaitForSeconds(delay);
        hasPowerUp = false;
        isImmortal = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch(hit.gameObject.tag)
        {
            case "Obstacle":
                Crash(hit.gameObject.GetComponent<GameObject>());
                break;
        }    
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Coin":
                CoinCollection();
                break;
            case "standOff":
                InitiateStandOff();
                break;
            case "PowerUp":
                OnPowerUPTaken(other.GetComponent<PowerupType>().pType);
                break;
        }
    }
}
