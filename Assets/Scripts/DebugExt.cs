using UnityEngine;

public static class DebugExt
{
	public static void DrawMarker(
		Vector3 position,
		float size,
		Color color,
		float duration = 0f,
		bool depthTest = true)
	{
		#if UNITY_EDITOR

		Vector3 line1PosA = position + Vector3.up * size * 0.5f;
		Vector3 line1PosB = position - Vector3.up * size * 0.5f;

		Vector3 line2PosA = position + Vector3.right * size * 0.5f;
		Vector3 line2PosB = position - Vector3.right * size * 0.5f;

		Vector3 line3PosA = position + Vector3.forward * size * 0.5f;
		Vector3 line3PosB = position - Vector3.forward * size * 0.5f;

		Debug.DrawLine(line1PosA, line1PosB, color, duration, depthTest);
		Debug.DrawLine(line2PosA, line2PosB, color, duration, depthTest);
		Debug.DrawLine(line3PosA, line3PosB, color, duration, depthTest);

		#endif // if UNITY_EDITOR
	}
	
	public static void DrawBox2D(
		Vector3 position,
		Vector2 size,
		Color color,
		float duration = 0f,
		bool depthTest = true)
	{
		#if UNITY_EDITOR

		Vector3 bottomLeftCorner = position - (Vector3)(size / 2f);
		Vector3 bottomRightCorner = bottomLeftCorner + Vector3.right * size.x;
		Vector3 topLeftCorner = bottomLeftCorner + Vector3.up * size.y;
		Vector3 topRightCorner = bottomRightCorner + Vector3.up * size.y;
		
		Debug.DrawLine(bottomLeftCorner, bottomRightCorner, color, duration, depthTest);
		Debug.DrawLine(bottomRightCorner, topRightCorner, color, duration, depthTest);
		Debug.DrawLine(topRightCorner, topLeftCorner, color, duration, depthTest);
		Debug.DrawLine(topLeftCorner, bottomLeftCorner, color, duration, depthTest);

		#endif // if UNITY_EDITOR
	}

	public static void DrawCircle2D(
		Vector3 position,
		float radius,
		Color color,
		float duration = 0f,
		bool depthTest = true)
	{
		DrawCircleInternal(position, radius, color, Quaternion.identity, duration, depthTest);
	}
	
	public static void DrawWireSphere(
		Vector3 position,
		float radius,
		Color color,
		Quaternion rotation,
		float duration = 0f,
		bool depthTest = true)
	{
		DrawCircleInternal(position, radius, color, rotation, duration, depthTest);
		DrawCircleInternal(position, radius, color, rotation * Quaternion.Euler(90f, 0f, 0f), duration, depthTest);
		DrawCircleInternal(position, radius, color, rotation * Quaternion.Euler(0f, 90f, 0f), duration, depthTest);
	}
	
	public static void DrawCircleInternal(
		Vector3 position,
		float radius,
		Color color,
		Quaternion rotation,
		float duration,
		bool depthTest)
	{
		#if UNITY_EDITOR

		const int iteration = 32;

		Vector3 previousPoint = position + rotation * Vector3.up * radius;

		for (int i = 0; i < iteration; i++)
		{
			Vector3 currentPoint = position + rotation * Quaternion.Euler(0, 0, (i + 1) * (360f / iteration)) * Vector3.up * radius;
			Debug.DrawLine(previousPoint, currentPoint, color, duration, depthTest);
			previousPoint = currentPoint;
		}
		
		#endif // if UNITY_EDITOR
	}
}
