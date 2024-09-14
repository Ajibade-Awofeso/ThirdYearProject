using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private string _titleScreen;
    [SerializeField] private string _firstLevel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(_firstLevel);
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene(_titleScreen);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
