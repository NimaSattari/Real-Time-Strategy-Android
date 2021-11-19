using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Building building = null;
    [SerializeField] Image iconImage = null;
    [SerializeField] TMP_Text priceText = null;
    [SerializeField] LayerMask floorMask = new LayerMask();

    Camera mainCamera;
    private BoxCollider buildingCollider;
    RTSPlayer player;
    GameObject buildingPreviewInstance;
    Renderer buildingRendererInstance;

    bool canAffordBuilding = true;
    private bool CanAffordBuilding
    {
        get
        {
            return canAffordBuilding;
        }
        set
        {
            if (canAffordBuilding == value) return;

            iconImage.color = (value) ? new Color(1f, 1f, 1f) : new Color(0.25f, 0.25f, 0.25f);
            canAffordBuilding = value;
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        iconImage.sprite = building.GetIcon();
        priceText.text = building.GetPrice().ToString();
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        buildingCollider = building.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        CanAffordBuilding = (building.GetPrice() <= player.GetResources());
        if (buildingPreviewInstance == null) { return; }
        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UnitSelectionHandler unitSelection = FindObjectOfType<UnitSelectionHandler>();
        foreach (Unit selectedUnit in unitSelection.SelectedUnits)
        {
            selectedUnit.DeSelect();
        }
        unitSelection.SelectedUnits.Clear();
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        if (player.GetResources() < building.GetPrice()) { return; }
        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();
        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buildingPreviewInstance == null) { return; }
        Ray ray;
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        }
        else
        {
            ray = mainCamera.ScreenPointToRay(Input.GetTouch(0).position);
        }
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            player.CmdTryPlaceBuilding(building.GetId(), hit.point);
        }
        Destroy(buildingPreviewInstance);
    }

    private void UpdateBuildingPreview()
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
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            return;
        }
        buildingPreviewInstance.transform.position = hit.point;
        if (!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }
        Color color = player.CanPlaceBuilding(buildingCollider,hit.point) ? Color.green : Color.red;
        buildingRendererInstance.material.SetColor("_BaseColor", color);
    }
}
