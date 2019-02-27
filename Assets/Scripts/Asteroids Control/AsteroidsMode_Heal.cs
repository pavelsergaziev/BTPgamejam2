using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidsMode_Heal : IAsteroidsMode
{
    public void HandleRaycastHit(RaycastHit2D hit)
    {
        if (hit.transform.tag == "Mini Planet")
        {
            Debug.Log(hit.transform + "пробует хиляться");
            hit.transform.gameObject.GetComponent<Planet>().GainHealth();            
        }
    }
    
}
