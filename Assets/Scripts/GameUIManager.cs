using UnityEngine;
using UnityEngine.SceneManagement; // for 씬 관리
using UnityEngine.UI; // for 버튼 컴포넌트

public class GameUIManager : MonoBehaviour
{
	// Restart 버튼을 여기에 연결
	[SerializeField] private Button restartButton;

	void Awake()
	{
		// restartButton이 Inspector에서 할당되었는지 확인
		if (restartButton == null)
		{
			Debug.LogError("GameUIManager: Restart Button이 할당되지 않았습니다. Inspector를 확인해주세요.");
			return; // 버튼이 없으면 스크립트 초기화 중단
		}

		// 버튼 클릭 시 RestartGame 함수를 호출하도록 리스너를 추가함
		restartButton.onClick.AddListener(RestartGame);
	}

	// 게임을 다시 시작하는 함수
	public void RestartGame()
	{
		// 현재 활성화된 씬의 이름을 가져옴
		string currentSceneName = SceneManager.GetActiveScene().name;

		// 현재 씬을 다시 로드!
		SceneManager.LoadScene(currentSceneName);

		Debug.Log("게임을 다시 시작합니다: " + currentSceneName);

		// 게임이 일시정지 상태였다면 다시 시작
		// Time.timeScale = 1f;
	}
}