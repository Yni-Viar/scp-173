using Godot;
using System;

public partial class Scp173Npc : CharacterBody3D
{
	RandomNumberGenerator rng = new RandomNumberGenerator();
	Node3D target;
	AudioStreamPlayer3D walkSound;
    AudioStreamPlayer3D interactSound;

	[Export] internal float health = 500.0f;
    [Export] float speed = 40.0f;
	internal bool canMove = false;
	internal bool isWatching = false;

	public override void _Ready()
	{
		SetRandomFace();
		walkSound = GetNode<AudioStreamPlayer3D>("WalkSound");
        interactSound = GetNode<AudioStreamPlayer3D>("InteractSound");
    }

	public override void _PhysicsProcess(double delta)
	{
        // basic SCP-173 rules - player don't see - 173 is closer.
		if ((canMove || !isWatching) && target != null)
		{
			var playerDirection = (target.GlobalPosition - this.GlobalPosition).Normalized();
			Velocity += speed * playerDirection * (float)delta;
            this.LookAt(target.GlobalPosition);

			walkSound.Stream = GD.Load<AudioStream>("res://Sounds/Character/173/Rattle" + rng.RandiRange(1, 3) + ".ogg");
			walkSound.Play();
		}
        // else SCP-173 must stay still!
        else
        {
            Velocity = Vector3.Zero;
        }
		MoveAndSlide();

        // Crunch handler
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D checkCollision = GetSlideCollision(i);
            var collidedWith = checkCollision.GetCollider();
			if (collidedWith is PlayerScript player)
			{
                if (player.canMove)
                {
                    interactSound.Stream = GD.Load<AudioStream>("res://Sounds/Character/173/NeckSnap" + rng.RandiRange(1, 3) + ".ogg");
                    interactSound.Play();
                    player.ChangeState(false); //crunch
                }
            }
		}
	}

	private void OnFindPlayerAreaBodyEntered(Node3D body)
	{
		if (body.IsInGroup("Players"))
		{
			target = body;
		}
	}


	private void OnFindPlayerAreaBodyExited(Node3D body)
	{
		if (body.IsInGroup("Players"))
		{
			target = null;
		}
	}

	internal void HealthManage(float amount)
	{
		health += amount;
		if (health < 0)
		{
			Node3D node = ResourceLoader.Load<PackedScene>("res://Assets/NPC/scp173_fracture.tscn").Instantiate<Node3D>();
			node.Position = GlobalPosition;
			GetParent().AddChild(node);
			QueueFree();
		}
	}

    private void OnDontLookAtMeScreenEntered()
    {
        canMove = false;
    }

    private void OnDontLookAtMeScreenExited()
    {
        canMove = true;
    }

    void SetRandomFace()
	{
		ShaderMaterial mat = new ShaderMaterial();
		mat.Shader = ResourceLoader.Load<Shader>("res://Shaders/MixShader/mix.gdshader");
		mat.SetShaderParameter("texture_a", ResourceLoader.Load<Texture2D>("res://Assets/Models/scp173-BaseTexture/scp173NEO_low_Merged_PM3D_Sphere3D4_AlbedoTransparency.png"));
		mat.SetShaderParameter("texture_b", ResourceLoader.Load<Texture2D>("res://Assets/Models/scp173-FaceTextures/face_" + rng.RandiRange(1, 10).ToString() + ".png"));
		GetNode<MeshInstance3D>("SCP173_Rig/Skeleton3D/scp173_MESH").MaterialOverride = mat;
	}
}