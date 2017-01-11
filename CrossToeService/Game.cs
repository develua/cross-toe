using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossToeService
{
    class Game
    {
        public List<User> Users { get; private set; }
        public char[] Field { get; private set; }
        public int FirstUserID { get; set; }

        public Game()
        {
            Users = new List<User>();
            Field = new char[9] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
            FirstUserID = 0;
        }
    }
}
