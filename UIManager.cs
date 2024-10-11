using Godot;
using System;

public partial class UIManager : Node2D
{

    PackedScene terrainUIScene, cityUIScene;
    TerrainTileUI terrainUI = null;
    CityUI cityUI = null;

    GeneralUi generalUI;

    public delegate void EndTurnEventHandler();
    public event EndTurnEventHandler EndTurn;

    public override void _Ready()
    {
        terrainUIScene = ResourceLoader.Load<PackedScene>("TerrainTileUI.tscn");
        cityUIScene = ResourceLoader.Load<PackedScene>("cityUI.tscn");

        generalUI = GetNode<Panel>("GeneralUI") as GeneralUi;

        Button endTurnButton = generalUI.GetNode<Button>("EndTurnButton");
        endTurnButton.Pressed += SignalEndTurn;
    }

    public void SignalEndTurn()
    {
        EndTurn.Invoke();
        generalUI.IncrementTurnCounter();
        RefreshUI();
    }

    public void SetTerrainUI(Hex h)
    {
        HideAllPopups();

        terrainUI = terrainUIScene.Instantiate() as TerrainTileUI;
        AddChild(terrainUI);
        terrainUI.SetHex(h);

    }

    public void SetCityUI(City c)
    {
        HideAllPopups();
        cityUI = cityUIScene.Instantiate() as CityUI;
        AddChild(cityUI);
        cityUI.SetCityUI(c);
    }

    public void RefreshUI()
    {
        if (cityUI is not null) cityUI.Refresh();
    }

    public void HideAllPopups()
    {
        if (terrainUI is not null)
        {
            terrainUI.QueueFree();
            terrainUI = null;
        }
        if (cityUI is not null)
        {
            cityUI.QueueFree();
            cityUI = null;
        }
    }
}
