﻿using Microsoft.Xna.Framework;
using OctoAwesome.Components;
using OctoAwesome.Model.Blocks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noises;

namespace OctoAwesome.Model
{
    public sealed class World
    {
        private readonly Vector3[] CollisionOrder = new[] 
        {
            // new Vector3(0, -2, 0),

            // Block direkt unter dem Player
            new Vector3(0, -1, 0),

            // Blocks am Boden um den Player
            new Vector3(-1, -1, 0),new Vector3(1, -1, 0),new Vector3(0, -1, -1),new Vector3(0, -1, 1),

            // Kollision mit der Decke
            new Vector3(0, 6, 0),
            new Vector3(-1, 6, 0),new Vector3(1, 6, 0),new Vector3(0, 6, -1),new Vector3(0, 6, 1),
            new Vector3(0, 5, 0),                
            new Vector3(-1, 5, 0),new Vector3(1, 5, 0),new Vector3(0, 5, -1),new Vector3(0, 5, 1),
            new Vector3(0, 4, 0),                
            new Vector3(-1, 4, 0),new Vector3(1, 4, 0),new Vector3(0, 4, -1),new Vector3(0, 4, 1),
            new Vector3(0, 3, 0),                
            new Vector3(-1, 3, 0),new Vector3(1, 3, 0),new Vector3(0, 3, -1),new Vector3(0, 3, 1),
            new Vector3(0, 2, 0),                
            new Vector3(-1, 2, 0),new Vector3(1, 2, 0),new Vector3(0, 2, -1),new Vector3(0, 2, 1),
            new Vector3(0, 1, 0),                
            new Vector3(-1, 1, 0),new Vector3(1, 1, 0),new Vector3(0, 1, -1),new Vector3(0, 1, 1),
            new Vector3(0, 0, 0),                
            new Vector3(-1, 0, 0),new Vector3(1, 0, 0),new Vector3(0, 0, -1),new Vector3(0, 0, 1),
        };

        public ChunkManager ChunkM { get; private set; }

        public Player Player { get; private set; }

        public World(IInputSet input)
        {
            
            ChunkM = new ChunkManager(new PerlinNoiseGenerator(100));
            Player = new Player(input);
        }

        public void Update(GameTime frameTime)
        {
            Player.Update(frameTime);

            // Modifikation der Geschwindigkeit
            Player.Velocity += Player.Mass * new Vector3(0, -5f, 0) * (float)frameTime.ElapsedGameTime.TotalSeconds;

            // velocity *= cell.VelocityFactor;
            Vector3 move = Player.Velocity * (float)frameTime.ElapsedGameTime.TotalSeconds;

            BoundingBox playerBox = new BoundingBox(
                new Vector3(
                    Player.Position.X + move.X - Player.Radius,
                    Player.Position.Y + move.Y,
                    Player.Position.Z + move.Z - Player.Radius),
                new Vector3(
                    Player.Position.X + move.X + Player.Radius,
                    Player.Position.Y + move.Y + 4f,
                    Player.Position.Z + move.Z + Player.Radius));

            Player.OnGround = false;

            foreach (var collisionBox in CollisionOrder)
            {
                int x = (int)(collisionBox.X + Player.Position.X + move.X);
                int y = (int)(collisionBox.Y + Player.Position.Y + move.Y);
                int z = (int)(collisionBox.Z + Player.Position.Z + move.Z);

                int chunkX = (int)((Player.Position.X) / Chunk.CHUNKSIZE_X);
                int chunkZ = (int)((Player.Position.Z) / Chunk.CHUNKSIZE_Z);

                Chunk chunk = ChunkM.GetChunk(x, 0, z);



                //if (x < 0 || x >= Chunk.CHUNKSIZE_X ||
                //    y < 0 || y >= Chunk.CHUNKSIZE_Y ||
                //    z < 0 || z >= Chunk.CHUNKSIZE_Z)
                //    continue;

                IBlock block = ChunkM.GetBlock(x, y, z);

                if (block == null)
                    continue;

                BoundingBox[] boxes = block.GetCollisionBoxes();

                foreach (var box in boxes)
                {
                    BoundingBox transformedBox = new BoundingBox(
                        box.Min + new Vector3(x, y, z),
                        box.Max + new Vector3(x, y, z));

                    // (1) Kollisionscheck
                    bool collisionX = (transformedBox.Min.X < playerBox.Max.X && transformedBox.Max.X > playerBox.Min.X);
                    bool collisionY = (transformedBox.Min.Y < playerBox.Max.Y && transformedBox.Max.Y > playerBox.Min.Y);
                    bool collisionZ = (transformedBox.Min.Z < playerBox.Max.Z && transformedBox.Max.Z > playerBox.Min.Z);

                    float gap = 0.01f;

                    if (collisionX && collisionY && collisionZ)
                    {
                        // (2) Kollisionszeitpunkt ermitteln
                        float nx = 1f;
                        if (move.X > 0)
                        {
                            float diff = playerBox.Max.X - transformedBox.Min.X;
                            if (diff < move.X) nx = 1f - (diff / move.X) - gap;

                        }
                        else if (move.X < 0)
                        {
                            float diff = transformedBox.Max.X - playerBox.Min.X;
                            if (diff < -move.X) nx = 1f - (diff / -move.X) - gap;
                        }

                        float ny = 1f;
                        if (move.Y > 0)
                        {
                            float diff = playerBox.Max.Y - transformedBox.Min.Y;
                            if (diff < move.Y) ny = 1f - (diff / move.Y) - gap;

                        }
                        else if (move.Y < 0)
                        {
                            float diff = transformedBox.Max.Y - playerBox.Min.Y;
                            if (diff < -move.Y) // TODO: Toleranzfall (kleine Stufe) if (diff < -move.Y + 1.1f)
                            {
                                ny = 1f - (diff / -move.Y) - gap;
                                Player.Velocity = new Vector3(Player.Velocity.X, 0, Player.Velocity.Z);
                                Player.OnGround = true;
                            }
                        }

                        float nz = 1f;
                        if (move.Z > 0)
                        {
                            float diff = playerBox.Max.Z - transformedBox.Min.Z;
                            if (diff < move.Z) nz = 1f - (diff / move.Z) - gap;

                        }
                        else if (move.Z < 0)
                        {
                            float diff = transformedBox.Max.Z - playerBox.Min.Z;
                            if (diff < -move.Z) nz = 1f - (diff / -move.Z) - gap;
                        }

                        // (3) Kollisionsauflösung 
                        if (nx < ny)
                        {
                            if (nx < nz) move = new Vector3(move.X * nx, move.Y, move.Z);
                            else move = new Vector3(move.X, move.Y, move.Z * nz);
                        }
                        else
                        {
                            if (ny < nz) move = new Vector3(move.X, move.Y * ny, move.Z);
                            else move = new Vector3(move.X, move.Y, move.Z * nz);
                        }
                    }
                }
            }

            Player.Position += move;
        }
    }
}