using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
	public static ScoreManager Instance { get; private set; } // 싱글톤 패턴

	[SerializeField] private TMP_Text scoreText; // Text대신 TMP_Text 사용 (TextMesh Pro임)
	private int currentScore = 0; // 현재 점수

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			Instance = this;
		}
		// 초기 점수 표시
		UpdateScoreUI();
	}

	// 점수 추가 함수 (Collectable.cs에서 호출)
	public void AddScore(float amount)
	{
		currentScore += Mathf.RoundToInt(amount); // 소수점 제거하고 정수로 추가
		UpdateScoreUI();
		Debug.Log("점수 획득: " + amount + ", 현재 점수: " + currentScore);
	}

	// 점수 감소 함수 (ShipMovement.cs에서 호출)
	public void DecreaseScore(float amount)
	{
		currentScore -= Mathf.RoundToInt(amount); // 소수점 제거하고 정수로 감소
		if (currentScore < 0) // 0 이하로 떨어지지 않도록
		{
			currentScore = 0;
		}
		UpdateScoreUI();
		Debug.Log("점수 감소: " + amount + ", 현재 점수: " + currentScore);
	}

	// UI Text 업데이트
	private void UpdateScoreUI()
	{
		if (scoreText != null)
		{
			scoreText.text = "Score: " + currentScore.ToString();
		}
	}
}