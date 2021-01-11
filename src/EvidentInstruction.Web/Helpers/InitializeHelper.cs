﻿using EvidentInstruction.Web.Models.PageObject.Attributes;
using EvidentInstruction.Web.Models.PageObject.Models;
using EvidentInstruction.Web.Models.PageObject.Models.Blocks;
using EvidentInstruction.Web.Models.PageObject.Models.Elements;
using EvidentInstruction.Web.Models.PageObject.Models.Elements.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace EvidentInstruction.Web.Helpers
{
    public static class InitializeHelper
    {
        public static ConcurrentDictionary<string, Frame> Frames(IEnumerable<FieldInfo> fields)
        {
            var dictionary = new ConcurrentDictionary<string, Frame>();

            foreach (var elementField in fields)
            {
                var attribute = elementField.GetCustomAttribute<FrameAttribute>();
                var element = (Frame)Activator.CreateInstance(elementField.FieldType, new { attribute.Name, attribute.Locator });

                dictionary.TryAdd(attribute.Name, element);
            }

            return dictionary;
        }

        public static ConcurrentDictionary<string, Block> Blocks(IEnumerable<FieldInfo> fields)
        {
            var dictionary = new ConcurrentDictionary<string, Block>();

            foreach (var elementField in fields)
            {
                var attribute = elementField.GetCustomAttribute<BlockAttribute>();
                var element = (Block)Activator.CreateInstance(elementField.FieldType, new { attribute.Name, attribute.Locator });

                dictionary.TryAdd(attribute.Name, element);
            }

            return dictionary;
        }

        public static (ConcurrentDictionary<string, IElement> elements, IEnumerable<IElement> primary) Elements(IEnumerable<FieldInfo> fields)
        {
            var dictionary = new ConcurrentDictionary<string, IElement>();
            var primaryElements = new List<IElement>();

            foreach (var elementField in fields)
            {
                var attribute = elementField.GetCustomAttribute<ElementAttribute>();
                var element = (Element)Activator.CreateInstance(elementField.FieldType, new { attribute.Name, attribute.Locator });

                if (attribute.Optional)
                {
                    primaryElements.Add(element);
                }

                dictionary.TryAdd(attribute.Name, element);
            }

            return (dictionary, primaryElements);
        }
    }
}
