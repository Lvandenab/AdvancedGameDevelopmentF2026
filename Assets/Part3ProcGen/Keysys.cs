using System.Runtime.CompilerServices;
using UnityEngine;

public class Keysys : MonoBehaviour
{
    public bool hasYellowKey = false;
    public bool hasBlueKey = false;
    private void OnControllerColliderHit(ControllerColliderHit collision)
    {
        if (collision.gameObject.CompareTag("Yellow"))
        {
            hasYellowKey = true;
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("Blue"))
        {
            hasBlueKey = true;
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("BlueDoor") && hasBlueKey)
        {
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("YellowDoor") && hasYellowKey)
        {
            Destroy(collision.gameObject);
        }
    }
}

