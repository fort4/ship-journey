using UnityEngine;

public class ShipMovement : MonoBehaviour
{
	[Header("���� ���� ����")]
	[SerializeField] private float forwardForce = 200f; // ���� ������
	[SerializeField] private float rotationSpeed = 60f; // ȸ�� ��ũ
	[SerializeField] private float linearDragCoefficient = 1f; // ���� ���� ��� (�ӵ��� ���)
	[SerializeField] private float angularDragCoefficient = 3.3f; // ȸ�� ���� ��� (���ӵ��� ���)
	[SerializeField] private float maxSpeed = 20f; // �ִ� �ӵ� ����
	[SerializeField] private float accelerationMultiplier = 2f; // ���� �� ���� ������ ����
	[SerializeField] private float acceleratedMaxSpeed = 40f; // ���� �� �ִ� �ӵ�

	[Header("�浹 ����")]
	[SerializeField] private float collisionSpeedReductionFactor = 0.5f; // �浹 �� �ӵ� ������
	[SerializeField] private string obstacleTag = "Obstacle"; // ��ֹ� �±�

	[Header("�� Ƣ�� ȿ��")]
	[SerializeField] private GameObject waterSplashPrefab; // �� Ƣ�� ��ƼŬ ������
	[SerializeField] private Transform[] splashPoints; // �� Ƣ���� �߻��� ��ġ
	[SerializeField] private float minSpeedForSplash = 1.0f; // �� �ӵ� �̻��� �� �� Ƣ�� �߻�
	[SerializeField] private float minAngularSpeedForSplash = 0.5f; // �� ȸ�� �ӵ� �̻��� �� �� Ƣ�� �߻�
	[SerializeField] private float splashInterval = 0.2f; // �� Ƣ�� ȿ�� ��� ���� (�ʹ� ���� Ƣ�� �� ����)

	//[Header("�ڵ� ���� ����")]
	//[SerializeField] private float autoRecoveryThreshold = 45f; // �� ���� �̻� �������� ���� ���� (��)
	//[SerializeField] private float autoRecoverySpeed = 2f; // ���� �ӵ�
	//[SerializeField] private float autoRecoveryAngularDrag = 5f; // ���� �� ȸ�� ���� ����

