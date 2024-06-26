﻿using System.Runtime.Versioning;

namespace ConsoleGame;

static class Program
{
    // static Mesh MeshCube => Mesh.MakeCube();
    // static Mesh MeshSpaceship => Obj.LoadFile($"{AssetsRuntime}VideoShip.obj");
    // static Mesh MeshTeapot => Obj.LoadFile($"{AssetsRuntime}teapot.obj");
    // static Mesh MeshAxis => Obj.LoadFile($"{AssetsRuntime}axis.obj");
    // static Mesh MeshMountains => Obj.LoadFile($"{AssetsRuntime}mountains.obj");
    // static Mesh MeshTerrain => Obj.LoadFile($"{AssetsRuntime}uploads_files_3707747_landscape.obj").Scale(10f);
    // static Mesh MeshYeah => Obj.LoadFile(@"C:\Users\bazsi\Desktop\yeah.obj");
    // 
    // static Image ImgUv => Ppm.LoadFile($"{AssetsRuntime}bruh.ppm");

    [SupportedOSPlatform("windows")]
    static void Main(string[] args)
    {
        /*
        {
            Color[] p = new Color[32 * 32];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    p[x + (y * 32)] = new Color(x / 32f, y / 32f, 0);
                }
            }
            Image img = new(p, 32, 32);
            Ppm.SaveFile(img, @"C:\Users\bazsi\Desktop\uv.ppm");
        }
        return;
        */
        /*
        {
            Rect size = new(0, 0, 1000, 1000);
            StaticQuadTree<int> ints1 = new(size);
            List<Rect> ints2 = new();

            for (int i = 0; i < 1000000; i++)
            {
                Vector point = Random.Point(size);
                int v = Random.Integer(int.MinValue, int.MaxValue);
                Rect r = new(point, Vector.One);

                ints1.Add(v, r);
                ints2.Add(r);
            }

            int overlapped;
            Rect overlapRect = new(100, 100, 100, 100);
            Stopwatch sw = new();

            Console.WriteLine($"Linear");
            for (int i = 0; i < 16; i++)
            {
                overlapped = 0;

                sw.Restart();
                for (int j = 0; j < ints2.Count; j++)
                {
                    if (overlapRect.Overlaps(ints2[j]))
                    {
                        overlapped++;
                    }
                }
                sw.Stop();

                if (i == 0) Console.WriteLine($"Overlapped: {overlapped}");
                Console.WriteLine($"{sw.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture)} ms");
            }

            Console.WriteLine($"QuadTree");
            for (int i = 0; i < 16; i++)
            {
                overlapped = 0;

                sw.Restart();
                overlapped += ints1.Search(overlapRect).Length;
                sw.Stop();

                if (i == 0) Console.WriteLine($"Overlapped: {overlapped}");
                Console.WriteLine($"{sw.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture)} ms");
            }
        }
        return;
        */
        /*
        if (false) // { 0.00 - 0.31(Avg: 0.09, N: 16581375) }
        {
            Yeah yeah = new();
            for (byte r = byte.MinValue; r < byte.MaxValue; r++)
                for (byte g = byte.MinValue; g < byte.MaxValue; g++)
                    for (byte b = byte.MinValue; b < byte.MaxValue; b++)
                    {
                        Color color = Color.From24bitRGB(r, g, b);
                        byte irgb = Color.To4bitIRGB_BruteForce(color);
                        float error = Color.Distance((Color)irgb, color);
                        yeah.Add(error);
                    }

            Console.WriteLine(yeah);
        }

        if (false) // { 0.00 - 1.35 (Avg: 0.28, N: 16581375) }
        {
            Yeah yeah = new();
            for (byte r = byte.MinValue; r < byte.MaxValue; r++)
                for (byte g = byte.MinValue; g < byte.MaxValue; g++)
                    for (byte b = byte.MinValue; b < byte.MaxValue; b++)
                    {
                        Color color = Color.From24bitRGB(r, g, b);
                        byte irgb = Color.To4bitIRGB(color);
                        float error = Color.Distance((Color)irgb, color);
                        yeah.Add(error);
                    }

            Console.WriteLine(yeah);
        }

        if (true) // { 0.00 - 0.38 (Avg: 0.10, N: 16581375) }
        {
            Yeah yeah = new();
            for (byte r = byte.MinValue; r < byte.MaxValue; r++)
                for (byte g = byte.MinValue; g < byte.MaxValue; g++)
                    for (byte b = byte.MinValue; b < byte.MaxValue; b++)
                    {
                        Color color = Color.From24bitRGB(r, g, b);
                        byte irgb = Color.RgbiApprox(color);
                        float error = Color.Distance((Color)irgb, color);
                        yeah.Add(error);
                    }

            Console.WriteLine(yeah);
        }

        return;
        */

        args = new string[]
        {
            // "-3D",
            // Assets.GetAsset("uploads_files_3707747_landscape.obj")!,
            // @"C:\Users\bazsi\Desktop\yeah.obj",
        };

        try
        {
            if (args.Length == 0)
            {
                Game game = new();
                game.Start();
            }
            else if (args.Length is 2 or 3)
            {
                if (args[0].ToLowerInvariant() != "-3d")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unexpected argument \"{args[0]}\" at {0}.");
                    Console.ResetColor();
                    return;
                }

                string objFilePath = args[1];

                if (!File.Exists(objFilePath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"File \"{objFilePath}\" does not exists");
                    Console.ResetColor();
                    return;
                }

                string? imgFilePath = (args.Length == 3) ? args[2] : null;

                if (imgFilePath != null && !File.Exists(imgFilePath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"File \"{imgFilePath}\" does not exists");
                    Console.ResetColor();
                    return;
                }

                MeshRenderer meshRenderer = new(objFilePath, imgFilePath);
                meshRenderer.Start();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Wrong number of arguments passed. Expected 0, 2 or 3, got {args.Length}.");
                Console.ResetColor();
                return;
            }
        }
        catch (Win32.WindowsException windowsException)
        { windowsException.ShowMessageBox(); }
        finally
        {
            Sound.Dispose();
        }
    }
}
