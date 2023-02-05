using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public static GameCamera Instance;

    public new Camera camera;

    public float ditanceFromTarget;

    public Transform followTarget;

    public Animator camAnimator;

    private void Awake()
    {
        Instance = this;
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



        if (graffPos)
        {
            
        }

    }


    private Transform graffPos;

    private float delta = 2;

    public void SetGraffTarget(Transform pos)
    {
        graffPos = pos;
    }

    public void RecoverFromGraffing()
    {
        graffPos = null;

    }
}
