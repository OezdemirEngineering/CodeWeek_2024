using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Game.Engine.Dtos
{
    public class MoveDto
    {
        public bool IsMovingLeft;
        public bool IsMovingRight;
        public bool IsMovingUp;
        public bool IsMovingDown;
        public double Speed;
        public UIElement Object;

        public MoveDto(UIElement obj, double speed)
        {
            Object = obj;
            Speed = speed;
        }


    }
}
