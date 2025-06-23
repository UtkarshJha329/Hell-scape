using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(CharacterProperties))]
[RequireComponent(typeof(CharacterController))]
public class CharacterMovementController : MonoBehaviour
{

    [Header("References")]
    public Transform cameraHolder;

    //================ Private Variables =======================
    
    //================ References ==============================
    private CharacterController characterController;
    private CharacterProperties s_CharacterProperties;

    private Vector3 newCameraRotation = Vector3.zero;
    private Vector3 newCharacterRotation = Vector3.zero;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        s_CharacterProperties = GetComponent<CharacterProperties>();

        if(cameraHolder == null)
        {
            Debug.LogError("Reference to Camera Holder variable in Character Movement Controller component attached to " + gameObject.name + " is missing!");
        }

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        PerformCameraMovement();
        PerformCharacterMovement();
    }

    private void PerformCameraMovement()
    {
        newCharacterRotation.y += s_CharacterProperties.mouseDelta.x * s_CharacterProperties.mouseXSensitivity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(newCharacterRotation);

        newCameraRotation.x -= s_CharacterProperties.mouseDelta.y * s_CharacterProperties.mouseYSensitivity * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, s_CharacterProperties.mouseYClampAngles.x, s_CharacterProperties.mouseYClampAngles.y);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void PerformCharacterMovement()
    {
        if(characterController.isGrounded && s_CharacterProperties.velocity.y < 0.0f)
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
            if(Vector3.Dot(hitInfo.normal, currentHorizontalPlaneVelocity.normalized) > 0.0f)
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

        Debug.DrawRay(transform.position, s_CharacterProperties.velocity, Color.blue);
        Debug.DrawRay(transform.position, currentHorizontalPlaneVelocity, Color.green);
        //Debug.DrawRay(transform.position, currentPlaneVelocity, Color.yellow);

        Vector3 moveAmount = s_CharacterProperties.velocity * Time.deltaTime;
        characterController.Move(moveAmount);

    }
}
