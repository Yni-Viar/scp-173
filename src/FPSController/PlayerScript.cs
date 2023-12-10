using Godot;
using System;

public partial class PlayerScript : CharacterBody3D
{
    /* This script was created by dzejpi. License - The Unlicense. 
     * Some parts used from elmarcoh (this script is also public domain).
     */

    Node3D playerHead;
    RayCast3D ray;

    bool isBlinking;
    [Export] internal bool canMove = true;
    float speed = 4.5f;
    float jump = 4.5f;
    float gravity = 9.8f;

    float blinkTimer = 3.0f;

    float groundAcceleration = 8.0f;
    float airAcceleration = 8.0f;
    float acceleration;

    float slidePrevention = 10.0f;
    Vector2 mouseSensivity = new Vector2(0.125f, 2f);

    Vector3 direction = new Vector3();
    Vector3 vel = new Vector3();
    Vector3 movement = new Vector3();
    Vector3 gravityVector = new Vector3();
    Scp173Npc scp173;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        playerHead = GetNode<Node3D>("PlayerHead");
        ray = GetNode<RayCast3D>("PlayerHead/RayCast3D");
        acceleration = groundAcceleration;
        Input.MouseMode = Input.MouseModeEnum.Captured;
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            InputEventMouseMotion m = (InputEventMouseMotion) @event;
            this.RotateY(Mathf.DegToRad(-m.Relative.X * mouseSensivity.Y / 10));
            playerHead.RotateX(Mathf.Clamp(-m.Relative.Y * mouseSensivity.X / 10, -90, 90));

            Vector3 cameraRot = playerHead.Rotation;
            cameraRot.X = Mathf.Clamp(playerHead.Rotation.X, Mathf.DegToRad(-85f), Mathf.DegToRad(85f));
            playerHead.Rotation = cameraRot;
        }
        direction = new Vector3();
        direction.Z = -Input.GetActionStrength("move_forward") + Input.GetActionStrength("move_backward");
        direction.X = -Input.GetActionStrength("move_left") + Input.GetActionStrength("move_right");
        direction = direction.Normalized().Rotated(Vector3.Up, Rotation.Y);

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsOnFloor())
        {
            gravityVector = Vector3.Zero; //if is on floor, gravity should be 0, not previously summed value!
        }
        else
        {
            gravityVector += Vector3.Down * gravity * (float)delta;
        }
        if (Input.IsActionJustPressed("move_jump") && IsOnFloor())
        {
            gravityVector = Vector3.Up * jump;
        }
        if (Input.IsActionPressed("move_sprint"))
        {
            vel = vel.Lerp(direction * speed * 2, acceleration * (float)delta);
        }
        else
        {
            vel = vel.Lerp(direction * speed, acceleration * (float)delta);
        }


        if (ray.IsColliding() && ray.Enabled)
        {
            var collider = ray.GetCollider();
            if (collider is Scp173Npc scp173instance)
            {
                scp173 = scp173instance;
                scp173.isWatching = true;
            }
            else
            {
                if (scp173 != null)
                {
                    scp173.isWatching = false;
                }
            }
        }
        movement.Z = vel.Z + gravityVector.Z;
        movement.X = vel.X + gravityVector.X;
	    movement.Y = gravityVector.Y;
        if (canMove)
        {
            Velocity = movement;
        }
        else
        {
            Velocity = Vector3.Zero;
        }

        if (blinkTimer < 0f)
        {
            Blink();
        }
        else
        {
            blinkTimer -= (float)delta;
        }
        UpDirection = Vector3.Up;
        MoveAndSlide();
    }

    internal void ChangeState(bool alive)
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetNode<Control>("UI/State").Visible = true;
        if (alive)
        {
            GetNode<Label>("UI/State/Message").Text = "You won :)";
        }
        else
        {
            canMove = false;
            GetNode<Label>("UI/State/Message").Text = "Oh no, SCP-173 crunched your neck :(";
        }
    }

    async void Blink()
    {
        GetNode<ColorRect>("UI/Blink").Visible = true;
        ray.Enabled = false;
        await ToSignal(GetTree().CreateTimer(0.5), "timeout");
        GetNode<ColorRect>("UI/Blink").Visible = false;
        ray.Enabled = true;
        blinkTimer = 3f;
    }
}
