using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterProperties))]
[RequireComponent(typeof(EnemyProperties))]
public class EnemyCharacterInput : MonoBehaviour
{
    private CharacterProperties s_CharacterProperties;
    private EnemyProperties s_EnemyProperties;

    private Vector3 currentDestination = Vector3.zero;

    private float idleStartTime = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        s_CharacterProperties = GetComponent<CharacterProperties>();
        s_EnemyProperties = GetComponent<EnemyProperties>();

        GeneratePathToFollow(EnemyType.WideAgent);

        s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
        s_EnemyProperties.genericState = EnemyGenericStates.Patroling;
    }

    // Update is called once per frame
    void Update()
    {
        EnemyStateManager();
    }

    private void OnDrawGizmos()
    {
        if(s_EnemyProperties != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentDestination, s_EnemyProperties.distanceToStopFromPathPoint);
        }
    }

    public void EnemyStateManager()
    {
        if(s_EnemyProperties.pathingState == EnemyPathingStates.ReachedEndOfPath && s_EnemyProperties.genericState == EnemyGenericStates.Patroling)
        {
            idleStartTime = Time.time;
            s_EnemyProperties.genericState = EnemyGenericStates.Idling;
            s_CharacterProperties.horizontalPlaneInput = Vector2.zero;
            //Debug.Log("Switching to idling.");
        }
        else
        {
            if (s_EnemyProperties.pathingState == EnemyPathingStates.ReachedEndOfPath && s_EnemyProperties.genericState == EnemyGenericStates.Idling)
            {
                if(Time.time >= idleStartTime + EnemyProperties.enemyGenericStateParameters[EnemyGenericStates.Idling].timeToStayInState)
                {
                    //Debug.Log("Returning to patrol.");
                    s_EnemyProperties.genericState = EnemyGenericStates.Patroling;
                    s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
                }
            }
            else
            {
                //Debug.Log("Patroling.");
                s_EnemyProperties.genericState = EnemyGenericStates.Patroling;
                s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
                FollowCurrentPathPoints();
            }

        }
    }

    public void FollowCurrentPathPoints()
    {
        Vector3 currentPathPoint = Vector3.zero;

        bool changedPoints = GetCurrentClosestPathPoint(ref currentPathPoint);

        Vector3 moveDir = (currentPathPoint - transform.position).normalized;
        s_CharacterProperties.horizontalPlaneInput = new Vector2(moveDir.x, moveDir.z);

        s_CharacterProperties.mouseDelta = Vector2.zero;

        currentDestination = currentPathPoint;
    }

    public void GeneratePathToFollow(EnemyType enemyType)
    {
        s_EnemyProperties.pathPoints.Clear();
        Vector3 startCalculatingPathFrom = transform.position;
        for (int i = 0; i < s_EnemyProperties.patrolPoints.Count; i++)
        {
            List<Vector3> pathPointsToCurrentPatrolFromSpecifiedStart = new List<Vector3>();

            NavMesh.CalculatePath(startCalculatingPathFrom, s_EnemyProperties.patrolPoints[i].position, NavMeshesManager.enemyTypeQueryFilter[enemyType], s_EnemyProperties.navMeshPath);

            for (int j = 0; j < s_EnemyProperties.navMeshPath.corners.Length; j++)
            {
                pathPointsToCurrentPatrolFromSpecifiedStart.Add(s_EnemyProperties.navMeshPath.corners[j]);
            }

            s_EnemyProperties.pathPoints.Add(pathPointsToCurrentPatrolFromSpecifiedStart);
            startCalculatingPathFrom = pathPointsToCurrentPatrolFromSpecifiedStart[s_EnemyProperties.navMeshPath.corners.Length - 1];
        }
    }

    public bool GetCurrentClosestPathPoint(ref Vector3 currentPathPoint)
    {
        bool movingToNextPathPoint = true;

        Vector3 _currentPathPointBeingFollowed = s_EnemyProperties.pathPoints[s_EnemyProperties.currentPathIndex][s_EnemyProperties.currentPathPointIndex];
        if (Vector3.Distance(transform.position, _currentPathPointBeingFollowed) < s_EnemyProperties.distanceToStopFromPathPoint)
        {
            s_EnemyProperties.currentPathPointIndex++; // Advance to next path point index.

            if (s_EnemyProperties.loopPathPoints)
            {
                if (s_EnemyProperties.currentPathPointIndex >= s_EnemyProperties.pathPoints[s_EnemyProperties.currentPathIndex].Count)
                {
                    // Is this the final path in the patrol list ? Then restart path : Otherwise move onto the next path in the patrol list.
                    if (s_EnemyProperties.currentPathIndex + 1 >= s_EnemyProperties.pathPoints.Count)
                    {
                        movingToNextPathPoint = true;
                        s_EnemyProperties.currentPathIndex = 0;
                        s_EnemyProperties.currentPathPointIndex = 0;
                        s_EnemyProperties.pathingState = EnemyPathingStates.ReachedEndOfPath;
                    }
                    else
                    {
                        movingToNextPathPoint = true;
                        s_EnemyProperties.currentPathIndex++;
                        s_EnemyProperties.currentPathPointIndex = 0;
                        s_EnemyProperties.pathingState = EnemyPathingStates.ReachedEndOfPath;
                    }
                }
            }
            else
            {
                if (s_EnemyProperties.currentPathPointIndex >= s_EnemyProperties.pathPoints[s_EnemyProperties.currentPathIndex].Count)
                {
                    // Is this the final path in the patrol list ? Then stop moving : Otherwise move onto the next path in the patrol list.
                    if (s_EnemyProperties.currentPathIndex + 1 >= s_EnemyProperties.pathPoints.Count)
                    {
                        s_EnemyProperties.pathingState = EnemyPathingStates.ReachedEndOfPatrolRoute;
                        s_EnemyProperties.currentPathPointIndex--;
                        movingToNextPathPoint = false;
                    }
                    else
                    {
                        movingToNextPathPoint = true;
                        s_EnemyProperties.currentPathIndex++;
                        s_EnemyProperties.currentPathPointIndex = 0;
                        s_EnemyProperties.pathingState = EnemyPathingStates.ReachedEndOfPath;
                    }
                }
            }

            currentPathPoint = s_EnemyProperties.pathPoints[s_EnemyProperties.currentPathIndex][s_EnemyProperties.currentPathPointIndex];
            return movingToNextPathPoint;
        }
        else
        {
            movingToNextPathPoint = false;
            s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
            currentPathPoint = s_EnemyProperties.pathPoints[s_EnemyProperties.currentPathIndex][s_EnemyProperties.currentPathPointIndex];
            return movingToNextPathPoint;
        }
    }

    public void GeneratePathToPlayer(EnemyType enemyType)
    {
        //s_EnemyProperties.pathPointsToPlayer.Clear();
        //Vector3 startCalculatingPathFrom = transform.position;
        //NavMesh.CalculatePath(startCalculatingPathFrom, EnemyProperties.playerTransform.position, NavMeshesManager.enemyTypeQueryFilter[enemyType], s_EnemyProperties.navMeshPath);

        //for (int j = 0; j < s_EnemyProperties.navMeshPath.corners.Length; j++)
        //{
        //    s_EnemyProperties.pathPoints.Add(s_EnemyProperties.navMeshPath.corners[j]);
        //}
    }
}
