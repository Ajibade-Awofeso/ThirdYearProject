using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRotation : MonoBehaviour
{
    [SerializeField] private GameObject _level;

    [SerializeField] private float _rotationSpeed;
    public bool isHoldingLeftRotation;
    public bool isHoldingRightRotation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (isHoldingLeftRotation)
        {
            _level.transform.RotateAround(transform.position, -Vector3.forward, _rotationSpeed * Time.deltaTime);
        }

        if (isHoldingRightRotation)
        {
            _level.transform.RotateAround(transform.position, Vector3.forward, _rotationSpeed * Time.deltaTime);
        }
    }
}
