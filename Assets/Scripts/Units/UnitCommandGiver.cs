using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Camera mainCamera;
    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        mainCamera = Camera.main;

        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {
        Ray ray;
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }
            ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        }
        else
        {
            ray = mainCamera.ScreenPointToRay(Input.GetTouch(0).position);
        }

        if (!Physics.Raycast(ray,out RaycastHit hit , Mathf.Infinity, layerMask)) { return; }
        if(hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            if (target.hasAuthority)
            {
                TryMove(hit.point);
                return;
            }
            TryTarget(target);
            return;
        }
        TryMove(hit.point);
    }

    private void TryTarget(Targetable target)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter().CmdSetTarget(target.gameObject);
        }
    }

    private void TryMove(Vector3 point)
    {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().CmdMove(point);
        }
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}
