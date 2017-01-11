using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace CrossToeService
{
    [ServiceContract(CallbackContract = typeof(IClientCallback))]
    interface IDuplexService
    {
        [OperationContract]
        int AddNewUser(string name);
    }

    interface IClientCallback
    {
        [OperationContract(IsOneWay = true)]
        void GetUpdate(int posMove, string message, int lineWin = -1);
    }
}
