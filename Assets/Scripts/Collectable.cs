using UnityEngine;
using System.Collections; // Coroutine을 위해 필요한거

public class Collectable : MonoBehaviour
{
	[SerializeField] private string playerTag = "Ship";
	[SerializeField] private float scoreValue = 10f;
	[SerializeField] private float respawnTime = 5f; // 리스폰 시간 (초)

	[Header("시각 효과")]
	[SerializeField] private GameObject collectEffectPrefab; // 수집 시 재생될 파티클 프리팹
	[SerializeField] private GameObject respawnEffectPrefab; // 리스폰 시 재생될 파티클 프리팹

	private MeshRenderer meshRenderer; // 시각적 메시 (오브젝트를 숨기거나 보이게 할 때 사용)
	private Collider collectableCollider; // 충돌체 (충돌 감지를 활성화/비활성화할 때 사용)

	void Awake()
	{
		// 스크립트가 붙은 오브젝트에서 MeshRenderer와 Collider 컴포넌트를 가져옴
		meshRenderer = GetComponent<MeshRenderer>();
		collectableCollider = GetComponent<Collider>();

		// 만약 컴포넌트가 없으면 오류 로그를 남기고 스크립트를 비활성화
		if (meshRenderer == null)
		{
			Debug.LogError("Collectable 스크립트에 MeshRenderer 컴포넌트가 없습니다. 추가해주세요.");
			enabled = false;
		}
		if (collectableCollider == null)
		{
			Debug.LogError("Collectable 스크립트에 Collider 컴포넌트가 없습니다. 추가해주세요.");
			enabled = false;
		}
	}

	// 다른 오브젝트가 이 Collectable의 Trigger Collider에 진입했을 때 호출됨
	void OnTriggerEnter(Collider other)
	{
		Debug.Log("OnTriggerEnter called by: " + gameObject.name + " with: " + other.gameObject.name);
		// 충돌한 오브젝트의 태그가 플레이어 선박의 태그와 일치하는지 확인함
		if (other.CompareTag(playerTag))
		{
			// ScoreManager의 인스턴스를 통해 점수를 추가한다
			// ScoreManager.Instance가 씬에 없거나 할당되지 않ㄴ았다면 null 참조 오류가 발생할 수 있음
			if (ScoreManager.Instance != null)
			{
				ScoreManager.Instance.AddScore(scoreValue);
			}
			else
			{
				Debug.LogWarning("ScoreManager.Instance가 씬에 없습니다. 점수 추가 실패.");
			}

			// Collectable을 비활성화 (수집된 것처럼 보이게)
			Collect();
		}
	}

	// Collectable을 수집했을 때 호출되는 함수
	private void Collect()
	{
		if (meshRenderer != null)
		{
			meshRenderer.enabled = false; // 시각적으로 숨김
		}
		if (collectableCollider != null)
		{
			collectableCollider.enabled = false; // 충돌 감지 비활성화
		}

		// 수집 효과 재생
		if (collectEffectPrefab != null)
		{
			// 이펙트를 현재 캡슐 위치에 생성
			GameObject effect = Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
			// 이펙트 재생이 끝나면 자동으로 파괴되도록 설정
			ParticleSystem ps = effect.GetComponent<ParticleSystem>();
			if (ps != null)
			{
				Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
			}
			else
			{
				// 파티클 시스템이 없으면 적당한 시간 후 파괴
				Destroy(effect, 2f);
			}
		}

		// 리스폰 코루틴 시작
		StartCoroutine(RespawnRoutine());
	}

	// Collectable이 일정 시간 후 다시 나타나도록 하는 코루틴
	IEnumerator RespawnRoutine()
	{
		// 10초 동안 대기.
		yield return new WaitForSeconds(10f); // respawnTime 변수 대신 직접 10f로 변경 또는 respawnTime 유지 가능

		// Collectable을 다시 활성화 (시각적 및 충돌 감지)
		if (meshRenderer != null)
		{
			meshRenderer.enabled = true;
		}
		if (collectableCollider != null)
		{
			collectableCollider.enabled = true;
		}

		// 리스폰 효과 재생
		if (respawnEffectPrefab != null)
		{
			GameObject effect = Instantiate(respawnEffectPrefab, transform.position, Quaternion.identity);
			ParticleSystem ps = effect.GetComponent<ParticleSystem>();
			if (ps != null)
			{
				Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
			}
			else
			{
				Destroy(effect, 2f);
			}
		}

		Debug.Log(gameObject.name + " 리스폰되었습니다.");
	}

}