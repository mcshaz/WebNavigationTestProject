using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WebNavigationTestProject.AuthorizationHandlers
{
    public interface IActionFilterMap
    {
        IEnumerable<IAsyncAuthorizationFilter> GetFilters(string area, string controller, string action);
    }
    public class CustomApplicationModelProvider : IApplicationModelProvider, IActionFilterMap
    {
        //It will be executed after AuthorizationApplicationModelProvider, which has order -990
        public int Order => 0;

        private ReadOnlyDictionary<ActionKey, IEnumerable<IAsyncAuthorizationFilter>> _authDictionary;

        public IEnumerable<IAsyncAuthorizationFilter> GetFilters(string area, string controller, string action)
        {
            var key = new ActionKey(area, controller, action);
            if (_authDictionary.TryGetValue(key, out IEnumerable<IAsyncAuthorizationFilter> returnVar))
            {
                return returnVar;
            }
            return null;//returning null rather than Enumerable.Empty so consuming method can detect if action found and has no Authorization, or action not found

        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            var returnVar = new Dictionary<ActionKey, IEnumerable<IAsyncAuthorizationFilter>>();
            foreach (var controllerModel in context.Result.Controllers)
            {
                var controllerFilters = controllerModel.Filters.OfType<IAsyncAuthorizationFilter>().ToList();
                string area = controllerModel.Attributes.OfType<AreaAttribute>().FirstOrDefault()?.RouteValue;
                foreach (ActionModel action in controllerModel.Actions)//todo restrain to get
                {
                    var method = action.Attributes.OfType<HttpMethodAttribute>().FirstOrDefault();
                    if (method == null || method.HttpMethods.Contains("GET"))
                    {
                        var key = new ActionKey(area, controllerModel.ControllerName, action.ActionName);
                        if (action.Filters.OfType<AllowAnonymousFilter>().Any())
                        {
                            returnVar.Add(key, Enumerable.Empty<IAsyncAuthorizationFilter>());
                        }
                        else
                        {
                            var filters = controllerFilters.Concat(action.Filters.OfType<IAsyncAuthorizationFilter>()).ToArray();
                            returnVar.Add(key, filters);
                        }
                    }
                }
                _authDictionary = new ReadOnlyDictionary<ActionKey, IEnumerable<IAsyncAuthorizationFilter>>(returnVar);
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            //empty
        }

        private class ActionKey : Tuple<string, string, string>
        {
            public ActionKey(string area, string controller, string action) : base(area ?? string.Empty, controller, action)
            {
                _hashCode = base.GetHashCode();
            }
            
            int _hashCode;
            public override int GetHashCode()
            {
                return _hashCode;
            }
        }
    }
}
