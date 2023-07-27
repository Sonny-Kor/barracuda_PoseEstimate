using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaypointType
{
    NormalPoint,
    MissionPoint
}

public enum MissionType
{
    LeftHand,
    RightHand,
    BothHands
}

[System.Serializable]
public class MissionInfo
{
    public MissionType missionType;
    public string missionName;
    public string missionDescription;
}

public class WaypointInfo : MonoBehaviour
{
    public WaypointType waypointType;

    // MissionPoint�� �������� �� ǥ���� �̼� ����
    [SerializeField]
    public MissionInfo mission;
    public bool isClear;

    public string[] dialog;
    public float dialogInterval;

}