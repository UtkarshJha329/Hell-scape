using UnityEngine;

public enum LifeState
{
    Alive,
    TakenHit,
    Dead
}

public class CharacterProperties : MonoBehaviour
{
    [Header("Movement Input Data")]
    public Vector2 horizontalPlaneInput = Vector2.zero;
    public bool jumped = false;

    [Header("Camera Input Data")]
    public Vector2 mouseDelta = Vector2.zero;

    [Header("Camera Movement Settings")]
    public float mouseXSensitivity = 50.0f;
    public float mouseYSensitivity = 50.0f;
    public Vector2 mouseYClampAngles = new Vector2(-60.0f, 60.0f);

    [Header("Character Movement Settings")]
    public float characterMoveSpeed = 10.0f;
    public float verticalJumpSpeed = 10.0f;
    public float gravityMultiplierWhileAscent = 2.0f;
    public float gravityMultiplierWhileDescent = 4.0f;

    public Vector3 velocity = Vector3.zero;


    [Header("Health Stuff")]
    public LifeState lifeState;

}
