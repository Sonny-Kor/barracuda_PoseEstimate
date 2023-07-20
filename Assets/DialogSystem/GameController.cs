using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;                    // Player ������Ʈ�� �ν����Ϳ��� �Ҵ��ϱ� ���� ����
    public Transform waypointParent;             // Waypoint�� �θ� ������Ʈ�� �ν����Ϳ��� �Ҵ��ϱ� ���� ����
    public Transform[] waypoints;                // Waypoint���� Transform�� ������ �迭
    [SerializeField] private int currentWaypointIndex = 0;   // ���� Waypoint�� �ε��� (�ν����Ϳ� ����ǵ��� SerializeField ���)
    [SerializeField] private PlayerMovement playerMovement; // PlayerMovement ��ũ��Ʈ ������ �ν����Ϳ��� �Ҵ��ϱ� ���� ����
    [SerializeField] private DialogManager dialogManager;   // DialogManager ��ũ��Ʈ ������ �ν����Ϳ��� �Ҵ��ϱ� ���� ����

    void Start()
    {
        playerMovement = player.GetComponent<PlayerMovement>();   // PlayerMovement ��ũ��Ʈ�� player ������Ʈ�κ��� ������
        dialogManager = GetComponent<DialogManager>();            // DialogManager ��ũ��Ʈ�� ���� ���� ������Ʈ�κ��� ������
        CollectWaypoints();                                       // Waypoint���� Transform�� �����Ͽ� waypoints �迭�� ����

        if (currentWaypointIndex == 0)
        {
            WaypointInfo currentWaypointInfo = waypoints[currentWaypointIndex].GetComponent<WaypointInfo>();
            if (currentWaypointInfo != null)
            {
                dialogManager.ShowDialog(currentWaypointInfo.dialog);
            }
        }
        SetNextWaypoint();                                        // �ʱ� Waypoint�� �̵�
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
        if (playerMovement.IsMoving())
        {
            float distanceToWaypoint = Vector3.Distance(player.transform.position, waypoints[currentWaypointIndex].position);
            // Player�� �̵� ���̰�, ���� Waypoint������ �Ÿ��� 0.5f ������ ���
            if (distanceToWaypoint <= 0.5f)
            {
                WaypointInfo currentWaypointInfo = waypoints[currentWaypointIndex].GetComponent<WaypointInfo>();
                // ���� Waypoint�� WaypointInfo ��ũ��Ʈ�� ������
                if (currentWaypointInfo != null)
                {
                    // ���� Waypoint�� ����� ��ȭ(dialog)�� DialogManager�� ���� ���
                    dialogManager.ShowDialog(currentWaypointInfo.dialog);

                    // ���� Waypoint�� ������ ���� ������ ó��
                    if (currentWaypointInfo.waypointType == WaypointType.MissionPoint)
                    {
                        playerMovement.StopMoving();   // �̼� ����Ʈ�� ������ ��� Player�� �̵��� ����
                    }
                    else
                    {
                        SetNextWaypoint();            // �Ϲ� ����Ʈ�� ������ ��� ���� Waypoint�� �̵�
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Return))    // Player�� �̵��� ���߾��� �� Enter Ű�� ���� ���
        {
            playerMovement.ResumeMoving();            // Player�� �̵� �簳
            SetNextWaypoint();                        // ���� Waypoint�� �̵�
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
}