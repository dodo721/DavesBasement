using Godot;
using Godot.Collections;
using System;

public partial class Player : CharacterBody3D
{

	[ExportGroup("Mouse")]
	[Export]
	public float lookSpeed;

	[ExportGroup("Walking")]
	[Export]
	public float speed;
	[Export]
	public float jump;

	[ExportGroup("Interaction")]
	[Export]
	public float interactionRange;

	[ExportGroup("Joint")]
	[Export]
	public Joint joint;
	[Export]
	public PostProcessController postProcess;
	[Export]
	public float highUpSpeed;
	[Export]
	public float highDownSpeed;

	private Camera3D _camera;
	private JointLight _jointLight;
	private float _highLevel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_camera = GetNode<Camera3D>("PlayerCamera");
		_jointLight = GetNode<JointLight>("JointLight");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("smoke") && !_jointLight.hitLightLimit) {
			joint.smoked -= joint.smokeRate * (float)delta;
			_jointLight.smoking = true;
			_highLevel = Mathf.Min(_highLevel + (highUpSpeed * (float)delta), 1.0f);
		} else {
			_jointLight.smoking = false;
			_highLevel = Mathf.Max(_highLevel - (highDownSpeed * (float)delta), 0.0f);
		}
		postProcess.high = _highLevel;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Defs
		Vector3 forward = -Transform.Basis.Z;
		Vector3 right = Transform.Basis.X;
		
		// Movement
		Velocity = Vector3.Zero;
		if (Input.IsActionPressed("forward")) {
			Velocity += speed * (float)delta * forward;
		}
		if (Input.IsActionPressed("backward")) {
			Velocity += speed * (float)delta * -forward;
		}
		if (Input.IsActionPressed("left")) {
			Velocity += speed * (float)delta * -right;
		}
		if (Input.IsActionPressed("right")) {
			Velocity += speed * (float)delta * right;
		}
		MoveAndSlide();

		// Raycast
		if (Input.IsActionJustPressed("interact")) {
			PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

			Vector3 origin = _camera.GlobalPosition;
			Vector3 end = origin - (_camera.GlobalTransform.Basis.Z * interactionRange);
			PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin, end);
			query.CollideWithAreas = true;
			query.Exclude = new Array<Rid>{GetRid()};

			Dictionary result = spaceState.IntersectRay(query);
			
			if (result.Count > 0) {
				RigidBody3D collider = (RigidBody3D)result["collider"];
				if (collider.Name == "NPCRigidBody3D") {
					NPC npc = collider.GetParent<NPC>();
					npc?.Interact();
				} else if (collider.Name == "InteractRigidBody3D") {
					Interactable i = collider.GetParent<Interactable>();
					i?.Interact();
				}
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("focus")) {
			Input.MouseMode = Input.MouseModeEnum.Captured;
		} else if (@event.IsActionPressed("exit")) {
			Input.MouseMode = Input.MouseModeEnum.Visible;
		} if (@event is InputEventMouseMotion motionEvent) {
			if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
			// get mouse rotation
			Vector2 _mouseVelocity = motionEvent.Relative;
			float rotVertAngle = -Mathf.DegToRad(_mouseVelocity.Y * lookSpeed);
			// limit axis of rotation
			float curVertAngle = _camera.Rotation.X;
			if (curVertAngle + rotVertAngle >= Mathf.Pi / 2f) {
				rotVertAngle = (Mathf.Pi / 2f) - curVertAngle;
			} else if (curVertAngle + rotVertAngle <= -Mathf.Pi / 2f) {
				rotVertAngle = -(Mathf.Pi / 2f) - curVertAngle;
			}
			// make rotation
			_camera.RotateObjectLocal(Vector3.Right, rotVertAngle);
			RotateY(-Mathf.DegToRad(_mouseVelocity.X * lookSpeed));
		}
		
	}
}
