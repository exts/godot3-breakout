using Godot;
using System;
using System.Collections.Generic;

public class Main : Node
{
    [Export]
    public PackedScene BrickScene;

    public Vector2 BrickSize = new Vector2(64, 32);

    public override void _Ready()
    {
        BrickScene = ResourceLoader.Load("res://Brick.tscn") as PackedScene;        

        //generate brick grid
        GenerateBrickGrid();
    }

    protected void GenerateBrickGrid()
    {
        var window_size = GetViewport().GetSize();
        var bricks_per_row = (int) window_size.x/BrickSize.x;
        string[] brick_types = {"green", "blue", "red"};

        int brick_row = 0;
        foreach(var brick_type in brick_types) {
            for(int br = 0; br < bricks_per_row; br++) {
                var brick_pos = new Vector2(br * BrickSize.x + BrickSize.x/2, brick_row * BrickSize.y + BrickSize.y/2);
                var brick = BrickScene.Instance() as Brick;
                brick.Position = brick_pos;
                brick.SetBrickType(brick_type);
                AddChild(brick);
            }
            ++brick_row;
        }
    }
}
