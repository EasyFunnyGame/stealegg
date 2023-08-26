using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelUnLockConfig
{
    public static Dictionary<int, int> LEVEL_UNLOCK_CONFIG = new Dictionary<int, int> {

        {5, 10},
        {10, 20},

        {17, 40},
        {22, 50},

        {29, 60},
        {34, 90},
    };
}
