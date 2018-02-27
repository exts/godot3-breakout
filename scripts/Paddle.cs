using Godot;
using System;

namespace scripts
{
    public class Paddle : Area2D
    {
        // just needed a signal to emit when the paddle gets hit
        [Signal] public delegate void PaddleHit();

        public const int PADDLE_SPEED = 200;

        public override void _Ready()
        {
        }
    }
}