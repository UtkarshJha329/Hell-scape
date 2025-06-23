using UnityEngine;

[RequireComponent(typeof(CharacterProperties))]
public class CharacterInput : MonoBehaviour
{
    private CharacterProperties s_CharacterProperties;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        s_CharacterProperties = GetComponent<CharacterProperties>();
    }

    // Update is called once per frame
    void Update()
    {
        s_CharacterProperties.horizontalPlaneInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        s_CharacterProperties.jumped = Input.GetKeyDown(KeyCode.Space);

        s_CharacterProperties.mouseDelta = Input.mousePositionDelta;
    }
}
