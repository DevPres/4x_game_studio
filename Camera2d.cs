using Godot;
using System;



public partial class Camera2d : Camera2D
{
    // Height and widht of the map
    [Export]
    public float velocity = 15;

    [Export]
    public float zoom_speed = 0.05f;


    bool mouseWheelScrollingUp = false;
    bool mouseWheelScrollingDown = false;

    bool zooming = true;
    float originalVelocity = 0;


    float leftBound, rightBound, topBound, bottomBound;
    HexTileMap map;

    public override void _Ready()
    {
        map = GetNode<HexTileMap>("../HexTileMap");

        originalVelocity = velocity;

        leftBound = ToGlobal(map.MapToLocal(new Vector2I(0, 0))).X - 400;
        rightBound = ToGlobal(map.MapToLocal(new Vector2I(map.width, 0))).X - 400;
        topBound = ToGlobal(map.MapToLocal(new Vector2I(0, 0))).Y - 400;
        bottomBound = ToGlobal(map.MapToLocal(new Vector2I(0, map.height))).Y - 400;
    }

    public override void _PhysicsProcess(double delta)
    {


        if (Input.IsActionPressed("map_right") && this.Position.X < rightBound)
        {
            this.Position += new Vector2(velocity, 0);
        }
        if (Input.IsActionPressed("map_left") && this.Position.X > leftBound)
        {
            this.Position += new Vector2(-velocity, 0);
        }
        if (Input.IsActionPressed("map_up") && this.Position.Y > topBound)
        {
            this.Position += new Vector2(0, -velocity);
        }
        if (Input.IsActionPressed("map_down") && this.Position.Y < bottomBound)
        {
            this.Position += new Vector2(0, velocity);
        }

        if (Input.IsActionPressed("map_zoom_in") || mouseWheelScrollingUp)
        {
            if (this.Zoom < new Vector2(3f, 3f))
                this.Zoom += new Vector2(zoom_speed, zoom_speed);
        }

        if (Input.IsActionPressed("map_zoom_out") || mouseWheelScrollingDown)
        {
            if (this.Zoom > new Vector2(0.1f, 0.1f))
                this.Zoom -= new Vector2(zoom_speed, zoom_speed);
        }
        if (!Input.IsActionJustReleased("mouse_zoom_in"))
        {
            mouseWheelScrollingUp = false;
            zooming = false;
        }

        if (!Input.IsActionJustReleased("mouse_zoom_out"))
        {
            mouseWheelScrollingDown = false;
            zooming = false;
        }

        if (Input.IsActionJustReleased("mouse_zoom_in"))
        {
            mouseWheelScrollingUp = true;
            zooming = true;
        }

        if (Input.IsActionJustReleased("mouse_zoom_out"))
        {
            mouseWheelScrollingDown = true;
            zooming = true;
        }


        if (Input.IsActionJustPressed("map_zoom_in") || Input.IsActionJustPressed("map_zoom_out"))
        {
            zooming = true;
        }
        if (Input.IsActionJustReleased("map_zoom_in") || Input.IsActionJustReleased("map_zoom_out"))
        {
            zooming = false;
        }
        if (zooming)
        {
            velocity = originalVelocity * (1 / this.Zoom.X);
        }

    }


}
