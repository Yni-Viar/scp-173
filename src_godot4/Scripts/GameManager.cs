using Godot;
using System;

public partial class GameManager : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (GetNodeOrNull("scp173") == null)
		{
			GetNode<PlayerScript>("Player").ChangeState(true);
		}
	}
}
