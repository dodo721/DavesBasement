using Godot;
using System;

public partial class VentTrap : Vent
{

	[Export]
	public NodePath ethan;

	private Ethan _ethan;

	public override void _Ready()
	{
		base._Ready();
		_ethan = GetNode<Ethan>(ethan);
	}

	

	public override void Interact()
	{
		base.Interact();
		
	}
}
