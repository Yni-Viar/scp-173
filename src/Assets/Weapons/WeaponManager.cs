using Godot;
using System;
/// <summary>
/// Weapon Manager.
/// THIS CODE WAS WRITTEN BY HUBERT MOSZKA FOR SCP: SECRET LAB v.6.0.0.
/// Author's commentary:
/// This stream is licensed under the CC-BY-SA 3.0 license
/// 
/// If stream is licensed under this license, so the code is also lice3nsed by this license (I think...)
/// </summary>
public partial class WeaponManager : Node3D
{
    RandomNumberGenerator rng = new RandomNumberGenerator();

    //generic weapon properties
    [Export] int maxAmmo;
    [Export] int ammoType;
    [Export] float damage;
    [Export] float reloadingTime;
    [Export] bool isAuto;
    [Export] float weaponCooldown;
    float fireCooldown;
    [Export] bool isTranquilizer = false;

    //Zooming
    [Export] bool allowZoom;
    [Export] float zoomFov = 80;
    //[Export] float targetFov;
    [Export] float fovAjustingSpeed;
    bool zoomed = false;

    //Recoil
    [Export] float recoilScale;
    [Export] float recoilSpeed;
    Vector3 recoilTarget;

    //Godot-specific variables
    [Export] Godot.Collections.Array<AudioStream> poofSounds = new Godot.Collections.Array<AudioStream>();
    [Export] AudioStream reloadSound;
    Camera3D camera;
    AudioStreamPlayer3D audio;
    RayCast3D rayCast;
    AnimationPlayer anim;


    public override void _Ready()
    {
        recoilTarget = new Vector3 { X = rng.RandiRange(-20, 20), Y = rng.RandiRange(0, 70), Z = rng.RandiRange(-20, 20) };
        audio = GetNode<AudioStreamPlayer3D>("WeaponSound");
        camera = GetParent().GetParent().GetNode<Camera3D>("PlayerCamera");
        rayCast = GetNode<RayCast3D>("ShootingRange");
    }

    public override void _Process(double delta)
    {
        if (isAuto ? Input.IsActionPressed("weapon_shoot") : Input.IsActionJustPressed("weapon_shoot"))
        {
            if (IsDeductCooldownEnded())
            {
                Shoot();
            }
        }
        if (Input.IsActionJustPressed("weapon_zoom") && allowZoom)
        {
            UpdateFov();
        }
        if (Input.IsActionJustPressed("weapon_reload") && GetParent().GetParent().GetParent<PlayerScript>().GetNode<AmmoSystem>("AmmoSystem").ammo[ammoType] > 0)
        {
            Reload();
        }
        if (fireCooldown > 0)
        {
            fireCooldown -= (float)delta;
        }
    }
    /// <summary>
    /// Spawn bullet shot (if is not a player)
    /// </summary>
    /// <param name="point">Point, where bullet collided</param>
    /// <param name="normal">Normal of the point (for rotating the bullet to player)</param>
    void SpawnDecal(Vector3 point, Vector3 normal)
    {
        CpuParticles3D smokeDecal = ResourceLoader.Load<PackedScene>("res://Decals/smoke.tscn").Instantiate<CpuParticles3D>();
        GetTree().Root.GetNode<GameManager>("Game").AddChild(smokeDecal);
        smokeDecal.GlobalPosition = point;
        if (normal == Vector3.Down)
        {
            smokeDecal.LookAt(normal, Vector3.Down);
        }
        else
        {
            smokeDecal.LookAt(normal);
        }
    }
    /// <summary>
    /// The main shooting mechanic
    /// </summary>
    void Shoot()
    {
        if (rayCast.IsColliding() && GetParent().GetParent().GetParent<PlayerScript>().GetNode<AmmoSystem>("AmmoSystem").CurrentAmmo[ammoType] > 0)
        {
            var collider = rayCast.GetCollider();
            if (collider is Scp173Npc scp173)
            {
                //Deplete HP
                //playerScript.RpcId(int.Parse(playerScript.Name), "HealthManage", -damage / GlobalPosition.DistanceTo(playerScript.GlobalPosition), "Shot by " + Name);

                scp173.HealthManage(-damage / GlobalPosition.DistanceTo(scp173.GlobalPosition));
            }
            else
            {
                SpawnDecal(rayCast.GetCollisionPoint() + rayCast.GetCollisionNormal() * 0.9f, rayCast.GetCollisionNormal()); //Hit point
            }
            //Minus one bullet
            GetParent().GetParent().GetParent<PlayerScript>().GetNode<AmmoSystem>("AmmoSystem").CurrentAmmo[ammoType]--;
            //Play animation and audio
            audio.Stream = poofSounds[rng.RandiRange(0, poofSounds.Count - 1)];
            audio.Play();
            fireCooldown = weaponCooldown;
        }
        Vector3 recoilVector = Vector3.Zero.Lerp(recoilTarget, recoilScale);
        GetParent().GetParent().GetParent<PlayerScript>().GetNode<Node3D>("PlayerHead").Rotation.Lerp(recoilVector, recoilSpeed);
    }
    /// <summary>
    /// Reloading mechanic.
    /// </summary>
    void Reload()
    {
        //The main reloading system.
        GetParent().GetParent().GetParent<PlayerScript>().GetNode<AmmoSystem>("AmmoSystem").ReloadAmmo(ammoType, maxAmmo);
        //Sounds
        if (GetParent().GetParent().GetParent<PlayerScript>().IsMultiplayerAuthority())
        {
            SetState("reload");
        }
        if (reloadSound != null)
        {
            audio.Stream = reloadSound;
            audio.Play();
        }
    }
    /// <summary>
    /// Check fire cooldown
    /// </summary>
    /// <returns>If is cooldown - then false, else true</returns>
    bool IsDeductCooldownEnded()
    {
        if (fireCooldown > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    /// <summary>
    /// Changes player FOV
    /// </summary>
    void UpdateFov()
    {
        camera.Fov = Mathf.Lerp(zoomed ? zoomFov : camera.Fov, zoomed ? camera.Fov : zoomFov, fovAjustingSpeed);
        zoomed = !zoomed;
    }
    /// <summary>
    /// Set animation to a weapon.
    /// </summary>
    /// <param name="s">Animation name</param>
    private void SetState(string s)
    {
        if (GetNode<AnimationPlayer>("AnimationPlayer").CurrentAnimation != s || !GetNode<AnimationPlayer>("AnimationPlayer").IsPlaying())
        {
            //Change the animation.
            GetNode<AnimationPlayer>("AnimationPlayer").Play(s);
        }
    }
}
