using MyProtocol.Packets;
using MyProtocol.Serializator;
using UnityEngine;

public class BilliardController : MonoBehaviour
{
    public GameObject selectedBall;
    public LineRenderer lineRenderer;
    public float forceMultiplier = 10f;

    private Vector2 mousePosition;
    private Vector2 direction;
    private bool isSelected = false;

    void Update()
    {
        if ((Program.currentPlayer ?? "1").Equals(MenuInput.name)) return;
        if (BallManager.Instance.AreBallsMoving())
        {
            Debug.Log("asd");
            return; // ≈сли шары движутс€, не позвол€ем выбирать и бить
        }
        else if (PocketHandler.needsChange && Program.currentPlayer == MenuInput.name)
        {
            Program.client.QueuePacketSend(PacketConverter.Serialize(MyProtocol.PacketType.ChangeTurn, new PacketChangeTurn() { PlayerName = Program.currentPlayer }).ToPacket());
            PocketHandler.needsChange = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            SelectBall();
        }

        if (isSelected)
        {
            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            direction = (mousePosition - (Vector2)selectedBall.transform.position).normalized;
            DrawLine();

            if (Input.GetMouseButtonDown(1))
            {
                HitBall();
            }
        }
    }

    void SelectBall()
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Ball"))
        {
            selectedBall = hit.collider.gameObject;
            isSelected = true;
        }
        else
        {
            isSelected = false;
            lineRenderer.positionCount = 0;
        }
    }

    void DrawLine()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, selectedBall.transform.position);
        lineRenderer.SetPosition(1, mousePosition);
    }

    void HitBall()
    {
        Rigidbody2D rb = selectedBall.GetComponent<Rigidbody2D>();
        rb.AddForce(-direction * forceMultiplier, ForceMode2D.Impulse);
        Program.client.QueuePacketSend(PacketConverter.Serialize(MyProtocol.PacketType.StrikeBall, new PacketStrikeBall() { BallNumber = int.Parse(rb.name.Split(new char[] { '(', ')' })[1]), Direction = new MyProtocol.Vector2D(-direction.x, -direction.y), PlayerName =MenuInput.name, Power = 10f }).ToPacket());
        isSelected = false;
        lineRenderer.positionCount = 0;
    }
}
