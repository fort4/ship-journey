using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
	public static CollectableManager Instance { get; private set; } // �̱��� ����

	// ��� Collectable���� �ʱ� ��ġ�� ���� (������ �� ���� ��ġ��)
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

		// ���� ��� Collectable ������Ʈ�� ã�Ƽ� �ʱ� ��ġ ����
		Collectable[] allCollectables = FindObjectsOfType<Collectable>();
		foreach (Collectable col in allCollectables)
		{
			initialPositions[col.gameObject] = col.transform.position;
		}
	}

	// Collectable�� �ı��Ǿ��� �� ȣ���Ͽ� �������� �����ϴ� �Լ� (Collectable.cs���� Destroy ��� ���)
	// ����� Collectable.cs ��ü���� RespawnRoutine�� �����Ƿ� �ʿ����
	public void RequestRespawn(GameObject collectableObject, float time)
	{
		StartCoroutine(RespawnCollectableRoutine(collectableObject, time));
	}

	private IEnumerator RespawnCollectableRoutine(GameObject collectableObject, float time)
	{
		// collectableObject.SetActive(false); // ������Ʈ ��ü�� ��Ȱ��ȭ�ϴ� ���

		yield return new WaitForSeconds(time);

		// collectableObject.SetActive(true); // ������Ʈ ��ü�� Ȱ��ȭ�ϴ� ���
	}
}