	private Rigidbody rb;
	private float lastSplashTime = 0f; // ������ �� Ƣ�� �߻� �ð�
	//private Quaternion initialRotation; // ������ �ʱ� ȸ�� (���� ����)

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			Debug.LogError("���ڿ� Rigidbody ������Ʈ�� �����ϴ�. �߰����ּ���.");
			enabled = false;
			return;
		}
		rb.drag = 0f;
		rb.angularDrag = 0f;
		//initialRotation = transform.rotation; // ������ �ʱ� ȸ�� ���� (���� ����)
		Application.targetFrameRate = 40; // webgl ������ ������ �ϴ� �����ص�

		// �� Ƣ�� ���� �迭�� ��������� ���
		if (splashPoints == null || splashPoints.Length == 0)
		{
			Debug.LogWarning("WaterSplashEffect: �� Ƣ�� ����(Splash Points)�� �Ҵ���� �ʾҽ��ϴ�. ������ �ڽ����� �� GameObject�� ����� �Ҵ����ּ���.");
		}
	}

	void FixedUpdate()
	{
		HandleInput();
		ApplyDragForces();
		LimitSpeed();
		// �� Ƣ�� ȿ�� ���� �߰�
		HandleWaterSplash();
		// �ڵ� ���� ���� �߰�
		//HandleAutoRecovery();
	}

	private void HandleInput()
	{
		float verticalInput = Input.GetAxis("Vertical");
		float horizontalInput = Input.GetAxis("Horizontal");

		// ���� ���� Ȯ��
		float currentForwardForce = forwardForce;
		//float currentMaxSpeed = maxSpeed; // LimitSppeed() ���� �̵���Ŵ. ���� ������

		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			currentForwardForce *= accelerationMultiplier; // ������ ����
		  //currentMaxSpeed = acceleratedMaxSpeed; // �ִ� �ӵ� ���� // �굵 limitSpeed() ���� �̵���Ŵ.
		}

		// ����/���� �� ���� (������ ���� �� ��������)
		// ����: forwardForce ��� currentForwardForce ���
		rb.AddForce(transform.forward * verticalInput * currentForwardForce, ForceMode.Acceleration);

		// ȸ�� ��ũ ���� (Y���� ����)
		rb.AddTorque(Vector3.up * horizontalInput * rotationSpeed, ForceMode.Acceleration);

		// LimitSpeed() �Լ��� ���� ����� �ִ� �ӵ� ����
		//LimitSpeed(currentMaxSpeed);
	}

	private void ApplyDragForces()
	{
		// ���� ���� (�ӵ��� ���)
		Vector3 currentVelocity = rb.velocity;
		Vector3 dragForce = -currentVelocity * linearDragCoefficient * currentVelocity.magnitude;
		rb.AddForce(dragForce, ForceMode.Acceleration);

		// ȸ�� ���� (���ӵ��� ���)
		Vector3 currentAngularVelocity = rb.angularVelocity;
		Vector3 angularDragForce = -currentAngularVelocity * angularDragCoefficient * currentAngularVelocity.magnitude;
		rb.AddTorque(angularDragForce, ForceMode.Acceleration);
	}

	// private void LimitSpeed(float limit)���� ���ڻ��� ������
	private void LimitSpeed() // ���� ����
	{
		// ���� ������ �ִ� �ӵ� ����
		float currentMaxSpeedLimit = maxSpeed; // �⺻�� �Ϲ� �ִ� �ӵ�

		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			currentMaxSpeedLimit = acceleratedMaxSpeed; // Shift Ű�� ������ ���� �ִ� �ӵ� ����
		}

		// �ִ� �ӵ� ���� ����
		// ��imit ��� currentMaxSpeedLimit ������Τ� ����
		if (rb.velocity.magnitude > currentMaxSpeedLimit)
		{
			rb.velocity = rb.velocity.normalized * currentMaxSpeedLimit;
		}
	}

	// ������ �ٸ� Collider�� ���������� �浹���� �� ȣ���.
	void OnCollisionEnter(Collision collision)
	{
		// �浹�� ������Ʈ�� �±װ� Obstacle���� Ȯ��
		// ��ֹ��±׿� Obstacle �����������
		if (collision.gameObject.CompareTag(obstacleTag))
		{
			// �浹 �� ������ �ӵ��� ����
			rb.velocity *= collisionSpeedReductionFactor;
			rb.angularVelocity *= collisionSpeedReductionFactor; // ȸ�� �ӵ��� ����

			// �α����
			Debug.Log("��ֹ��� �浹! �ӵ� ���� �� ��� �޽��� ���.");
			// ���� ���� ���� �߰�
			ScoreManager.Instance.DecreaseScore(20f); // 20�� ����
		}
	}

	// �� Ƣ�� ȿ�� ��� ó�� �Լ�
	private void HandleWaterSplash()
	{
		// ���� ���ݸ��ٸ� �� Ƣ�� ȿ�� üũ
		if (Time.time < lastSplashTime + splashInterval)
		{
			return;
		}

		bool shouldSplash = false;
		// ���� �ӵ��� ���� �̻��� �� (����/����)
		if (rb.velocity.magnitude > minSpeedForSplash)
		{
			shouldSplash = true;
		}
		// ���� ȸ�� �ӵ��� ���� �̻��� ��
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

	// �� Ƣ�� �������� ���� �������� ���
	private void PlaySplashEffects()
	{
		if (waterSplashPrefab == null) return;

		foreach (Transform point in splashPoints)
		{
			if (point != null)
			{
				// ��ƼŬ �ý��� �ν��Ͻ� ���� �� ���
				GameObject splash = Instantiate(waterSplashPrefab, point.position, point.rotation);
				ParticleSystem ps = splash.GetComponent<ParticleSystem>();
				if (ps != null)
				{
					ps.Play(); // ��ƼŬ �ý��� ��� ����
										 // ��ƼŬ �ý����� ������ �ڵ����� �ı��ǵ��� ���� (������ ���� ��� ����)
					Destroy(splash, ps.main.duration);
				}
				else
				{
					// ��ƼŬ �ý����� ���� GameObject�� ��� ��� �ı�
					Destroy(splash, 3f); // 3�� �ڿ� �ı� (����)
				}
			}
		}
	} // PlaySplashEffects ��

	// �ڵ� ���� ó�� �Լ�
	//private void HandleAutoRecovery()
	//{
	//	// ������ ���� X, Z�� ȸ�� ����(��������κ��� ������ ����)���� ���
	//	// Quaternion.Angle(from, to)�� �� ȸ�� ������ ������ ��ȯ
	//	float currentAngleX = Quaternion.Angle(Quaternion.identity, Quaternion.Euler(transform.eulerAngles.x, 0, 0));
	//	float currentAngleZ = Quaternion.Angle(Quaternion.identity, Quaternion.Euler(0, 0, transform.eulerAngles.z));

	//	// X �Ǵ� Z������ ���� ���� �̻� ���������� Ȯ��
	//	if (currentAngleX > autoRecoveryThreshold || currentAngleZ > autoRecoveryThreshold)
	//	{
	//		// ������ �ִٸ� ���� ���� ȸ��(initialRotation, Y�� ȸ���� ����)���� ������ ����
	//		// Slerp�� �� Quaternion ���̸� �ε巴�� �����ϴ� ��.
	//		// transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.fixedDeltaTime * autoRecoverySpeed);

	//		// �ʱ� ȸ������ Y�� ���� ���� ������ Y�� ������ ������Ʈ�Ͽ� ������ �����ϰ� ���⸸ ����
	//		Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0); // Y�� ȸ���� ����� X,Z�� 0����

	//		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * autoRecoverySpeed);

	//		// ���� �߿��� ���ӵ�(ȸ��) ������ �Ͻ������� ������ ���ʿ��� ��鸲�� ����
	//		rb.angularDrag = autoRecoveryAngularDrag;
	//	}
	//	else
	//	{
	//		// ���Ⱑ �Ӱ谪 ���϶�� �Ϲ����� ȸ�� �������� �ǵ�����
	//		rb.angularDrag = angularDragCoefficient; // ������ ������ ����� ����
	//	}
	//} // HandleAutoRecovery ��


} // ����Ŭ���� ��