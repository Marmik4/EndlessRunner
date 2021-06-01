using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const int COIN_SCORE_AMOUNT = 1;
    private const int STANDOFF_DISTANCE = 100;
    //private const int MAXPOWERUP_DISTANCE = 50;

    public static GameManager Instance { set; get; }

    public bool isDead { set; get; }
    public bool gameIsRunning = false;
    public bool standOff { set; get; }
    public bool PowerUP { set; get; }
    public GameObject Player;
    private PlayerMovement movement;

    //DEATH MENU
    public Animator deathAnim;
    public Text deadScoreText, deadCoinScoreText;

    //UI AND UI FIELDS
    public Text Menu;
    public Text coinText, modifierText, scoreText;
    private float score, coinScore, modifierScore;
    private int distanceMoved, distanceMovedafterPowerUP;
    private int lastScore, lastDistance = 0;
    //private int lastPowerUPDistance = 0;

    //STANDOFF SWIPES
    public bool standOffcollided = false;
    private int temp = -1;  //0 = up, 1 = down, 2 = left, 3 = right, -1 = DEFAULT
    private int id;
    private int timeToSwipe = 4;
    private int lastSwipeTime = 0;
    private int passFlag = 0;  //0 = pass , 1 = 1st fail , 2 = failed
    private int failFlagValue = 2;
    private int swipeCounter = 3;
    private int temp_counter = 0;
    public List<GameObject> swipes = new List<GameObject>();

    //SWIPE VARIABLES
    private int swipe = -2;  //0 = up, 1 = down, 2 = left, 3 = right
    private float startSwipeTime;
    private Vector2 startSwipePosition;
    private float swipeTime;
    private float swipeDistance;

    private float endSwipeTime;
    private Vector2 endSwipePosition;

    public float maxSwipeTime;
    public float minSwipeDistance;

    //PLAYER
    private Animator playerAnim;

    private void Awake()
    {
        Instance = this;
        playerAnim = Player.GetComponent<Animator>();
        movement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        standOff = false;
        PowerUP = false;
        modifierScore = 1f;
        score = 0;
        coinScore = 0;
        modifierText.text = "x" + modifierScore.ToString("0.0");
        Mathf.Clamp(passFlag, 0, 3);
    }

    private void Update()
    {
        distanceMoved = (int)GameObject.FindGameObjectWithTag("Player").transform.position.z - lastDistance;
        distanceMovedafterPowerUP = (int)GameObject.FindGameObjectWithTag("Player").transform.position.z - lastDistance;

        if (distanceMoved > STANDOFF_DISTANCE)
        {
            lastDistance = (int)GameObject.FindGameObjectWithTag("Player").transform.position.z;
            standOff = true;
        }

        /*if (distanceMovedafterPowerUP > MAXPOWERUP_DISTANCE)
        {
            lastPowerUPDistance = (int)GameObject.FindGameObjectWithTag("Player").transform.position.z;
            PowerUP = true;
        }*/

        if (Input.touchCount > 0 && !gameIsRunning)
        {
            gameIsRunning = true;
            movement.StartRunning();
            playerAnim.SetBool("gameisRunning", true);
            Menu.enabled = false;
            FindObjectOfType<BuildingSpawner>().isScrolling = true;
            FindObjectOfType<CameraFollow>().IsMoving = true;
        }
        
        //Score UI update
        if (gameIsRunning && !isDead)
        {
            score += (Time.deltaTime * modifierScore);
            if (lastScore != (int)score)
            {
                lastScore = (int)score;
                scoreText.text = score.ToString("0");
            }
        }

        #region standoff
        if (standOffcollided)
        {
            CheckSwipe();

            if (temp_counter == 0)
                SwipeSpawn();

            if (swipe == -2 && (int)Time.time - lastSwipeTime > timeToSwipe)
            {
                passFlag++;
                swipes[id].SetActive(false);
                SwipeSpawn();
            }

            if (swipe != temp && swipe != -2 && (int)Time.time - lastSwipeTime < timeToSwipe)
            {
                passFlag++;
                swipes[id].SetActive(false);
                SwipeSpawn();
            }

            if (swipe == temp && (int)Time.time - lastSwipeTime < timeToSwipe)
            {
                passFlag = 0;
                swipes[id].SetActive(false);
                SwipeSpawn();
            }

            if (passFlag >= failFlagValue)
            {
                OnStandOffExit();
                OnDead();
            }
        }
        #endregion
    }

    public void GetCoin()
    {
        coinScore += COIN_SCORE_AMOUNT;
        score += coinScore;
        coinText.text = coinScore.ToString("0");
    }

    public void UpdateModifier(float modifierAmount)
    {
        modifierScore = 1f + modifierAmount;
        modifierText.text = "x" + modifierScore.ToString("0.0");
    }

    public void OnPlayButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level01");
    }

    public void OnStandOffEnter()
    {
        lastSwipeTime = (int)Time.time;
        //Time.timeScale = 0.2f;
        //Time.fixedDeltaTime = 0.02f * Time.timeScale;
        movement.speed = 0.5f;
        standOffcollided = true;
        temp_counter = 0;
        passFlag = 0;
    }
    
    public void OnStandOffExit()
    {
        swipes[id].SetActive(false);
        standOffcollided = false;
        LevelManager.Instance.standOffGenerated = false;
        standOff = false;
        movement.speed = movement.speedSave;
    }

    private void SwipeSpawn()
    {
        if (temp_counter < swipeCounter)
        {
            temp_counter++;
            lastSwipeTime = (int)Time.time;
            id = UnityEngine.Random.Range(0, swipes.Count);
            swipes[id].SetActive(true);
            temp = swipes[id].GetComponent<Swipes>().swipeIndex;
        }
        else
            OnStandOffExit();
    }

    public void OnDead()
    {
        movement.isRunning = false;
        isDead = true;
        gameIsRunning = false;
        FindObjectOfType<BuildingSpawner>().isScrolling = false;
        playerAnim.SetTrigger("isDead");
        deathAnim.SetBool("Dead", true);
        StartCoroutine("AdMenu", 5f);
        deadScoreText.text = score.ToString("0");
        deadCoinScoreText.text = coinScore.ToString("0");
    }

    IEnumerator AdMenu(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        deathAnim.SetBool("Dead", false);
    }
    private void CheckSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
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
            swipe = -2;
        }       
    }

    private void SwipeControl()
    {
        Vector2 Distance = endSwipePosition - startSwipePosition;
        float xDistance = Mathf.Abs(Distance.x);
        float yDistance = Mathf.Abs(Distance.y);

        if (xDistance > yDistance)
        {
            if (Distance.x > 0)
            {
                swipe = 3;
            }
            else if (Distance.x < 0)
            {
                swipe = 2;
            }
        }
        else if (xDistance < yDistance)
        {
            if (Distance.y > 0)
            {
                swipe = 0;
            }
            if (Distance.y < 0)
            {
                swipe = 1;
            }
        }
    }
}
