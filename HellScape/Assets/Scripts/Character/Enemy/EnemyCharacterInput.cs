using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterProperties))]
[RequireComponent(typeof(EnemyProperties))]
public class EnemyCharacterInput : MonoBehaviour
{
    private CharacterProperties s_CharacterProperties;
    private EnemyProperties s_EnemyProperties;

    private Vector3 currentDestination = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        s_CharacterProperties = GetComponent<CharacterProperties>();
        s_EnemyProperties = GetComponent<EnemyProperties>();

        GeneratePathToFollow(EnemyType.WideAgent);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPathPoint = Vector3.zero;

        bool changedPoints = GetCurrentClosestPathPoint(ref currentPathPoint);
        //Debug.Log(changedPoints);

        Vector3 moveDir = (currentPathPoint - transform.position).normalized;
        s_CharacterProperties.horizontalPlaneInput = new Vector2(moveDir.x, moveDir.z);

        s_CharacterProperties.mouseDelta = Vector2.zero;

        currentDestination = currentPathPoint;
    }

    private void OnDrawGizmos()
    {
        if(s_EnemyProperties != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentDestination, s_EnemyProperties.distanceToStopFromPathPoint);
        }
    }

    public void GeneratePathToFollow(EnemyType enemyType)
    {
        Vector3 startCalculatingPathFrom = transform.position;
        for (int i = 0; i < s_EnemyProperties.patrolPoints.Count; i++)
        {
            //NavMesh.CalculatePath(startCalculatingPathFrom, s_EnemyProperties.patrolPoints[i].position, NavMesh.AllAreas, s_EnemyProperties.navMeshPath);
            NavMesh.CalculatePath(startCalculatingPathFrom, s_EnemyProperties.patrolPoints[i].position, EnemyProperties.enemyTypeQueryFilter[enemyType], s_EnemyProperties.navMeshPath);

            for (int j = 0; j < s_EnemyProperties.navMeshPath.corners.Length; j++)
            {
                s_EnemyProperties.pathPoints.Add(s_EnemyProperties.navMeshPath.corners[j]);
            }

            startCalculatingPathFrom = s_EnemyProperties.pathPoints[s_EnemyProperties.pathPoints.Count - 1];
        }
    }

    public bool GetCurrentClosestPathPoint(ref Vector3 currentPathPoint)
    {
        bool movingToNextPathPoint = true;

        if (Vector3.Distance(transform.position, s_EnemyProperties.pathPoints[s_EnemyProperties.currentPathPointIndex]) < s_EnemyProperties.distanceToStopFromPathPoint)
        {
            s_EnemyProperties.currentPathPointIndex++;

            if (s_EnemyProperties.loopPathPoints)
            {
                if (s_EnemyProperties.currentPathPointIndex >= s_EnemyProperties.pathPoints.Count)
                {
                    movingToNextPathPoint = true;
                    s_EnemyProperties.currentPathPointIndex = 0;
                    s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
                }
            }
            else
            {
                if (s_EnemyProperties.currentPathPointIndex >= s_EnemyProperties.pathPoints.Count)
                {
                    s_EnemyProperties.pathingState = EnemyPathingStates.ReachedEndOfPath;
                    s_EnemyProperties.currentPathPointIndex--;
                    movingToNextPathPoint = false;
                }
            }

            currentPathPoint = s_EnemyProperties.pathPoints[s_EnemyProperties.currentPathPointIndex];
            return movingToNextPathPoint;
        }
        else
        {
            movingToNextPathPoint = false;
            s_EnemyProperties.pathingState = EnemyPathingStates.FollowingPath;
            currentPathPoint = s_EnemyProperties.pathPoints[s_EnemyProperties.currentPathPointIndex];
            return movingToNextPathPoint;
        }
    }

    public void GeneratePathToPlayer(EnemyType enemyType)
    {
        Vector3 startCalculatingPathFrom = transform.position;
        //NavMesh.CalculatePath(startCalculatingPathFrom, EnemyProperties.playerTransform.position, NavMesh.AllAreas, s_EnemyProperties.navMeshPath);
        NavMesh.CalculatePath(startCalculatingPathFrom, EnemyProperties.playerTransform.position, EnemyProperties.enemyTypeQueryFilter[enemyType], s_EnemyProperties.navMeshPath);

        for (int j = 0; j < s_EnemyProperties.navMeshPath.corners.Length; j++)
        {
            s_EnemyProperties.pathPoints.Add(s_EnemyProperties.navMeshPath.corners[j]);
        }
    }
}
