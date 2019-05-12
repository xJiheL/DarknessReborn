using System;
using UnityEngine;

[Serializable]
public struct CurrentTransform
{
	private Vector3 _direction;
	private Vector2 _inputMove;
	private Vector3 _velocity;
	private Vector3 _position;
	private Quaternion _rotation;
	private Vector3 _up;
	private Vector3 _forward;
	private Vector3 _right;
	private Collider _standingCollider;
	private float _deltaTime;

	public CurrentTransform(Vector3 direction, Vector2 inputMove, Vector3 velocity, Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right, Collider standingCollider, float deltaTime)
	{
		_direction = direction;
		_inputMove = inputMove;
		_velocity = velocity;
		_position = position;
		_rotation = rotation;
		_up = up;
		_forward = forward;
		_right = right;
		_standingCollider = standingCollider;
		_deltaTime = deltaTime;
	}

	public Vector3 Direction => _direction;

	public Vector2 InputMove => _inputMove;

	public Vector3 Velocity => _velocity;

	public Vector3 Position => _position;

	public Quaternion Rotation => _rotation;

	public Vector3 Up => _up;

	public Vector3 Forward => _forward;

	public Vector3 Right => _right;

	public Collider StandingCollider => _standingCollider;

	public float DeltaTime => _deltaTime;
}