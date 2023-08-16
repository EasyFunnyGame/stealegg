using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class CustomGameObjectArrays
{
	public GameObject[] Array;
	public GameObject this[int index]
    {
        get
        {
			return Array[index];
        }
    }

	public CustomGameObjectArrays()
    {
		Array = new GameObject[4];
    }

	public CustomGameObjectArrays(int count)
	{
		Array = new GameObject[count];
	}
}



public class CameraMultiTarget : MonoBehaviour
{
	public float Pitch;
	public float Yaw;
	public float Roll;

	public float Init_PaddingLeft = 1.0f;
	public float Init_PaddingRight = 1.0f;
	public float Init_PaddingUp = 1.2f;
	public float Init_PaddingDown = 0.5f;

	private float PaddingLeft;
	private float PaddingRight;
	private float PaddingUp;
	private float PaddingDown;
	private float MoveSmoothTime = 0.15f;
	public float moveSmoothTime
    {
        get
		{
			return MoveSmoothTime;
		}
    }

	public bool debuging = false;

	public float initHeight = 0.0f;

	private Camera _camera;
	public GameObject[] _targets = new GameObject[0];
	//private DebugProjection _debugProjection;

	[SerializeField]
	[Range(-0.25f, 0.25f)]
	public float m_customPadding = 0.0f;

	private bool zooming = false;


	public float customPadding
    {
        set
        {
			m_customPadding = value;
			m_customPadding = Mathf.Clamp(m_customPadding, -0.25f, 0.25f);
			zooming = true;
		}
        get
        {
			return m_customPadding;
        }
    }

#if UNITY_EDITOR
	private void OnValidate()
    {
		//zooming = true;
	}
#endif

	enum DebugProjection { DISABLE, IDENTITY, ROTATED }
	enum ProjectionEdgeHits { TOP_BOTTOM, LEFT_RIGHT }

	public bool useConfig = false;

	public void SetTargets(GameObject[] targets) {
		_targets = targets;
		useConfig = false;
	}

	public int _currentCameraPositionIndex = 0;
	public Vector3 _currentCameraTargetPosition = Vector3.zero;
	public List<Vector3> _cameraPositions;

	public void InitTargets()
    {
		initHeight = 0;
		ChangeCameraTargetPosition(0);
	}

	public void RecoverCamera()
    {
		useConfig = true;
		_currentCameraTargetPosition = _cameraPositions[_currentCameraPositionIndex];
	}

	public void ChangeCameraTargetPosition( int index )
    {
		if( index >= _cameraPositions.Count || index < 0 )
        {
			return;
        }
		if(_currentCameraPositionIndex == index )
        {
			return;
        }
		useConfig = true;
		_currentCameraPositionIndex = index;
		_currentCameraTargetPosition = _cameraPositions[_currentCameraPositionIndex];
	}

	
	private void Awake()
	{
		_camera = gameObject.GetComponent<Camera>();
		if( _cameraPositions.Count>0 )
        {
			transform.position = _cameraPositions[0];
        }
		//_debugProjection = DebugProjection.ROTATED;
	}

	private void LateUpdate() {
		if (_targets.Length == 0 || debuging )
			return;


        PaddingLeft = Init_PaddingLeft + m_customPadding;
        PaddingRight = Init_PaddingRight + m_customPadding;
        PaddingUp = Init_PaddingUp + m_customPadding;
        PaddingDown = Init_PaddingDown + m_customPadding;

        //if (zooming)
        //{
        //    transform.position = targetPositionAndRotation.Position;
        //    zooming = false;
        //    Debug.Log("zooming");
        //}
        if (useConfig)
		{

			Vector3 velocity = Vector3.zero;
			var toPosition = Vector3.SmoothDamp(transform.position, _currentCameraTargetPosition, ref velocity, MoveSmoothTime);
			if (Vector3.Distance(toPosition, transform.position) < 0.0001f)
			{
				if (initHeight == 0)
				{
					initHeight = transform.position.y;
					debuging = true;
				}
			}
			transform.position = toPosition;
		}
		else
		{
			var targetPositionAndRotation = TargetPositionAndRotation(_targets);
			Vector3 velocity = Vector3.zero;
			var toPosition = Vector3.SmoothDamp(transform.position, targetPositionAndRotation.Position, ref velocity, MoveSmoothTime);
			if (Vector3.Distance(toPosition, transform.position) < 0.0001f)
			{
				if (initHeight == 0)
				{
					initHeight = transform.position.y;
				}
			}
			transform.position = toPosition;
			transform.rotation = targetPositionAndRotation.Rotation;
		}
	}
	
