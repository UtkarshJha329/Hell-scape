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

    private float generatePathToPlayerAtTime = 0.0f;
    private float generatePathToPlayerEveryXSeconds = 1.0f;

    private bool generatedPathToReturnToPatrol = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        s_CharacterProperties = GetComponent<CharacterProperties>();
        s_EnemyProperties = GetComponent<EnemyProperties>();

        GeneratePathToFollow(s_EnemyProperties.enemyType);

        s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
        s_EnemyProperties.genericState = EnemyGenericStates.Patroling;
    }

    // Update is called once per frame
    void Update()
    {
        if(s_EnemyProperties.enemyActionWhenInterractingWithPlayer != EnemyActionsWhenInterractingWithPlayer.PlayDead)
        {
            EnemyInteractingWithPlayerStateManager();
            if (s_EnemyProperties.genericState != EnemyGenericStates.InteractingWithPlayer
                && s_EnemyProperties.genericState != EnemyGenericStates.ReturningToPatrol)
            {
                EnemyPatrolAndIdleStateManager();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(s_EnemyProperties != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentDestination, s_EnemyProperties.distanceToStopFromPathPoint);
        }
    }

    public void EnemyInteractingWithPlayerStateManager()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, EnemyProperties.playerTransform.position);
        if (distanceToPlayer <= s_EnemyProperties.noticePlayerDistance)
        {
            s_EnemyProperties.genericState = EnemyGenericStates.InteractingWithPlayer;
            generatedPathToReturnToPatrol = false;

            if(s_EnemyProperties.enemyType == EnemyType.Ghoul)
            {
                MoveToPlayerAndPerformAction(distanceToPlayer, EnemyActionsWhenInterractingWithPlayer.Attack);
            }
        }
        else
        {
            CooldownFromInteractingWithPlayer();
        }
    }

    public void MoveToPlayerAndPerformAction(float distanceToPlayer, EnemyActionsWhenInterractingWithPlayer _enemyActionsWhenInterractingWithPlayer)
    {
        if (distanceToPlayer <= s_EnemyProperties.characterAttackFromDistance && s_EnemyProperties.pathingState == EnemyPathingStates.ReachedEndOfPath)
        {
            //Debug.Log("Action on player := " + _enemyActionsWhenInterractingWithPlayer);
            s_EnemyProperties.enemyActionWhenInterractingWithPlayer = _enemyActionsWhenInterractingWithPlayer;
        }
        else
        {
            if (generatePathToPlayerAtTime <= Time.time)
            {
                GeneratePathToPlayer(s_EnemyProperties.enemyType);
                generatePathToPlayerAtTime = Time.time + generatePathToPlayerEveryXSeconds;
            }

            if (s_EnemyProperties.tempPathPointsToCurrentTarget.Count > 0)
            {
                FollowCurrentPathPointsToTarget();
            }
        }
    }

    public void CooldownFromInteractingWithPlayer()
    {
        if (s_EnemyProperties.genericState == EnemyGenericStates.InteractingWithPlayer)
        {
            if (!generatedPathToReturnToPatrol)
            {
                GeneratePathBackToCurrentPatrolPathPoint(s_EnemyProperties.enemyType);
                s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
                generatedPathToReturnToPatrol = true;
            }
        }

        if(s_EnemyProperties.tempPathPointsToCurrentTarget.Count > 0 && s_EnemyProperties.pathingState != EnemyPathingStates.ReachedEndOfPath)
        {
            s_EnemyProperties.genericState = EnemyGenericStates.ReturningToPatrol;

            //Follow the path generated to return to patroling from where you left off.
            FollowCurrentPathPointsToTarget();

            if(s_EnemyProperties.pathingState == EnemyPathingStates.ReachedEndOfPath)
            {
                s_EnemyProperties.tempPathPointsToCurrentTarget.Clear();
            }
        }
        else
        {
            if(s_EnemyProperties.genericState == EnemyGenericStates.ReturningToPatrol && s_EnemyProperties.pathingState == EnemyPathingStates.ReachedEndOfPath)
            {
                //Debug.Log("Resetting back to normal patrol.");

                // Reset back to patroling the area.
                s_EnemyProperties.genericState = EnemyGenericStates.Patroling;
                s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
            }
        }
    }

    public void EnemyPatrolAndIdleStateManager()
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
                    //Debug.Log("Idle finished, returning to patrol.");
                    s_EnemyProperties.genericState = EnemyGenericStates.Patroling;
                    s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
                }
                else
                {
                    //Debug.Log("Idling...");
                }
            }
            else
            {
                //Debug.Log("Simple Patroling.");
                s_EnemyProperties.genericState = EnemyGenericStates.Patroling;
                s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
                FollowCurrentPathPoints();
            }

        }
    }

    public void FollowCurrentPathPointsToTarget()
    {
        Vector3 currentPathPoint = Vector3.zero;
        GetCurrentClosestTempPathPointToTarget(ref currentPathPoint);

        float distance = Vector3.Distance(currentPathPoint, transform.position);
        if(distance > s_EnemyProperties.distanceToStopFromTempPathPointToTarget)
        {
            Vector3 moveDir = (currentPathPoint - transform.position).normalized;
            s_CharacterProperties.horizontalPlaneInput = new Vector2(moveDir.x, moveDir.z);
        }
        else
        {
            //Vector3 moveDir = (currentPathPoint - transform.position).normalized;
            //s_CharacterProperties.horizontalPlaneInput = new Vector2(moveDir.x, moveDir.z);

            s_CharacterProperties.horizontalPlaneInput = Vector2.zero;
        }

        s_CharacterProperties.mouseDelta = Vector2.zero;
        currentDestination = currentPathPoint;

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

    public void GetCurrentClosestTempPathPointToTarget(ref Vector3 currentPathPoint)
    {
        Vector3 _currentPathPointBeingFollowed = s_EnemyProperties.tempPathPointsToCurrentTarget[s_EnemyProperties.currentTempPathPointToTargetIndex];

        if (Vector3.Distance(transform.position, _currentPathPointBeingFollowed) <= s_EnemyProperties.distanceToStopFromTempPathPointToTarget)
        //if (Vector3.Distance(transform.position, _currentPathPointBeingFollowed) <= 1.5f)
        {
            //Debug.Log("Changing to next path point to target.");
            s_EnemyProperties.currentTempPathPointToTargetIndex++;

            if (s_EnemyProperties.currentTempPathPointToTargetIndex >= s_EnemyProperties.tempPathPointsToCurrentTarget.Count)
            {
                s_EnemyProperties.currentTempPathPointToTargetIndex--;
                s_EnemyProperties.pathingState = EnemyPathingStates.ReachedEndOfPath;
            }

            currentPathPoint = s_EnemyProperties.tempPathPointsToCurrentTarget[s_EnemyProperties.currentTempPathPointToTargetIndex];
        }
        else
        {
            //Debug.Log("Following path point to target.");
            s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
            currentPathPoint = s_EnemyProperties.tempPathPointsToCurrentTarget[s_EnemyProperties.currentTempPathPointToTargetIndex];
        }
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
                        Debug.Log("Finished all paths, returning to start.");

                        movingToNextPathPoint = true;
                        s_EnemyProperties.currentPathIndex = 0;
                        s_EnemyProperties.currentPathPointIndex = 0;
                        s_EnemyProperties.pathingState = EnemyPathingStates.ReachedEndOfPath;
                    }
                    else
                    {

                        Debug.Log("Finished this path, moving to next one.");

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
                        Debug.Log("Finished patroling all paths.");

                        s_EnemyProperties.pathingState = EnemyPathingStates.ReachedEndOfPatrolRoute;
                        s_EnemyProperties.currentPathPointIndex--;
                        movingToNextPathPoint = false;
                    }
                    else
                    {

                        Debug.Log("Finished this path, moving to the next one, no looping.");

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
            Debug.Log("Simply patroling the current path that I have been given.");

            movingToNextPathPoint = false;
            s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
            currentPathPoint = s_EnemyProperties.pathPoints[s_EnemyProperties.currentPathIndex][s_EnemyProperties.currentPathPointIndex];
            return movingToNextPathPoint;
        }
    }

    public void GeneratePathToPlayer(EnemyType enemyType)
    {
        // Clear to start with a fresh list that was calculated this time.
        s_EnemyProperties.tempPathPointsToCurrentTarget.Clear();

        // With a new list, previous path index is invalid.
        s_EnemyProperties.currentTempPathPointToTargetIndex = 0;

        Vector3 startCalculatingPathFrom = transform.position;

        NavMesh.CalculatePath(startCalculatingPathFrom, EnemyProperties.playerTransform.position, NavMeshesManager.enemyTypeQueryFilter[enemyType], s_EnemyProperties.navMeshPathToTarget);

        for (int i = 0; i < s_EnemyProperties.navMeshPathToTarget.corners.Length; i++)
        {
            s_EnemyProperties.tempPathPointsToCurrentTarget.Add(s_EnemyProperties.navMeshPathToTarget.corners[i]);
        }

        //Debug.Log("Generated path to player := " + s_EnemyProperties.tempPathPointsToCurrentTarget.Count);
    }

    public void GeneratePathBackToCurrentPatrolPathPoint(EnemyType enemyType)
    {
        // Clear to start with a fresh list that was calculated this time.
        s_EnemyProperties.tempPathPointsToCurrentTarget.Clear();

        // With a new list, previous path index is invalid.
        s_EnemyProperties.currentTempPathPointToTargetIndex = 0;

        Vector3 startCalculatingPathFrom = transform.position;

        NavMesh.CalculatePath(startCalculatingPathFrom, s_EnemyProperties.pathPoints[s_EnemyProperties.currentPathIndex][s_EnemyProperties.currentPathPointIndex], NavMeshesManager.enemyTypeQueryFilter[enemyType], s_EnemyProperties.navMeshPathToTarget);

        for (int i = 0; i < s_EnemyProperties.navMeshPathToTarget.corners.Length; i++)
        {
            s_EnemyProperties.tempPathPointsToCurrentTarget.Add(s_EnemyProperties.navMeshPathToTarget.corners[i]);
        }

        Debug.Log("Generated path to go back to patroling path point := " + s_EnemyProperties.tempPathPointsToCurrentTarget.Count);
    }
}
