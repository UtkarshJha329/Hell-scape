using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshesManager : MonoBehaviour
{
    [Header("Navmesh Properties")]
    public List<NavMeshSurfaceEnemyType> surfaceAndCorrespondingEnemyTypes = new List<NavMeshSurfaceEnemyType>();
    public static Dictionary<EnemyType, NavMeshQueryFilter> enemyTypeQueryFilter = new Dictionary<EnemyType, NavMeshQueryFilter>();

    private void Awake()
    {
        for (int i = 0; i < surfaceAndCorrespondingEnemyTypes.Count; i++)
        {
            NavMeshQueryFilter enemyAgentFilter = new NavMeshQueryFilter();
            enemyAgentFilter.agentTypeID = surfaceAndCorrespondingEnemyTypes[i].navMeshSurface.agentTypeID;
            enemyAgentFilter.areaMask = NavMesh.AllAreas;

            enemyTypeQueryFilter.Add(surfaceAndCorrespondingEnemyTypes[i].enemyType, enemyAgentFilter);
        }

    }
}