	PositionAndRotation TargetPositionAndRotation(GameObject[] targets)
	{
		float halfVerticalFovRad = (_camera.fieldOfView * Mathf.Deg2Rad) / 2f;
		float halfHorizontalFovRad = Mathf.Atan(Mathf.Tan(halfVerticalFovRad) * _camera.aspect);

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

		//DebugDrawProjectionRays(cameraPositionIdentity, 
		//	viewProjectionLeftAndRightEdgeHits, 
		//	viewProjectionTopAndBottomEdgeHits, 
		//	requiredCameraPerpedicularDistanceFromProjectionPlane, 
		//	targetsRotatedToCameraIdentity, 
		//	projectionPlaneZ, 
		//	halfHorizontalFovRad, 
		//	halfVerticalFovRad);

		return new PositionAndRotation(rotation * cameraPositionIdentity, rotation);
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
			return new[] {target.x + projectionHalfSpan, target.x - projectionHalfSpan};
		}
		else
		{
			return new[] {target.y + projectionHalfSpan, target.y - projectionHalfSpan};
		}
	
	}
	
	//private void DebugDrawProjectionRays(Vector3 cameraPositionIdentity, ProjectionHits viewProjectionLeftAndRightEdgeHits,
	//	ProjectionHits viewProjectionTopAndBottomEdgeHits, float requiredCameraPerpedicularDistanceFromProjectionPlane,
	//	IEnumerable<Vector3> targetsRotatedToCameraIdentity, float projectionPlaneZ, float halfHorizontalFovRad,
	//	float halfVerticalFovRad) {
		
	//	if (_debugProjection == DebugProjection.DISABLE)
	//		return;
		
	//	DebugDrawProjectionRay(
	//		cameraPositionIdentity,
	//		new Vector3((viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
	//			(viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
	//			requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));
	//	DebugDrawProjectionRay(
	//		cameraPositionIdentity,
	//		new Vector3((viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
	//			-(viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
	//			requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));
	//	DebugDrawProjectionRay(
	//		cameraPositionIdentity,
	//		new Vector3(-(viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
	//			(viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
	//			requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));
	//	DebugDrawProjectionRay(
	//		cameraPositionIdentity,
	//		new Vector3(-(viewProjectionLeftAndRightEdgeHits.Max - viewProjectionLeftAndRightEdgeHits.Min) / 2f,
	//			-(viewProjectionTopAndBottomEdgeHits.Max - viewProjectionTopAndBottomEdgeHits.Min) / 2f,
	//			requiredCameraPerpedicularDistanceFromProjectionPlane), new Color32(31, 119, 180, 255));

	//	foreach (var target in targetsRotatedToCameraIdentity) {
	//		float distanceFromProjectionPlane = projectionPlaneZ - target.z;
	//		float halfHorizontalProjectionVolumeCircumcircleDiameter = Mathf.Sin(Mathf.PI - ((Mathf.PI / 2f) + halfHorizontalFovRad)) / (distanceFromProjectionPlane);
	//		float projectionHalfHorizontalSpan = Mathf.Sin(halfHorizontalFovRad) / halfHorizontalProjectionVolumeCircumcircleDiameter;
	//		float halfVerticalProjectionVolumeCircumcircleDiameter = Mathf.Sin(Mathf.PI - ((Mathf.PI / 2f) + halfVerticalFovRad)) / (distanceFromProjectionPlane);
	//		float projectionHalfVerticalSpan = Mathf.Sin(halfVerticalFovRad) / halfVerticalProjectionVolumeCircumcircleDiameter;
			
	//		DebugDrawProjectionRay(target,
	//			new Vector3(projectionHalfHorizontalSpan, 0f, distanceFromProjectionPlane),
	//			new Color32(214, 39, 40, 255));
	//		DebugDrawProjectionRay(target,
	//			new Vector3(-projectionHalfHorizontalSpan, 0f, distanceFromProjectionPlane),
	//			new Color32(214, 39, 40, 255));
	//		DebugDrawProjectionRay(target,
	//			new Vector3(0f, projectionHalfVerticalSpan, distanceFromProjectionPlane),
	//			new Color32(214, 39, 40, 255));
	//		DebugDrawProjectionRay(target,
	//			new Vector3(0f, -projectionHalfVerticalSpan, distanceFromProjectionPlane),
	//			new Color32(214, 39, 40, 255));
	//	}
	//}

	//private void DebugDrawProjectionRay(Vector3 start, Vector3 direction, Color color)
	//{
	//	Quaternion rotation = _debugProjection == DebugProjection.IDENTITY ? Quaternion.identity : transform.rotation;
	//	Debug.DrawRay(rotation * start, rotation * direction, color);
	//}

}
