using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    private static Base MyBase;
    private static Base OpponentsBase;
    private static int HeroesPerPlayer;
    private static State CurrentState;
    private static int Turn;
    private static List<int> EntitiesControlledByMe = new List<int>();

    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        MyBase = new Base
        {
            IsMine = true,
            Position = new Point(int.Parse(inputs[0]), int.Parse(inputs[1])),
            IsLeft = int.Parse(inputs[0]) < 8800
        };
        OpponentsBase = new Base()
        {
            IsMine = false,
            Position = MyBase.IsLeft ? new Point(17630, 9000) : new Point(0, 0),
            IsLeft = !MyBase.IsLeft
        };
        // var startPointX = MyBase.Position.X > 8800 ? MyBase.Position.X - 1000 : MyBase.Position.X + 1000;
        // var startPointY = MyBase.Position.Y > 4400 ? MyBase.Position.Y - 1700 : MyBase.Position.Y + 1700;
        // MyBase.StartPoint = new Point(startPointX, startPointY);

        HeroesPerPlayer = int.Parse(Console.ReadLine()); // Always 3

        // game loop
        while (true)
        {
            Turn++;
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

            if (Turn == 1)
            {
            }

            List<Point> startPoints = new List<Point>();
            if (MyBase.IsLeft)
            {
                startPoints.Add(new Point(5500, 1500));
                startPoints.Add(new Point(4800, 3400));
                startPoints.Add(new Point(3000, 4600));
            }
            else
            {
                startPoints.Add(new Point(12300, 7700));
                startPoints.Add(new Point(13200, 5500));
                startPoints.Add(new Point(15100, 4200));
            }

            for (var i = 0; i < myHeroes.Count; i++)
            {
                myHeroes[i].StartPoint = startPoints[i];
                Console.Error.WriteLine(myHeroes[i].StartPoint.X);
            }


            foreach (var myHero in myHeroes)
            {
                if (!monsters.Any())
                {
                    Console.Error.WriteLine($"turn {Turn}");
                    Console.WriteLine($"MOVE {myHero.StartPoint.X} {myHero.StartPoint.Y}");
                    continue;
                }

                var closestOpponent = opponents
                    .OrderBy(p => p.Position.GetDistance(myHero.Position))
                    .FirstOrDefault();
                
                // if (closestOpponent != null && closestOpponent.Position.GetDistance(myHero.Position) <= 2200 &&
                //     CurrentState.MyMana > 20 && myHero.ShieldLife == 0)
                // {
                //     Console.WriteLine($"SPELL SHIELD {myHero.Id}");
                //     continue;
                // }

                // if (myHero.IsControlled && CurrentState.MyMana > 20)
                // {
                //     Console.WriteLine($"SPELL SHIELD {myHero.Id}");
                //     continue;
                // }

                var selectedMonster = monsters
                    .Where(p => p.TargetsMyBase)
                    .OrderBy(p => p.Position.GetDistance(MyBase.Position))
                    .FirstOrDefault(p => p.TargetsMyBase);
                
                if (selectedMonster != null && selectedMonster.Position.GetDistance(myHero.Position) <= 1280 && CurrentState.MyMana > 20 && selectedMonster.Position.GetDistance(MyBase.Position) < 900)
                {
                    Console.WriteLine(
                        $"SPELL WIND {OpponentsBase.Position.X} {OpponentsBase.Position.Y}");
                    continue;
                }
                
                if (selectedMonster != null && selectedMonster.Position.GetDistance(myHero.Position) <= 1280 && CurrentState.MyMana > 20 && selectedMonster.Position.GetDistance(MyBase.Position) > myHero.Position.GetDistance(MyBase.Position) - 1280)
                {
                    Console.WriteLine(
                        $"SPELL WIND {OpponentsBase.Position.X} {OpponentsBase.Position.Y}");
                    continue;
                }

                if (selectedMonster == null)
                {
                    if (myHero.Position.GetDistance(MyBase.Position) > 8000)
                    {
                        Console.WriteLine($"MOVE {myHero.StartPoint.X} {myHero.StartPoint.Y}");
                        continue;
                    }
                    if (monsters.Count > EntitiesControlledByMe.Count)
                    {
                        selectedMonster = monsters
                            .Where(p => !EntitiesControlledByMe.Contains(p.Id))
                            .OrderBy(p => p.TargetsMyBase)
                            .ThenBy(p => p.Position.GetDistance(myHero.Position))
                            .First();
                    }
                    else
                    {
                        selectedMonster = monsters
                            .OrderBy(p => p.TargetsMyBase)
                            .ThenBy(p => p.Position.GetDistance(myHero.Position))
                            .First();
                    }


                    var monsterDistanceToHero = selectedMonster.Position.GetDistance(myHero.Position);
                    var monsterDistanceToBase = selectedMonster.Position.GetDistance(MyBase.Position);
                    var myHeroDistanceToBase = myHero.Position.GetDistance(MyBase.Position);

                    if ((myHero.Id == 0  || myHero.Id == 3) && monsterDistanceToHero <= 2200 &&
                        CurrentState.MyMana > 20)
                    {
                        // Console.WriteLine(
                        //     $"SPELL CONTROL {selectedMonster.Id} {OpponentsBase.Position.X} {OpponentsBase.Position.Y}");
                        // EntitiesControlledByMe.Add(selectedMonster.Id);
                        // continue;
                    }
                }
                // else
                // {
                //     var monsterDistanceToHero = selectedMonster.Position.GetDistance(myHero.Position);
                //     var monsterDistanceToBase = selectedMonster.Position.GetDistance(MyBase.Position);
                //     var myHeroDistanceToBase = myHero.Position.GetDistance(MyBase.Position);
                //
                //     if (monsterDistanceToHero <= 2200 && monsterDistanceToBase < myHeroDistanceToBase &&
                //         CurrentState.MyMana > 20)
                //     {
                //         Console.WriteLine(
                //             $"SPELL CONTROL {selectedMonster.Id} {OpponentsBase.Position.X} {OpponentsBase.Position.Y}");
                //         continue;
                //     }
                // }

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
        public bool IsLeft { get; set; }
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