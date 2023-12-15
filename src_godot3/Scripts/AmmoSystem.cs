using Godot;
using System;

public partial class AmmoSystem : Node
{
	//Godot 3 for some reason don't support arrays (NOT list) in inspector...
    [Export] internal Godot.Collections.Array<int> ammo = new Godot.Collections.Array<int>();
    [Export] internal Godot.Collections.Array<int> CurrentAmmo = new Godot.Collections.Array<int>();
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
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
