using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    private float chanceToSpawn;
    private int t_count;

    public GameObject[] powerUps;

    private void Awake()
    { 
        OnDisable();
    }

    private void OnEnable()
    {
        /*if (GameManager.Instance.PowerUP)
        {
            t_count = UnityEngine.Random.Range(0, powerUps.Length);
            powerUps[t_count].SetActive(true);
            GameManager.Instance.PowerUP = false;
        }*/
        t_count = UnityEngine.Random.Range(0, powerUps.Length);
        chanceToSpawn = powerUps[t_count].GetComponent<PowerupType>().spawnChance;

        if (UnityEngine.Random.Range(0.0f, 1.0f) > chanceToSpawn)
            return;

        powerUps[t_count].SetActive(true);
    }

    private void OnDisable()
    {
        foreach (GameObject go in powerUps)
            go.SetActive(false);
    }
}
