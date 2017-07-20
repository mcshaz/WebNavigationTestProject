using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace WebNavigationTestProject.AuthorizationHandlers
{
    public interface IActionFilterMap
    {
        IDictionary<ControllerActionNameKey, List<PerRequestFilter>> GetNewFilterDictionary();
    }
    public class CustomApplicationModelProvider : IApplicationModelProvider, IActionFilterMap
    {
        //It will be executed after AuthorizationApplicationModelProvider, which has order -990
        public int Order => 0;

        private IEnumerable<Tuple<IAsyncAuthorizationFilter, List<ControllerActionNameKey>>> _authorizations;
        private List<ControllerActionNameKey> _noAuthorizationRequired;

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            var dict = new Dictionary<AuthFilterWrapper, List<ControllerActionNameKey>>();
            _noAuthorizationRequired = new List<ControllerActionNameKey>();

            foreach (var controllerModel in context.Result.Controllers)
            {
                var controllerFilters = controllerModel.Filters.OfType<IAsyncAuthorizationFilter>().ToList();
                string area = controllerModel.Attributes.OfType<AreaAttribute>().FirstOrDefault()?.RouteValue;
                foreach (ActionModel action in controllerModel.Actions)//todo restrain to get
                {
                    var method = action.Attributes.OfType<HttpMethodAttribute>().FirstOrDefault();
                    if (method == null || method.HttpMethods.Contains("GET"))
                    {
                        var key = new ControllerActionNameKey(area, controllerModel.ControllerName, action.ActionName);
                        if (action.Filters.OfType<AllowAnonymousFilter>().Any())
                        {
                            _noAuthorizationRequired.Add(key);
                        }
                        else
                        {
                            var filters = controllerFilters.Concat(action.Filters.OfType<IAsyncAuthorizationFilter>()).ToList();
                            if (!filters.Any())
                            {
                                _noAuthorizationRequired.Add(key);
                            }
                            else
                            {
                                foreach (var f in filters)
                                {
                                    dict.AddTo(new AuthFilterWrapper(f), key);
                                }
                            }
                        }
                    }
                }
            }
            Debug.WriteLine("total auth filters:" + dict.Count);
            _authorizations = new ReadOnlyCollection<Tuple<IAsyncAuthorizationFilter, List<ControllerActionNameKey>>>(dict.Select(d => Tuple.Create(d.Key.Filter, d.Value)).ToList());
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            //empty
        }

        public IDictionary<ControllerActionNameKey, List<PerRequestFilter>> GetNewFilterDictionary()
        {
            var emptyList = new List<PerRequestFilter>();
            var returnVar = _noAuthorizationRequired.ToDictionary(a => a, a => emptyList);
            foreach (var f in _authorizations)
            {
                var prf = new PerRequestFilter(f.Item1);
                foreach (var a in f.Item2)
                {
                    returnVar.AddTo(a, prf);
                }
            }
            return returnVar;
        }

        private class AuthFilterWrapper
        {
            public AuthFilterWrapper(IAsyncAuthorizationFilter filter)
            {
                Filter = filter;
                var af = filter as AuthorizeFilter;
                if (af != null)
                {
                    _authorizedRoleNames = new List<string>();
                    _others = new List<IAuthorizationRequirement>(af.Policy.Requirements.Count);
                    foreach (var r in af.Policy.Requirements)
                    {
                        var ra = r as RolesAuthorizationRequirement;
                        if (r is RolesAuthorizationRequirement rar)
                        {
                            if (_authorizedRoleNames.Count > 0)
                            {
                                throw new NotImplementedException("Expected Policy.Requirements to contain only a single RolesAuthorizationRequirement within the collection.");
                            }
                            _authorizedRoleNames = rar.AllowedRoles.ToList();
                        }
                        else
                        {
                            _others.Add(r);
                        }
                    }
                }
            }
            public IAsyncAuthorizationFilter Filter { get; private set; }
            public List<string> _authorizedRoleNames;
            public List<IAuthorizationRequirement> _others;

            public override bool Equals(object obj)
            {
                var compareTo = obj as AuthFilterWrapper;
                if (compareTo == null) { return false; }
                bool filtersEqual = Filter.Equals(compareTo.Filter);
                if (_others == null || filtersEqual) { return filtersEqual; }

                return ListEquivalentComparer.Equals(_authorizedRoleNames, compareTo._authorizedRoleNames)
                    && ListEquivalentComparer.Equals(_others, compareTo._others);
            }

            public override int GetHashCode()
            {
                if (_others == null)
                {
                    return Filter.GetHashCode();
                }
                return ListEquivalentComparer.GetHashCode(_authorizedRoleNames)
                    ^ ListEquivalentComparer.GetHashCode(_others);
            }
        }
    }
    public class PerRequestFilter 
    {
        public PerRequestFilter(IAsyncAuthorizationFilter filter)
        {
            Filter = filter;
        }
        public IAsyncAuthorizationFilter Filter { get; private set; }
        public bool? Authorized { get; set; }
    }
}
