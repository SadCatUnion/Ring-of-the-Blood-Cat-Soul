using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public bool inputEnabled = true;

    public string keyUp     = "w";
    public string keyDown   = "s";
    public string keyLeft   = "a";
    public string keyRight  = "d";

    public float up;
    public float right;
    private float targetUp;
    private float targetRight;

    private float velocityUp;
    private float velocityRight;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        targetUp    = (Input.GetKey(keyUp) ? 1.0f : 0.0f) - (Input.GetKey(keyDown) ? 1.0f : 0.0f);
        targetRight = (Input.GetKey(keyRight) ? 1.0f : 0.0f) - (Input.GetKey(keyLeft) ? 1.0f : 0.0f);

        if (!inputEnabled)
        {
            targetUp    = 0.0f;
            targetRight = 0.0f;
        }

        up      = Mathf.SmoothDamp(up, targetUp, ref velocityUp, 0.1f);
        right   = Mathf.SmoothDamp(right, targetRight, ref velocityRight, 0.1f);
    }
}
