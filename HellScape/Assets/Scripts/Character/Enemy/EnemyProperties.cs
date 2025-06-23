using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyPathingStates
{
    FollowingPath,
    ReachedEndOfPath
}

public enum EnemyPlayerRelationStates
{
    FollowingPlayer,
    ChasingPlayer,
    AttackingPlayer
}

public class EnemyProperties : MonoBehaviour
{
    [Header("Enemy Character Properties")]
    public float headRotateSpeed = 10.0f;

    [Header("Path Following Properties")]
    public float distanceToStopFromPathPoint = 0.5f;
    public bool loopPathPoints = true;
    
    [Header("Path Points")]
    public List<Transform> patrolPoints = new List<Transform>();
    public List<Vector3> pathPoints = new List<Vector3>();
    public NavMeshPath navMeshPath;

    public int currentPathPointIndex = 0;

    public EnemyPathingStates pathingState;
    public EnemyPlayerRelationStates playerRelationState;

    private void Start()
    {
        navMeshPath = new NavMeshPath();
    }
}
