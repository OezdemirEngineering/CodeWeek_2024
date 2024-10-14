using Game.Engine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Game.Engine.Dtos
{
    internal class Projectile
    {
        public UIElement Object { get; set; }
        public double Speed { get; set; }
        public Enums.Direction Direction { get; set; }

        public Projectile(UIElement obj, double speed, Enums.Direction direction)
        {
            Object = obj;
            Speed = speed;
            Direction = direction;
        }
    }
}
