using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Ocuda.Utility.Models;

namespace Ocuda.Utility.Helpers
{
    public static class ModelStateHelper
    {
        public static string SerializeModelState(ModelStateDictionary modelState)
        {
            var itemList = modelState
                .Select(_ => new ModelStateItem
                {
                    Key = _.Key,
                    AttemptedValue = _.Value.AttemptedValue,
                    RawValue = _.Value.RawValue,
                    ErrorMessages = _.Value.Errors.Select(e => e.ErrorMessage).ToList(),
                })
                .ToList();

            var storage = new ModelStateStorage
            {
                ModelStateItems = itemList,
                Time = DateTimeOffset.Now.ToUnixTimeSeconds()
            };

            return JsonConvert.SerializeObject(storage);
        }

        public static (ModelStateDictionary modelState, long time) DeserializeModelState(
            string modelStateStorage)
        {
            var storage = JsonConvert.DeserializeObject<ModelStateStorage>(modelStateStorage);
            var modelState = new ModelStateDictionary();

            foreach (var item in storage.ModelStateItems)
            {
                modelState.SetModelValue(item.Key, item.RawValue, item.AttemptedValue);
                foreach (var error in item.ErrorMessages)
                {
                    modelState.AddModelError(item.Key, error);
                }
            }
            return (modelState, storage.Time);
        }

        public static string GetModelStateKey(RouteValueDictionary routeValues)
        {
            var routeString = string.Join("-", routeValues.Values).ToLower();
            return $"ModelState_{routeString}";
        }

        public static string GetModelStateKey(string key)
        {
            return $"ModelState_{key}";
        }
    }
}
