using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum TerrainType { PLAINS, WATER, DESERT, MOUNTAIN, ICE, SHALLOW_WATER, FOREST, BEACH, CIV_COLOR_BASE }
public class Hex
{
    public readonly Vector2I coordinates;
    public TerrainType terrainType;

    public int food;

    public int production;

    public City ownerCity;

    public bool isCityCenter = false;

    public Hex(Vector2I coords)
    {
        this.coordinates = coords;
        ownerCity = null;
    }

    public override string ToString()
    {
        return $"Coordinates {this.coordinates.X}, {this.coordinates.Y}. TerrainTypes: {this.terrainType}. Food: {this.food}. Production: {this.production}";
    }

}



public partial class HexTileMap : Node2D
{
    PackedScene cityScene;
    // Height and widht of the map
    [Export]
    public int width = 100;

    [Export]
    public int height = 60;

    [Export]
    public int NUM_AI_CIVS = 6;

    [Export]
    public Color PLAYER_COLOR = new Color(255, 255, 255);

    TileMapLayer baseLayer, civColorsLayer, borderLayer, overlayLayer;

    TileSetAtlasSource terrainAtlas;

    Dictionary<Vector2I, Hex> mapData;
    Dictionary<TerrainType, Vector2I> terrainTextures;

    UIManager uimanager;

    public Dictionary<Vector2I, City> cities;
    public List<Civilization> civs;

    public delegate void SendHexDataEventHandler(Hex h);
    public event SendHexDataEventHandler SendHexData;
    public delegate void SendCityUIInfoEventHandler(City c);
    public event SendCityUIInfoEventHandler SendCityUIInfo;
    public delegate void HideAllPopupsEventHandler();
    public event HideAllPopupsEventHandler HideAllPopups;


    public override void _Ready()
    {
        cityScene = ResourceLoader.Load<PackedScene>("City.tscn");

        baseLayer = GetNode<TileMapLayer>("BaseLayer");
        civColorsLayer = GetNode<TileMapLayer>("CivColorsLayer");
        borderLayer = GetNode<TileMapLayer>("HexBorderLayer");
        overlayLayer = GetNode<TileMapLayer>("SelectionOverlayLayer");

        uimanager = GetNode<UIManager>("/root/Game/CanvasLayer/UIManager");

        this.terrainAtlas = civColorsLayer.TileSet.GetSource(0) as TileSetAtlasSource;

        mapData = new Dictionary<Vector2I, Hex>();
        terrainTextures = new Dictionary<TerrainType, Vector2I>
        {
            {TerrainType.PLAINS, new Vector2I(0,0)},
            {TerrainType.WATER, new Vector2I(1,0)},
            {TerrainType.DESERT, new Vector2I(0,1)},
            {TerrainType.MOUNTAIN, new Vector2I(1,1)},
            {TerrainType.SHALLOW_WATER, new Vector2I(1,2)},
            {TerrainType.BEACH, new Vector2I(0,2)},
            {TerrainType.FOREST, new Vector2I(1,3)},
            {TerrainType.ICE, new Vector2I(0,3)},
            {TerrainType.CIV_COLOR_BASE, new Vector2I(0,3)},
        };

        GenerateTerrain();

        GenerateResources();

        civs = new List<Civilization>();
        cities = new Dictionary<Vector2I, City>();

        List<Vector2I> starts = GenerateCivStartingLocations(NUM_AI_CIVS + 1);

        Civilization playerCiv = CreatePlayerCiv(starts[0]);
        starts.RemoveAt(0);

        GenerateAICivs(starts);

        this.SendHexData += uimanager.SetTerrainUI;
        this.SendCityUIInfo += uimanager.SetCityUI;
        this.HideAllPopups += uimanager.HideAllPopups;
        uimanager.EndTurn += ProcessTurn;

    }

    Vector2I currentSelectedCell = new Vector2I(-1, -1);

