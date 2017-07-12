using cloudscribe.Web.Navigation;
using cloudscribe.Web.Navigation.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebNavigationTestProject.AuthorizationHandlers
{
    public class NavigationNodeAutoPermissionResolver : INavigationNodePermissionResolver
    {

        public NavigationNodeAutoPermissionResolver(IHttpContextAccessor httpContextAccessor, 
            IActionContextAccessor actionContextAccessor, 
            IActionFilterMap filterMap,
            ILogger<NavigationNodeAutoPermissionResolver> logger)
        {
            _httpContext = httpContextAccessor.HttpContext;
            // if the AuthorizationHandler examines the AuthorizationFilterContext for RouteData
            // usually the filter would assume the route is applicable to the action it is adorning
            // however this will be an invalid assumption. Therefore it may be worth setting the
            // RouteData property of the ActionContext to a new (empty) instance. However it may be 
            // very usefull if, for instance if a 'DepartmentId' value in the RouteData were compared with 
            // a user claim for 'DepartmentId'. 
            _actionContext = new ActionContext(actionContextAccessor.ActionContext);
            _filterMap = filterMap;
            _logger = logger;
        }
        
        private HttpContext _httpContext;
        private ActionContext _actionContext;
        private IActionFilterMap _filterMap;
        private ILogger _logger;

        public const string AllUsers = "*"; //note  - shouldn't this is "AllUsers;" in the default implementation

        public virtual bool ShouldAllowView(TreeNode<NavigationNode> menuNode)
        {
            if (string.IsNullOrEmpty(menuNode.Value.ViewRoles)) {
                var authFilters = _filterMap.GetFilters(menuNode.Value.Area, menuNode.Value.Controller, menuNode.Value.Action);
                if (authFilters == null)
                {
                    _logger.LogWarning($"could not find area:'{menuNode.Value.Area}'/controller:'{menuNode.Value.Controller}'/action:'{menuNode.Value.Action}'");
                }
                else if (authFilters.Any())
                {
                    return Task.Run(()=>IsValid(authFilters, _actionContext)).GetAwaiter().GetResult();
                }
                else
                {
                    return true;
                }
            }
            if (AllUsers.Equals(menuNode.Value.ViewRoles, StringComparison.OrdinalIgnoreCase)) { return true; }

            return _httpContext.User.IsInRoles(menuNode.Value.ViewRoles);
        }

        private async Task<bool> IsValid(IEnumerable<IAsyncAuthorizationFilter> filters, ActionContext actionContext)
        {
            var context = new AuthorizationFilterContext(actionContext, filters.Cast<IFilterMetadata>().ToList());
            foreach (var f in filters)
            {
                await f.OnAuthorizationAsync(context);
                if (context.Result != null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
