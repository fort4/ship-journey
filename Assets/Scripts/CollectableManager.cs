using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
	public static CollectableManager Instance { get; private set; } // 싱글톤 패턴

	// 모든 Collectable들의 초기 위치를 저장 (리스폰 시 원래 위치로)
	private Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();

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

		// 씬의 모든 Collectable 오브젝트를 찾아서 초기 위치 저장
		Collectable[] allCollectables = FindObjectsOfType<Collectable>();
		foreach (Collectable col in allCollectables)
		{
			initialPositions[col.gameObject] = col.transform.position;
		}
	}

	// Collectable이 파괴되었을 때 호출하여 리스폰을 예약하는 함수 (Collectable.cs에서 Destroy 대신 사용)
	// 현재는 Collectable.cs 자체에서 RespawnRoutine을 돌리므로 필요없음
	public void RequestRespawn(GameObject collectableObject, float time)
	{
		StartCoroutine(RespawnCollectableRoutine(collectableObject, time));
	}

	private IEnumerator RespawnCollectableRoutine(GameObject collectableObject, float time)
	{
		// collectableObject.SetActive(false); // 오브젝트 전체를 비활성화하는 경우

		yield return new WaitForSeconds(time);

		// collectableObject.SetActive(true); // 오브젝트 전체를 활성화하는 경우
	}
}