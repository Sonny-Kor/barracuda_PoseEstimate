using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;                    // Player 오브젝트를 인스펙터에서 할당하기 위한 변수
    public Transform waypointParent;             // Waypoint의 부모 오브젝트를 인스펙터에서 할당하기 위한 변수
    public Transform[] waypoints;                // Waypoint들의 Transform을 저장할 배열
    [SerializeField] private int currentWaypointIndex = 0;   // 현재 Waypoint의 인덱스 (인스펙터에 노출되도록 SerializeField 사용)
    [SerializeField] private PlayerMovement playerMovement; // PlayerMovement 스크립트 참조를 인스펙터에서 할당하기 위한 변수
    [SerializeField] private DialogManager dialogManager;   // DialogManager 스크립트 참조를 인스펙터에서 할당하기 위한 변수

    void Start()
    {
        playerMovement = player.GetComponent<PlayerMovement>();   // PlayerMovement 스크립트를 player 오브젝트로부터 가져옴
        dialogManager = GetComponent<DialogManager>();            // DialogManager 스크립트를 현재 게임 오브젝트로부터 가져옴
        CollectWaypoints();                                       // Waypoint들의 Transform을 수집하여 waypoints 배열에 저장

        if (currentWaypointIndex == 0)
        {
            WaypointInfo currentWaypointInfo = waypoints[currentWaypointIndex].GetComponent<WaypointInfo>();
            if (currentWaypointInfo != null)
            {
                dialogManager.ShowDialog(currentWaypointInfo.dialog);
            }
        }
        SetNextWaypoint();                                        // 초기 Waypoint로 이동
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
        if (playerMovement.IsMoving())
        {
            float distanceToWaypoint = Vector3.Distance(player.transform.position, waypoints[currentWaypointIndex].position);
            // Player가 이동 중이고, 현재 Waypoint까지의 거리가 0.5f 이하인 경우
            if (distanceToWaypoint <= 0.5f)
            {
                WaypointInfo currentWaypointInfo = waypoints[currentWaypointIndex].GetComponent<WaypointInfo>();
                // 현재 Waypoint에 WaypointInfo 스크립트가 있으면
                if (currentWaypointInfo != null)
                {
                    // 현재 Waypoint에 저장된 대화(dialog)를 DialogManager를 통해 출력
                    dialogManager.ShowDialog(currentWaypointInfo.dialog);

                    // 현재 Waypoint의 유형에 따라 동작을 처리
                    if (currentWaypointInfo.waypointType == WaypointType.MissionPoint)
                    {
                        playerMovement.StopMoving();   // 미션 포인트에 도착한 경우 Player의 이동을 멈춤
                    }
                    else
                    {
                        SetNextWaypoint();            // 일반 포인트에 도착한 경우 다음 Waypoint로 이동
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Return))    // Player가 이동을 멈추었을 때 Enter 키를 누른 경우
        {
            playerMovement.ResumeMoving();            // Player의 이동 재개
            SetNextWaypoint();                        // 다음 Waypoint로 이동
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
}