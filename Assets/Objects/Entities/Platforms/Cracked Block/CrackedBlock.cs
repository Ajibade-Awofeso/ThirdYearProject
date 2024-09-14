using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrackedBlock : MonoBehaviour, IBreakable, IEntity
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Break()
    {
        gameObject.SetActive(false);
    }

    public void Reset()
    {
        gameObject.SetActive(true);
    }
}
