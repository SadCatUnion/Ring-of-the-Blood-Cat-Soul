using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSensor : MonoBehaviour
{
    public PlayerController controller;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground")
            Debug.Log("On Ground");
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ground")
            Debug.Log("Not On Ground");
    }
}
