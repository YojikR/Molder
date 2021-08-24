﻿using Molder.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Molder.Web.Models
{
    [ExcludeFromCodeCoverage]
    public class TreePages
    {
        private static AsyncLocal<IEnumerable<Node>> _pages = new() { Value = null };
        private static Lazy<VariableController> _variableController;

        private TreePages() { }

        public static IEnumerable<Node> Get()
        {
            if (_pages.Value != null) return _pages.Value;
            var pageObject = new PageObject(_variableController.Value);
            _pages.Value = pageObject.Pages;
            return _pages.Value;
        }

        public static void SetVariables(VariableController variableController) => _variableController = new Lazy<VariableController>(() => variableController);
    }
}