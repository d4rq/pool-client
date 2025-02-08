using UnityEngine;

public class BallManager : MonoBehaviour
{
    public static BallManager Instance;
    public GameObject[] balls; // Массив всех шаров
    private bool areBallsMoving; // Флаг для проверки движения шаров

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Автоматическое заполнение массива шаров
        UpdateBallsArray();
    }

    void Update()
    {
        areBallsMoving = false;

        UpdateBallsArray();

        foreach (GameObject ball in balls)
        {
            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            if (rb.linearVelocity.magnitude > 0.1f || rb.angularVelocity > 0.1f)
            {
                areBallsMoving = true;
                break;
            }
        }
    }

    public bool AreBallsMoving()
    {
        return areBallsMoving;
    }

    public void UpdateBallsArray()
    {
        balls = GameObject.FindGameObjectsWithTag("Ball");
    }
}
