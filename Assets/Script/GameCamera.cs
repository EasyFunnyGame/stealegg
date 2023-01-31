using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public static GameCamera instance;

    public new Camera camera;

    public float ditanceFromTarget;

    public Transform followTarget;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (followTarget==null)
        {
            return;
        }
        
        transform.position = followTarget.position;
    }
}
