using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class FocusCamera : MonoBehaviour
{
	public float focusDistance = 10.0f;
	// 光圈
	public float aperture = 0.5f;

	private Vector3 _leftBottomCorner;
	private Vector3 _rightTopCorner;
	private Vector2 _size;

	private Camera _c;

    // Update is called once per frame
    void Update()
    {
		if (_c == null)
			_c = GetComponent<Camera>();

		float thetaRad = Mathf.Deg2Rad * _c.fieldOfView;
		float halfHeight = Mathf.Tan(thetaRad * .5f);
		float halfWidth = _c.aspect * halfHeight;
		_leftBottomCorner = transform.position + transform.forward * focusDistance
							- transform.right * focusDistance * halfWidth
							- transform.up * focusDistance * halfHeight;

		_size.x = focusDistance * halfWidth * 2.0f;
		_size.y = focusDistance * halfHeight * 2.0f;

		_rightTopCorner = _leftBottomCorner + transform.right * _size.x + transform.up * _size.y;
    }

	public void updateShaderParams(CommandBuffer cmd, RayTracingShader shader) {
		cmd.SetRayTracingVectorParam(shader, ShaderId.SID_LeftBottomCorner, _leftBottomCorner);
		cmd.SetRayTracingVectorParam(shader, ShaderId.SID_Right, _c.transform.right);
		cmd.SetRayTracingVectorParam(shader, ShaderId.SID_Up, _c.transform.up);
		cmd.SetRayTracingVectorParam(shader, ShaderId.SID_Size, _size);
		cmd.SetRayTracingFloatParam(shader, ShaderId.SID_HalfAperture, aperture * 0.5f);
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Vector3 p1 = _leftBottomCorner;
		Vector3 p2 = p1 + transform.right * _size.x;
		Vector3 p3 = _rightTopCorner;
		Vector3 p4 = p1 + transform.up * _size.y;

		// one triangle
		Gizmos.DrawLine(p1, p2);
		Gizmos.DrawLine(p2, p4);
		Gizmos.DrawLine(p4, p1);

		// two triangle
		Gizmos.DrawLine(p4, p2);
		Gizmos.DrawLine(p2, p3);
		Gizmos.DrawLine(p3, p4);

		Gizmos.DrawWireSphere(transform.position, aperture * 0.5f);

		Gizmos.color = Color.white;
	}

	private static class ShaderId {
		public static readonly int SID_LeftBottomCorner = Shader.PropertyToID("_FocusCameraLeftBottomCorner");
		public static readonly int SID_Right = Shader.PropertyToID("_FocusCameraRight");
		public static readonly int SID_Up = Shader.PropertyToID("_FocusCameraUp");
		public static readonly int SID_Size = Shader.PropertyToID("_FocusCameraSize");
		public static readonly int SID_HalfAperture = Shader.PropertyToID("_FocusCameraHalfApertureV");
	}
}
