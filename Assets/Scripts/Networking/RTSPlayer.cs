using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] public Transform cameraTransform = null;
    [SerializeField] private float buildingRangeLimit = 5f;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private List<Unit> myUnits = new List<Unit>();
    [SerializeField] private List<Building> myBuildings = new List<Building>();
    [SerializeField] private Building[] buildings = new Building[0];
    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    [SerializeField] private int resources = 1500;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))] bool isPartyOwner = false;
    public event Action<int> ClientOnResourcesUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
    public static event Action ClientOnInfoUpdated;

    Color teamColor = new Color();
    [SyncVar(hook =nameof(ClientHandleDisplayNameUpdated))] string displayName;

    [SerializeField] GameObject mobileInputs = null;

    private List<Unit> A_UnitsSorted = new List<Unit>();
    private List<Unit> C_UnitsSorted = new List<Unit>();
    private List<Unit> CS_UnitsSorted = new List<Unit>();
    private List<Unit> HI_UnitsSorted = new List<Unit>();
    private List<Unit> LI_UnitsSorted = new List<Unit>();
    [SerializeField] Image A_Image;
    [SerializeField] Image C_Image;
    [SerializeField] Image CS_Image;
    [SerializeField] Image HI_Image;
    [SerializeField] Image LI_Image;
    [SerializeField] TMP_Text A_Text;
    [SerializeField] TMP_Text C_Text;
    [SerializeField] TMP_Text CS_Text;
    [SerializeField] TMP_Text HI_Text;
    [SerializeField] TMP_Text LI_Text;

    [SerializeField] GameObject allUnitsSortedUI;

    public void SetActiveMobileInputs(bool activity)
    {
        mobileInputs.SetActive(activity);
    }

    public string GetDisplayName()
    {
        return displayName;
    }

    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }

    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    public Color GetTeamColor()
    {
        return teamColor;
    }

    public int GetResources()
    {
        return resources;
    }

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }
    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }

    public bool CanPlaceBuilding(BoxCollider buildingCollider,Vector3 point)
    {
        if (Physics.CheckBox(point + buildingCollider.center, buildingCollider.size / 2, Quaternion.identity, buildingBlockLayer))
        {
            return false;
        }

        foreach (Building building1 in myBuildings)
        {
            if ((point - building1.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }
        return false;
    }

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDeSpawned += ServerHandleUnitDeSpawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDeSpawned += ServerHandleBuildingDeSpawned;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDeSpawned -= ServerHandleUnitDeSpawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDeSpawned -= ServerHandleBuildingDeSpawned;
    }

    [Server] public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server] public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    [Server]
    public void SetTeamColor(Color newTeamColor)
    {
        teamColor = newTeamColor;
    }

    [Server]
    public void SetResources(int newResources)
    {
        resources = newResources;
    }

    [Command] public void CmdStartGame(string mapName)
    {
        if (!isPartyOwner) { return; }
        ((RTSNetworkManager)NetworkManager.singleton).StartGame(mapName);
    }

    [Command] public void CmdTryPlaceBuilding(int buildingId,Vector3 point)
    {
        Building buildingToPlace = null;
        foreach(Building building in buildings)
        {
            if (building.GetId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }
        if(buildingToPlace == null) { return; }
        if (resources < buildingToPlace.GetPrice()) { return; }
        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

        if (!CanPlaceBuilding(buildingCollider, point)) { return; }

        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(buildingInstance, connectionToClient);
        SetResources(resources - buildingToPlace.GetPrice());
    }

    private void ServerHandleUnitSpawned(Unit unit)
    {
        if(unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
        myUnits.Add(unit);
        AddUnitToSortedList(unit);
    }
    private void ServerHandleUnitDeSpawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
        myUnits.Remove(unit);
        RemoveUnitToSortedList(unit);
    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
        myBuildings.Add(building);
    }
    private void ServerHandleBuildingDeSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
        myBuildings.Remove(building);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if(NetworkServer.active) { return; }

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDeSpawned += AuthorityHandleUnitDeSpawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDeSpawned += AuthorityHandleBuildingDeSpawned;
    }

    public override void OnStartClient()
    {
        if (NetworkServer.active) { return; }
        DontDestroyOnLoad(gameObject);
        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
    }

    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();
        if (!isClientOnly) { return; }
        ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);
        if (!hasAuthority) { return; }
        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDeSpawned -= AuthorityHandleUnitDeSpawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDeSpawned -= AuthorityHandleBuildingDeSpawned;
    }

    private void ClientHandleResourcesUpdated(int oldResources,int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }

    private void ClientHandleDisplayNameUpdated(string oldDisplayName,string newDisplayName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState,bool newState)
    {
        if (!hasAuthority) { return; }
        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
        AddUnitToSortedList(unit);
    }
    private void AuthorityHandleUnitDeSpawned(Unit unit)
    {
        myUnits.Remove(unit);
        RemoveUnitToSortedList(unit);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }
    private void AuthorityHandleBuildingDeSpawned(Building building)
    {
        myBuildings.Remove(building);
    }

    private void AddUnitToSortedList(Unit addedUnit)
    {
        allUnitsSortedUI.SetActive(true);
        if (addedUnit.id == 0)
        {
            A_UnitsSorted.Add(addedUnit);
            if (A_UnitsSorted.Count == 1)
            {
                A_Image.gameObject.SetActive(true);
                A_Text.text = A_UnitsSorted.Count.ToString();
            }
            else
            {
                A_Text.text = A_UnitsSorted.Count.ToString();
            }
        }
        if (addedUnit.id == 1)
        {
            C_UnitsSorted.Add(addedUnit);
            if (C_UnitsSorted.Count == 1)
            {
                C_Image.gameObject.SetActive(true);
                C_Text.text = C_UnitsSorted.Count.ToString();
            }
            else
            {
                C_Text.text = C_UnitsSorted.Count.ToString();
            }
        }
        if (addedUnit.id == 2)
        {
            CS_UnitsSorted.Add(addedUnit);
            if (CS_UnitsSorted.Count == 1)
            {
                CS_Image.gameObject.SetActive(true);
                CS_Text.text = CS_UnitsSorted.Count.ToString();
            }
            else
            {
                CS_Text.text = CS_UnitsSorted.Count.ToString();
            }
        }
        if (addedUnit.id == 3)
        {
            HI_UnitsSorted.Add(addedUnit);
            if (HI_UnitsSorted.Count == 1)
            {
                HI_Image.gameObject.SetActive(true);
                HI_Text.text = HI_UnitsSorted.Count.ToString();
            }
            else
            {
                HI_Text.text = HI_UnitsSorted.Count.ToString();
            }
        }
        if (addedUnit.id == 4)
        {
            LI_UnitsSorted.Add(addedUnit);
            if (LI_UnitsSorted.Count == 1)
            {
                LI_Image.gameObject.SetActive(true);
                LI_Text.text = LI_UnitsSorted.Count.ToString();
            }
            else
            {
                LI_Text.text = LI_UnitsSorted.Count.ToString();
            }
        }
    }

    private void RemoveUnitToSortedList(Unit addedUnit)
    {
        if (addedUnit.id == 0)
        {
            A_UnitsSorted.Remove(addedUnit);
            if (A_UnitsSorted.Count == 0)
            {
                A_Image.gameObject.SetActive(false);
                A_Text.text = A_UnitsSorted.Count.ToString();
            }
            else
            {
                A_Text.text = A_UnitsSorted.Count.ToString();
            }
        }
        if (addedUnit.id == 1)
        {
            C_UnitsSorted.Remove(addedUnit);
            if (C_UnitsSorted.Count == 0)
            {
                C_Image.gameObject.SetActive(false);
                C_Text.text = C_UnitsSorted.Count.ToString();
            }
            else
            {
                C_Text.text = C_UnitsSorted.Count.ToString();
            }
        }
        if (addedUnit.id == 2)
        {
            CS_UnitsSorted.Remove(addedUnit);
            if (CS_UnitsSorted.Count == 0)
            {
                CS_Image.gameObject.SetActive(false);
                CS_Text.text = CS_UnitsSorted.Count.ToString();
            }
            else
            {
                CS_Text.text = CS_UnitsSorted.Count.ToString();
            }
        }
        if (addedUnit.id == 3)
        {
            HI_UnitsSorted.Remove(addedUnit);
            if (HI_UnitsSorted.Count == 0)
            {
                HI_Image.gameObject.SetActive(false);
                HI_Text.text = HI_UnitsSorted.Count.ToString();
            }
            else
            {
                HI_Text.text = HI_UnitsSorted.Count.ToString();
            }
        }
        if (addedUnit.id == 4)
        {
            LI_UnitsSorted.Remove(addedUnit);
            if (LI_UnitsSorted.Count == 0)
            {
                LI_Image.gameObject.SetActive(false);
                LI_Text.text = LI_UnitsSorted.Count.ToString();
            }
            else
            {
                LI_Text.text = LI_UnitsSorted.Count.ToString();
            }
        }
    }

    #endregion
}
