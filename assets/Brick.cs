using Godot;
using System;

public class Brick : RigidBody2D
{
    public Vector2 Size = new Vector2(64, 32);

    public override void _Ready()
    {
    }

    public void SetBrickType(string brick_type)
    {
        var BrickSprite = GetNode("AnimatedSprite") as AnimatedSprite;  
        BrickSprite.Animation = brick_type;
    }

}
