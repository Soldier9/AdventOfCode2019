﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Solvers
{
    class Day18Solver : AbstractSolver
    {
        class Location
        {
            public readonly Point Position;
            public readonly char Type;

            public int Steps;
            public Location Previous;

            public Location(Location location)
            {
                Position = location.Position;
                Type = location.Type;
                Steps = location.Steps;
            }

            public Location(Point position, char type)
            {
                Position = position;
                Type = type;
            }

            public override bool Equals(object obj)
            {
                return obj is Location location &&
                       EqualityComparer<Point>.Default.Equals(Position, location.Position);
            }

            public override int GetHashCode()
            {
                return -425505606 + Position.GetHashCode();
            }

            public IEnumerable<Point> NeighboorPositions()
            {
                yield return new Point(Position.X - 1, Position.Y);
                yield return new Point(Position.X, Position.Y - 1);
                yield return new Point(Position.X + 1, Position.Y);
                yield return new Point(Position.X, Position.Y + 1);
            }
        }

        class StateEqualityComparer : IEqualityComparer<Tuple<Point, BitArray>>
        {
            public bool Equals(Tuple<Point, BitArray> x, Tuple<Point, BitArray> y)
            {
                if (x.Item1 != y.Item1 || x.Item2.Length != y.Item2.Length) return false;
                for (var i = 0; i < x.Item2.Length; i++)
                {
                    if (x.Item2[i] != y.Item2[i]) return false;
                }
                return true;
            }

            public int GetHashCode(Tuple<Point, BitArray> obj)
            {
                int hash = obj.Item1.GetHashCode();
                for (var i = 0; i < obj.Item2.Length; i++) if(obj.Item2[i]) hash += 2356345 * i;
                return hash;
            }
        }


        Dictionary<Point, Location> Maze = new Dictionary<Point, Location>();
        Dictionary<Tuple<Point, BitArray>, HashSet<Location>> CachedReachableKeys = new Dictionary<Tuple<Point, BitArray>, HashSet<Location>>(new StateEqualityComparer());

        HashSet<Location> SearchForReachableKeys(Point currentPosition, BitArray collectedKeys, int stepsSoFar)
        {
            var currentState = Tuple.Create(currentPosition, collectedKeys);
            HashSet<Location> reachableKeys;
            if (CachedReachableKeys.ContainsKey(currentState))
            {
                reachableKeys = CachedReachableKeys[currentState];
            }
            else
            {
                reachableKeys = new HashSet<Location>();
                var queue = new Queue<Location>();

                foreach (var location in Maze.Values) location.Steps = int.MaxValue;
                Maze[currentPosition].Steps = 0;
                queue.Enqueue(Maze[currentPosition]);

                while (queue.Count > 0)
                {
                    var currentLocation = queue.Dequeue();

                    if (currentLocation.Type >= 97 && currentLocation.Type <= 122 && !collectedKeys[currentLocation.Type - 97])
                    {
                        reachableKeys.Add(new Location(currentLocation));
                        continue;
                    }

                    foreach (var neighboor in currentLocation.NeighboorPositions())
                    {
                        if (!Maze.ContainsKey(neighboor)
                            || (Maze[neighboor].Type >= 65 && Maze[neighboor].Type <= 90 && !collectedKeys[Maze[neighboor].Type - 65])) continue;

                        if (Maze[neighboor].Steps == int.MaxValue)
                        {
                            Maze[neighboor].Steps = currentLocation.Steps + 1;
                            Maze[neighboor].Previous = currentLocation;
                            queue.Enqueue(Maze[neighboor]);
                        }
                    }
                }

                Location bestKeyInThisIteration = null;
                var bestReturnedKeys = new HashSet<Location>();
                int bestReturnedSteps = int.MaxValue;

                foreach (var k in reachableKeys)
                {
                    var recursiveCollectedKeys = (BitArray)collectedKeys.Clone();
                    recursiveCollectedKeys[k.Type - 97] = true;
                    var returnedKeys = SearchForReachableKeys(k.Position, recursiveCollectedKeys, k.Steps + stepsSoFar);
                    if (bestReturnedKeys.Count == 0 || returnedKeys.Count > 0 && bestReturnedSteps > returnedKeys.Max(l => l.Steps) + k.Steps)
                    {
                        bestReturnedKeys = returnedKeys;
                        bestKeyInThisIteration = k;
                        bestReturnedSteps = k.Steps + (bestReturnedKeys.Count == 0 ? 0 : bestReturnedKeys.Max(l => l.Steps)) ;
                    }
                }

                var bestReturnedKeysCloned = new HashSet<Location>();
                if (bestKeyInThisIteration != null)
                {
                    foreach (var k in bestReturnedKeys)
                    {
                        var clonedKey = new Location(k);
                        clonedKey.Steps += bestKeyInThisIteration.Steps;
                        bestReturnedKeysCloned.Add(clonedKey);
                    }
                    bestReturnedKeysCloned.Add(new Location(bestKeyInThisIteration));
                }
                reachableKeys = bestReturnedKeysCloned;

                CachedReachableKeys.Add(currentState, reachableKeys);
            }

            return reachableKeys;
        }


        public override string Part1()
        {
            Point currentPosition = new Point();
            using (var input = File.OpenText(InputFile))
            {
                int y = 0;
                while (!input.EndOfStream)
                {
                    int x = -1;
                    foreach (var c in input.ReadLine())
                    {
                        x++;
                        if (c == '#') continue;
                        var location = new Location(new Point(x, y), c);
                        Maze.Add(location.Position, location);
                        if (location.Type == '@') currentPosition = location.Position;
                    }
                    y++;
                }
            }

            return SearchForReachableKeys(currentPosition, new BitArray(26), 0).Max(k => k.Steps).ToString();
        }

        public override string Part2()
        {
            throw new NotImplementedException();
        }
    }
}
