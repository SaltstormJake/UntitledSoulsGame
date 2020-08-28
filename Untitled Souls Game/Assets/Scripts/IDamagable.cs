using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Authors:
 * Jordan Gilbreath
 * Graham Porter
 * Jake Smith
 */
interface IDamagable
{
    int HitPoints
    {
        get;
        set;
    }

    int MaxHitPoints
    {
        get;
    }

    float Health
    {
        get;
    }
}
