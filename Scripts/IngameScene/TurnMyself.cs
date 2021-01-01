using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnMyself : MonoBehaviour
{
    float turnSpeed;

    private void Start()
    {
        if (Random.Range(0, 2) == 0) turnSpeed = Random.Range(20, 10);
        else turnSpeed = Random.Range(-20, -10);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
    }
}
