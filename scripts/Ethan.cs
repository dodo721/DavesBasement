using Godot;
using Godot.Collections;
using System;

public partial class Ethan : CharacterBody3D
{

	[Export]
	public NodePath player;

	[ExportGroup("Movement")]
	[Export]
	public float walkSpeed = 1.0f;
	[Export]
	public float chaseSpeed = 2.0f;
	[Export]
	public float animationScale = 0.5f;
	[Export]
	public float turnSpeed = 1f;

	[ExportGroup("AI")]
	[Export]
	public float searchRadius = 10f;
	[Export]
	public float lookMaxTime = 10f;
	[Export]
	public float lookMintime = 5f;
	[Export]
	public float searchAccuracy = 0.75f;
	[Export]
	public float visionRange = 10f;
	public enum EthanState { SEARCHING_WALKING, SEARCHING_LOOKING, CHASING, VENT_CLIMBING, IDLE };
	[Export]
	public EthanState ETHAN_STATE = EthanState.SEARCHING_WALKING;
	private EthanState _PREV_STATE = EthanState.IDLE;

	// References
	private NavigationAgent3D _navigationAgent;
	private Player _player;
	private EthanAnimator _animator;
	private Node3D _eyes;

	// General variables
	private bool _navReady = false;
	private float _movementSpeed;

	// State specific
	private Vector3 _searchOrigin;
	private float _lookTime;
	private float _lookStartTime;

	public override void _Ready()
	{
		base._Ready();

		_player = GetNode<Player>(player);
		_animator = GetNode<EthanAnimator>("ethan_model");
		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_eyes = GetNode<Node3D>("ethan_model/EthanBones/Skeleton3D/EyeAttachment/Eyes");

		// These values need to be adjusted for the actor's speed
		// and the navigation layout.
		_navigationAgent.PathDesiredDistance = 0.5f;
		_navigationAgent.TargetDesiredDistance = 0.5f;

		// Make sure to not await during _Ready.
		Callable.From(ActorSetup).CallDeferred();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		// Wait until ready
		if (!_navReady) return;

		// Record state
		bool justSwitchedState = _PREV_STATE != ETHAN_STATE;
		_PREV_STATE = ETHAN_STATE;

		// Setup general variables
		Vector3 newVelocity = Vector3.Zero;
		Vector3 nextPathPosition = GlobalPosition;

		// Raycast to player
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		Vector3 origin = _eyes.GlobalPosition;
		Vector3 end = origin - (_eyes.GlobalTransform.Basis.Z * visionRange);
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin, end);
		query.CollideWithAreas = true;
		query.Exclude = new Array<Rid>{GetRid()};
		Dictionary result = spaceState.IntersectRay(query);
		
		bool canSeePlayer = false;
		if (result.Count > 0) {
			PhysicsBody3D collider = (PhysicsBody3D)result["collider"];
			if (collider.Name == "Player") {
				canSeePlayer = true;
			}
		}

		// State machine
		if (ETHAN_STATE == EthanState.CHASING) {
			
			if (justSwitchedState) {
				_movementSpeed = chaseSpeed;
			}
			// Run towards player
			_navigationAgent.TargetPosition = _player.GlobalPosition;
			// Cancel on no navigation
			if (_navigationAgent.IsNavigationFinished()) return;
			nextPathPosition = _navigationAgent.GetNextPathPosition();

			if (!canSeePlayer) {
				ETHAN_STATE = EthanState.SEARCHING_WALKING;
			}

		} else if (ETHAN_STATE == EthanState.SEARCHING_WALKING) {
			
			if (justSwitchedState) {
				// Record where search begins
				_searchOrigin = GlobalPosition;
				_movementSpeed = walkSpeed;
				// Choose a random location within radius of search origin
				Vector3 dirToPlayer = (_player.GlobalPosition - _searchOrigin).Normalized();
				float searchArc = (1 - searchAccuracy) * Mathf.Pi;
				Vector3 randomDirection = dirToPlayer.Rotated(Vector3.Up, new RandomNumberGenerator().RandfRange(-searchArc, searchArc));
				Vector3 randomPosition = _searchOrigin + (randomDirection * searchRadius);
				_navigationAgent.TargetPosition = randomPosition;
			}

			// Update navigation
			nextPathPosition = _navigationAgent.GetNextPathPosition();
			
			if (_navigationAgent.IsNavigationFinished()) {
				// Switch to looking state
				ETHAN_STATE = EthanState.SEARCHING_LOOKING;
			}

			if (canSeePlayer) {
				ETHAN_STATE = EthanState.CHASING;
				_animator.StopLooking();
			}

		} else if (ETHAN_STATE == EthanState.SEARCHING_LOOKING) {
			
			// Setup looking animation and timings
			if (justSwitchedState) {
				_animator.Looking();
				_lookTime = new RandomNumberGenerator().RandfRange(lookMintime * 1000, lookMaxTime * 1000);
				_lookStartTime = Time.GetTicksMsec();
			}

			// Switch back once time is up
			if (Time.GetTicksMsec() - _lookStartTime >= _lookTime) {
				_animator.StopLooking();
				ETHAN_STATE = EthanState.SEARCHING_WALKING;
			}

			if (canSeePlayer) {
				ETHAN_STATE = EthanState.CHASING;
				_animator.StopLooking();
			}

		} else if (ETHAN_STATE == EthanState.VENT_CLIMBING) {
			
			if (justSwitchedState)
				_animator.Crawl();

		}

		// Sync velocity
		Vector3 currentAgentPosition = GlobalTransform.Origin;
		newVelocity = (nextPathPosition - currentAgentPosition).Normalized();
		newVelocity *= _movementSpeed;
		Velocity = newVelocity;
		// Sync animation
		_animator.SetWalkSpeed(_movementSpeed * animationScale);

		// Face velocity
		if (newVelocity.Length() > 0) {
			Transform3D lookTransform = Transform.LookingAt(nextPathPosition, Vector3.Up);
			Quaternion slerped = Transform.Basis.GetRotationQuaternion().Slerp(lookTransform.Basis.GetRotationQuaternion(), turnSpeed * (float)delta);
			Transform3D slerpedTransform = Transform;
			slerpedTransform.Basis = new Basis(slerped);
			Transform = slerpedTransform;
		}

		MoveAndSlide();

	}

	public void TeleportToVent (VentTrap vent) {
		Node3D tp = vent.GetNode<Node3D>("EthanTP");
		GlobalPosition = tp.GlobalPosition;
		ETHAN_STATE = EthanState.VENT_CLIMBING;
	}

	private async void ActorSetup()
	{
		// Wait for the first physics frame so the NavigationServer can sync.
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		// Now that the navigation map is no longer empty, set the movement target.
		_navReady = true;
	}
}
