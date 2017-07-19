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
            // RouteData property of the ActionContext to a new (empty) RouteData instance. However it may be 
            // very usefull if, for instance if a 'DepartmentId' value in the RouteData were compared with 
            // a user claim for 'DepartmentId'. if the property is 'Id' refering to different tables in 
            // different controllers, and the IDs are not GUIDs, this could be problematic
            _actionContext = new ActionContext(actionContextAccessor.ActionContext);
            _filterMap = filterMap.GetNewFilterDictionary();
            _logger = logger;
        }
        
        private HttpContext _httpContext;
        private ActionContext _actionContext;
        private IDictionary<ControllerActionNameKey, List<PerRequestFilter>> _filterMap;
        private ILogger _logger;

        public const string AllUsers = "*"; //note - this is "AllUsers;" in the default implementation

        public virtual bool ShouldAllowView(TreeNode<NavigationNode> menuNode)
        {
            if (menuNode.Value.ViewRoles.Length == 0) {
                if (menuNode.Value.NamedRoute.Length > 0)
                {
                    //this could be implemented, but as I never use named routes, feel free to implement yourself
                    throw new NotImplementedException("The current implementation does not know which named routes map to which actions");
                }
                //if no NamedRoute attribute and no action attribute a url must have been provided
                //we could also use something like if (menuNode.Value.Url[0] != '~')
                if (menuNode.Value.Action.Length == 0) {
                    return true; 
                } 
                if (!_filterMap.TryGetValue(new ControllerActionNameKey(menuNode.Value.Area, menuNode.Value.Controller, menuNode.Value.Action), out List<PerRequestFilter> authFilters))
                {
                    _logger.LogWarning($"could not find area:'{menuNode.Value.Area}'/controller:'{menuNode.Value.Controller}'/action:'{menuNode.Value.Action}'");
                    return true;
                }
                else if(authFilters.Any(af=>af.Authorized == false))
                {
                    return false;
                }
                else if(authFilters.All(af=>af.Authorized == true))
                {
                    return true;
                }
                return Task.Run(() => IsValid(authFilters.Where(f => !f.Authorized.HasValue), _actionContext)).GetAwaiter().GetResult();
            }
            if (AllUsers.Equals(menuNode.Value.ViewRoles, StringComparison.OrdinalIgnoreCase)) { return true; }

            return _httpContext.User.IsInRoles(menuNode.Value.ViewRoles);
        }

        private static async Task<bool> IsValid(IEnumerable<PerRequestFilter> filters, ActionContext actionContext)
        {
            var context = new AuthorizationFilterContext(actionContext, filters.Select(f=>(IFilterMetadata)f.Filter).ToList());
            foreach (var f in filters)
            {
                await f.Filter.OnAuthorizationAsync(context);
                f.Authorized = context.Result == null;
                if (f.Authorized == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
