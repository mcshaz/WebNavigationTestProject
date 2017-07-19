using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebNavigationTestProject.AuthorizationHandlers
{
    public class AuthorizationFilterEqualityComparer : IEqualityComparer<IAsyncAuthorizationFilter>
    {
        IListEquivalentComparer<IAuthorizationRequirement> _listComparer = new IListEquivalentComparer<IAuthorizationRequirement>();
        public bool Equals(IAsyncAuthorizationFilter x, IAsyncAuthorizationFilter y)
        {
            var afx = x as AuthorizeFilter;
            if (afx == null)
            {
                return object.Equals(x, y);
            }
            var afy = y as AuthorizeFilter;
            if (afy == null)
            {
                return object.Equals(x, y);
            }
            var arar = afx.Policy.Requirements.OfType<RolesAuthorizationRequirement>().ToList();
            return _listComparer.Equals(afx.Policy.Requirements, afy.Policy.Requirements);
        }
        public int GetHashCode(IAsyncAuthorizationFilter obj)
        {
            //var o = obj as ValidationFilter
            var af = obj as AuthorizeFilter;
            if (af == null)
            {
                return obj.GetHashCode();
            }
            return _listComparer.GetHashCode(af.Policy.Requirements);
        }
    }
}
