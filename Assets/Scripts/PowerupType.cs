using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PowerUPType
{
    magnet = 0,
    powerJump = 1,
    immortal = 2,
    fly = 3,
}

public class PowerupType : MonoBehaviour
{
    public PowerUPType pType;
    [Range(0.0f,1.0f)]
    public float spawnChance;
}
