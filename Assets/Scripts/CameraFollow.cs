using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[Header("ī�޶� ����")]
	[SerializeField] private Transform target; // ī�޶� ���� ��� (����)
	[SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f); // ���� ���� ������
	[SerializeField] private float smoothSpeed = 5f; // ī�޶� �̵� �ε巯�� ����

	[Header("�ü� ����")]
	[SerializeField] private float lookAtHeightOffset = 1f; // ���ں��� �ణ ���� �ٶ󺸰� ��

	void FixedUpdate()
	{
		if (target == null)
		{
			Debug.LogWarning("CameraFollow ��ũ��Ʈ�� ���(Target)�� �������� �ʾҽ��ϴ�.");
			return;
		}

		// ��ǥ ��ġ ���: ��� ��ġ + ������
		Vector3 desiredPosition = target.position + target.TransformDirection(offset);
		// TransformDirection�� ����ؼ� �������� ����� ���� ��ǥ�踦 �������� ��
		// -> ������ ȸ���ϸ� ī�޶� �����µ� ���� ȸ���Ͽ� ���� �ڿ��� ���̰� ��

		// ���� ī�޶� ��ġ���� ��ǥ ��ġ�� �ε巴�� �̵�
		Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.fixedDeltaTime);
		transform.position = smoothedPosition;

		// ī�޶� ������ �ٶ󺸵��� ����
		// ������ �߽ɺ��ٴ� �ణ ���� �ٶ󺸰� �ؼ� �þ߸� �����ؾߵ�
		Vector3 lookAtTarget = target.position + Vector3.up * lookAtHeightOffset;
		transform.LookAt(lookAtTarget);
	}
}