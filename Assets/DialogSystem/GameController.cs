using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;                    // Player 오브젝트를 인스펙터에서 할당하기 위한 변수
    public Transform waypointParent;             // Waypoint의 부모 오브젝트를 인스펙터에서 할당하기 위한 변수
    public Transform[] waypoints;                // Waypoint들의 Transform을 저장할 배열
    private int currentWaypointIndex = 0;   // 현재 Waypoint의 인덱스
    [SerializeField]
    private PlayerMovement playerMovement; // PlayerMovement 스크립트 참조를 인스펙터에서 할당하기 위한 변수
    private DialogManager dialogManager;   // DialogManager 스크립트 참조를 인스펙터에서 할당하기 위한 변수
    
    private List<bool> myActType;
    private PoseEstimator poseEstimator;
    private bool isMission;
    void Start()
    {
        isMission = false;
        poseEstimator = GetComponent<PoseEstimator>();
        StartCoroutine("UpdateActType");
        playerMovement = player.GetComponent<PlayerMovement>();   // PlayerMovement 스크립트를 player 오브젝트로부터 가져옴
        dialogManager = GetComponent<DialogManager>();            // DialogManager 스크립트를 현재 게임 오브젝트로부터 가져옴
        CollectWaypoints();                                       // Waypoint들의 Transform을 수집하여 waypoints 배열에 저장
        if (currentWaypointIndex == 0)
        {
            playerMovement.MoveToWaypoint(waypoints[currentWaypointIndex].position);
            WaypointInfo currentWaypointInfo = waypoints[currentWaypointIndex].GetComponent<WaypointInfo>();
            if (currentWaypointInfo != null)
            {
                dialogManager.ShowDialog(currentWaypointInfo.dialog, currentWaypointInfo.dialogInterval);
            }
        }                                    
    }

    void CollectWaypoints()
    {
        waypoints = new Transform[waypointParent.childCount];     // Waypoint의 개수에 맞게 waypoints 배열 초기화
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);           // Waypoint의 Transform을 waypoints 배열에 저장
        }
    }
    void Update()
    {
        if (playerMovement.IsMoving() && currentWaypointIndex < waypoints.Length)
        {

            float distanceToWaypoint = Vector3.Distance(player.transform.position, waypoints[currentWaypointIndex].position);
            // Player가 이동 중이고, 현재 Waypoint까지의 거리가 0.5f 이하인 경우
            if (distanceToWaypoint <= 0.6f)
            {
                WaypointInfo currentWaypointInfo = waypoints[currentWaypointIndex].GetComponent<WaypointInfo>();
                // 현재 Waypoint에 WaypointInfo 스크립트가 있으면
                if (currentWaypointInfo != null)
                {
                    // 현재 Waypoint에 저장된 대화(dialog)를 DialogManager를 통해 출력
                    dialogManager.ShowDialog(currentWaypointInfo.dialog, currentWaypointInfo.dialogInterval);
                    // 현재 Waypoint의 유형에 따라 동작을 처리
                    if (currentWaypointInfo.waypointType == WaypointType.MissionPoint && !currentWaypointInfo.isClear)
                    {
                        playerMovement.StopMoving();
                        isMission = true;
                    }
                    else
                    {
                        SetNextWaypoint();                 // 일반 포인트에 도착한 경우 다음 Waypoint로 이동
                    }
                }
            }
        }
        else if(isMission)
        {
            if(currentWaypointIndex > 0)
            {
                WaypointInfo currentWaypointInfo = waypoints[currentWaypointIndex].GetComponent<WaypointInfo>();
                if (ProcessMission(currentWaypointInfo))
                {
                    currentWaypointInfo.isClear = true;
                    Debug.Log("성공");
                    isMission = false;
                    playerMovement.ResumeMoving();
                    SetNextWaypoint();
                }
            }
        }
    }

    void SetNextWaypoint()
    {

        currentWaypointIndex++;                         // 다음 Waypoint의 인덱스로 이동
        if (currentWaypointIndex < waypoints.Length)    // 다음 Waypoint가 배열 범위 내에 있는지 확인
        {
            playerMovement.MoveToWaypoint(waypoints[currentWaypointIndex].position);
            // 다음 Waypoint의 위치로 Player 이동
        }
        else
        {
            Debug.Log("모든 Waypoint를 돌았습니다.");
        }
    }

    bool ProcessMission(WaypointInfo waypointInfo)
    {
       if(waypointInfo.waypointType == WaypointType.MissionPoint)
        {
            //for (int i = 0; i < myActType.Count; i++)
            //{
            //    Debug.Log("Element " + i + ": " + myActType[i]);
            //}
        }

        bool missionSuccess = false;
        if (waypointInfo.waypointType == WaypointType.MissionPoint)
        {
            switch (waypointInfo.mission.missionType)
            {
                case MissionType.LeftHand:
                    missionSuccess = myActType[1];
                    Debug.Log("왼손들기 진행중");
                    break;

                case MissionType.RightHand:
                    missionSuccess = myActType[2];
                    Debug.Log("오른손들기 진행중");
                    break;

                case MissionType.BothHands:
                    missionSuccess = myActType[8];
                    Debug.Log("양손들기 진행중");
                    break;

                default:
                    Debug.LogWarning("Unknown MissionType!");
                    break;
            }
        }
        
        return missionSuccess;
    }
    IEnumerator UpdateActType()
    {
        while (true)
        {
            myActType = poseEstimator.GetMotionType();
            yield return null;
        }
    }
}

