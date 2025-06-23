using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyPathingStates
{
    FollowingPath,
    ReachedEndOfPath
}

public enum EnemyPlayerRelationStates
{
    ManipulatingPlayer,
    ChasingPlayer,
    AttackingPlayer
}

public enum EnemyType
{
    Ghoul,
    WideAgent,
    Mimics,
    Illusory,
    Cyclops,
    WingedWretch,
    GateGuardian,
    GorgonSerpent
}

[System.Serializable]
public struct NavMeshSurfaceEnemyType
{
    public NavMeshSurface navMeshSurface;
    public EnemyType enemyType;
}

public class EnemyProperties : MonoBehaviour
{
    [Header("Player Data Visible To Enemy")]
    public static Transform playerTransform;

    [Header("Enemy Character Properties")]
    public float headRotateSpeed = 10.0f;
    public float characterNoticeDistance = 20.0f;

    // VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV REFACTOR THIS PROPERLY SO THAT ONLY ONE PLACE HAS ACCESS TO THIS IN THE INSPECTOR ON A COMPLETELY DIFFERENT MANAGER OBJECT!!!!!!!
    [Header("Navmesh Properties")]
    public List<NavMeshSurfaceEnemyType> surfaceAndCorrespondingEnemyTypes = new List<NavMeshSurfaceEnemyType>();
    public static Dictionary<EnemyType, NavMeshQueryFilter> enemyTypeQueryFilter = new Dictionary<EnemyType, NavMeshQueryFilter>();
    //public NavMeshSurface[] navMeshSurfaces;

    [Header("Path Following Properties")]
    public float distanceToStopFromPathPoint = 0.5f;
    public bool loopPathPoints = true;
    
    [Header("Path Points")]
    public List<Transform> patrolPoints = new List<Transform>();
    public List<Vector3> pathPoints = new List<Vector3>();
    public List<Vector3> pathPointsToPlayer = new List<Vector3>();
    public NavMeshPath navMeshPath;
    public NavMeshPath navMeshPathToPlayer;

    public int currentPathPointIndex = 0;

    public EnemyPathingStates pathingState;
    public EnemyPlayerRelationStates playerRelationState;

    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectsWithTag("Player")[0].transform;

        for (int i = 0; i < surfaceAndCorrespondingEnemyTypes.Count; i++)
        {
            NavMeshQueryFilter enemyAgentFilter = new NavMeshQueryFilter();
            enemyAgentFilter.agentTypeID = surfaceAndCorrespondingEnemyTypes[i].navMeshSurface.agentTypeID;
            enemyAgentFilter.areaMask = NavMesh.AllAreas;

            enemyTypeQueryFilter.Add(surfaceAndCorrespondingEnemyTypes[i].enemyType, enemyAgentFilter);
        }
    }

    private void Start()
    {
        navMeshPath = new NavMeshPath();
    }
}
