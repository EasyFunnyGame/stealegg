﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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

	public float ChangeSmoothTime = 0.01f;

	public float near = 4;

	public float far = 5;

	public float height;

	public Camera m_camera;

	public GameObject[] _targets = new GameObject[0];
	private DebugProjection _debugProjection;

	enum DebugProjection { DISABLE, IDENTITY, ROTATED }
	enum ProjectionEdgeHits { TOP_BOTTOM, LEFT_RIGHT }

	public void SetTargets(GameObject[] targets)
	{
		_targets = targets;
	}

	private void Awake()
	{
		Instance = this;
		_debugProjection = DebugProjection.ROTATED;
	}

    public void Near()
    {
		if (Game.Instance?.player == null) return;
		var player = Game.Instance.player;
		var nearPlaneNodesAround = player.boardManager.FindNodesAround(player.currentTile.name, 3,true);
		var targets = new List<GameObject>(nearPlaneNodesAround.Count);
		foreach (var kvp in nearPlaneNodesAround)
		{
			targets.Add(kvp.Value.gameObject);
		}
		SetTargets(targets.ToArray());
		upper = false;
		height = near;
	}

	public void Far()
	{
		if (Game.Instance?.player == null) return;

		var player = Game.Instance.player;
		var nearPlaneNodesAround = player.boardManager.FindNodesAround(player.currentTile.name, 3, true);
		var targets = new List<GameObject>(nearPlaneNodesAround.Count);
		foreach (var kvp in nearPlaneNodesAround)
		{
			targets.Add(kvp.Value.gameObject);
		}
		SetTargets(targets.ToArray());
		height = far;
		upper = true;
	}
	bool init = false;
	private void Update()
    {
		if(!init)
        {
			if (Game.Instance.player)
            {
				init = true;
				//near = Game.Instance.player.near_front.transform.localPosition.z;
				//far = Game.Instance.player.far_front.transform.localPosition.z;
				if (upper)
                {
					Far();
                }
				else
                {
					Near();
                }
            }
        }
	}

    private void LateUpdate()
	{
		if (_targets.Length == 0)
			return;

		var targetPositionAndRotation = TargetPositionAndRotation(_targets);

		Vector3 velocity = Vector3.zero;


		transform.position = Vector3.SmoothDamp(transform.position, targetPositionAndRotation.Position, ref velocity, MoveSmoothTime);
		transform.rotation = targetPositionAndRotation.Rotation;
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
			ViewProjectionEdgeHits(targetsRotatedToCameraIdentity, ProjectionEdgeHits.LEFT_RIGHT, projectionPlaneZ, halfHorizontalFovRad).AddPadding(PaddingRight, PaddingLeft);
		ProjectionHits viewProjectionTopAndBottomEdgeHits =
			ViewProjectionEdgeHits(targetsRotatedToCameraIdentity, ProjectionEdgeHits.TOP_BOTTOM, projectionPlaneZ, halfVerticalFovRad).AddPadding(PaddingUp, PaddingDown);

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

	
}
