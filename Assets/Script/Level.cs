using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private BoardManager _boardMgr;

    private void Awake()
    {
        _boardMgr = GetComponent<BoardManager>();
        if(_boardMgr == null)
        {
            throw new System.Exception("No BoardManager Attached To This GameObject");
        }
        _boardMgr.init();
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount==1)
        {
            Debug.Log("手指按下");
        }
    }
}
