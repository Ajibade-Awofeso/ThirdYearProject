using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{

    [Header("Player Components")]
    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _collider;
    private Animator _animator;
    private NeuralNetwork _neuralNetwork;

    private LevelManager _level;

    [Header("Start Variables")]
    private Vector2 _startPosition;
    private Vector2 _startSpeed;

    [Header("Movement Variables")]
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _friction;

    [SerializeField] private float _controlLock;

    [SerializeField] private float _groundSpeed;

    [Header("Input Variables")]
    [SerializeField] public Vector2 moveStick;
    [SerializeField] public bool hasPressedAction;
    [SerializeField] public bool isHoldingAction;
    [SerializeField] public float actionBuffer;
    [SerializeField] public bool _hasNotReleasedAction;

    [Header("Input Buttons")]
    public bool leftButton;
    public bool rightButton;
    public bool upButton;
    public bool downButton;
    public bool actionButton;

    [Header("Air Variables")]
    [SerializeField] private float _accelerationAir;
    [SerializeField] private float _gravity;
    [SerializeField] private float _airSpeed;

    [SerializeField] private float _dashPower;
    [SerializeField] private bool _isDashing;
    [SerializeField] private float _dashTime;
    [SerializeField] private Vector2 _dashDirection;

    [SerializeField] private GroundChecker _groundChecker;
    [SerializeField] private bool _isGrounded;
    RaycastHit2D _belowHitLeft;
    RaycastHit2D _belowHitRight;
    [SerializeField] private float _belowCheckDistance;
    [SerializeField] private float _belowCheckWidth;

    [Header("Layer Variables")]
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] LayerMask _breakableLayer;
    [SerializeField] LayerMask _spikeLayer;
    [SerializeField] LayerMask _honeyLayer;
    [SerializeField] LayerMask _goalLayer;

    [SerializeField] private float _goalDistance;
    [SerializeField] private float _startDistance;
    [SerializeField] private float _currentProgress;
    [SerializeField] private float _topProgress;

    [Header("Neural Network Variables")]
    public bool isUsingNetwork;

    public float playerFitness;
    public bool isTheBest;

    public float playerHoneyCount;
    [SerializeField] float[] _neuralInputs;
    [SerializeField] float[] _neuralOutputs;

    [SerializeField] private float[] _groundTileDistances = new float[8];
    [SerializeField] private float[] _positions = new float[8];

    [SerializeField] private GameObject _closestBreakable;
    [SerializeField] private GameObject _closestHoney;
    [SerializeField] private GameObject _closestSpikeball;

    public float[] _stateInputs = new float[2];

    [SerializeField] private float _range;
    [SerializeField] private Vector2[] _rayDirections = new Vector2[8];

    public float _mutationAmount = 0.8f;
    public float _mutationChance = 0.2f;

    public bool _willMutate;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CapsuleCollider2D>();
        _animator = GetComponent<Animator>();
        _neuralNetwork = GetComponent<NeuralNetwork>();

        _groundChecker = GetComponentInChildren<GroundChecker>();

        _level = FindObjectOfType<LevelManager>();

        _rayDirections[0] = Vector2.left;
        _rayDirections[1] = Vector2.right;
        _rayDirections[2] = Vector2.up;
        _rayDirections[3] = Vector2.down;

        _rayDirections[4] = new Vector2(-1, 1);
        _rayDirections[5] = new Vector2(1, 1);
        _rayDirections[6] = new Vector2(-1, -1);
        _rayDirections[7] = new Vector2(1, -1);

        GetGoalDistance();
        _startDistance = _goalDistance;

        _startPosition = transform.position;
        _startSpeed = _rigidbody.velocity;
    }

    // Update is called once per frame
    void Update()
    {
        if (_willMutate)
        {
            MutatePlayer();
            _willMutate = false;
        }
    }

    private void FixedUpdate()
    {
        SenseGroundTiles();

        if (_level.isBreakablePresent)
        {
            GetClosestBreakable();
        }

        if (_level.isHoneyPresent)
        {
            GetClosestHoney();
        }

        if (_level.isSpikeballPresent)
        {
            GetClosestSpikeball();
        }

        SetClosestArray();
        GetGoalDistance();

        if (isUsingNetwork)
        {
            ActionInput();
            SetUpNeuralNetwork();
        }

        if (_isGrounded) //GROUND STATE
        {
            Dashing();
            Running();
            CheckGround();
        }
        else if (!_isGrounded) //AIR STATE
        {
            Dashing();
            AirPhysics();
            CheckGround();
        }

        SetAnimation();
        SetFitness();
    }


    void Running()
    {

        //MOVES PLAYER BASED ON INPUT
        Vector2 targetSpeed;

        if (moveStick.x != 0 && _controlLock == 0)
        {
            if (Mathf.Sign(_groundSpeed) == Mathf.Sign(moveStick.x))
            {
                _groundSpeed = Mathf.MoveTowards(_rigidbody.velocity.x, _runSpeed * moveStick.x, _acceleration * Time.deltaTime);
            }
            else
            {
                _groundSpeed = Mathf.MoveTowards(_rigidbody.velocity.x, _runSpeed * moveStick.x, _friction * Time.deltaTime);
            }
        }
        else if (moveStick.x == 0)
        {
            _groundSpeed = Mathf.MoveTowards(_rigidbody.velocity.x, 0, _acceleration * Time.deltaTime);
        }

        _rigidbody.velocity -= new Vector2(0, 5) * Time.deltaTime;

        targetSpeed = new Vector2(_groundSpeed, _rigidbody.velocity.y);
        _rigidbody.velocity = targetSpeed;
    }

    void AirPhysics()
    {
        _airSpeed = Mathf.Abs(_rigidbody.velocity.x);

        //APPLIES GRAVITY
        _rigidbody.velocity -= new Vector2(0, _gravity) * Time.deltaTime;

        //MOVES THE PLAYER BASED ON INPUT
        Vector2 targetSpeed;

        if (moveStick.x != 0)
        {
            targetSpeed = new Vector3(_runSpeed * moveStick.x, _rigidbody.velocity.y);

            _rigidbody.velocity = Vector2.MoveTowards(_rigidbody.velocity, targetSpeed, _accelerationAir * Time.deltaTime);
        }
    }
    void Dashing()
    {
        if (actionBuffer > 0 && !_isDashing)
        {
            _dashDirection = new Vector2(Mathf.Round(moveStick.x), Mathf.Round(moveStick.y)).normalized;

            actionBuffer = 0;
            StartCoroutine(DashAttack());

            if (_dashDirection != Vector2.zero)
            {
                _rigidbody.velocity = _dashDirection * _dashPower;
            }
        }
    }

    IEnumerator DashAttack()
    {
        _isDashing = true;
        yield return new WaitForSeconds(_dashTime);
        _isDashing = false;
    }

    void CheckGround()
    {
        if (_groundChecker.isHittingGround)
        {
            if (!_isGrounded)
            {
                LandOnGround();
            }

            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    void LandOnGround()
    {
        _groundSpeed = _rigidbody.velocity.x;
    }

    void SetUpNeuralNetwork()
    {
        _stateInputs[0] = _isGrounded ? 1 : 0;
        _stateInputs[1] = _isDashing ? 1 : 0;

        _neuralInputs = new float[18];

        //COMBINES THE ALL ARRAYS INTO A SINGLE ARRAY FOR NEURAL INPUT
        for (int i = 0; i < _groundTileDistances.Length; i++)
        {
            _neuralInputs[i] = _groundTileDistances[i];
        }

        for (int i = 0; i < _positions.Length; i++)
        {
            _neuralInputs[i + _groundTileDistances.Length] = _positions[i];
        }


        for (int i = 0; i < _stateInputs.Length; i++)
        {
            _neuralInputs[i + _groundTileDistances.Length + _positions.Length] = _stateInputs[i];
        }

        _neuralOutputs = _neuralNetwork.Brain(_neuralInputs);

        //OUTPUT DECIDES IF BUTTON IS PRESSED OR NOT
        //POSITVE - BUTTON PRESSED
        //NEGATIVE - BUTTON NOT PRESSED
        leftButton = _neuralOutputs[0] > 0;
        rightButton = _neuralOutputs[1] > 0;
        upButton = _neuralOutputs[2] > 0;
        downButton = _neuralOutputs[3] > 0;
        actionButton = _neuralOutputs[4] > 0;
    }

    void SetAnimation()
    {
        _animator.SetBool("grounded", _isGrounded);
        _animator.SetBool("dashing", _isDashing);

        if (_groundSpeed != 0)
        {
            _animator.SetBool("moving", true);
        }
        else
        {
            _animator.SetBool("moving", false);
        }

        if (_rigidbody.velocity.x > 0)
        {
            transform.localScale = new Vector2(1, 1);
        }
        else if (_rigidbody.velocity.x < 0)
        {
            transform.localScale = new Vector2(-1, 1);
        }
    }

    void SenseGroundTiles()
    {
        RaycastHit2D hit;

        //RETURNS DISTANCES FROM THE GROUND OR WALLS
        for (int i = 0; i < _groundTileDistances.Length; i++)
        {
            hit = Physics2D.Raycast(transform.position, _rayDirections[i], _range, _groundLayer);

            if (hit)
            {
                _groundTileDistances[i] = hit.distance;
            }
            else
            {
                _groundTileDistances[i] = 500;
            }

            Debug.DrawRay(transform.position, _rayDirections[i] * _range, Color.blue);
        }
    }

    void GetClosestBreakable()
    {
        float distance;
        float closestDistance = 1000;

        for (int i = 0; i < _level.breakables.Length; i++)
        {
            distance = Vector2.Distance(this.transform.position, _level.breakables[i].transform.position);

            if (distance < closestDistance && _level.breakables[i].activeSelf)
            {
                _closestBreakable = _level.breakables[i];
                closestDistance = distance;
            }
        }
    }
    void GetClosestHoney()
    {
        float distance;
        float closestDistance = 1000;

        for (int i = 0; i < _level.honeys.Length; i++)
        {
            distance = Vector2.Distance(this.transform.position, _level.honeys[i].transform.position);

            if (distance < closestDistance && _level.honeys[i].activeSelf)
            {
                _closestHoney = _level.honeys[i];
                closestDistance = distance;
            }
        }
    }

    void GetClosestSpikeball()
    {
        float distance;
        float closestDistance = 1000;

        for (int i = 0; i < _level.spikeballs.Length; i++)
        {
            distance = Vector2.Distance(this.transform.position, _level.spikeballs[i].transform.position);

            if (distance < closestDistance && _level.spikeballs[i].activeSelf)
            {
                _closestSpikeball = _level.spikeballs[i];
                closestDistance = distance;
            }
        }
    }

    void SetClosestArray()
    {
        _positions[0] = this.transform.position.x;
        _positions[1] = this.transform.position.y;

        if (_level.isBreakablePresent)
        {
            _positions[2] = this.transform.position.x - _closestBreakable.transform.position.x;
            _positions[3] = this.transform.position.y - _closestBreakable.transform.position.y;
        }
        else
        {
            _positions[2] = 1000;
            _positions[3] = 1000;
        }

        if (_level.isHoneyPresent)
        {
            _positions[4] = this.transform.position.x - _closestHoney.transform.position.x;
            _positions[5] = this.transform.position.y - _closestHoney.transform.position.y;
        }
        else
        {
            _positions[4] = 1000;
            _positions[5] = 1000;
        }

        if (_level.isSpikeballPresent)
        {
            _positions[6] = this.transform.position.x - _closestSpikeball.transform.position.x;
            _positions[7] = this.transform.position.x - _closestSpikeball.transform.position.y;
        }
        else
        {
            _positions[6] = 1000;
            _positions[7] = 1000;
        }


    }

    void ActionInput()
    {
        if (actionButton)
        {
            if (!_hasNotReleasedAction)
            {
                _hasNotReleasedAction = true;
                StartCoroutine(ActionButtonPressed());
                actionBuffer = 0.1f;
            }
        }
        else if (!actionButton)
        {
            _hasNotReleasedAction = false;
        }
    }
    IEnumerator ActionButtonPressed()
    {
        hasPressedAction = true;
        yield return new WaitForEndOfFrame();
        hasPressedAction = false;
    }

    void GetGoalDistance()
    {
        RaycastHit2D hit;

        hit = Physics2D.Raycast(transform.position, Vector2.right, 10000, _goalLayer);

        if (hit)
        {
            _goalDistance = hit.distance;
        }

        _currentProgress = ((_startDistance - _goalDistance) / _startDistance) * 100;
        _currentProgress = MathF.Round(_currentProgress);

        if (_currentProgress > _topProgress)
        {
            _topProgress = _currentProgress;
            _level.ResetProgressTimer();
        }
    }

    void SetFitness()
    {
        playerFitness = _topProgress + (playerHoneyCount * 5);
    }

    void MutatePlayer()
    {
        _mutationAmount += Random.Range(-1.0f, 1.0f) / 100;
        _mutationChance += Random.Range(-1.0f, 1.0f) / 100;

        //make sure mutation amount and chance are positive using max function
        _mutationAmount = Mathf.Max(_mutationAmount, 0);
        _mutationChance = Mathf.Max(_mutationChance, 0);

        _neuralNetwork.MutateNetwork(_mutationAmount, _mutationChance);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Honey"))
        {
            other.gameObject.SetActive(false);
            _level.AddHoney();
            playerHoneyCount++;
        }

        if (other.gameObject.CompareTag("Goal"))
        {
            _level.CompleteLevel();
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out IBreakable breakable) && _isDashing)
        {
            //breakable.Break();
        }

        if (other.gameObject.CompareTag("Spikeball"))
        {
            _level.ResetLevel();
        }

        if (other.gameObject.CompareTag("Breakable"))
        {
            if (_isDashing)
            {
                other.gameObject.SetActive(false);
                _level.ResetProgressTimer();
            }
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out IBreakable breakable) && _isDashing)
        {
            breakable.Break();
        }
    }

    public void Reset()
    {
        if (isTheBest)
        {
            _level.neuralNetwork.layers = _neuralNetwork.CopyLayers();
        }

        if (!isTheBest)
        {
            _neuralNetwork.layers = _level.neuralNetwork.CopyLayers();
        }

        transform.position = _startPosition;
        _rigidbody.velocity = _startSpeed;
        _willMutate = true;
        _currentProgress = 0;
        _topProgress = 0;
        playerHoneyCount = 0;

        leftButton = false;
        rightButton = false;
        upButton = false;
        downButton = false;
        actionButton = false;
    }
}

