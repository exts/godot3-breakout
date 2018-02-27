using Godot;
using System;

namespace scripts
{
    public class Paddle : Area2D
    {
        [Signal] public delegate void PaddleHit();
        [Export] public const int PADDLE_SPEED = 200;

        public override void _Ready()
        {
        }
    }
}