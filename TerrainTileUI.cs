using Godot;
using System;
using System.Collections.Generic;

public partial class TerrainTileUI : Panel
{
    public static Dictionary<TerrainType, string> terrainTypeStrings = new Dictionary<TerrainType, string>
    {
        { TerrainType.PLAINS, "Plains"},
        { TerrainType.FOREST, "Forest"},
        { TerrainType.MOUNTAIN, "Mountain"},
        { TerrainType.DESERT, "Desert"},
        { TerrainType.BEACH, "Beach"},
        { TerrainType.WATER, "Water"},
        { TerrainType.SHALLOW_WATER, "Shallow Water"},
        { TerrainType.ICE, "Ice"},
    };

    public static Dictionary<TerrainType, Texture2D> terrainTypeImages = new();
    public static void LoadTerrainImages()
    {
        Texture2D plains = ResourceLoader.Load("res://img/plains.jpg") as Texture2D;
        Texture2D forest = ResourceLoader.Load("res://img/forest.jpg") as Texture2D;
        Texture2D mountain = ResourceLoader.Load("res://img/mountain.jpg") as Texture2D;
        Texture2D desert = ResourceLoader.Load("res://img/desert.jpg") as Texture2D;
        Texture2D beach = ResourceLoader.Load("res://img/beach.jpg") as Texture2D;
        Texture2D water = ResourceLoader.Load("res://img/water.jpg") as Texture2D;
        Texture2D shallowWater = ResourceLoader.Load("res://img/shallow.jpg") as Texture2D;
        Texture2D ice = ResourceLoader.Load("res://img/ice.jpg") as Texture2D;

        terrainTypeImages = new Dictionary<TerrainType, Texture2D>
        {
            { TerrainType.PLAINS, plains},
            { TerrainType.FOREST, forest},
            { TerrainType.MOUNTAIN, mountain},
            { TerrainType.DESERT, desert},
            { TerrainType.BEACH, beach},
            { TerrainType.WATER, water},
            { TerrainType.SHALLOW_WATER, shallowWater},
            { TerrainType.ICE, ice},
        };
    }
    Hex h = null;
    TextureRect terrainImage;
    Label terrainLabel, foodLabel, productionLabel;

    public override void _Ready()
    {
        terrainLabel = GetNode<Label>("TerrainLabel");
        foodLabel = GetNode<Label>("FoodLabel");
        productionLabel = GetNode<Label>("ProductionLabel");
        terrainImage = GetNode<TextureRect>("TerrainImage");
    }

    public void SetHex(Hex h)
    {
        this.h = h;

        terrainImage.Texture = terrainTypeImages[h.terrainType];
        foodLabel.Text = $"Food: {h.food}";
        productionLabel.Text = $"Production: {h.production}";
        terrainLabel.Text = $"Terrain: {terrainTypeStrings[h.terrainType]}";
    }
}
