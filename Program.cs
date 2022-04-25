using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    private static Base MyBase;
    private static int HeroesPerPlayer;
    private static State CurrentState;

    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        MyBase = new Base
        {
            IsMine = true,
            Position = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]))
        };
        var startPointX = MyBase.Position.X > 8800 ? MyBase.Position.X - 1000 : MyBase.Position.X + 1000;
        var startPointY = MyBase.Position.Y > 4400 ? MyBase.Position.Y - 1700 : MyBase.Position.Y + 1700;
        MyBase.StartPoint = new Point(startPointX, startPointY);

        HeroesPerPlayer = int.Parse(Console.ReadLine()); // Always 3

        // game loop
        while (true)
        {
            CurrentState = new State();
            inputs = Console.ReadLine()!.Split(' ');
            CurrentState.MyHealth = int.Parse(inputs[0]); // Your base health
            CurrentState.MyMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell
            
            inputs = Console.ReadLine()!.Split(' ');
            CurrentState.OpponentHealth = int.Parse(inputs[0]);
            CurrentState.OpponentMana = int.Parse(inputs[1]);

            int entityCount = int.Parse(Console.ReadLine()); // Amount of heros and monsters you can see
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine()!.Split(' ');
                var entity = new Entity
                {
                    Id = int.Parse(inputs[0]), // Unique identifier
                    Type = (EntityType)int.Parse(inputs[1]), // 0=monster, 1=your hero, 2=opponent hero
                    Position = new Point(int.Parse(inputs[2]), int.Parse(inputs[3])), // Position of this entity
                    ShieldLife = int.Parse(inputs[4]), // Ignore for this league; Count down until shield spell fades
                    IsControlled =
                        int.Parse(inputs[5]) ==
                        1, // Ignore for this league; Equals 1 when this entity is under a control spell
                    Health = int.Parse(inputs[6]), // Remaining health of this monster
                    Trajectory = new Point(int.Parse(inputs[7]), int.Parse(inputs[8])), // Trajectory of this monster
                    TargetsMyBase =
                        int.Parse(inputs[9]) == 1 &&
                        int.Parse(inputs[10]) == 1, // 0=monster with no target yet, 1=monster targeting a base
                    TargetsOpponentBase =
                        int.Parse(inputs[9]) == 1 &&
                        int.Parse(inputs[10]) ==
                        2 // Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither
                };
                CurrentState.Entities.Add(entity);
            }

            var myHeroes = CurrentState.Entities.Where(p => p.Type == EntityType.MyHero).ToList();
            var monsters = CurrentState.Entities.Where(p => p.Type == EntityType.Monster).ToList();
            var opponents = CurrentState.Entities.Where(p => p.Type == EntityType.OpponentHero).ToList();

            foreach (var myHero in myHeroes)
            {
                if (!monsters.Any())
                {
                    if (MyBase.Position.X > 8800 && MyBase.Position.Y > 4400)
                    {
                        Console.WriteLine($"MOVE 14300 8300");
                        Console.WriteLine($"MOVE 15200 6000");
                        Console.WriteLine($"MOVE 16800 5800");
                    }
                    else
                    {
                        Console.WriteLine("MOVE 5000 1000");
                        Console.WriteLine("MOVE 4100 3000");
                        Console.WriteLine("MOVE 1500 4000");
                    }

                    break;
                }

                var closestOpponent = opponents
                    .OrderBy(p => p.Position.GetDistance(myHero.Position))
                    .FirstOrDefault();
                Console.Error.WriteLine(closestOpponent?.Position.GetDistance(myHero.Position));
                Console.Error.WriteLine(CurrentState.MyMana);
                if (closestOpponent != null && closestOpponent.Position.GetDistance(myHero.Position) <= 2200 &&
                    CurrentState.MyMana > 20)
                {
                    Console.WriteLine($"SPELL SHIELD {myHero.Id}");
                    continue;
                }

                if (myHero.IsControlled && CurrentState.MyMana > 20)
                {
                    Console.WriteLine($"SPELL SHIELD {myHero.Id}");
                    continue;
                }

                var selectedMonster = monsters
                    .Where(p => p.TargetsMyBase)
                    .OrderBy(p => p.Position.GetDistance(MyBase.Position))
                    .FirstOrDefault(p => p.TargetsMyBase);
                if (selectedMonster == null)
                {
                    selectedMonster = monsters
                        .OrderBy(p => p.TargetsMyBase)
                        .ThenBy(p => p.Position.GetDistance(myHero.Position))
                        .First();
                }

                var x = selectedMonster.Position.X;
                var y = selectedMonster.Position.Y;

                Console.WriteLine($"MOVE {x} {y}");
            }
        }
    }

    // private void CalcStartPoints()
    // {
    //     MyBase
    // }

    class State
    {
        public int MyHealth { get; set; }
        public int OpponentHealth { get; set; }
        public int MyMana { get; set; }
        public int OpponentMana { get; set; }
        public List<Entity> Entities { get; set; } = new List<Entity>();
    }

    class Base : Entity
    {
        public bool IsMine { get; set; }
    }

    class Entity
    {
        public int Id { get; set; }
        public Point Position { get; set; }
        public Point Trajectory { get; set; }
        public EntityType Type { get; set; }
        public int ShieldLife { get; set; }
        public int Health { get; set; }
        public bool IsControlled { get; set; }
        public bool TargetsMyBase { get; set; }
        public bool TargetsOpponentBase { get; set; }
        public Point StartPoint { get; set; }
    }

    class Point
    {
        public Point(int x, int y) => (X, Y) = (x, y);
        public int X { get; set; }
        public int Y { get; set; }

        public double GetDistance(Point anotherPoint)
        {
            return Math.Sqrt((anotherPoint.X - X) * (anotherPoint.X - X) + (anotherPoint.Y - Y) * (anotherPoint.Y - Y));
        }
    }

    public enum EntityType
    {
        Monster = 0,
        MyHero = 1,
        OpponentHero = 2
    }

    public enum ThreatFor
    {
        Nothing = 0,
        MyBase = 1,
        OpponentBase = 2
    }
}