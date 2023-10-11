using Godot;
using System;
using System.Collections.Generic;

public partial class EthanAnimator : Node3D
{
	public static string PARAM_WALK_SPEED = "parameters/WalkSpeed/scale";
	public static string PARAM_WALK_INTENSITY = "parameters/WalkIntensity/blend_amount";
	public static string PARAM_CRAWL_TRIGGER = "parameters/CrawlTrigger/request";
	public static string PARAM_LOOK_TRIGGER = "parameters/LookingTrigger/request";

	private AnimationTree animator;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		animator = GetNode<AnimationTree>("AnimationTree");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Walk() {
		SetWalkSpeed(0.5f);
	}

	public void Run() {
		SetWalkSpeed(1f);
	}

	public void SetWalkSpeed(float walkSpeed) {
		animator.Set(PARAM_WALK_SPEED, walkSpeed);
		animator.Set(PARAM_WALK_INTENSITY, walkSpeed);
	}

	public void Still() {
		animator.Set(PARAM_WALK_SPEED, 0f);
		animator.Set(PARAM_WALK_INTENSITY, 0f);
		animator.Set(PARAM_LOOK_TRIGGER, (int)AnimationNodeOneShot.OneShotRequest.FadeOut);
	}

	public void Looking() {
		animator.Set(PARAM_LOOK_TRIGGER, (int)AnimationNodeOneShot.OneShotRequest.Fire);
	}

	public void StopLooking() {
		animator.Set(PARAM_LOOK_TRIGGER, (int)AnimationNodeOneShot.OneShotRequest.FadeOut);
	}

	public void Crawl() {
		animator.Set(PARAM_CRAWL_TRIGGER, (int)AnimationNodeOneShot.OneShotRequest.Fire);
	}

	private void _OnAnimationFinish(StringName animName)
	{
		GD.Print("FINISHED " + animName);
	}

}
