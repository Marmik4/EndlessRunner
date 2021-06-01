using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerLookat;
    public Vector3 offset;
    private Vector3 defaultOffset;
    //public GameManager manager;
    public PlayerMovement playerMove;
    public Vector3 rotation = new Vector3(15, 0, 0);

    public bool IsMoving { set; get; }

    private void Awake()
    {
        defaultOffset = offset;
    }

    private void LateUpdate()
    {
        if (GameManager.Instance.isDead)
            return;

        if (!IsMoving)
            return;

        if (GameManager.Instance.standOffcollided)
            offset.z = -5;
        else
            offset = defaultOffset;

        Vector3 desiredPosition = playerLookat.position + offset;
        if (playerMove.Lane==1)
            desiredPosition.x = 0;
        else if(playerMove.Lane == 0)
            desiredPosition.x = -1f;
        else if (playerMove.Lane == 2)
            desiredPosition.x = 1f;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotation), 0.1f);
    }
}
