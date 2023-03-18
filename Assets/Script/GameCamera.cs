using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class GameCamera : MonoBehaviour
{
    public static GameCamera Instance;

    public Animator camAnimator;

	public bool upper;

	public float Pitch;
	public float Yaw;
	public float Roll;
	public float PaddingLeft;
	public float PaddingRight;
	public float PaddingUp;
	public float PaddingDown;
	public float MoveSmoothTime = 0.19f;

	public float height;

	public float targetHeight;

	public Camera m_camera;

	public bool forceUpdate = false;

	public GameObject[] _targets = new GameObject[0];
	private DebugProjection _debugProjection;

	enum DebugProjection { DISABLE, IDENTITY, ROTATED }
	enum ProjectionEdgeHits { TOP_BOTTOM, LEFT_RIGHT }

	public float max_height;

	public float min_heieght;

	public void SetTargets(GameObject[] targets)
	{
		_targets = targets;
	}

	private void Awake()
	{
		Instance = this;

		targetHeight = height;
		_debugProjection = DebugProjection.ROTATED;
	}


	PositionAndRotation TargetPositionAndRotation(GameObject[] targets)
	{
		float halfVerticalFovRad = (m_camera.fieldOfView * Mathf.Deg2Rad) / 2f;
		float halfHorizontalFovRad = Mathf.Atan(Mathf.Tan(halfVerticalFovRad) * m_camera.aspect);

		var rotation = Quaternion.Euler(Pitch, Yaw, Roll);
		var inverseRotation = Quaternion.Inverse(rotation);

		var targetsRotatedToCameraIdentity = targets.Select(target => inverseRotation * target.transform.position).ToArray();

		float furthestPointDistanceFromCamera = targetsRotatedToCameraIdentity.Max(target => target.z);
		float projectionPlaneZ = furthestPointDistanceFromCamera + 3f;

		ProjectionHits viewProjectionLeftAndRightEdgeHits =
			ViewProjectionEdgeHits(targetsRotatedToCameraIdentity, ProjectionEdgeHits.LEFT_RIGHT, projectionPlaneZ, halfHorizontalFovRad).AddPadding(UpperPaddingRight, UpperPaddingLeft);
		ProjectionHits viewProjectionTopAndBottomEdgeHits =
			ViewProjectionEdgeHits(targetsRotatedToCameraIdentity, ProjectionEdgeHits.TOP_BOTTOM, projectionPlaneZ, halfVerticalFovRad).AddPadding(UpperPaddingUp, UpperPaddingDown);

		var requiredCameraPerpedicularDistanceFromProjectionPlane =
			Mathf.Max(
				RequiredCameraPerpedicularDistanceFromProjectionPlane(viewProjectionTopAndBottomEdgeHits, halfVerticalFovRad),
				RequiredCameraPerpedicularDistanceFromProjectionPlane(viewProjectionLeftAndRightEdgeHits, halfHorizontalFovRad)
		);

		Vector3 cameraPositionIdentity = new Vector3(
			(viewProjectionLeftAndRightEdgeHits.Max + viewProjectionLeftAndRightEdgeHits.Min) / 2f,
			(viewProjectionTopAndBottomEdgeHits.Max + viewProjectionTopAndBottomEdgeHits.Min) / 2f,
			projectionPlaneZ - requiredCameraPerpedicularDistanceFromProjectionPlane);

		DebugDrawProjectionRays(cameraPositionIdentity,
			viewProjectionLeftAndRightEdgeHits,
			viewProjectionTopAndBottomEdgeHits,
			requiredCameraPerpedicularDistanceFromProjectionPlane,
			targetsRotatedToCameraIdentity,
			projectionPlaneZ,
			halfHorizontalFovRad,
			halfVerticalFovRad);

		return new PositionAndRotation(rotation * cameraPositionIdentity, rotation, height);
	}

	private static float RequiredCameraPerpedicularDistanceFromProjectionPlane(ProjectionHits viewProjectionEdgeHits, float halfFovRad)
	{
		float distanceBetweenEdgeProjectionHits = viewProjectionEdgeHits.Max - viewProjectionEdgeHits.Min;
		return (distanceBetweenEdgeProjectionHits / 2f) / Mathf.Tan(halfFovRad);
	}

	private ProjectionHits ViewProjectionEdgeHits(IEnumerable<Vector3> targetsRotatedToCameraIdentity, ProjectionEdgeHits alongAxis, float projectionPlaneZ, float halfFovRad)
	{
		float[] projectionHits = targetsRotatedToCameraIdentity
			.SelectMany(target => TargetProjectionHits(target, alongAxis, projectionPlaneZ, halfFovRad))
			.ToArray();
		return new ProjectionHits(projectionHits.Max(), projectionHits.Min());
	}

	private float[] TargetProjectionHits(Vector3 target, ProjectionEdgeHits alongAxis, float projectionPlaneDistance, float halfFovRad)
	{
		float distanceFromProjectionPlane = projectionPlaneDistance - target.z;
		float projectionHalfSpan = Mathf.Tan(halfFovRad) * distanceFromProjectionPlane;

		if (alongAxis == ProjectionEdgeHits.LEFT_RIGHT)
		{
			return new[] { target.x + projectionHalfSpan, target.x - projectionHalfSpan };
		}
		else
		{
			return new[] { target.y + projectionHalfSpan, target.y - projectionHalfSpan };
		}

	}

	private void DebugDrawProjectionRays(Vector3 cameraPositionIdentity, ProjectionHits viewProjectionLeftAndRightEdgeHits,
		ProjectionHits viewProjectionTopAndBottomEdgeHits, float requiredCameraPerpedicularDistanceFromProjectionPlane,
		IEnumerable<Vector3> targetsRotatedToCameraIdentity, float projectionPlaneZ, float halfHorizontalFovRad,
		float halfVerticalFovRad)
	{

		if (_debugProjection == DebugProjection.DISABLE)
			return;

		DebugDrawProjectionRay(
			cameraPositionIdentity,
			new Vector3((viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
				(viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
				requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));
		DebugDrawProjectionRay(
			cameraPositionIdentity,
			new Vector3((viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
				-(viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
				requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));
		DebugDrawProjectionRay(
			cameraPositionIdentity,
			new Vector3(-(viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
				(viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
				requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));
		DebugDrawProjectionRay(
			cameraPositionIdentity,
			new Vector3(-(viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
				-(viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
				requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));

		foreach (var target in targetsRotatedToCameraIdentity)
		{
			float distanceFromProjectionPlane = projectionPlaneZ - target.z;
			float halfHorizontalProjectionVolumeCircumcircleDiameter = Mathf.Sin(Mathf.PI - ((Mathf.PI / 2f) + halfHorizontalFovRad)) / (distanceFromProjectionPlane);
			float projectionHalfHorizontalSpan = Mathf.Sin(halfHorizontalFovRad) / halfHorizontalProjectionVolumeCircumcircleDiameter;
			float halfVerticalProjectionVolumeCircumcircleDiameter = Mathf.Sin(Mathf.PI - ((Mathf.PI / 2f) + halfVerticalFovRad)) / (distanceFromProjectionPlane);
			float projectionHalfVerticalSpan = Mathf.Sin(halfVerticalFovRad) / halfVerticalProjectionVolumeCircumcircleDiameter;

			DebugDrawProjectionRay(target,
				new Vector3(projectionHalfHorizontalSpan, 0f, distanceFromProjectionPlane),
				new Color32(214, 39, 40, 255));
			DebugDrawProjectionRay(target,
				new Vector3(-projectionHalfHorizontalSpan, 0f, distanceFromProjectionPlane),
				new Color32(214, 39, 40, 255));
			DebugDrawProjectionRay(target,
				new Vector3(0f, projectionHalfVerticalSpan, distanceFromProjectionPlane),
				new Color32(214, 39, 40, 255));
			DebugDrawProjectionRay(target,
				new Vector3(0f, -projectionHalfVerticalSpan, distanceFromProjectionPlane),
				new Color32(214, 39, 40, 255));
		}
	}

	private void DebugDrawProjectionRay(Vector3 start, Vector3 direction, Color color)
	{
		Quaternion rotation = _debugProjection == DebugProjection.IDENTITY ? Quaternion.identity : transform.rotation;
		Debug.DrawRay(rotation * start, rotation * direction, color);
	}

	public float playerPaddingUp;
	public float playerPaddingDown;
	public float playerPaddingLeft;
	public float playerPaddingRight;
	public Vector3 targetPosition = new Vector3();

	public float UpperPaddingUp=0.5f;
	public float UpperPaddingDown = 0.5f;
	public float UpperPaddingLeft = 0.5f;
	public float UpperPaddingRight = 0.5f;

	private void LateUpdate()
	{
		if (Game.Instance.result != GameResult.NONE && !forceUpdate) return;
		if (upper && _targets.Length > 0)
		{
			var targetPositionAndRotation = TargetPositionAndRotation(_targets);
			Vector3 velocity = Vector3.zero;
			transform.position = Vector3.SmoothDamp(transform.position, targetPositionAndRotation.Position, ref velocity, 0.05f);
			transform.rotation = targetPositionAndRotation.Rotation;
			return;
		}

		if (playerPaddingDown < PaddingDown)
        {
			transform.Translate(new Vector3(0,0,-MoveSmoothTime), Space.World);
		}
		if (playerPaddingUp < PaddingUp)
        {
			transform.Translate(new Vector3(0, 0, MoveSmoothTime), Space.World);
		}
		if (playerPaddingLeft < PaddingLeft)
		{
			transform.Translate(new Vector3(-MoveSmoothTime, 0, 0), Space.World);
		}
		if (playerPaddingRight < PaddingRight)
		{
			transform.Translate(new Vector3(MoveSmoothTime, 0, 0), Space.World);
		}

		//if(upper)
  //      {
		//	targetHeight = height + 1;
  //      }
  //      else
  //      {
		//	targetHeight = height;
  //      }

		if (transform.position.y < targetHeight)
        {
			transform.Translate(new Vector3(0, MoveSmoothTime, 0), Space.World);
		}

		if (transform.position.y > targetHeight)
		{
			transform.Translate(new Vector3(0, -MoveSmoothTime, 0), Space.World);
		}
		var rotation = Quaternion.Euler(Pitch, Yaw, Roll);
		transform.rotation = rotation;

	}


	public void UpdatePlayerPositionOnScreen(RectTransform canvasRect, Vector3 position, Image playerImage)
    {
		float resolutionRotioWidth = canvasRect.sizeDelta.x;
		float resolutionRotioHeight = canvasRect.sizeDelta.y;
		float widthRatio = resolutionRotioWidth / Screen.width;
		float heightRatio = resolutionRotioHeight / Screen.height;

		float posX = position.x *= widthRatio;

		float posY = position.y *= heightRatio;

		float halfCanvasHeight = resolutionRotioHeight / 2;
		float halfCanvasWidth = resolutionRotioWidth / 2;


		playerPaddingUp = halfCanvasHeight - playerImage.GetComponent<RectTransform>().anchoredPosition.y;
		playerPaddingDown = halfCanvasHeight + playerImage.GetComponent<RectTransform>().anchoredPosition.y;
		playerPaddingLeft = halfCanvasWidth + playerImage.GetComponent<RectTransform>().anchoredPosition.x;
		playerPaddingRight = halfCanvasWidth - playerImage.GetComponent<RectTransform>().anchoredPosition.x;

		playerImage.rectTransform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, playerPaddingUp);
		playerImage.rectTransform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -playerPaddingDown);
		playerImage.rectTransform.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(-playerPaddingLeft, 0);
		playerImage.rectTransform.GetChild(3).GetComponent<RectTransform>().anchoredPosition = new Vector2(playerPaddingRight, 0);
	}


}
