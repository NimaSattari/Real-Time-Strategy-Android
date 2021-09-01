using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectionArea = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Vector2 startPos;

    private RTSPlayer player;
    private Camera mainCamera;
    public List<Unit> SelectedUnits { get; } = new List<Unit>();
    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        Unit.AuthorityOnUnitDeSpawned += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDeSpawned -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }
    int TapCount;
    public float MaxDubbleTapTime;
    float NewTime;
    private void Update()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                StartSelectionArea();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ClearSelectionArea();
            }
            else if (Mouse.current.leftButton.isPressed)
            {
                UpdateSelectionArea();
            }
        }
        else
        {
            if (Touch.activeFingers.Count == 1)
            {
                if (Input.GetTouch(0).tapCount == 2)
                {
                    Select(Touch.activeTouches[0]);
                }
            }
        }
    }

    private void Select(Touch touch)
    {
        if(touch.phase == TouchPhase.Began)
        {
            StartSelectionArea();
        }
        else if(touch.phase == TouchPhase.Ended)
        {
            ClearSelectionArea();
        }
        else if(touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
        {
            UpdateSelectionArea();
        }
    }

    private void StartSelectionArea()
    {
        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.DeSelect();
            }
            SelectedUnits.Clear();
        }
        unitSelectionArea.gameObject.SetActive(true);
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            startPos = Mouse.current.position.ReadValue();
        }
        else
        {
            startPos = Input.GetTouch(0).position;
        }
        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePos;
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            mousePos = Mouse.current.position.ReadValue();
        }
        else
        {
            mousePos = Input.GetTouch(0).position;
        }
        float areaWidth = mousePos.x - startPos.x;
        float areaHeight = mousePos.y - startPos.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPos + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);
        if(unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray;
            if (SystemInfo.deviceType == DeviceType.Desktop)
            {
                ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            }
            else
            {
                ray = mainCamera.ScreenPointToRay(Input.GetTouch(0).position);
            }
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }
            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) { return; }
            if (!unit.hasAuthority) { return; }
            SelectedUnits.Add(unit);
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }
            return;
        }
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach(Unit unit1 in player.GetMyUnits())
        {
            if (SelectedUnits.Contains(unit1)) { continue; }
            Vector3 screenPos = mainCamera.WorldToScreenPoint(unit1.transform.position);
            if(screenPos.x > min.x && screenPos.x < max.x && screenPos.y>min.y && screenPos.y < max.y)
            {
                SelectedUnits.Add(unit1);
                unit1.Select();
            }
        }
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}
