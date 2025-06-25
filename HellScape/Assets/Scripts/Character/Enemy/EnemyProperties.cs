using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyPathingStates
{
    FollowingPath,
    ReachedEndOfPath,
    ReachedEndOfPatrolRoute
}

public enum EnemyPlayerRelationStates
{
    ManipulatingPlayer,
    ChasingPlayer,
    AttackingPlayer
}

public enum EnemyGenericStates
{
    Idling,
    Patroling,
    InteractingWithPlayer,
    ReturningToPatrol
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

public enum EnemyActionsWhenInterractingWithPlayer
{
    Attack,
    PlayDead,
    None,
    CastDebuff,
}

[System.Serializable]
public struct NavMeshSurfaceEnemyType
{
    public NavMeshSurface navMeshSurface;
    public EnemyType enemyType;
}

[System.Serializable]
public struct EnemyStateParameters
{
    public EnemyGenericStates enemyGenericState;
    public float timeToStayInState;
}

public class EnemyProperties : MonoBehaviour
{
    [Header("Player Data Visible To Enemy")]
    public static Transform playerTransform = null;
    public static CharacterProperties s_PlayerCharacterProperties;
    public LayerMask playerLayerMask;

    [Header("Enemy Character Properties")]
    public EnemyType enemyType;
    public float headRotateSpeed = 10.0f;
    public float noticePlayerDistance = 20.0f;
    public float characterAttackFromDistance = 1.5f;

    [Header("Path To Player Properties")]
    public float updateInSeconds = 1.0f;

    [Header("Path Following Properties")]
    public float distanceToStopFromPathPoint = 1.5f;
    public float distanceToStopFromTempPathPointToTarget = 2.0f;
    public bool loopPathPoints = true;
    
    [Header("Path Points")]
    public List<Transform> patrolPoints = new List<Transform>();
    public List<List<Vector3>> pathPoints = new List<List<Vector3>>();
    public List<Vector3> tempPathPointsToCurrentTarget = new List<Vector3>();

    // Do we really need two of them?
    public NavMeshPath navMeshPath;
    public NavMeshPath navMeshPathToTarget;

    public int currentPathIndex = 0;
    public int currentPathPointIndex = 0;

    public int currentTempPathPointToTargetIndex = 0;

    // VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV ADD IT INTO ITS OWN FILE, using scriptable objects or whatever.
    [Header("Generic States Behaviour")]
    public List<EnemyStateParameters> enemyStateParameters = new List<EnemyStateParameters>();
    public static Dictionary<EnemyGenericStates, EnemyStateParameters> enemyGenericStateParameters = new Dictionary<EnemyGenericStates, EnemyStateParameters>();

    [Header("Ghoul Interaction Properties")]
    public int damageAmountAtOnceBeforePlayDead = 5;
    public float playDeadForTime = 15.0f;
    public bool startedPlayingDead = false;

    [Header("Enemy Attack Specific Variables")]
    public float attackSphereColliderRadius = 5.0f;
    public float enemyAttackTravelDistance = 2.5f;
    public int attackDamageAmount = 5;

    // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA


    [Header("Enemy States")]
    public EnemyPathingStates pathingState;
    public EnemyPlayerRelationStates playerRelationState;
    public EnemyGenericStates genericState;

    public EnemyActionsWhenInterractingWithPlayer enemyActionWhenInterractingWithPlayer;

    private void Awake()
    {
        if(playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectsWithTag("Player")[0].transform;
            s_PlayerCharacterProperties = playerTransform.GetComponent<CharacterProperties>();
        }

        for (int i = 0; i < enemyStateParameters.Count; i++)
        {
            enemyGenericStateParameters.Add(enemyStateParameters[i].enemyGenericState, enemyStateParameters[i]);
        }
    }

    private void Start()
    {
        navMeshPath = new NavMeshPath();
        navMeshPathToTarget = new NavMeshPath();
    }
}
