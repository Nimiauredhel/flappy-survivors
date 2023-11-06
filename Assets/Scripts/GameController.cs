using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private TouchReceiver _touchReceiver;
    [SerializeField] private PlayerCharacter _playerCharacter;
    [SerializeField] private ObjectMover _objectMover;

    private void Awake()
    {
        _touchReceiver.PointerDown += OnPointerDown;
        _touchReceiver.PointerUp += OnPointerUp;
    }

    private void Start()
    {
        _objectMover.Initialize();
    }

    private void Update()
    {
        _playerCharacter.DoUpdate();
        _objectMover.DoUpdate();
    }

    private void FixedUpdate()
    {
        _playerCharacter.DoFixedUpdate();
        _objectMover.DoFixedUpdate();
    }

    private void OnDestroy()
    {
        _touchReceiver.PointerDown -= OnPointerDown;
        _touchReceiver.PointerUp -= OnPointerUp;
    }

    private void OnPointerDown()
    {
        _playerCharacter.ClimbCommand();
    }
    
    private void OnPointerUp()
    {
        _playerCharacter.DiveCommand();
    }
}
