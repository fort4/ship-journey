using UnityEngine;
using UnityEngine.SceneManagement; // for �� ����
using UnityEngine.UI; // for ��ư ������Ʈ

public class GameUIManager : MonoBehaviour
{
	// Restart ��ư�� ���⿡ ����
	[SerializeField] private Button restartButton;

	void Awake()
	{
		// restartButton�� Inspector���� �Ҵ�Ǿ����� Ȯ��
		if (restartButton == null)
		{
			Debug.LogError("GameUIManager: Restart Button�� �Ҵ���� �ʾҽ��ϴ�. Inspector�� Ȯ�����ּ���.");
			return; // ��ư�� ������ ��ũ��Ʈ �ʱ�ȭ �ߴ�
		}

		// ��ư Ŭ�� �� RestartGame �Լ��� ȣ���ϵ��� �����ʸ� �߰���
		restartButton.onClick.AddListener(RestartGame);
	}

	// ������ �ٽ� �����ϴ� �Լ�
	public void RestartGame()
	{
		// ���� Ȱ��ȭ�� ���� �̸��� ������
		string currentSceneName = SceneManager.GetActiveScene().name;

		// ���� ���� �ٽ� �ε�!
		SceneManager.LoadScene(currentSceneName);

		Debug.Log("������ �ٽ� �����մϴ�: " + currentSceneName);

		// ������ �Ͻ����� ���¿��ٸ� �ٽ� ����
		// Time.timeScale = 1f;
	}
}