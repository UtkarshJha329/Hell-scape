using UnityEngine;

[RequireComponent (typeof(EnemyProperties))]
[RequireComponent (typeof(CharacterProperties))]
public class EnemyPlayerInterraction : MonoBehaviour
{
    private CharacterProperties s_CharacterProperties;
    private EnemyProperties s_EnemyProperties;

    private float playDeadUntilTime = 0.0f;

    private void Awake()
    {
        s_CharacterProperties = GetComponent<CharacterProperties>();
        s_EnemyProperties = GetComponent<EnemyProperties>();
    }

    private void Update()
    {
        if (s_EnemyProperties.enemyType == EnemyType.Ghoul)
        {
            if (s_EnemyProperties.enemyActionWhenInterractingWithPlayer == EnemyActionsWhenInterractingWithPlayer.Attack)
            {
                AttackMelee();
            }
            else if (s_EnemyProperties.enemyActionWhenInterractingWithPlayer == EnemyActionsWhenInterractingWithPlayer.PlayDead)
            {
                PlayDead();
            }
        }

        HandleDeath();
    }

    private void AttackMelee()
    {
        // Attack player.
        // Apply knockback on player.
        // Continue until player is either dead or you are knocked out.
        //Debug.Log("Meelee attack performed.");

        RaycastHit hitInfo;
        if(Physics.SphereCast(transform.position, s_EnemyProperties.attackSphereColliderRadius, transform.forward, out hitInfo, s_EnemyProperties.enemyAttackTravelDistance, s_EnemyProperties.playerLayerMask, QueryTriggerInteraction.Ignore))
        {
            //Debug.Log("Collided with player layer." + hitInfo.collider.gameObject.name);
            if (hitInfo.collider.CompareTag("Player"))
            {
                Debug.Log("Attack landed on player.");
            }
        }
    }

    private void PlayDead()
    {
        // Freeze movement.
        // Play fake death sound.
        // 'Die' on the ground.
        if (!s_EnemyProperties.startedPlayingDead)
        {
            playDeadUntilTime = Time.time + s_EnemyProperties.playDeadForTime;
            s_EnemyProperties.startedPlayingDead = true;
            transform.localScale = Vector3.one * 0.25f;
        }

        if (playDeadUntilTime <= Time.time)
        {
            s_EnemyProperties.startedPlayingDead = false;
            s_EnemyProperties.enemyActionWhenInterractingWithPlayer = EnemyActionsWhenInterractingWithPlayer.None;
            transform.localScale = Vector3.one;
        }

    }

    private void HandleDeath()
    {
        if(s_CharacterProperties.health <= 0)
        {
            // Die.
            Debug.Log(gameObject.name + "says, 'I am dead!'");
            s_CharacterProperties.lifeState = LifeState.Dead;
            gameObject.SetActive(false);
        }
    }

    public void RecievedAttackFromPlayer(int damageAmount)
    {
        if(s_EnemyProperties.enemyType == EnemyType.Ghoul)
        {
            if(damageAmount > s_EnemyProperties.damageAmountAtOnceBeforePlayDead && damageAmount < s_CharacterProperties.health)
            {
                s_EnemyProperties.enemyActionWhenInterractingWithPlayer = EnemyActionsWhenInterractingWithPlayer.PlayDead;
            }
            else
            {
                s_CharacterProperties.health -= damageAmount;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(s_EnemyProperties != null)
        {
            Gizmos.DrawWireSphere(transform.position, s_EnemyProperties.attackSphereColliderRadius);
            Gizmos.DrawWireSphere(transform.position + transform.forward * s_EnemyProperties.enemyAttackTravelDistance, s_EnemyProperties.attackSphereColliderRadius);
        }
    }
}
