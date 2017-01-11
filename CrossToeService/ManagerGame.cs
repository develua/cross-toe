using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrossToeService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class ManagerGame : IManagerGame, IDuplexService
    {
        List<Game> Games = new List<Game>();
        int EndUserID = 0;

        public int AddNewUser(string name)
        {
            try
            {
                User newUser = new User { Name = name };
                newUser.Callback = OperationContext.Current.GetCallbackChannel<IClientCallback>();

                if (Games.Count == 0 || Games.Last().Users.Count == 2)
                {
                    Game newGame = new Game();
                    newUser.Symbol = 'X';
                    newGame.Users.Add(newUser);
                    Games.Add(newGame);
                }
                else
                {
                    Game gameLast = Games.Last();
                    newUser.Symbol = 'O';
                    gameLast.Users.Add(newUser);

                    Task.Factory.StartNew(() =>
                    {
                        gameLast.Users[0].Callback.GetUpdate(-1, MessageExpense(gameLast));
                        gameLast.Users[1].Callback.GetUpdate(-2, MessageExpense(gameLast));
                    });
                }

                return ++EndUserID;
            }
            catch { }

            return 0; 
        }

        public void SetPosition(int userID, int posMove)
        {
            try
            {
                int gameID = (int)Math.Ceiling(userID / 2.0) - 1;
                int numUser = (userID % 2 == 0) ? 0 : 1;
                char symbol = (userID % 2 == 0) ? 'O' : 'X';
                string message = "";
                bool isPartyEnd = false;

                Games[gameID].Field[posMove - 1] = symbol;

                int lineWin = -1;
                char resСheckWin = СheckWinField(Games[gameID].Field, ref lineWin);

                if (resСheckWin != ' ')
                {
                    message = "Поздравляем, победителя - ";

                    if (resСheckWin == 'X')
                        message += Games[gameID].Users[0].Name + "!\n";
                    else if (resСheckWin == 'O')
                        message += Games[gameID].Users[1].Name + "!\n";
                    else if (resСheckWin == '+')
                        message = "Поздравляем, победила дружба!\n";

                    if (resСheckWin == 'X')
                        Games[gameID].Users[0].Expense += 1;
                    else if (resСheckWin == 'O')
                        Games[gameID].Users[1].Expense += 1;

                    message += MessageExpense(Games[gameID]);
                    isPartyEnd = true;
                }

                Games[gameID].Users[numUser].Callback.GetUpdate(posMove - 1, message, lineWin);

                if (isPartyEnd)
                {
                    int twoUser = (numUser == 0) ? 1 : 0;
                    Games[gameID].Users[twoUser].Callback.GetUpdate(-1, message, lineWin);

                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(5000);
                        ClearField(Games[gameID]);
                        int lastUser = Games[gameID].FirstUserID;
                        int firstUser = Games[gameID].FirstUserID = (Games[gameID].FirstUserID == 0) ? 1 : 0;
                        Games[gameID].Users[firstUser].Callback.GetUpdate(-1, MessageExpense(Games[gameID]));
                        Games[gameID].Users[lastUser].Callback.GetUpdate(-2, MessageExpense(Games[gameID]));
                    });
                }
            }
            catch { }
        }

        private char СheckWinField(char[] Field, ref int lineID)
        {
            lineID = -1;

            // Проверка по горизонтали
            if (Field[0] != ' ' && Field[0] == Field[1] && Field[1] == Field[2])
            {
                lineID = 1;
                return Field[0];
            }

            if (Field[3] != ' ' && Field[3] == Field[4] && Field[4] == Field[5])
            {
                lineID = 2;
                return Field[3];
            }

            if (Field[6] != ' ' && Field[6] == Field[7] && Field[7] == Field[8])
            {
                lineID = 3;
                return Field[6];
            }

            // Проверка по вертикали
            if (Field[0] != ' ' && Field[0] == Field[3] && Field[3] == Field[6])
            {
                lineID = 4;
                return Field[0];
            }

            if (Field[1] != ' ' && Field[1] == Field[4] && Field[4] == Field[7])
            {
                lineID = 5;
                return Field[1];
            }

            if (Field[2] != ' ' && Field[2] == Field[5] && Field[5] == Field[8])
            {
                lineID = 6;
                return Field[2];
            }

            // Проверка по диагонали
            if (Field[0] != ' ' && Field[0] == Field[4] && Field[4] == Field[8])
            {
                lineID = 7;
                return Field[0];
            }

            if (Field[2] != ' ' && Field[2] == Field[4] && Field[4] == Field[6])
            {
                lineID = 8;
                return Field[2];
            }

            int countFillCall = 0;

            for (int i = 0; i < Field.Length; i++)
                if (Field[i] == ' ')
                    countFillCall++;

            if (countFillCall == 0)
            {
                lineID = 0;
                return '+';
            }

            return ' ';
        }

        private string MessageExpense(Game game)
        {
            return String.Format("Текущий счет - {0} {1} : {2} {3}", game.Users[0].Name, game.Users[0].Expense, game.Users[1].Expense, game.Users[1].Name);
        }

        private void ClearField(Game game)
        {
            for (int i = 0; i < game.Field.Length; i++)
                game.Field[i] = ' ';
        }
    }
}
