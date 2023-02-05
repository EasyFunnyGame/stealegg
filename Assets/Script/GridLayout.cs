using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLayout : MonoBehaviour
{
    public int row;

    public int col;

    public float size;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Layout(int row, int col, float size)
    {
        this.row = row;
        this.col = col;
        this.size = size;

        var index = 0;
        for (var x = 0; x < row; x++)
        {
            for(var z = 0; z < col; z++)
            {
                if (index < transform.childCount)
                {
                    var child = transform.GetChild(index);
                    child.transform.localPosition = new Vector3(x*size, 0, z*size);
                }
                index++;
            }
        }
    }
}
