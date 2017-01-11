using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CrossToeService
{
    [ServiceContract]
    interface IManagerGame
    {
        [OperationContract]
        void SetPosition(int userID, int posMove);
    }
}
