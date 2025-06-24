using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
	public static ScoreManager Instance { get; private set; } // �̱��� ����

	[SerializeField] private TMP_Text scoreText; // Text��� TMP_Text ��� (TextMesh Pro��)
	private int currentScore = 0; // ���� ����

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
		// �ʱ� ���� ǥ��
		UpdateScoreUI();
	}

	// ���� �߰� �Լ� (Collectable.cs���� ȣ��)
	public void AddScore(float amount)
	{
		currentScore += Mathf.RoundToInt(amount); // �Ҽ��� �����ϰ� ������ �߰�
		UpdateScoreUI();
		Debug.Log("���� ȹ��: " + amount + ", ���� ����: " + currentScore);
	}

	// ���� ���� �Լ� (ShipMovement.cs���� ȣ��)
	public void DecreaseScore(float amount)
	{
		currentScore -= Mathf.RoundToInt(amount); // �Ҽ��� �����ϰ� ������ ����
		if (currentScore < 0) // 0 ���Ϸ� �������� �ʵ���
		{
			currentScore = 0;
		}
		UpdateScoreUI();
		Debug.Log("���� ����: " + amount + ", ���� ����: " + currentScore);
	}

	// UI Text ������Ʈ
	private void UpdateScoreUI()
	{
		if (scoreText != null)
		{
			scoreText.text = "Score: " + currentScore.ToString();
		}
	}
}