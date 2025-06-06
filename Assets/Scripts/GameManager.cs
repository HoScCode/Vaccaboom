using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{   
    private static GameManager _instance;

    public float timeLimit = 90f;
    private float timeLeft;
    private bool timeRunning;
    private int score = 0;

    [SerializeField]
    TextMeshProUGUI timerText;

    [SerializeField]
    TextMeshProUGUI scoreText;

    private GameObject player;

    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }

        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.Log("Error: Player object was not found");
        }
    }

    public int GetScore()
    {
        return score;
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreDisplay();
    }

    public void ModifyScore(int amount)
    {
        score += amount;
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        scoreText.text = score.ToString() + "$";
    }

    private void Start()
    {
        timeLeft = timeLimit;
        timeRunning = true;
        ResetScore();
    }

    private void Update()
    {
        if (timeRunning)
        {
            ModifyTimeLeft(-Time.deltaTime);
        }               
    }

    private void OnTimeExpired()
    {
        timeLeft = 0f;
        UpdateTimeDisplay();
        timeRunning = false;

        if (player != null)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.DisableMovement();
            }
        }
    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }

    private void UpdateTimeDisplay()
    {
        float displayTime;
        
        displayTime = Mathf.Ceil(timeLeft);

        timerText.text = displayTime.ToString();
    }

    private void ModifyTimeLeft(float amount)
    {
        timeLeft += amount;
        UpdateTimeDisplay();

        if (timeLeft <= 0)
        {
            OnTimeExpired();
        }
    }

}
