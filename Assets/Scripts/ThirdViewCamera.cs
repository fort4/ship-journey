using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
	[Header("카메라 타겟")]
	[SerializeField] private Transform target; // 카메라가 따라갈 선박
	[SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f); // 선박 기준 오프셋

	[Header("카메라 회전")]
	[SerializeField] private float rotationSpeed = 3f; // 마우스 회전 속도
	[SerializeField] private float yMinLimit = -20f; // 수직 회전 최소 각도
	[SerializeField] private float yMaxLimit = 80f; // 수직 회전 최대 각도

	private float currentX = 0f;
	private float currentY = 0f;

	void Start()
	{
		if (target == null)
		{
			Debug.LogError("ThirdPersonCamera 스크립트에 타겟(Target)이 할당되지 않았음!.");
			enabled = false;
			return;
		}

		// 초기 카메라 위치 및 회전 설정
		Vector3 initialPosition = target.position + target.TransformDirection(offset);
		transform.position = initialPosition;
		transform.LookAt(target.position);

		// 현재 회전 값 초기화 (카메라의 현재 오일러 각도에서 가져옴)
		Vector3 angles = transform.eulerAngles;
		currentX = angles.y;
		currentY = angles.x;

		// 마우스 커서 숨기기 및 잠금
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void LateUpdate()
	{
		// 마우스 입력으로 카메라 회전
		currentX += Input.GetAxis("Mouse X") * rotationSpeed;
		currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
		currentY = ClampAngle(currentY, yMinLimit, yMaxLimit);

		Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

		// 타겟의 위치 + 오프셋을 기반으로 카메라 위치 계산
		// offset은 로컬 좌표계로 적용되어야 선박이 회전해도 오프셋이 유지됨
		Vector3 desiredPosition = target.position + rotation * offset;

		transform.position = desiredPosition;
		transform.LookAt(target.position); // 항상 타겟을 바라보도록 설정
	}

	// 각도 제한 함수
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
		// 스크립트 비활성화 시 마우스 커서 다시 보이게 함
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
}