    public void ProcessTurn()
    {
        GD.Print("Turn ended");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse)
        {
            Vector2I mapCoords = baseLayer.LocalToMap(ToLocal(GetGlobalMousePosition()));
            if (mapCoords.X >= 0 && mapCoords.X < width && mapCoords.Y >= 0 && mapCoords.Y < height)
            {
                Hex h = mapData[mapCoords];

                if (mouse.ButtonMask == MouseButtonMask.Left)
                {

                    if (cities.ContainsKey(mapCoords))
                    {
                        SendCityUIInfo?.Invoke(cities[mapCoords]);
                    }
                    else
                    {
                        SendHexData?.Invoke(h);
                        if (mapCoords != currentSelectedCell)
                        {

                            overlayLayer.SetCell(currentSelectedCell, -1);
                        }
                        overlayLayer.SetCell(mapCoords, 0, new Vector2I(0, 1));
                        currentSelectedCell = mapCoords;
                    }


                }
            }
            else
            {
                overlayLayer.SetCell(currentSelectedCell, -1);
                HideAllPopups.Invoke();
            }
        }
    }

    public Civilization CreatePlayerCiv(Vector2I start)
    {
        Civilization playerCiv = new Civilization();
        playerCiv.id = 0;
        playerCiv.playerCiv = true;
        playerCiv.territoryColor = new Color(PLAYER_COLOR);

        int id = terrainAtlas.CreateAlternativeTile(terrainTextures[TerrainType.CIV_COLOR_BASE]);
        terrainAtlas.GetTileData(terrainTextures[TerrainType.CIV_COLOR_BASE], id).Modulate = playerCiv.territoryColor;

        playerCiv.territoryColorAltTileId = id;

        civs.Add(playerCiv);
        CreateCity(playerCiv, start, "Player City");

        return playerCiv;
    }

    public void GenerateAICivs(List<Vector2I> civStarts)
    {
        for (int i = 0; i < civStarts.Count; i++)
        {
            Civilization currentCiv = new Civilization
            {
                id = i + 1,
                playerCiv = false
            };

            currentCiv.SetRandomColor();

            int id = terrainAtlas.CreateAlternativeTile(terrainTextures[TerrainType.CIV_COLOR_BASE]);
            terrainAtlas.GetTileData(terrainTextures[TerrainType.CIV_COLOR_BASE], id).Modulate = currentCiv.territoryColor;

            currentCiv.territoryColorAltTileId = id;

            CreateCity(currentCiv, civStarts[i], "City " + civStarts[i].X);

            civs.Add(currentCiv);
        }

    }

    public List<Vector2I> GenerateCivStartingLocations(int numLocations)
    {
        List<Vector2I> locations = new List<Vector2I>();
        List<Vector2I> plainsTiles = new List<Vector2I>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapData[new Vector2I(x, y)].terrainType == TerrainType.PLAINS)
                {
                    plainsTiles.Add(new Vector2I(x, y));
                }
            }
        }

        Random r = new Random();
        for (int i = 0; i < numLocations; i++)
        {
            Vector2I coord = new Vector2I();

            bool valid = false;
            int counter = 0;

            while (!valid && counter < 10000)
            {
                coord = plainsTiles[r.Next(plainsTiles.Count)];
                valid = isValidLocation(coord, locations);
                counter++;
            }

            plainsTiles.Remove(coord);
            foreach (Hex h in GetSurroundingHexes(coord))
            {
                foreach (Hex j in GetSurroundingHexes(h.coordinates))
                {
                    foreach (Hex k in GetSurroundingHexes(j.coordinates))
                    {
                        plainsTiles.Remove(h.coordinates);
                        plainsTiles.Remove(j.coordinates);
                        plainsTiles.Remove(k.coordinates);
                    }
                }
            }
            GD.Print(valid);
            locations.Add(coord);
        }

        return locations;
    }

    private bool isValidLocation(Vector2I coord, List<Vector2I> locations)
    {
        if (coord.X < 3 || coord.X > width - 3 || coord.Y < 3 || coord.Y > height - 3)
        {
            return false;
        }

        foreach (Vector2I l in locations)
        {
            if (Math.Abs(coord.X - l.X) < 20 || Math.Abs(coord.Y - l.Y) < 20)
            {
                return false;
            }

        }

        return true;
    }

    public void CreateCity(Civilization civ, Vector2I coords, string name)
    {
        City city = cityScene.Instantiate() as City;
        city.map = this;
        civ.cities.Add(city);
        city.civ = civ;

        AddChild(city);

        city.SetIconColor(civ.territoryColor);

        city.SetCityName(name);

        city.centerCoordinates = coords;
        city.Position = baseLayer.MapToLocal(coords);
        mapData[coords].isCityCenter = true;

        city.AddTerritory(new List<Hex> { mapData[coords] });

        List<Hex> surrounding = GetSurroundingHexes(coords);
        foreach (Hex h in surrounding)
        {
            if (h.ownerCity == null)
                city.AddTerritory(new List<Hex> { h });
        }

        UpdateCivTerritoryMap(civ);

        cities[coords] = city;
    }

    public void UpdateCivTerritoryMap(Civilization civ)
    {
        foreach (City c in civ.cities)
        {
            foreach (Hex h in c.territory)
            {
                civColorsLayer.SetCell(h.coordinates, 0, terrainTextures[TerrainType.CIV_COLOR_BASE], civ.territoryColorAltTileId);
            }
        }
    }

    public List<Hex> GetSurroundingHexes(Vector2I coords)
    {
        List<Hex> result = new List<Hex>();

        foreach (Vector2I coord in baseLayer.GetSurroundingCells(coords))
        {
            if (HexInBounds(coord))
                result.Add(mapData[coord]);
        }

        return result;
    }

    public bool HexInBounds(Vector2I coords)
    {
        if (coords.X < 0 || coords.X >= width || coords.Y < 0 | coords.Y >= height)
            return false;

        return true;
    }

    public void GenerateResources()
    {
        Random r = new Random();

        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Hex h = mapData[new Vector2I(x, y)];

                switch (h.terrainType)
                {
                    case TerrainType.PLAINS:
                        h.food = r.Next(2, 6);
                        h.production = r.Next(0, 3);
                        break;
                    case TerrainType.FOREST:
                        h.food = r.Next(1, 4);
                        h.production = r.Next(2, 6);
                        break;
                    case TerrainType.DESERT:
                        h.food = r.Next(0, 2);
                        h.production = r.Next(0, 2);
                        break;
                    case TerrainType.BEACH:
                        h.food = r.Next(2, 6);
                        h.production = r.Next(0, 2);
                        break;
                }
            }
        }
    }

    public void GenerateTerrain()
    {
        float[,] noiseMap = new float[width, height];
        float[,] forestMap = new float[width, height];
        float[,] desertMap = new float[width, height];
        float[,] mountainMap = new float[width, height];

        Random r = new Random();
        int seed = r.Next(100000);

        FastNoiseLite noise = new FastNoiseLite();

        noise.Seed = seed;
        noise.Frequency = 0.008f;
        noise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
        noise.FractalOctaves = 4;
        noise.FractalLacunarity = 2.25f;

        float noiseMax = 0f;


        FastNoiseLite forestNoise = new FastNoiseLite();

        forestNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular;
        forestNoise.Seed = seed;
        forestNoise.Frequency = 0.04f;
        forestNoise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
        forestNoise.FractalLacunarity = 2.5f;

        float forestNoiseMax = 0f;

        FastNoiseLite desertNoise = new FastNoiseLite();

        desertNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        desertNoise.Seed = seed;
        desertNoise.Frequency = 0.015f;
        desertNoise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
        desertNoise.FractalLacunarity = 2.5f;

        float desertNoiseMax = 0f;

        FastNoiseLite mountainNoise = new FastNoiseLite();

        mountainNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        mountainNoise.Seed = seed;
        mountainNoise.Frequency = 0.02f;
        mountainNoise.FractalType = FastNoiseLite.FractalTypeEnum.Ridged;
        mountainNoise.FractalLacunarity = 2.5f;

        float mountainNoiseMax = 0f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseMap[x, y] = Math.Abs(noise.GetNoise2D(x, y));
                if (noiseMap[x, y] > noiseMax) noiseMax = noiseMap[x, y];

                desertMap[x, y] = Math.Abs(desertNoise.GetNoise2D(x, y));
                if (desertMap[x, y] > desertNoiseMax) desertNoiseMax = desertMap[x, y];

                forestMap[x, y] = Math.Abs(forestNoise.GetNoise2D(x, y));
                if (forestMap[x, y] > forestNoiseMax) forestNoiseMax = forestMap[x, y];

                mountainMap[x, y] = Math.Abs(mountainNoise.GetNoise2D(x, y));
                if (mountainMap[x, y] > mountainNoiseMax) mountainNoiseMax = mountainMap[x, y];
            }
        }

        List<(float Min, float Max, TerrainType Type)> terrainGenValues = new List<(float Min, float Max, TerrainType Type)>
        {
            (0, noiseMax/10 * 2.5f, TerrainType.WATER),
            (noiseMax/10 * 2.5f, noiseMax/10 * 4f, TerrainType.SHALLOW_WATER),
            (noiseMax/10 * 4f, noiseMax/10 * 4.5f, TerrainType.BEACH),
            (noiseMax/10 * 4.5f, noiseMax + 0.05f, TerrainType.PLAINS),

        };

        Vector2 forestGenValues = new Vector2(forestNoiseMax / 10 * 6, forestNoiseMax + 0.05f);
        Vector2 desertGenValues = new Vector2(desertNoiseMax / 10 * 6, desertNoiseMax + 0.05f);
        Vector2 mountainGenValues = new Vector2(mountainNoiseMax / 10 * 5.5f, mountainNoiseMax + 0.05f);


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Hex h = new Hex(new Vector2I(x, y));
                float noiseValue = noiseMap[x, y];

                h.terrainType = terrainGenValues.First(range => noiseValue >= range.Min && noiseValue < range.Max).Type;

                mapData[new Vector2I(x, y)] = h;

                if (desertMap[x, y] >= desertGenValues[0] && desertMap[x, y] <= desertGenValues[1] && h.terrainType == TerrainType.PLAINS)
                {
                    h.terrainType = TerrainType.DESERT;
                }
                if (forestMap[x, y] >= forestGenValues[0] && forestMap[x, y] <= forestGenValues[1] && h.terrainType == TerrainType.PLAINS && h.terrainType != TerrainType.DESERT)
                {
                    h.terrainType = TerrainType.FOREST;
                }
                if (mountainMap[x, y] >= mountainGenValues[0] && mountainMap[x, y] <= mountainGenValues[1] && h.terrainType == TerrainType.PLAINS)
                {
                    h.terrainType = TerrainType.MOUNTAIN;
                }
                baseLayer.SetCell(new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
                borderLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(0, 0));
            }

        }

        int maxIce = 5;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < r.Next(maxIce) + 1; y++)
            {
                Hex h = mapData[new Vector2I(x, y)];
                h.terrainType = TerrainType.ICE;
                baseLayer.SetCell(new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
            }
            for (int y = height - 1; y > height - 1 - r.Next(maxIce) - 1; y--)
            {
                Hex h = mapData[new Vector2I(x, y)];
                h.terrainType = TerrainType.ICE;
                baseLayer.SetCell(new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
            }
        }
    }

    public Vector2 MapToLocal(Vector2I coords)
    {
        return baseLayer.MapToLocal(coords);
    }

}
