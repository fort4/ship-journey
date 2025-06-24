using UnityEngine;
using System.Collections; // Coroutine�� ���� �ʿ��Ѱ�

public class Collectable : MonoBehaviour
{
	[SerializeField] private string playerTag = "Ship";
	[SerializeField] private float scoreValue = 10f;
	[SerializeField] private float respawnTime = 5f; // ������ �ð� (��)

	[Header("�ð� ȿ��")]
	[SerializeField] private GameObject collectEffectPrefab; // ���� �� ����� ��ƼŬ ������
	[SerializeField] private GameObject respawnEffectPrefab; // ������ �� ����� ��ƼŬ ������

	private MeshRenderer meshRenderer; // �ð��� �޽� (������Ʈ�� ����ų� ���̰� �� �� ���)
	private Collider collectableCollider; // �浹ü (�浹 ������ Ȱ��ȭ/��Ȱ��ȭ�� �� ���)

	void Awake()
	{
		// ��ũ��Ʈ�� ���� ������Ʈ���� MeshRenderer�� Collider ������Ʈ�� ������
		meshRenderer = GetComponent<MeshRenderer>();
		collectableCollider = GetComponent<Collider>();

		// ���� ������Ʈ�� ������ ���� �α׸� ����� ��ũ��Ʈ�� ��Ȱ��ȭ
		if (meshRenderer == null)
		{
			Debug.LogError("Collectable ��ũ��Ʈ�� MeshRenderer ������Ʈ�� �����ϴ�. �߰����ּ���.");
			enabled = false;
		}
		if (collectableCollider == null)
		{
			Debug.LogError("Collectable ��ũ��Ʈ�� Collider ������Ʈ�� �����ϴ�. �߰����ּ���.");
			enabled = false;
		}
	}

	// �ٸ� ������Ʈ�� �� Collectable�� Trigger Collider�� �������� �� ȣ���
	void OnTriggerEnter(Collider other)
	{
		Debug.Log("OnTriggerEnter called by: " + gameObject.name + " with: " + other.gameObject.name);
		// �浹�� ������Ʈ�� �±װ� �÷��̾� ������ �±׿� ��ġ�ϴ��� Ȯ����
		if (other.CompareTag(playerTag))
		{
			// ScoreManager�� �ν��Ͻ��� ���� ������ �߰��Ѵ�
			// ScoreManager.Instance�� ���� ���ų� �Ҵ���� �ʤ��Ҵٸ� null ���� ������ �߻��� �� ����
			if (ScoreManager.Instance != null)
			{
				ScoreManager.Instance.AddScore(scoreValue);
			}
			else
			{
				Debug.LogWarning("ScoreManager.Instance�� ���� �����ϴ�. ���� �߰� ����.");
			}

			// Collectable�� ��Ȱ��ȭ (������ ��ó�� ���̰�)
			Collect();
		}
	}

	// Collectable�� �������� �� ȣ��Ǵ� �Լ�
	private void Collect()
	{
		if (meshRenderer != null)
		{
			meshRenderer.enabled = false; // �ð������� ����
		}
		if (collectableCollider != null)
		{
			collectableCollider.enabled = false; // �浹 ���� ��Ȱ��ȭ
		}

		// ���� ȿ�� ���
		if (collectEffectPrefab != null)
		{
			// ����Ʈ�� ���� ĸ�� ��ġ�� ����
			GameObject effect = Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
			// ����Ʈ ����� ������ �ڵ����� �ı��ǵ��� ����
			ParticleSystem ps = effect.GetComponent<ParticleSystem>();
			if (ps != null)
			{
				Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
			}
			else
			{
				// ��ƼŬ �ý����� ������ ������ �ð� �� �ı�
				Destroy(effect, 2f);
			}
		}

		// ������ �ڷ�ƾ ����
		StartCoroutine(RespawnRoutine());
	}

	// Collectable�� ���� �ð� �� �ٽ� ��Ÿ������ �ϴ� �ڷ�ƾ
	IEnumerator RespawnRoutine()
	{
		// 10�� ���� ���.
		yield return new WaitForSeconds(10f); // respawnTime ���� ��� ���� 10f�� ���� �Ǵ� respawnTime ���� ����

		// Collectable�� �ٽ� Ȱ��ȭ (�ð��� �� �浹 ����)
		if (meshRenderer != null)
		{
			meshRenderer.enabled = true;
		}
		if (collectableCollider != null)
		{
			collectableCollider.enabled = true;
		}

		// ������ ȿ�� ���
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

		Debug.Log(gameObject.name + " �������Ǿ����ϴ�.");
	}

}