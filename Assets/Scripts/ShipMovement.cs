using UnityEngine;

public class ShipMovement : MonoBehaviour
{
	[Header("선박 물리 설정")]
	[SerializeField] private float forwardForce = 200f; // 전진 추진력
	[SerializeField] private float rotationSpeed = 60f; // 회전 토크
	[SerializeField] private float linearDragCoefficient = 1f; // 선형 저항 계수 (속도에 비례)
	[SerializeField] private float angularDragCoefficient = 3.3f; // 회전 저항 계수 (각속도에 비례)
	[SerializeField] private float maxSpeed = 20f; // 최대 속도 제한
	[SerializeField] private float accelerationMultiplier = 2f; // 가속 시 전진 추진력 배율
	[SerializeField] private float acceleratedMaxSpeed = 40f; // 가속 시 최대 속도

	[Header("충돌 설정")]
	[SerializeField] private float collisionSpeedReductionFactor = 0.5f; // 충돌 시 속도 감소율
	[SerializeField] private string obstacleTag = "Obstacle"; // 장애물 태그

	[Header("물 튀김 효과")]
	[SerializeField] private GameObject waterSplashPrefab; // 물 튀김 파티클 프리팹
	[SerializeField] private Transform[] splashPoints; // 물 튀김이 발생할 위치
	[SerializeField] private float minSpeedForSplash = 1.0f; // 이 속도 이상일 때 물 튀김 발생
	[SerializeField] private float minAngularSpeedForSplash = 0.5f; // 이 회전 속도 이상일 때 물 튀김 발생
	[SerializeField] private float splashInterval = 0.2f; // 물 튀김 효과 재생 간격 (너무 자주 튀는 거 방지)

	//[Header("자동 복구 설정")]
	//[SerializeField] private float autoRecoveryThreshold = 45f; // 이 각도 이상 기울어지면 복구 시작 (도)
	//[SerializeField] private float autoRecoverySpeed = 2f; // 복구 속도
	//[SerializeField] private float autoRecoveryAngularDrag = 5f; // 복구 중 회전 저항 증가

