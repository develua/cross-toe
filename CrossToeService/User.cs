using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossToeService
{
    class User
    {
        public string Name { get; set; }
        public char Symbol { get; set; }
        public int Expense { get; set; }
        public IClientCallback Callback { get; set; }

        public User()
        {
            Expense = 0;
            Callback = null;
        }
    }
}
