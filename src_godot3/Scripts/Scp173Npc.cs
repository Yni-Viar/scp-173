using Godot;
using System;

public partial class Scp173Npc : KinematicBody
{
    RandomNumberGenerator rng = new RandomNumberGenerator();
    Spatial target;
    AudioStreamPlayer3D walkSound;
    AudioStreamPlayer3D interactSound;
    Vector3 Velocity = new Vector3();

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

    public override void _PhysicsProcess(float delta)
    {
        // basic SCP-173 rules - player don't see - 173 is closer.
        if ((canMove || !isWatching) && target != null)
        {
            var playerDirection = (target.Transform.origin - Transform.origin).Normalized();
            Velocity += speed * playerDirection * (float)delta;
            this.LookAt(target.Transform.origin, Vector3.Up);

            walkSound.Stream = GD.Load<AudioStream>("res://Sounds/Character/173/Rattle" + rng.RandiRange(1, 3) + ".ogg");
            walkSound.Play();
        }
        // else SCP-173 must stay still!
        else
        {
            Velocity = Vector3.Zero;
        }
        MoveAndSlide(Velocity);

        // Crunch handler
        for (int i = 0; i < GetSlideCount(); i++)
        {
            KinematicCollision checkCollision = GetSlideCollision(i);
            var collidedWith = checkCollision.Collider;
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

    private void OnFindPlayerAreaBodyEntered(Spatial body)
    {
        if (body.IsInGroup("Players"))
        {
            target = body;
        }
    }


    private void OnFindPlayerAreaBodyExited(Spatial body)
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
            Spatial node = ResourceLoader.Load<PackedScene>("res://Assets/NPC/scp173_fracture.tscn").Instance<Spatial>();
            node.Translation = Transform.origin;
            GetParent().AddChild(node);
            QueueFree();
        }
    }

    void SetRandomFace()
    {
        ShaderMaterial mat = new ShaderMaterial();
        mat.Shader = ResourceLoader.Load<Shader>("res://Shaders/MixShader/mix.gdshader");
        mat.SetShaderParam("texture_a", ResourceLoader.Load<Texture>("res://Assets/Models/scp173_base/scp173NEO_low_Merged_PM3D_Sphere3D4_AlbedoTransparency.png"));
        mat.SetShaderParam("texture_b", ResourceLoader.Load<Texture>("res://Assets/Models/scp173_faces/face_" + rng.RandiRange(1, 10).ToString() + ".png"));
        GetNode<MeshInstance>("SCP173_Rig/Skeleton/scp173_MESH").MaterialOverride = mat;
    }
    
    
    private void OnVisibilityNotifierScreenEntered()
    {
        isWatching = true;
    }


    private void OnVisibilityNotifierScreenExited()
    {
        isWatching = false;
    }
}
