using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetect : MonoBehaviour
{
    public OneActionGameManager GM;
    public bool Boundaries;
    public void OnCollisionEnter(Collision collision)
    {
        if (Boundaries)
        {
            GM.BoundaryEffect();
        }
        else
        {
            GM.CollisionEffect();
        }
    }
}
