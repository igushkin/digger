using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Digger
{
    class Conditions
    {
        public static bool Common(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < Game.MapWidth && y < Game.MapHeight && !(Game.Map[x, y] is Sack));
        }

        public static bool Monster(int x, int y, int tmpX, int tmpY)
        {
            int PX = 0, PY = 0;

            if (!Common(tmpX, tmpY))
                return false;
            else
            {
                for (var i = 0; i < Game.MapHeight; i++)
                    for (var j = 0; j < Game.MapWidth; j++)
                        if (Game.Map[j, i] is Player)
                        {
                            PX = j;
                            PY = i;
                            Game.IsOver = false;
                        }
                var a = Math.Abs(x - PX) > Math.Abs(tmpX - PX);
                var b = Math.Abs(y - PY) > Math.Abs(tmpY - PY);
                var c = !(Game.Map[tmpX, tmpY] is Terrain);
                var d = !Game.IsOver;
                return c && (a || b) && d;
            }
        }
    }

    class GameInfo
    {
        public static Dictionary<Keys, CreatureCommand> Moves = new Dictionary<Keys, CreatureCommand>
        {
            { Keys.None, new CreatureCommand()},
            { Keys.Up, new CreatureCommand{DeltaY =-1 } },
            { Keys.Down, new CreatureCommand{DeltaY =+1 } },
            { Keys.Left, new CreatureCommand{DeltaX =-1 } },
            { Keys.Right, new CreatureCommand{DeltaX =+1 } }
        };

        public static CreatureCommand MakeMove(int x, int y, ICreature curObject, Keys key)
        {
            int stepX = x + Moves[key].DeltaX;
            int stepY = y + Moves[key].DeltaY;
            if (Conditions.Common(stepX, stepY))
            {
                if (curObject is Monster)
                {
                    if (!(Game.Map[stepX, stepY] is Monster))
                        return Moves[key];
                }
                else
                    return Moves[key];
            }
            return Moves[Keys.None];
        }
    }

    public class Terrain : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            return GameInfo.Moves[Keys.None];
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return true;
        }

        public int GetDrawingPriority()
        {
            return 1;
        }

        public string GetImageFileName()
        {
            return "Terrain.png";
        }
    }

    public class Player : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            return GameInfo.MakeMove(x, y, this, Game.KeyPressed);
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Sack || conflictedObject is Monster)
            {
                Game.IsOver = true;
                return true;
            }
            return false;
        }

        public int GetDrawingPriority()
        {
            return 2;
        }

        public string GetImageFileName()
        {
            return "Digger.png";
        }
    }

    public class Sack : ICreature
    {
        public int FlyCount = 0;

        public CreatureCommand Act(int x, int y)
        {
            var pointUnderSack = Math.Min(Game.MapHeight - 1, y + 1);
            if (Game.Map[x, pointUnderSack] == null ||
                (FlyCount > 0 && Game.Map[x, pointUnderSack] is Player) ||
                (FlyCount > 0 && Game.Map[x, pointUnderSack] is Monster))
            {
                FlyCount++;
                return new CreatureCommand { DeltaY = +1 };
            }
            else
            {
                if (FlyCount > 1)
                {
                    FlyCount = 0;
                    return new CreatureCommand { TransformTo = new Gold() };
                }
                FlyCount = 0;
                return new CreatureCommand();
            }
        }
        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }

        public int GetDrawingPriority()
        {
            return 1;
        }

        public string GetImageFileName()
        {
            return "Sack.png";
        }
    }

    public class Gold : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            return GameInfo.Moves[Keys.None];
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            Game.Scores += 10;
            return true;
        }

        public int GetDrawingPriority()
        {
            return 1;
        }

        public string GetImageFileName()
        {
            return "Gold.png";
        }
    }

    public class Monster : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            Keys key = Keys.None;
            for (var i = 1; i < 5; i++)
            {
                var tmpX = x + GameInfo.Moves.ElementAt(i).Value.DeltaX;
                var tmpY = y + GameInfo.Moves.ElementAt(i).Value.DeltaY;
                if (Conditions.Monster(x, y, tmpX, tmpY))
                {
                    key = GameInfo.Moves.ElementAt(i).Key;
                    break;
                }
            }
            return GameInfo.MakeMove(x, y, this, key);
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Sack || conflictedObject is Monster)
                return true;
            return false;
        }

        public int GetDrawingPriority()
        {
            return 1;
        }

        public string GetImageFileName()
        {
            return "Monster.png";
        }
    }
}