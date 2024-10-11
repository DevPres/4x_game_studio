using Godot;
using System;

public partial class Game : Node
{

    public override void _EnterTree()
    {
        TerrainTileUI.LoadTerrainImages();
    }
}
