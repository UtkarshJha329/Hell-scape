using UnityEngine;

[RequireComponent(typeof(EnemyProperties))]
public class EnemyMovementController : MonoBehaviour
{
    [Header("References")]
    public Transform headHolder;

    //================ Private Variables =======================

    //================ References ==============================
    private CharacterController characterController;
    private CharacterProperties s_CharacterProperties;
    private EnemyProperties s_EnemyProperties;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        s_CharacterProperties = GetComponent<CharacterProperties>();
        s_EnemyProperties = GetComponent<EnemyProperties>();
    }

    private void Start()
    {
        if (EnemyProperties.playerTransform == null)
        {
            Debug.LogError("Reference to player transform variable in Enemy Movement Controller component attached to " + gameObject.name + " is missing!");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        PerformHeadMovement();
        PerformCharacterMovement();
    }

    private void PerformHeadMovement()
    {
        Vector3 lookDirection = EnemyProperties.playerTransform.position - transform.position;
        headHolder.rotation = Quaternion.Lerp(headHolder.rotation, Quaternion.LookRotation(lookDirection), s_EnemyProperties.headRotateSpeed * Time.deltaTime);
    }

    private void PerformCharacterMovement()
    {
        if (characterController.isGrounded && s_CharacterProperties.velocity.y < 0.0f)
        {
            s_CharacterProperties.velocity.y = 0.0f;
        }

        Vector3 currentHorizontalPlaneVelocity = transform.right * s_CharacterProperties.characterMoveSpeed * s_CharacterProperties.horizontalPlaneInput.x
                                + transform.forward * s_CharacterProperties.characterMoveSpeed * s_CharacterProperties.horizontalPlaneInput.y;


        Vector3 currentPlaneVelocity = currentHorizontalPlaneVelocity;
        Ray rayToDetermineGroundNormal = new Ray(transform.position + (Vector3.down * characterController.height * 0.5f), Vector3.down);
        Debug.DrawLine(rayToDetermineGroundNormal.origin, rayToDetermineGroundNormal.origin + rayToDetermineGroundNormal.direction * 0.5f, Color.yellow);
        RaycastHit hitInfo;
        if (Physics.Raycast(rayToDetermineGroundNormal, out hitInfo, 0.5f))
        {
            if (Vector3.Dot(hitInfo.normal, currentHorizontalPlaneVelocity.normalized) > 0.0f)
            {
                currentPlaneVelocity = Vector3.ProjectOnPlane(currentHorizontalPlaneVelocity, hitInfo.normal);
            }
        }

        if (characterController.isGrounded)
        {
            s_CharacterProperties.velocity = currentPlaneVelocity;
        }
        else
        {
            s_CharacterProperties.velocity = new Vector3(currentPlaneVelocity.x, s_CharacterProperties.velocity.y, currentPlaneVelocity.z);
        }

        if (characterController.isGrounded && s_CharacterProperties.jumped)
        {
            s_CharacterProperties.velocity.y = s_CharacterProperties.verticalJumpSpeed;
        }

        if (s_CharacterProperties.velocity.y > 0.0f)
        {
            //Debug.Log("Subtracted ascent gravity acceleration := " + s_CharacterProperties.velocity.y);
            s_CharacterProperties.velocity.y += s_CharacterProperties.gravityMultiplierWhileAscent * Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            //Debug.Log("Subtracted descent gravity acceleration := " + s_CharacterProperties.velocity.y);
            s_CharacterProperties.velocity.y += s_CharacterProperties.gravityMultiplierWhileDescent * Physics.gravity.y * Time.deltaTime;
        }

        Vector3 moveAmount = s_CharacterProperties.velocity * Time.deltaTime;
        characterController.Move(moveAmount);

    }
}
