using Godot;
using System;

public partial class settings : Node
{
	public static settings Instance {get; private set; } public override void _Ready() { Instance = this; }
	public bool host = false;
	public string connection_address = "";

	public bool controller_toggle = true;
}
