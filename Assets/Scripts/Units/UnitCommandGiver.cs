using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
            if (IsOverUI(Touch.activeTouches[0]))
            {
                return;
            }
            else if (Input.GetTouch(0).tapCount >= 2)
            {
                return;
            }
            else if (Input.GetTouch(0).tapCount == 1)
            {
                if(Touch.activeTouches[0].phase == TouchPhase.Began)
                {
                    ray = mainCamera.ScreenPointToRay(Input.GetTouch(0).position);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
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

    private bool IsOverUI(Touch touch)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = touch.screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject.GetComponent<CanvasRenderer>())
                {
                    return true;
                }
            }
        }

        return false;
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
