using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	private Vector3 velocity = Vector3.Zero;
	private Vector3 input_vector = Vector3.Zero;
	private Vector3 world_vector = Vector3.Zero;

	public override void _PhysicsProcess(double delta)
	{
		float felta = (float)delta;
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	// these functions are only called from Process and PhysicsProcess
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void GroundPhysics(float delta)
	{

	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AirPhysics(float delta)
	{

	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CommonPhysics(float delta)
	{

	}
}
