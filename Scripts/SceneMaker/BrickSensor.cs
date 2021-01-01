using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickSensor : MonoBehaviour
{
    public GameObject target;

    private void OnTriggerStay(Collider other)
    {
        target = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        target = null;
    }
}
