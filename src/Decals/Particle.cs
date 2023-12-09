using Godot;
using System;

public partial class Particle : CpuParticles3D
{
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		Emitting = true;
        await ToSignal(GetTree().CreateTimer(7.0), "timeout");
        Emitting = false;
        QueueFree();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
