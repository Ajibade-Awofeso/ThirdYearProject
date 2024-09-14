using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameManager instance;

    [SerializeField] private int _targetFrameRate = 60;
    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Application.targetFrameRate = _targetFrameRate;
    }

    // Update is called once per frame
    void Update()
    {
        if(Application.targetFrameRate != _targetFrameRate)
        {
            Application.targetFrameRate = _targetFrameRate;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
