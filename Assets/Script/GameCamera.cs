using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{

    public Camera camera;

    public Vector3 offset;

    public float ditanceFromTarget;

    public Transform followTarget;

    public bool tagetSwitching;

    public void SetFollowTarget(Transform targetTr)
    {
        offset = transform.position - targetTr.position;
        ditanceFromTarget = Vector3.Distance(transform.position, targetTr.position);

        if (followTarget != null && followTarget != targetTr)
        {
            tagetSwitching = true;
        }
        followTarget = targetTr;
    }

    private void Awake()
    {
        camera = GetComponent<Camera>();
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
        var distance = Vector3.Distance(transform.position, followTarget.position);
        if( Mathf.Abs(ditanceFromTarget - distance) < 0.01)
        {
            tagetSwitching = false;
        }
    }
}
