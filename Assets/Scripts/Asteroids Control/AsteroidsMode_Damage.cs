using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidsMode_Damage : IAsteroidsMode
{
    public void HandleRaycastHit(RaycastHit2D hit)
    {
        if (hit.transform.tag == "Mini Planet")
        {
            Debug.Log(hit.transform + "пробует убиться");
            hit.transform.gameObject.GetComponent<Planet>().LoseHealth();
        }
        else if (hit.transform.tag == "Player Star")
        {
            //try damage player planet?
        }
    }
}
