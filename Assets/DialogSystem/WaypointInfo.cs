using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaypointType
{
    NormalPoint,
    MissionPoint
}

public class WaypointInfo : MonoBehaviour
{
    public WaypointType waypointType;
    public string[] dialog;
}