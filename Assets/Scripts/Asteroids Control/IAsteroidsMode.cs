using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAsteroidsMode
{
    void HandleRaycastHit(RaycastHit2D hit);
}
