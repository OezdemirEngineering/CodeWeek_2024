using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Game.Engine.Dtos
{
    public class JumpStateDto
    {
        private bool _isFalling;
        private bool _isJumping;

        public JumpStateDto(UIElement player, double speed, int jumpCounter = 10)
        {
            Player = player;
            Speed = speed;
            JumpCounter = jumpCounter;
            _isFalling = false;
            _isJumping = true;
        }

        public void Jump(double speed, int jumpCounter = 10)
        {
            Speed = speed;
            JumpCounter = jumpCounter;
            _isJumping = true;
        }

        public bool IsJumping()
        {
            if (JumpCounter == 0 && _isJumping)
            {
                _isFalling = true;
                _isJumping = false;
                return false;
            }
            else
            {
                JumpCounter--;

                return _isJumping;
            }
        }

        public void MakeReady()
        {
            _isFalling = false;
        }

        public bool IsFalling()
        {
            return _isFalling;
        }

        public void SetFall()
            => _isFalling = true;


        public UIElement Player;
        public int JumpCounter;
        public double Speed;
    }
}
