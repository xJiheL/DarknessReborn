using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public static class DebugExt
{
	[Conditional("UNITY_EDITOR")]
	public static void DrawMarker(
		Vector3 position,
		float size,
		Color color,
		float duration = 0f,
		bool depthTest = true)
	{
		Vector3 line1PosA = position + Vector3.up * size * 0.5f;
		Vector3 line1PosB = position - Vector3.up * size * 0.5f;

		Vector3 line2PosA = position + Vector3.right * size * 0.5f;
		Vector3 line2PosB = position - Vector3.right * size * 0.5f;

		Vector3 line3PosA = position + Vector3.forward * size * 0.5f;
		Vector3 line3PosB = position - Vector3.forward * size * 0.5f;

		Debug.DrawLine(line1PosA, line1PosB, color, duration, depthTest);
		Debug.DrawLine(line2PosA, line2PosB, color, duration, depthTest);
		Debug.DrawLine(line3PosA, line3PosB, color, duration, depthTest);
	}
	
	[Conditional("UNITY_EDITOR")]
	public static void DrawBox2D(
		Vector3 position,
		Vector2 size,
		Color color,
		float duration = 0f,
		bool depthTest = true)
	{
		Vector3 bottomLeftCorner = position - (Vector3)(size / 2f);
		Vector3 bottomRightCorner = bottomLeftCorner + Vector3.right * size.x;
		Vector3 topLeftCorner = bottomLeftCorner + Vector3.up * size.y;
		Vector3 topRightCorner = bottomRightCorner + Vector3.up * size.y;
		
		Debug.DrawLine(bottomLeftCorner, bottomRightCorner, color, duration, depthTest);
		Debug.DrawLine(bottomRightCorner, topRightCorner, color, duration, depthTest);
		Debug.DrawLine(topRightCorner, topLeftCorner, color, duration, depthTest);
		Debug.DrawLine(topLeftCorner, bottomLeftCorner, color, duration, depthTest);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawCircle2D(
		Vector3 position,
		float radius,
		Color color,
		float duration = 0f,
		bool depthTest = true)
	{
		DrawCircleInternal(position, radius, color, Quaternion.identity, duration, depthTest, 0f, 360f);
	}
	
	[Conditional("UNITY_EDITOR")]
	public static void DrawWireSphere(
		Vector3 position,
		float radius,
		Color color,
		Quaternion rotation,
		float duration = 0f,
		bool depthTest = true)
	{
		DrawCircleInternal(position, radius, color, rotation, duration, depthTest, 0f, 360f);
		DrawCircleInternal(position, radius, color, rotation * Quaternion.Euler(90f, 0f, 0f), duration, depthTest, 0f, 360f);
		DrawCircleInternal(position, radius, color, rotation * Quaternion.Euler(0f, 90f, 0f), duration, depthTest, 0f, 360f);
	}
	
	[Conditional("UNITY_EDITOR")]
	public static void DrawWireCapsule(
		Vector3 point1,
		Vector3 point2,
		float radius,
		float height,
		Color color,
		Quaternion rotation,
		float duration = 0f,
		bool depthTest = true)
	{
		Vector3 up = (point2 - point1).normalized;
		Vector3 right = rotation * Quaternion.Euler(0f, 0f, -90f) * up;
		Vector3 forward = rotation * Quaternion.Euler(90f, 0f, 0f) * up;
		
		Debug.DrawLine(point1 + right * radius, point2 + right * radius, color, duration, depthTest);
		Debug.DrawLine(point1 - right * radius, point2 - right * radius, color, duration, depthTest);
		Debug.DrawLine(point1 + forward * radius, point2 + forward * radius, color, duration, depthTest);
		Debug.DrawLine(point1 - forward * radius, point2 - forward * radius, color, duration, depthTest);
		
		DrawCircleInternal(point1, radius, color, rotation, duration, depthTest, 90f, 180f);
		DrawCircleInternal(point1, radius, color, rotation * Quaternion.Euler(90f, 0f, 0f), duration, depthTest, 0f, 360f);
		DrawCircleInternal(point1, radius, color, rotation * Quaternion.Euler(0f, 90f, 0f), duration, depthTest, 90f, 180f);
		
		DrawCircleInternal(point2, radius, color, rotation, duration, depthTest, 270f, 180f);
		DrawCircleInternal(point2, radius, color, rotation * Quaternion.Euler(90f, 0f, 0f), duration, depthTest, 0f, 360f);
		DrawCircleInternal(point2, radius, color, rotation * Quaternion.Euler(0f, 90f, 0f), duration, depthTest, 270f, 180f);
	}
	
	[Conditional("UNITY_EDITOR")]
	private static void DrawCircleInternal(
		Vector3 position,
		float radius,
		Color color,
		Quaternion rotation,
		float duration,
		bool depthTest,
		float startAngle,
		float angle)
	{
		int iteration = (int)(angle / 360f * 32);

		Vector3 previousPoint = position + rotation * Quaternion.Euler(0, 0, startAngle) * Vector3.up * radius;

		for (int i = 0; i < iteration; i++)
		{
			Vector3 currentPoint = position + rotation * Quaternion.Euler(0, 0, startAngle + (i + 1) * (angle / iteration)) * Vector3.up * radius;
			Debug.DrawLine(previousPoint, currentPoint, color, duration, depthTest);
			previousPoint = currentPoint;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawTriangle(
		Vector3 a, 
		Vector3 b, 
		Vector3 c, 
		Color color,
		float duration = 0f,
		bool depthTest = true,
		Transform t = null)
    {
		if(t != null)
		{
			a = t.TransformPoint(a);
		    b = t.TransformPoint(b);
		    c = t.TransformPoint(c);
		}

        Debug.DrawLine(a, b, color);
        Debug.DrawLine(b, c, color);
        Debug.DrawLine(c, a, color);
    }
}
