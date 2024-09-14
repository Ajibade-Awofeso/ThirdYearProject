using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private float _levelID;

    [SerializeField] private int _honeyCount;

    [SerializeField] private float _timeScale;

    [SerializeField] private int _maxTime;
    [SerializeField] private float _timer;

    [SerializeField] private float _progressTimer;
    [SerializeField] private float _maxProgressTime;

    [SerializeField] private TextMeshProUGUI _honeyText;
    [SerializeField] private TextMeshProUGUI _timerText;

    [SerializeField] private string _nextLevel;
    [SerializeField] private string _levelSelect;

    [SerializeField] private bool _hasFoundTheBest;
    [SerializeField] private float _bestFitness;
    [SerializeField] private float _meanFitness;


    [SerializeField] private GameObject _pauseMenu;

    public int generation;

    public bool isInTraining;
    public bool isPaused;

    [SerializeField] private PlayerController[] _playerControllers;
    [SerializeField] public GameObject[] honeys;
    [SerializeField] public GameObject[] breakables;
    [SerializeField] public GameObject[] spikeballs;

    public bool isHoneyPresent;
    public bool isBreakablePresent;
    public bool isSpikeballPresent;


    public NeuralNetwork neuralNetwork;

    // Start is called before the first frame update
    void Start()
    {
        ResetProgressTimer();
        SetBestPlayer();
        _timer = _maxTime;
        ResumeGame();
        Time.timeScale = _timeScale;

        _playerControllers = FindObjectsOfType<PlayerController>();
        honeys = GameObject.FindGameObjectsWithTag("Honey");
        breakables = GameObject.FindGameObjectsWithTag("Breakable");
        spikeballs = GameObject.FindGameObjectsWithTag("Spikeball");

        if(honeys.Length > 0)
        {
            isHoneyPresent = true;
        }
        else
        {
            isHoneyPresent = true;
        }

        if (breakables.Length > 0)
        {
            isBreakablePresent = true;
        }
        else
        {
            isBreakablePresent = true;
        }

        if (spikeballs.Length > 0)
        {
            isSpikeballPresent = true;
        }
        else
        {
            isSpikeballPresent = true;
        }

        neuralNetwork = GetComponent<NeuralNetwork>();
    }

    // Update is called once per frame
    void Update()
    {
        _timer -= Time.deltaTime;
        _progressTimer -= Time.deltaTime;
        SetBestPlayer();
        GetAverageFitness();

        if (_timer < 0 || (_progressTimer < 0 && isInTraining))
        {
            ResetLevel();
        }

        

        _honeyText.text = _honeyCount.ToString("0");
        _timerText.text = _timer.ToString("0");
    }

    void GetAverageFitness()
    {
        float fitness = 0;

        for(int i = 0; i < _playerControllers.Length; i++)
        {
            fitness += _playerControllers[i].playerFitness;
        }

        _meanFitness = fitness / _playerControllers.Length;
    }

    public void AddHoney()
    {
        _honeyCount++;
        ResetProgressTimer();
    }

    public void ResetProgressTimer()
    {
        _progressTimer = _maxProgressTime;
    }

    public void SetBestPlayer()
    {
        _hasFoundTheBest = false;

        for (int i = 0; i < _playerControllers.Length; i++)
        {
            if (_playerControllers[i].playerFitness >= _bestFitness)
            {
                _hasFoundTheBest = true;
                _bestFitness = _playerControllers[i].playerFitness;
                _playerControllers[i].isTheBest = true;
            }
            else
            {
                _playerControllers[i].isTheBest = false;
            }
        }
    }

    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0.0f;
            _pauseMenu.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = _timeScale;
            _pauseMenu.SetActive(false);
        }
    }
    public void ExitLevel()
    {
        SceneManager.LoadScene(_levelSelect);
    }

    public void CompleteLevel()
    {
        SceneManager.LoadScene(_nextLevel);
    }

    public void ResetLevel()
    {
        if (isInTraining)
        {
            foreach (PlayerController player in _playerControllers)
            {
                player.Reset();
            }

            foreach (GameObject honey in honeys)
            {
                if (!honey.activeSelf)
                {
                    honey.SetActive(true);
                }
            }

            foreach (GameObject breakable in breakables)
            {
                if (!breakable.activeSelf)
                {
                    breakable.SetActive(true);
                }
            }

            _timer = _maxTime;
            _honeyCount = 0;
            ResetProgressTimer();
            _hasFoundTheBest = false;
            _bestFitness = 0;
            generation++;
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}