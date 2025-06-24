using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
	[Header("ī�޶� Ÿ��")]
	[SerializeField] private Transform target; // ī�޶� ���� ����
	[SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f); // ���� ���� ������

	[Header("ī�޶� ȸ��")]
	[SerializeField] private float rotationSpeed = 3f; // ���콺 ȸ�� �ӵ�
	[SerializeField] private float yMinLimit = -20f; // ���� ȸ�� �ּ� ����
	[SerializeField] private float yMaxLimit = 80f; // ���� ȸ�� �ִ� ����

	private float currentX = 0f;
	private float currentY = 0f;

	void Start()
	{
		if (target == null)
		{
			Debug.LogError("ThirdPersonCamera ��ũ��Ʈ�� Ÿ��(Target)�� �Ҵ���� �ʾ���!.");
			enabled = false;
			return;
		}

		// �ʱ� ī�޶� ��ġ �� ȸ�� ����
		Vector3 initialPosition = target.position + target.TransformDirection(offset);
		transform.position = initialPosition;
		transform.LookAt(target.position);

		// ���� ȸ�� �� �ʱ�ȭ (ī�޶��� ���� ���Ϸ� �������� ������)
		Vector3 angles = transform.eulerAngles;
		currentX = angles.y;
		currentY = angles.x;

		// ���콺 Ŀ�� ����� �� ���
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void LateUpdate()
	{
		// ���콺 �Է����� ī�޶� ȸ��
		currentX += Input.GetAxis("Mouse X") * rotationSpeed;
		currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
		currentY = ClampAngle(currentY, yMinLimit, yMaxLimit);

		Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

		// Ÿ���� ��ġ + �������� ������� ī�޶� ��ġ ���
		// offset�� ���� ��ǥ��� ����Ǿ�� ������ ȸ���ص� �������� ������
		Vector3 desiredPosition = target.position + rotation * offset;

		transform.position = desiredPosition;
		transform.LookAt(target.position); // �׻� Ÿ���� �ٶ󺸵��� ����
	}

	// ���� ���� �Լ�
	private float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}

	void OnDisable()
	{
		// ��ũ��Ʈ ��Ȱ��ȭ �� ���콺 Ŀ�� �ٽ� ���̰� ��
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
}