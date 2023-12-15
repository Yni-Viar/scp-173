using Godot;
using System;

public partial class PlayerScript : KinematicBody
{
    /* This script was created by dzejpi. License - The Unlicense. 
     * Some parts used from elmarcoh (this script is also public domain).
     */

    Spatial playerHead;
    RayCast ray;

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
    Vector3 Velocity = new Vector3();
    Scp173Npc scp173;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        playerHead = GetNode<Spatial>("PlayerHead");
        ray = GetNode<RayCast>("PlayerHead/RayCast");
        acceleration = groundAcceleration;
        Input.MouseMode = Input.MouseModeEnum.Captured;
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            InputEventMouseMotion m = (InputEventMouseMotion) @event;
            this.RotateY(Mathf.Deg2Rad(-m.Relative.x * mouseSensivity.y / 10));
            playerHead.RotateX(Mathf.Clamp(-m.Relative.y * mouseSensivity.x / 10, -90, 90));

            Vector3 cameraRot = playerHead.Rotation;
            cameraRot.x = Mathf.Clamp(playerHead.Rotation.x, Mathf.Deg2Rad(-85f), Mathf.Deg2Rad(85f));
            playerHead.Rotation = cameraRot;
        }
        direction = new Vector3();
        direction.z = -Input.GetActionStrength("move_forward") + Input.GetActionStrength("move_backward");
        direction.x = -Input.GetActionStrength("move_left") + Input.GetActionStrength("move_right");
        direction = direction.Normalized().Rotated(Vector3.Up, Rotation.y);

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            //GetTree().ChangeScene("res://Scenes/MainMenu.tscn");
            GetTree().Quit();
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (IsOnFloor())
        {
            gravityVector = Vector3.Zero; //if is on floor, gravity should be 0, not previously summed value!
        }
        else
        {
            gravityVector += Vector3.Down * gravity * delta;
        }
        if (Input.IsActionJustPressed("move_jump") && IsOnFloor())
        {
            gravityVector = Vector3.Up * jump;
        }
        if (Input.IsActionPressed("move_sprint"))
        {
            vel = vel.LinearInterpolate(direction * speed * 2, acceleration * delta);
        }
        else
        {
            vel = vel.LinearInterpolate(direction * speed, acceleration * delta);
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
        movement.z = vel.z + gravityVector.z;
        movement.x = vel.x + gravityVector.x;
	    movement.y = gravityVector.y;
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
        MoveAndSlide(Velocity);
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
        playerHead.GetNode<Camera>("PlayerCamera").Far = 0.1f;
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        GetNode<ColorRect>("UI/Blink").Visible = false;
        ray.Enabled = true;
        playerHead.GetNode<Camera>("PlayerCamera").Far = 100.0f;
        blinkTimer = 3f;
    }
}
