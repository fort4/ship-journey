using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[Header("카메라 설정")]
	[SerializeField] private Transform target; // 카메라가 따라갈 대상 (선박)
	[SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f); // 선박 기준 오프셋
	[SerializeField] private float smoothSpeed = 5f; // 카메라 이동 부드러움 정도

	[Header("시선 설정")]
	[SerializeField] private float lookAtHeightOffset = 1f; // 선박보다 약간 위를 바라보게 함

	void FixedUpdate()
	{
		if (target == null)
		{
			Debug.LogWarning("CameraFollow 스크립트에 대상(Target)이 설정되지 않았습니다.");
			return;
		}

		// 목표 위치 계산: 대상 위치 + 오프셋
		Vector3 desiredPosition = target.position + target.TransformDirection(offset);
		// TransformDirection을 사용해서 오프셋이 대상의 로컬 좌표계를 따르도록 함
		// -> 선박이 회전하면 카메라 오프셋도 같이 회전하여 선박 뒤에서 보이게 됨

		// 현재 카메라 위치에서 목표 위치로 부드럽게 이동
		Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.fixedDeltaTime);
		transform.position = smoothedPosition;

		// 카메라가 선박을 바라보도록 설정
		// 선박의 중심보다는 약간 위를 바라보게 해서 시야를 개선해야됨
		Vector3 lookAtTarget = target.position + Vector3.up * lookAtHeightOffset;
		transform.LookAt(lookAtTarget);
	}
}