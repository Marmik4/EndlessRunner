using UnityEngine;

public class Coin : MonoBehaviour
{
    private const float COIN_MOVESPEED = 15f;


    private Animator anim;
    private Transform player;
    private bool isMagnet = false;
    private Transform coinTransform;


    void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        coinTransform = gameObject.GetComponent<Transform>();
    }

    private void Update()
    {
        if(isMagnet)
            coinTransform.position = Vector3.MoveTowards(coinTransform.position, player.transform.position, COIN_MOVESPEED*Time.deltaTime);           
    }

    private void OnTriggerEnter(Collider other) 
    {
        switch (other.gameObject.tag)
        {
            case "Player":
                gameObject.SetActive(false);
                GameManager.Instance.GetCoin();
                break;
            case "MagnetCollider":
                isMagnet = true;
                break;
        }
    }
}
