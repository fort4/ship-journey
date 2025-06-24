using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RadarSystem : MonoBehaviour
{
	[Header("���̴� ����")]
	[SerializeField] private Transform shipTransform; // �÷��̾� ������ Transform
	[SerializeField] private Transform rayOriginTransform; // ����(Ray)�� �߻�� ���� ���� ���� Transform
	[SerializeField] private RectTransform radarUIArea; // ���̴� UI�� ǥ�õ� ��� �̹��� (RectTransform
	[SerializeField] private float radarRange = 25f; // ���̴��� Ž���� �� �ִ� �ִ� �Ÿ�
	[SerializeField] private int horizontalRayCount = 30; // ���� �������� �߻��� ������ ����
	[SerializeField] private float horizontalScanAngle = 120f; // ���� ���� ��ĵ ���� (�߻簢��)
	[SerializeField] private int verticalRayCount = 5; // ���� ����(Y��)���� �߻��� ������ ����
	[SerializeField] private float verticalScanAngle = 30f; // ���� ���� ��ĵ ���� (���Ʒ� ����)
	[SerializeField] private float scanInterval = 0.5f; // ���̴� ��ĵ�� �ٽ� �ϴ� �ֱ� (��)

	[Header("���̾� ����")]
	[SerializeField] private LayerMask obstacleLayer; // ��ֹ��� ���� ���̾�
	[SerializeField] private LayerMask collectableLayer; // ����(������)�� ���� ���̾�

	[Header("UI ǥ�� ����")]
	[SerializeField] private GameObject radarDotPrefab; // ���̴��� ǥ�õ� ��(��) ������
	[SerializeField] private float dotSize = 5f; // ���̴� ���� ũ��
	[SerializeField] private Color obstacleDotColor = Color.grey; // ��ֹ� ���� 
	[SerializeField] private Color collectableDotColor = Color.yellow; // ���� ����

	private List<GameObject> activeDots = new List<GameObject>(); // ���� ���̴��� ǥ�õ� ��� ������ �����ϴ� ����Ʈ

	void Awake()
	{
		// ��ũ��Ʈ �۵��� �ʿ��� ������Ʈ���� �ν����Ϳ� �Ҵ�Ǿ����� Ȯ��
		if (shipTransform == null || rayOriginTransform == null || radarUIArea == null || radarDotPrefab == null)
		{
			//Debug.LogError("���̴� �ý���: �ʼ� ������Ʈ �Ҵ� ����. �ν������� ��� �ʵ带 Ȯ�ο��.");
			enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
			return;
		}
		// ���̴� ��ĵ�� �ֱ������� �����ϴ� �ڷ�ƾ ����
		StartCoroutine(ScanRadarRoutine());
	}

	// ���̴� ��ĵ�� ������ �ֱ�� �ݺ��ϴ� �ڷ�ƾ
	IEnumerator ScanRadarRoutine()
	{
		while (true) // �� ������ ������ ����Ǵ� ���� ��� �ݺ���
		{
			ClearRadarDots(); // ���� ��ĵ���� ������� ��� ������ ����
			ScanRadar(); // ���ο� ��ĵ�� �����Ͽ� ������ �ٽ� �׸�
			yield return new WaitForSeconds(scanInterval); // ���� ��ĵ���� ��� ���
		}
	}

	// ���� ���̴� ��ĵ ������ �����ϴ� �Լ�
	private void ScanRadar()
	{
		// ���� ��ĵ ���� ��� (������ ������ �������� �¿�)
		float horizontalStartAngle = -horizontalScanAngle / 2f;
		float horizontalAngleStep = horizontalScanAngle / (horizontalRayCount - 1);

		// ���� ��ĵ ���� ��� (Y��, ���Ʒ�)
		float verticalStartAngle = -verticalScanAngle / 2f;
		// verticalRayCount�� 1�� ��� 0���� ������ ���� ���� (���̰� �� ���� ���� ���� ��ȭ ����)
		float verticalAngleStep = verticalScanAngle / (verticalRayCount > 1 ? verticalRayCount - 1 : 1);

		// ���̰� �߻�� ���� ���� (���� �ոӸ��� ������ RayOriginPoint�� ���� ��ġ)
		Vector3 rayStartPoint = rayOriginTransform.position;

		// ��� ���̸� �߻��Ͽ� �ֺ� ������Ʈ�� ����
		// Ŭ���ڵ带 ����.. �����ѹ��� ���� ����� ���� �ִ�.
		// ������ �ڵ带 �� ����� ���⶧���� ����
		for (int h = 0; h < horizontalRayCount; h++) // ���� ���� ���� �ݺ�
		{
			float currentHorizontalAngle = horizontalStartAngle + h * horizontalAngleStep;

			for (int v = 0; v < verticalRayCount; v++) // ���� ���� ���� �ݺ�
			{
				float currentVerticalAngle = verticalStartAngle + v * verticalAngleStep;

				// ������ ���� ���-> ���� ȸ���� ���� ȸ���� ����
				// Rayoriginpoint �� '�� '������ �������� ȸ����Ŵ
				Quaternion horizontalRotation = Quaternion.AngleAxis(currentHorizontalAngle, rayOriginTransform.up); // Y�� ȸ��
				Quaternion verticalRotation = Quaternion.AngleAxis(currentVerticalAngle, rayOriginTransform.right); // X�� ȸ��

				// ���� ������ �߻� ����
				Vector3 rayDirection = horizontalRotation * verticalRotation * rayOriginTransform.forward;

				RaycastHit hit; // ���� �浹 ������ ������ ����

				// Physics.Raycast: ���̸� �߻��Ͽ� �浹 ���� Ȯ��
				// rayStartPoint: ������ ���� ����
				// rayDirection: ������ ����
				// out hit: �浹 ������ ����� ����
				// radarRange: ������ �ִ� ����
				// obstacleLayer | collectableLayer: ���̰� ������ ���̾� ����ũ (��ֹ� �Ǵ� ���� ���̾� ��� ����)
				if (Physics.Raycast(rayStartPoint, rayDirection, out hit, radarRange, obstacleLayer | collectableLayer))
				{
					Color dotDisplayColor; // �� ���̿� ������ ���� ����

					// �浹�� ������Ʈ�� �±׸� Ȯ���Ͽ� ���� ������ ����
					if (hit.collider.CompareTag("Collectable"))
					{
						dotDisplayColor = collectableDotColor; // ������ �����
					}
					else if (hit.collider.CompareTag("Obstacle")) // ��ֹ� �±� Ȯ��
					{
						dotDisplayColor = obstacleDotColor; // ��ֹ��� ȸ�� �Ǵ� ������
					}
					else
					{
						// ���� ����ġ ���� �ٸ� �±��� ������Ʈ�� �����Ǹ� �⺻ ��������
						dotDisplayColor = Color.white;
					}

					// ������ ��ü�� ��ġ�� ���̴� UI�� ������ ǥ���ϴ� �Լ� ȣ��
					DisplayHitOnRadar(hit.point, dotDisplayColor);
				}

				// Scene �信�� ����׿����� ������ ��θ� �ð�ȭ�� (���� ȭ�鿡�� �Ⱥ���)
				Debug.DrawRay(rayStartPoint, rayDirection * radarRange, Color.cyan, scanInterval);
			}
		}
	}

	// ������ ��ü�� ���� ��ġ(hitPoint)�� ǥ���� ����(displayColor)�� �޾�
	// ���̴� UI�� ������ ǥ���ϴ� �Լ�
	private void DisplayHitOnRadar(Vector3 hitPoint, Color displayColor)
	{
		// 1. ������ �߽��� �������� �� ������� ���� ��ǥ���� ��ġ�� ���.
		//    => �̴ϸ��� �߽��� ������ ��ġ��� ���� �ǹ�
		Vector3 relativePosWorld = hitPoint - shipTransform.position;

		// 2. ������ ���� ȸ���� ������ ������ "������ �� ������ ���̴� UI�� ����"�� �ǵ���
		//    ��� ��ġ�� ���̴� UI �������� ��ȯ�մϴ�.
		Vector3 relativePosRadar = Quaternion.Inverse(shipTransform.rotation) * relativePosWorld;

		// 3. ���̴� Ž�� �Ÿ�(radarRange)�� �������� ��ֶ������ ��ǥ�� ���.
		//    (3D ������ XZ ����� UI�� XY ��鿡 ����)
		float normalizedX = relativePosRadar.x / radarRange; // -1.0 ~ 1.0 ����
		float normalizedY = relativePosRadar.z / radarRange; // -1.0 ~ 1.0 ���� (3D�� Z���� ui�� Y���� ��)

		// 4. ��ֶ������ ��ǥ�� ���� ���̴� UI ����(radarUIArea)�� �ȼ� ��ǥ�� ��ȯ��
		float radarWidth = radarUIArea.sizeDelta.x;
		float radarHeight = radarUIArea.sizeDelta.y;

		float uiX = normalizedX * (radarWidth / 2f);
		float uiY = normalizedY * (radarHeight / 2f);

		// 5. ���̴� �� �������� �ν��Ͻ�ȭ�ϰ� ���̴� UI ������ �ڽ����� ��ġ
		GameObject dot = Instantiate(radarDotPrefab, radarUIArea.transform);
		RectTransform dotRect = dot.GetComponent<RectTransform>();
		dotRect.anchoredPosition = new Vector2(uiX, uiY); // ���� �ȼ� ��ǥ ����
		dotRect.sizeDelta = new Vector2(dotSize, dotSize); // ���� ũ�� ����

		// ���� image ������Ʈ�� �ִٸ� ������ �����ϰ� ǥ��
		Image dotImage = dot.GetComponent<Image>();
		if (dotImage != null)
		{
			dotImage.color = displayColor; // ���޹��� �������� �� ���� ����
		}

		activeDots.Add(dot); // ���߿� ������ �����ϱ� ���� ����Ʈ�� �߰���
	}

	// ������ ������ ��� ���̴� ������ �����ϴ� �Լ�
	private void ClearRadarDots()
	{
		foreach (GameObject dot in activeDots)
		{
			Destroy(dot); // �� �� ������Ʈ �ı�
		}
		activeDots.Clear(); // ����Ʈ�� ���
	}
}