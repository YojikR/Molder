﻿using Microsoft.Extensions.Logging;
using Molder.Helpers;
using Molder.Infrastructures;
using Molder.Models.ReplaceMethod;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Molder.Extensions
{
    public static class ReplaceMethodsExtension
    {
        public static (string method, string[]) GetFunction(string function)
        {
            var regex = new Regex(StringPattern.METHOD, RegexOptions.Compiled);
            var match = regex.Match(function);
            if (!match.Success) return (null, null)!;
            var method = match.Groups[StringPattern.MethodPlaceholder].Value;
            var parameters = match.Groups[StringPattern.ParametersPlaceholder].Value;
            return (string.IsNullOrEmpty(parameters) ? (method, null) : (method, parameters.Split(',')))!;
        }

        [ExcludeFromCodeCoverage]
        public static object Invoke(string methodName, object[] parameters)
        {
            var method = Check(methodName);
            return method?.Invoke(null, parameters);
        }

        [ExcludeFromCodeCoverage]
        public static MethodInfo Check(string methodName)
        {
            var methods = GetMethods();
            try
            {
                return methods.SingleOrDefault(m => m.Name == methodName);
            }
            catch (InvalidOperationException)
            {
                Log.Logger().LogWarning($"Found two or more methods with name \"{methodName}\" to execute in replace.");
                return null;
            }
        }

        [ExcludeFromCodeCoverage]
        private static IEnumerable<MethodInfo> GetMethods()
        {
            var methods = new List<MethodInfo>();
            foreach (var type in ReplaceMethods.Get())
            {
                methods.AddRange(type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public));
            }
            return methods;
        }
    }
}