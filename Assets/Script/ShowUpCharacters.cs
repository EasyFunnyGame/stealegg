using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowUpCharacters : MonoBehaviour
{

    [SerializeField]
    public List<GameObject> characters;


    public void Show()
    {
        for(var index = 0; index < characters.Count; index++)
        {
            characters[index].SetActive(true);
        }
    }

    public void Hide()
    {
        for (var index = 0; index < characters.Count; index++)
        {
            characters[index].SetActive(false);
        }
    }
}
