using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebNavigationTestProject.AuthorizationHandlers
{
    public class ControllerActionNameKey : Tuple<string,string,string>
    {
        public ControllerActionNameKey(string area, string controller, string action) : base(area ?? string.Empty, controller, action)
        {
        }
    }
}
