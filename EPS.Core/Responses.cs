using EPS.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPS
{
    public class Response
    {
        public string Notification;
    }

    public class GenerateResponse : Response
    {
        public CodeGenerateEnum GenerateStatus;
    }

    public class ChekResponse : Response
    {

        public string[] ProducCodes;
        public int Action;
    }

    public class UseResponse : Response
    {
        public UseCodeEnum UseCodeStatus;
    }
}
