using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentPlayerLocation : MonoBehaviour
{
    public enum PlayerLocationState
    {
        Exploring,
        InCombat,
        InStore,
        WithBoss,
        VoidboundStore,
        BetweenRooms,
        InTreasureRoom,
        InSecretRoom,
        InStartRoom,
        Dying
    }

    [SerializeField] private PlayerLocationState _currentState = PlayerLocationState.Exploring;

    public PlayerLocationState CurrentState
    {
        get { return _currentState; }
        private set
        {
            if (_currentState != value)
            {
                PlayerLocationState oldState = _currentState;
                _currentState = value;
                OnStateChanged?.Invoke(oldState, _currentState);
                Debug.Log($"Player location changed: {oldState} -> {_currentState}");

                ResetCamera();
            }
        }
    }

    public delegate void StateChangeHandler(PlayerLocationState oldState, PlayerLocationState newState);
    public event StateChangeHandler OnStateChanged;

    private float _combatTimer = 0f;
    private float _timeToExitCombat = 5f;
    private PlayerState _player;
    private DynamicCamera _dynamicCamera;

    public static CurrentPlayerLocation Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _player = FindObjectOfType<PlayerState>();
        _dynamicCamera = FindObjectOfType<DynamicCamera>();
    }

    private void Update()
    {
        if (CurrentState == PlayerLocationState.InCombat)
        {
            _combatTimer += Time.deltaTime;
            if (_combatTimer >= _timeToExitCombat)
            {
                SetState(PlayerLocationState.Exploring);
            }
        }

        if (_player != null && _player.Dead && CurrentState != PlayerLocationState.Dying)
        {
            SetState(PlayerLocationState.Dying);
        }
        else if (_player != null && !_player.Dead && CurrentState == PlayerLocationState.Dying)
        {
            SetState(PlayerLocationState.Exploring);
        }
    }

    public void SetState(PlayerLocationState newState)
    {
        switch (CurrentState)
        {
            case PlayerLocationState.InCombat:
                _combatTimer = 0f;
                break;
        }

        switch (newState)
        {
            case PlayerLocationState.InCombat:
                _combatTimer = 0f;
                break;
        }

        CurrentState = newState;
    }

    private void ResetCamera()
    {
        if (_dynamicCamera == null)
        {
            _dynamicCamera = FindObjectOfType<DynamicCamera>();
            if (_dynamicCamera == null) return;
        }

        if (IsInShop())
        {
            _dynamicCamera.SetShopZoom();
        }
        else
        {
            _dynamicCamera.SetNormalZoom();
        }
    }

    public void EnterCombat()
    {
        if (CurrentState != PlayerLocationState.WithBoss &&
            CurrentState != PlayerLocationState.InStore &&
            CurrentState != PlayerLocationState.VoidboundStore)
        {
            SetState(PlayerLocationState.InCombat);
            _combatTimer = 0f;
        }
    }

    public void EnterStore()
    {
        SetState(PlayerLocationState.InStore);
    }

    public void EnterVoidboundStore()
    {
        SetState(PlayerLocationState.VoidboundStore);
    }

    public void EnterBossFight()
    {
        SetState(PlayerLocationState.WithBoss);
    }

    public void StartExploring()
    {
        if (CurrentState != PlayerLocationState.WithBoss &&
            CurrentState != PlayerLocationState.InStore &&
            CurrentState != PlayerLocationState.VoidboundStore &&
            CurrentState != PlayerLocationState.Dying)
        {
            SetState(PlayerLocationState.Exploring);
        }
    }

    public void EnterRoomTransition()
    {
        if (CurrentState != PlayerLocationState.WithBoss &&
            CurrentState != PlayerLocationState.InStore &&
            CurrentState != PlayerLocationState.VoidboundStore &&
            CurrentState != PlayerLocationState.Dying)
        {
            SetState(PlayerLocationState.BetweenRooms);
        }
    }

    public void EnterTreasureRoom()
    {
        SetState(PlayerLocationState.InTreasureRoom);
    }

    public void EnterSecretRoom()
    {
        SetState(PlayerLocationState.InSecretRoom);
    }

    public void EnterStartRoom()
    {
        SetState(PlayerLocationState.InStartRoom);
    }

    public bool IsInSafeArea()
    {
        return CurrentState == PlayerLocationState.InStore ||
               CurrentState == PlayerLocationState.VoidboundStore ||
               CurrentState == PlayerLocationState.InStartRoom;
    }

    public bool IsInShop()
    {
        return CurrentState == PlayerLocationState.InStore ||
               CurrentState == PlayerLocationState.VoidboundStore;
    }

    public void Reset()
    {
        SetState(PlayerLocationState.Exploring);
        _combatTimer = 0f;
    }
}