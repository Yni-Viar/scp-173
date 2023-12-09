using Godot;
using System;

public partial class AmmoSystem : Node
{
	//DO NOT BLOAT PLAYERSCRIPT MODULE!!! ;)
    [Export] internal int[] ammo;
	int[] currentAmmo;
    [Export] internal int[] CurrentAmmo;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	internal void ReloadAmmo(int ammoType, int maxAmmo)
	{
        //The main reloading system.
        if (ammo[ammoType] < maxAmmo)
        {
            CurrentAmmo[ammoType] = ammo[ammoType];
            ammo[ammoType] *= 0;
        }
        else
        {
            CurrentAmmo[ammoType] = maxAmmo;
            ammo[ammoType] -= maxAmmo;
        }
    }
}
