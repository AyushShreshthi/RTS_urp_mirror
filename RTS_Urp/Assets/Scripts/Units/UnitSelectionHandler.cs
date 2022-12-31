using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionHandler : MonoBehaviour
{
    private Camera mainCamera;

    private List<Unit> selectedUnits = new List<Unit>();


    private void Start()
    {
        mainCamera = Camera.main;
    }
}