	private Rigidbody rb;
	private float lastSplashTime = 0f; // 마지막 물 튀김 발생 시간
	//private Quaternion initialRotation; // 선박의 초기 회전 (수평 상태)

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			Debug.LogError("선박에 Rigidbody 컴포넌트가 없습니다. 추가해주세요.");
			enabled = false;
			return;
		}
		rb.drag = 0f;
		rb.angularDrag = 0f;
		//initialRotation = transform.rotation; // 선박의 초기 회전 저장 (수평 상태)
		Application.targetFrameRate = 40; // webgl 성능이 구려서 일단 설정해둠

		// 물 튀김 지점 배열이 비어있으면 경고
		if (splashPoints == null || splashPoints.Length == 0)
		{
			Debug.LogWarning("WaterSplashEffect: 물 튀김 지점(Splash Points)이 할당되지 않았습니다. 선박의 자식으로 빈 GameObject를 만들고 할당해주세요.");
		}
	}

	void FixedUpdate()
	{
		HandleInput();
		ApplyDragForces();
		LimitSpeed();
		// 물 튀김 효과 로직 추가
		HandleWaterSplash();
		// 자동 복구 로직 추가
		//HandleAutoRecovery();
	}

	private void HandleInput()
	{
		float verticalInput = Input.GetAxis("Vertical");
		float horizontalInput = Input.GetAxis("Horizontal");

		// 가속 상태 확인
		float currentForwardForce = forwardForce;
		//float currentMaxSpeed = maxSpeed; // LimitSppeed() 내로 이동시킴. 여기 안적고

		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			currentForwardForce *= accelerationMultiplier; // 추진력 증가
		  //currentMaxSpeed = acceleratedMaxSpeed; // 최대 속도 증가 // 얘도 limitSpeed() 내로 이동시킴.
		}

		// 전진/후진 힘 적용 (선박의 현재 앞 방향으로)
		// 수정: forwardForce 대신 currentForwardForce 사용
		rb.AddForce(transform.forward * verticalInput * currentForwardForce, ForceMode.Acceleration);

		// 회전 토크 적용 (Y축을 기준)
		rb.AddTorque(Vector3.up * horizontalInput * rotationSpeed, ForceMode.Acceleration);

		// LimitSpeed() 함수에 현재 적용될 최대 속도 전달
		//LimitSpeed(currentMaxSpeed);
	}

	private void ApplyDragForces()
	{
		// 선형 저항 (속도에 비례)
		Vector3 currentVelocity = rb.velocity;
		Vector3 dragForce = -currentVelocity * linearDragCoefficient * currentVelocity.magnitude;
		rb.AddForce(dragForce, ForceMode.Acceleration);

		// 회전 저항 (각속도에 비례)
		Vector3 currentAngularVelocity = rb.angularVelocity;
		Vector3 angularDragForce = -currentAngularVelocity * angularDragCoefficient * currentAngularVelocity.magnitude;
		rb.AddTorque(angularDragForce, ForceMode.Acceleration);
	}

	// private void LimitSpeed(float limit)에서 인자빼고 수정함
	private void LimitSpeed() // 인자 제거
	{
		// 현재 적용할 최대 속도 결정
		float currentMaxSpeedLimit = maxSpeed; // 기본은 일반 최대 속도

		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			currentMaxSpeedLimit = acceleratedMaxSpeed; // Shift 키를 누르면 가속 최대 속도 적용
		}

		// 최대 속도 제한 로직
		// ㅣimit 대신 currentMaxSpeedLimit 사용으로ㅓ 수정
		if (rb.velocity.magnitude > currentMaxSpeedLimit)
		{
			rb.velocity = rb.velocity.normalized * currentMaxSpeedLimit;
		}
	}

	// 선박이 다른 Collider와 물리적으로 충돌했을 때 호출됨.
	void OnCollisionEnter(Collision collision)
	{
		// 충돌한 오브젝트의 태그가 Obstacle인지 확인
		// 장애물태그에 Obstacle 적용해햐아함
		if (collision.gameObject.CompareTag(obstacleTag))
		{
			// 충돌 시 선박의 속도를 감소
			rb.velocity *= collisionSpeedReductionFactor;
			rb.angularVelocity *= collisionSpeedReductionFactor; // 회전 속도도 감소

			// 로그찍기
			Debug.Log("장애물과 충돌! 속도 감소 및 경고 메시지 출력.");
			// 점수 감소 로직 추가
			ScoreManager.Instance.DecreaseScore(20f); // 20점 감소
		}
	}

	// 물 튀김 효과 재생 처리 함수
	private void HandleWaterSplash()
	{
		// 일정 간격마다만 물 튀김 효과 체크
		if (Time.time < lastSplashTime + splashInterval)
		{
			return;
		}

		bool shouldSplash = false;
		// 선박 속도가 일정 이상일 때 (전진/후진)
		if (rb.velocity.magnitude > minSpeedForSplash)
		{
			shouldSplash = true;
		}
		// 선박 회전 속도가 일정 이상일 때
		if (rb.angularVelocity.magnitude > minAngularSpeedForSplash)
		{
			shouldSplash = true;
		}

		if (shouldSplash)
		{
			PlaySplashEffects();
			lastSplashTime = Time.time;
		}
	}

	// 물 튀김 프리팹을 여러 지점에서 재생
	private void PlaySplashEffects()
	{
		if (waterSplashPrefab == null) return;

		foreach (Transform point in splashPoints)
		{
			if (point != null)
			{
				// 파티클 시스템 인스턴스 생성 및 재생
				GameObject splash = Instantiate(waterSplashPrefab, point.position, point.rotation);
				ParticleSystem ps = splash.GetComponent<ParticleSystem>();
				if (ps != null)
				{
					ps.Play(); // 파티클 시스템 재생 시작
										 // 파티클 시스템이 끝나면 자동으로 파괴되도록 설정 (없으면 씬에 계속 남음)
					Destroy(splash, ps.main.duration);
				}
				else
				{
					// 파티클 시스템이 없는 GameObject인 경우 즉시 파괴
					Destroy(splash, 3f); // 3초 뒤에 파괴 (예비)
				}
			}
		}
	} // PlaySplashEffects 끝

	// 자동 복구 처리 함수
	//private void HandleAutoRecovery()
	//{
	//	// 선박의 현재 X, Z축 회전 각도(수평면으로부터 기울어진 정도)ㄹ를 계산
	//	// Quaternion.Angle(from, to)는 두 회전 사이의 각도를 반환
	//	float currentAngleX = Quaternion.Angle(Quaternion.identity, Quaternion.Euler(transform.eulerAngles.x, 0, 0));
	//	float currentAngleZ = Quaternion.Angle(Quaternion.identity, Quaternion.Euler(0, 0, transform.eulerAngles.z));

	//	// X 또는 Z축으로 일정 각도 이상 기울어졌는지 확인
	//	if (currentAngleX > autoRecoveryThreshold || currentAngleZ > autoRecoveryThreshold)
	//	{
	//		// 기울어져 있다면 원래 수평 회전(initialRotation, Y축 회전만 유지)으로 서서히 돌림
	//		// Slerp는 두 Quaternion 사이를 부드럽게 보간하는 것.
	//		// transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.fixedDeltaTime * autoRecoverySpeed);

	//		// 초기 회전에서 Y축 값만 현재 선박의 Y축 값으로 업데이트하여 방향은 유지하고 기울기만 복구
	//		Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0); // Y축 회전만 남기고 X,Z는 0으로

	//		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * autoRecoverySpeed);

	//		// 복구 중에는 각속도(회전) 저항을 일시적으로 높여서 불필요한 흔들림을 줄임
	//		rb.angularDrag = autoRecoveryAngularDrag;
	//	}
	//	else
	//	{
	//		// 기울기가 임계값 이하라면 일반적인 회전 저항으로 되돌린다
	//		rb.angularDrag = angularDragCoefficient; // 원래의 각저항 계수로 복구
	//	}
	//} // HandleAutoRecovery 끝


} // 메인클래스 끝