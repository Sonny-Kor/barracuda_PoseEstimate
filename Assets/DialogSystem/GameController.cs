using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;                    // Player ������Ʈ�� �ν����Ϳ��� �Ҵ��ϱ� ���� ����
    public Transform waypointParent;             // Waypoint�� �θ� ������Ʈ�� �ν����Ϳ��� �Ҵ��ϱ� ���� ����
    public Transform[] waypoints;                // Waypoint���� Transform�� ������ �迭
    private int currentWaypointIndex = 0;   // ���� Waypoint�� �ε���
    [SerializeField]
    private PlayerMovement playerMovement; // PlayerMovement ��ũ��Ʈ ������ �ν����Ϳ��� �Ҵ��ϱ� ���� ����
    private DialogManager dialogManager;   // DialogManager ��ũ��Ʈ ������ �ν����Ϳ��� �Ҵ��ϱ� ���� ����
    
    private List<bool> myActType;
    private PoseEstimator poseEstimator;
    private bool isMission;
    void Start()
    {
        isMission = false;
        poseEstimator = GetComponent<PoseEstimator>();
        StartCoroutine("UpdateActType");
        playerMovement = player.GetComponent<PlayerMovement>();   // PlayerMovement ��ũ��Ʈ�� player ������Ʈ�κ��� ������
        dialogManager = GetComponent<DialogManager>();            // DialogManager ��ũ��Ʈ�� ���� ���� ������Ʈ�κ��� ������
        CollectWaypoints();                                       // Waypoint���� Transform�� �����Ͽ� waypoints �迭�� ����
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
        waypoints = new Transform[waypointParent.childCount];     // Waypoint�� ������ �°� waypoints �迭 �ʱ�ȭ
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);           // Waypoint�� Transform�� waypoints �迭�� ����
        }
    }
    void Update()
    {
        if (playerMovement.IsMoving() && currentWaypointIndex < waypoints.Length)
        {

            float distanceToWaypoint = Vector3.Distance(player.transform.position, waypoints[currentWaypointIndex].position);
            // Player�� �̵� ���̰�, ���� Waypoint������ �Ÿ��� 0.5f ������ ���
            if (distanceToWaypoint <= 0.6f)
            {
                WaypointInfo currentWaypointInfo = waypoints[currentWaypointIndex].GetComponent<WaypointInfo>();
                // ���� Waypoint�� WaypointInfo ��ũ��Ʈ�� ������
                if (currentWaypointInfo != null)
                {
                    // ���� Waypoint�� ����� ��ȭ(dialog)�� DialogManager�� ���� ���
                    dialogManager.ShowDialog(currentWaypointInfo.dialog, currentWaypointInfo.dialogInterval);
                    // ���� Waypoint�� ������ ���� ������ ó��
                    if (currentWaypointInfo.waypointType == WaypointType.MissionPoint && !currentWaypointInfo.isClear)
                    {
                        playerMovement.StopMoving();
                        isMission = true;
                    }
                    else
                    {
                        SetNextWaypoint();                 // �Ϲ� ����Ʈ�� ������ ��� ���� Waypoint�� �̵�
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
                    Debug.Log("����");
                    isMission = false;
                    playerMovement.ResumeMoving();
                    SetNextWaypoint();
                }
            }
        }
    }

    void SetNextWaypoint()
    {

        currentWaypointIndex++;                         // ���� Waypoint�� �ε����� �̵�
        if (currentWaypointIndex < waypoints.Length)    // ���� Waypoint�� �迭 ���� ���� �ִ��� Ȯ��
        {
            playerMovement.MoveToWaypoint(waypoints[currentWaypointIndex].position);
            // ���� Waypoint�� ��ġ�� Player �̵�
        }
        else
        {
            Debug.Log("��� Waypoint�� ���ҽ��ϴ�.");
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
                    Debug.Log("�޼յ�� ������");
                    break;

                case MissionType.RightHand:
                    missionSuccess = myActType[2];
                    Debug.Log("�����յ�� ������");
                    break;

                case MissionType.BothHands:
                    missionSuccess = myActType[8];
                    Debug.Log("��յ�� ������");
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

