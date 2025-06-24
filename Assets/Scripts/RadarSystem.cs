using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RadarSystem : MonoBehaviour
{
	[Header("레이더 설정")]
	[SerializeField] private Transform shipTransform; // 플레이어 선박의 Transform
	[SerializeField] private Transform rayOriginTransform; // 레이(Ray)가 발사될 선박 내의 지점 Transform
	[SerializeField] private RectTransform radarUIArea; // 레이더 UI가 표시될 배경 이미지 (RectTransform
	[SerializeField] private float radarRange = 25f; // 레이더가 탐지할 수 있는 최대 거리
	[SerializeField] private int horizontalRayCount = 30; // 수평 방향으로 발사할 레이의 개수
	[SerializeField] private float horizontalScanAngle = 120f; // 수평 레이 스캔 각도 (발사각도)
	[SerializeField] private int verticalRayCount = 5; // 수직 방향(Y축)으로 발사할 레이의 개수
	[SerializeField] private float verticalScanAngle = 30f; // 수직 레이 스캔 각도 (위아래 각도)
	[SerializeField] private float scanInterval = 0.5f; // 레이더 스캔을 다시 하는 주기 (초)

	[Header("레이어 설정")]
	[SerializeField] private LayerMask obstacleLayer; // 장애물이 속한 레이어
	[SerializeField] private LayerMask collectableLayer; // 공구(아이템)가 속한 레이어

	[Header("UI 표시 설정")]
	[SerializeField] private GameObject radarDotPrefab; // 레이더에 표시될 점(닷) 프리팹
	[SerializeField] private float dotSize = 5f; // 레이더 점의 크기
	[SerializeField] private Color obstacleDotColor = Color.grey; // 장애물 색상 
	[SerializeField] private Color collectableDotColor = Color.yellow; // 공구 색상

	private List<GameObject> activeDots = new List<GameObject>(); // 현재 레이더에 표시된 모든 점들을 관리하는 리스트

	void Awake()
	{
		// 스크립트 작동에 필요한 오브젝트들이 인스펙터에 할당되었는지 확인
		if (shipTransform == null || rayOriginTransform == null || radarUIArea == null || radarDotPrefab == null)
		{
			//Debug.LogError("레이더 시스템: 필수 컴포넌트 할당 오류. 인스펙터의 모든 필드를 확인요망.");
			enabled = false; // 스크립트 비활성화
			return;
		}
		// 레이더 스캔을 주기적으로 시작하는 코루틴 실행
		StartCoroutine(ScanRadarRoutine());
	}

	// 레이더 스캔을 정해진 주기로 반복하는 코루틴
	IEnumerator ScanRadarRoutine()
	{
		while (true) // 이 루프는 게임이 실행되는 동안 계속 반복됨
		{
			ClearRadarDots(); // 이전 스캔에서 만들어진 모든 점들을 제거
			ScanRadar(); // 새로운 스캔을 수행하여 점들을 다시 그림
			yield return new WaitForSeconds(scanInterval); // 다음 스캔까지 잠시 대기
		}
	}

	// 실제 레이더 스캔 로직을 수행하는 함수
	private void ScanRadar()
	{
		// 수평 스캔 각도 계산 (선박의 전방을 기준으로 좌우)
		float horizontalStartAngle = -horizontalScanAngle / 2f;
		float horizontalAngleStep = horizontalScanAngle / (horizontalRayCount - 1);

		// 수직 스캔 각도 계산 (Y축, 위아래)
		float verticalStartAngle = -verticalScanAngle / 2f;
		// verticalRayCount가 1인 경우 0으로 나누는 것을 방지 (레이가 한 줄일 때는 각도 변화 없음)
		float verticalAngleStep = verticalScanAngle / (verticalRayCount > 1 ? verticalRayCount - 1 : 1);

		// 레이가 발사될 시작 지점 (선박 앞머리에 설정한 RayOriginPoint의 월드 위치)
		Vector3 rayStartPoint = rayOriginTransform.position;

		// 모든 레이를 발사하여 주변 오브젝트를 감지
		// 클린코드를 보면.. 매직넘버를 쓰지 말라는 말이 있다.
		// 하지만 코드를 볼 사람이 없기때문에 쓰기
		for (int h = 0; h < horizontalRayCount; h++) // 수평 방향 레이 반복
		{
			float currentHorizontalAngle = horizontalStartAngle + h * horizontalAngleStep;

			for (int v = 0; v < verticalRayCount; v++) // 수직 방향 레이 반복
			{
				float currentVerticalAngle = verticalStartAngle + v * verticalAngleStep;

				// 레이의 방향 계산-> 수평 회전과 수직 회전을 조합
				// Rayoriginpoint 의 '앞 '방향을 기준으로 회전시킴
				Quaternion horizontalRotation = Quaternion.AngleAxis(currentHorizontalAngle, rayOriginTransform.up); // Y축 회전
				Quaternion verticalRotation = Quaternion.AngleAxis(currentVerticalAngle, rayOriginTransform.right); // X축 회전

				// 최종 레이의 발사 방향
				Vector3 rayDirection = horizontalRotation * verticalRotation * rayOriginTransform.forward;

				RaycastHit hit; // 레이 충돌 정보를 저장할 변수

				// Physics.Raycast: 레이를 발사하여 충돌 여부 확인
				// rayStartPoint: 레이의 시작 지점
				// rayDirection: 레이의 방향
				// out hit: 충돌 정보가 저장될 변수
				// radarRange: 레이의 최대 길이
				// obstacleLayer | collectableLayer: 레이가 감지할 레이어 마스크 (장애물 또는 공구 레이어 모두 감지)
				if (Physics.Raycast(rayStartPoint, rayDirection, out hit, radarRange, obstacleLayer | collectableLayer))
				{
					Color dotDisplayColor; // 이 레이에 감지된 점의 색상

					// 충돌한 오브젝트의 태그를 확인하여 점의 색상을 결정
					if (hit.collider.CompareTag("Collectable"))
					{
						dotDisplayColor = collectableDotColor; // 공구는 노란색
					}
					else if (hit.collider.CompareTag("Obstacle")) // 장애물 태그 확인
					{
						dotDisplayColor = obstacleDotColor; // 장애물은 회색 또는 붉은색
					}
					else
					{
						// 만약 예상치 못한 다른 태그의 오브젝트가 감지되면 기본 색상으로
						dotDisplayColor = Color.white;
					}

					// 감지된 물체의 위치를 레이더 UI에 점으로 표시하는 함수 호출
					DisplayHitOnRadar(hit.point, dotDisplayColor);
				}

				// Scene 뷰에서 디버그용으로 레이의 경로를 시각화함 (게임 화면에는 안보임)
				Debug.DrawRay(rayStartPoint, rayDirection * radarRange, Color.cyan, scanInterval);
			}
		}
	}

	// 감지된 물체의 월드 위치(hitPoint)와 표시할 색상(displayColor)을 받아
	// 레이더 UI에 점으로 표시하는 함수
	private void DisplayHitOnRadar(Vector3 hitPoint, Color displayColor)
	{
		// 1. 선박의 중심을 기준으로 한 상대적인 월드 좌표상의 위치를 계산.
		//    => 미니맵의 중심이 선박의 위치라는 것을 의미
		Vector3 relativePosWorld = hitPoint - shipTransform.position;

		// 2. 선박의 현재 회전을 역으로 적용해 "선박의 앞 방향이 레이더 UI의 위쪽"이 되도록
		//    상대 위치를 레이더 UI 공간으로 변환합니다.
		Vector3 relativePosRadar = Quaternion.Inverse(shipTransform.rotation) * relativePosWorld;

		// 3. 레이더 탐지 거리(radarRange)를 기준으로 노멀라이즈된 좌표를 계산.
		//    (3D 월드의 XZ 평면을 UI의 XY 평면에 매핑)
		float normalizedX = relativePosRadar.x / radarRange; // -1.0 ~ 1.0 범위
		float normalizedY = relativePosRadar.z / radarRange; // -1.0 ~ 1.0 범위 (3D의 Z축이 ui의 Y축이 됨)

		// 4. 노멀라이즈된 좌표를 실제 레이더 UI 영역(radarUIArea)의 픽셀 좌표로 변환함
		float radarWidth = radarUIArea.sizeDelta.x;
		float radarHeight = radarUIArea.sizeDelta.y;

		float uiX = normalizedX * (radarWidth / 2f);
		float uiY = normalizedY * (radarHeight / 2f);

		// 5. 레이더 점 프리팹을 인스턴스화하고 레이더 UI 영역의 자식으로 배치
		GameObject dot = Instantiate(radarDotPrefab, radarUIArea.transform);
		RectTransform dotRect = dot.GetComponent<RectTransform>();
		dotRect.anchoredPosition = new Vector2(uiX, uiY); // 계산된 픽셀 좌표 설정
		dotRect.sizeDelta = new Vector2(dotSize, dotSize); // 점의 크기 설정

		// 점의 image 컴포넌트가 있다면 색상을 설정하거 표시
		Image dotImage = dot.GetComponent<Image>();
		if (dotImage != null)
		{
			dotImage.color = displayColor; // 전달받은 색상으로 점 색상 설정
		}

		activeDots.Add(dot); // 나중에 점들을 제거하기 위해 리스트에 추가함
	}

	// 이전에 생성된 모든 레이더 점들을 제거하는 함수
	private void ClearRadarDots()
	{
		foreach (GameObject dot in activeDots)
		{
			Destroy(dot); // 각 점 오브젝트 파괴
		}
		activeDots.Clear(); // 리스트를 비움
	}
}