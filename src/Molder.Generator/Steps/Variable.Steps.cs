﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Xml.Linq;
using FluentAssertions;
using Molder.Controllers;
using Molder.Helpers;
using TechTalk.SpecFlow;
using Microsoft.Extensions.Logging;
using Molder.Extensions;
using Molder.Generator.Extensions;
using Molder.Generator.Exceptions;

namespace Molder.Generator.Steps
{
    /// <summary>
    /// Общие шаги для работы с переменными.
    /// </summary>
    [Binding]
    public class VariableSteps
    {
        private readonly VariableController variableController;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Generator"/> class.
        /// Привязка общих шагов к работе с переменным через контекст.
        /// </summary>
        /// <param name="variableController">Контекст для работы с переменными.</param>
        public VariableSteps(VariableController variableController)
        {
            this.variableController = variableController;
        }

        [StepArgumentTransformation]
        public IEnumerable<object> TransformationTableToEnumerable(Table table)
        {
            return table.ToEnumerable(variableController);
        }

        [StepArgumentTransformation]
        public Dictionary<string,object> TransformationTableToDictionary(Table table)
        {
            return table.ToDictionary(variableController);
        }

        [StepArgumentTransformation]
        public TypeCode StringToTypeCode(string type)
        {
            var variablesType = new Dictionary<string, Type>()
            {
                { "int", typeof(int)},
                { "string", typeof(string)},
                { "double", typeof(double)},
                { "bool", typeof(bool)},
                { "object",typeof(object)},
                { "long",typeof(long)},
                { "float",typeof(float)}
            };
            type.Should().NotBeNull("Значение \"type\" не задано");
            type = type.ToLower();
            if (!variablesType.TryGetValue(type, out Type value)) throw new NotValideTypeException($"There is no type \"{type}\"");
            return Type.GetTypeCode(value);
        }

        /// <summary>
        /// Шаг для явного ожидания.
        /// </summary>
        /// <param name="seconds">Количество секунд ожидания.</param>
        [ExcludeFromCodeCoverage]
        [StepDefinition(@"я жду ([0-9]+) сек\.")]
        public void WaitForSeconds(int seconds)
        {
            seconds.Should().BePositive("Waiting time must be greater");
            seconds.Should().NotBe(0, "Waiting time cannot be equals zero");
            Thread.Sleep(seconds * 1000);
        }

        /// <summary>
        /// Шаг для удаления переменной.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я удаляю переменную ""(.+)""")]
        public void DeleteVariable(string varName)
        {
            this.variableController.Variables.Should().ContainKey(varName, $"переменная \"{varName}\" не существует");
            this.variableController.Variables.TryRemove(varName, out _);
        }

        /// <summary>
        /// Шаг для очистки значения переменной.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я очищаю переменную ""(.+)""")]
        public void EmtpyVariable(string varName)
        {
            this.variableController.Variables.Should().ContainKey(varName, $"переменная \"{varName}\" не существует");
            this.variableController.SetVariable(varName, typeof(object), null);
        }

        /// <summary>
        /// Шаг для изменения значения переменной.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="value">Значение переменной.</param>
        [StepDefinition(@"я изменяю значение переменной ""(.+)"" на ""(.+)""")]
        public void ChangeVariable(string varName, object value)
        {
            this.variableController.Variables.Should().ContainKey(varName, $"переменная \"{varName}\" не существует");
            this.variableController.SetVariable(varName, value.GetType(), value);
        }

        /// <summary>
        /// Шаг для сохранения значения однострочного текста в переменную.
        /// </summary>
        /// <param name="text">Текст для сохранения в переменную.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю текст ""(.*)"" в переменную ""(.+)""")]
        public void StoreAsVariableString(string text, string varName)
        {
            var str = variableController.ReplaceVariables(text);
            Log.Logger().LogDebug($"Replaced text with variables is equal to {Environment.NewLine}{str}");
            this.variableController.SetVariable(varName, typeof(string), str);
        }

        /// <summary>
        /// Шаг для сохранения зашифрованного значения однострочного текста в переменную.
        /// </summary>
        /// <param name="text">Зашифрованный текст для сохранения в переменную.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю зашифрованный текст ""(.*)"" в переменную ""(.+)""")]
        public void StoreAsVariableEncriptedString(string text, string varName)
        {
            this.variableController.SetVariable(varName, typeof(string), Encryptor.Decrypt(text));
        }

        /// <summary>
        /// Шаг для сохранения многострочного текста в переменную.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="text">Текст для сохранения в переменную.</param>
        [StepDefinition(@"я сохраняю текст в переменную ""(.+)"":")]
        public void StoreAsVariableText(string varName, string text)
        {
            var str = variableController.ReplaceVariables(text);

            Log.Logger().LogDebug(str.TryParseToXml()
                ? $"Replaced multiline text with variables is equal to {Environment.NewLine}{Converter.CreateXMLEscapedString(str)}"
                : $"Replaced multiline text with variables is equal to {Environment.NewLine}{str}");

            this.variableController.SetVariable(varName, typeof(string), str);
        }

        /// <summary>
        /// Шаг для сохранения числа в переменную (float, int).
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="number">Число.</param>
        [StepDefinition(@"я сохраняю число ""(.+)"" в переменную ""(.*)""")]
        public void StoreAsVariableNumber(string number, string varName)
        {
            if (decimal.TryParse(number, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out var dec))
            {
                this.variableController.SetVariable(varName, typeof(decimal), dec);
                return;
            }

            if (decimal.TryParse(number, System.Globalization.NumberStyles.Float, new System.Globalization.NumberFormatInfo() { PercentDecimalSeparator = ".", CurrencyDecimalSeparator = ".", NumberDecimalSeparator = "." }, out dec))
            {
                this.variableController.SetVariable(varName, typeof(decimal), dec);
                return;
            }

            this.variableController.SetVariable(varName, typeof(int), int.Parse(number));
        }

        /// <summary>
        /// Шаг для сохранения XML документа в переменную.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="xml">Текст, представленный в виде XML.</param>
        [StepDefinition(@"я сохраняю текст как XML документ в переменную ""(.+)"":")]
        public void StoreAsVariableXmlFromText(string varName, string xml)
        {
            var xmlBody = this.variableController.ReplaceVariables(xml);

            Log.Logger().LogInformation($"input xml is:{Environment.NewLine}{Converter.CreateXMLEscapedString(xmlBody)}");

            var doc = Converter.CreateXmlDoc(xmlBody);
            doc.Should().NotBeNull($"создать XmlDoc из строки {Environment.NewLine}\"{Converter.CreateXMLEscapedString(xmlBody)}\" не удалось");

            this.variableController.SetVariable(varName, doc.GetType(), doc);
        }

        /// <summary>
        /// Шаг для сохранения значения одной переменной в другую.
        /// </summary>
        /// <param name="varName">Исходная переменная.</param>
        /// <param name="newVarName">Переменная-результат.</param>
        [StepDefinition(@"я сохраняю значение переменной ""(.+)"" в переменную ""(.+)""")]
        public void StoreVariableValueToVariable(string varName, string newVarName)
        {
            var value = this.variableController.GetVariableValue(varName);
            value.Should().NotBeNull($"значения в переменной \"{varName}\" нет");

            this.variableController.SetVariable(newVarName, value.GetType(), value);
        }

        /// <summary>
        /// Шаг для сохранения содержимого (в виде текста) одной переменной в другую.
        /// </summary>
        /// <param name="varName">Исходная переменная.</param>
        /// <param name="newVarName">Переменная-результат.</param>
        [StepDefinition(@"я сохраняю содержимое переменной ""(.+)"" в переменную ""(.+)""")]
        public void StoreVariableTextToVariable(string varName, string newVarName)
        {
            var value = this.variableController.GetVariableValueText(varName);
            value.Should().NotBeNull($"содержимого в переменной \"{varName}\" нет");

            this.variableController.SetVariable(newVarName, value.GetType(), value);
        }

        /// <summary>
        /// Шаг сохранения результата значения переменной, содержащей cdata в переменную.
        /// </summary>
        /// <param name="cdata">Переменная с cdata.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю значение переменной \""(.+)\"" из CDATA в переменную \""(.+)\""")]
        public void StoreCDataVariable_ToVariable(string cdataVar, string varName)
        {
            var value = (string)this.variableController.GetVariableValue(varName);
            value.Should().NotBeNull($"значения в переменной \"{varName}\" нет");
            var cdata = Converter.CreateCData(value);
            cdata.Should().NotBeNull($"значение переменной \"{Environment.NewLine + cdata + Environment.NewLine}\" не является CDATA");

            this.variableController.SetVariable(varName, typeof(XDocument), cdata);
        }

        /// <summary>
        /// Шаг для подстановки значения переменной в текст и сохранения результата в новую переменную.
        /// </summary>
        /// <param name="varName">Идентификатор исходной переменной.</param>
        /// <param name="text">Текст.</param>
        /// <param name="newVarName">Идентификатор результирующей переменной.</param>
        [StepDefinition(@"я подставляю значение переменной ""(.+)"" в текст ""(.*)"" и сохраняю в переменную ""(.+)""")]
        public void StoreAsVariableStringFormat(string varName, string text, string newVarName)
        {
            var replacement = string.Empty;

            if (this.variableController.GetVariableValue(varName) != null)
            {
                if (this.variableController.Variables[varName].Type == typeof(string))
                {
                    replacement = (string)this.variableController.GetVariableValue(varName);
                }
                else
                {
                    replacement = this.variableController.GetVariableValue(varName).ToString();
                }
            }

            Log.Logger().LogInformation($"Result text is equal to {Environment.NewLine}{replacement}");
            this.variableController.SetVariable(newVarName, typeof(string), text?.Replace($"{{{varName}}}", replacement));
        }

        /// <summary>
        /// Шаг проверки, что значение переменной не является NULL.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" не является NULL")]
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" существует")]
        public void CheckVariableIsNotNull(string varName)
        {
            var value = this.variableController.GetVariableValue(varName);
            value.Should().NotBeNull($"значение переменной \"{varName}\" является NULL");
        }

        /// <summary>
        /// Шаг проверки, что значение переменной является NULL.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" является NULL")]
        [Then(@"я убеждаюсь, что значения переменной ""(.+)"" нет")]
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" не существует")]
        public void CheckVariableIsNull(string varName)
        {
            var value = this.variableController.GetVariableValue(varName);
            value.Should().BeNull($"значение переменной \"{varName}\" не является NULL");
        }

        /// <summary>
        /// Шаг проверки, что значение переменной не является пустой строкой.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" не является пустой строкой")]
        public void CheckVariableIsNotEmpty(string varName)
        {
            var value = this.variableController.GetVariableValue(varName);
            value.Should().NotBeNull($"значения в переменной \"{varName}\" нет");
            if (this.variableController.GetVariable(varName)?.Type == typeof(string))
            {
                string.IsNullOrWhiteSpace((string)value).Should().BeFalse($"значение переменной \"{varName}\" пустая строка");
            }
        }

        /// <summary>
        /// Шаг проверки, что значение переменной  является пустой строкой.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" пустая строка")]
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" равно пустой строке")]
        public void CheckVariableIsEmpty(string varName)
        {
            var value = this.variableController.GetVariableValue(varName);
            value.Should().NotBeNull($"значения в переменной \"{varName}\" нет");
            if (this.variableController.GetVariable(varName)?.Type == typeof(string))
            {
                string.IsNullOrWhiteSpace((string)value).Should().BeTrue($"значение переменной \"{varName}\" не пустая строка");
            }
        }

        /// <summary>
        /// Шаг проверки, что значение переменной равно переданному объекту.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="expected">Expected значение.</param>
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" равно ""(.+)""")]
        public void CheckVariableEquals(string varName, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;

            var actual = this.variableController.GetVariableValueText(varName);
            actual.Should().NotBeNull($"значения в переменной \"{varName}\" нет");
            expected.Should().Be(actual, $"значение переменной \"{varName}\":\"{actual}\" не равно \"{expected}\"");
        }

        /// <summary>
        /// Шаг проверки, что значение переменной не равно переданному объекту.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="expected">Expected значение.</param>
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" не равно ""(.+)""")]
        public void CheckVariableNotEquals(string varName, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;

            var actual = this.variableController.GetVariableValueText(varName);
            actual.Should().NotBeNull($"значения в переменной \"{varName}\" нет");
            expected.Should().NotBe(actual, $"значение переменной \"{varName}\":\"{actual}\" равно \"{expected}\"");
        }

        /// <summary>
        /// Шаг проверки того, что значение переменной содержит строку.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="expected">Expected значение.</param>
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" содержит ""(.+)""")]
        public void CheckVariableContains(string varName, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;

            var actual = this.variableController.GetVariableValueText(varName);
            actual.Should().NotBeNull($"значения в переменной \"{varName}\" нет");
            actual.Should().Contain(expected, $"значение переменной \"{varName}\":\"{actual}\" не содержит \"{expected}\"");
        }

        /// <summary>
        /// Шаг проверки того, что значение переменной не содержит строку.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="expected">Expected значение.</param>
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" не содержит ""(.+)""")]
        public void CheckVariableNotContains(string varName, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;

            var actual = this.variableController.GetVariableValueText(varName);
            actual.Should().NotBeNull($"значения в переменной \"{varName}\" нет");
            actual.Should().NotContain(expected, $"значение переменной \"{varName}\":\"{actual}\" содержит \"{expected}\"");
        }

        /// <summary>
        /// Шаг проверки того, что значение переменной начинается со строки.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="expected">Expected значение.</param>
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" начинается с ""(.+)""")]
        public void CheckVariableStartsWith(string varName, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;

            var actual = this.variableController.GetVariableValueText(varName);
            actual.Should().NotBeNull($"значения в переменной \"{varName}\" нет");
            actual.Should().StartWith(expected, $"значение переменной \"{varName}\":\"{actual}\" не начинается с \"{expected}\"");
        }

        /// <summary>
        /// Шаг проверки того, что значение переменной закачивается строкой.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="expected">Expected значение.</param>
        [Then(@"я убеждаюсь, что значение переменной ""(.+)"" заканчивается с ""(.+)""")]
        public void CheckVariableEndsWith(string varName, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;

            var actual = this.variableController.GetVariableValueText(varName);
            actual.Should().NotBeNull($"значения в переменной \"{varName}\" нет");
            actual.Should().EndWith(expected, $"значение переменной \"{varName}\":\"{actual}\" не заканчивается с \"{expected}\"");
        }

        /// <summary>
        /// Шаг для сохранения коллекции в переменную без указания типа.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="collection">Коллекция.</param>
        [StepDefinition(@"я сохраняю коллекцию в переменную ""(.+)"":")]
        public void StoreEnumerableAsVariableNoType(string varName, IEnumerable<object> collection)
        {
            varName.Should().NotBeNull("Значение \"varName\" не задано");
            this.variableController.SetVariable(varName, collection.GetType(), collection);
        }

        /// <summary>
        /// Шаг для сохранения коллекции в переменную c указанием типа.
        /// </summary>
        /// <param name="varType">Идентификатор типа переменной.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="collection">Коллекция.</param>
        [StepDefinition(@"я сохраняю коллекцию с типом ""(.+)"" в переменную ""(.+)"":")]
        public void StoreEnumerableAsVariableWithType(TypeCode varType, string varName, IEnumerable<object> collection)
        {
            varName.Should().NotBeNull("Значение \"varName\" не задано");
            switch (varType) {
                case TypeCode.Object:
                    var tmpParserObject = collection.TryParse<object>();
                    this.variableController.SetVariable(varName, tmpParserObject.GetType(), tmpParserObject);
                    break;
                case TypeCode.Int32:
                    var tmpParserInt = collection.TryParse<int>();
                    this.variableController.SetVariable(varName, tmpParserInt.GetType(), tmpParserInt);
                    break;
                case TypeCode.Boolean:
                    var tmpParserBool = collection.TryParse<bool>();
                    this.variableController.SetVariable(varName, tmpParserBool.GetType(), tmpParserBool);
                    break;
                case TypeCode.String:
                    var tmpParserString = collection.TryParse<string>();
                    this.variableController.SetVariable(varName, tmpParserString.GetType(), tmpParserString);
                    break;
                case TypeCode.Double:
                    var tmpParserStringDouble = collection.TryParse<double>();
                    this.variableController.SetVariable(varName, tmpParserStringDouble.GetType(), tmpParserStringDouble);
                    break;
                case TypeCode.Single:
                    var tmpParserStringFloat = collection.TryParse<float>();
                    this.variableController.SetVariable(varName, tmpParserStringFloat.GetType(), tmpParserStringFloat);
                    break;
                case TypeCode.Int64:
                    var tmpParserStringLong = collection.TryParse<long>();
                    this.variableController.SetVariable(varName, tmpParserStringLong.GetType(), tmpParserStringLong);
                    break;
            }
        }

        /// <summary>
        /// Шаг для сохранения произвольного значения из коллекции в переменную.
        /// </summary>
        /// <param name="collectionName">Идентификатор коллекции.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я выбираю произвольное значение из коллекции ""(.+)"" и записываю его в переменную ""(.+)""")]
        public void StoreRandomVariableFromEnumerable(string collectionName, string varName)
        {
            collectionName.Should().NotBeNull("Значение \"collectionName\" не задано");
            varName.Should().NotBeNull("Значение \"varName\" не задано");
            var collection = this.variableController.GetVariableValue(collectionName);
            collection.Should().NotBeNull($"Значения в переменной \"{collectionName}\" нет");
            (collection is IEnumerable).Should().BeTrue($"\"{collectionName}\" не является коллекцией");
            switch (collection)
            {
                case List<object> listObj:
                    var valueObj = listObj.GetRandomValueFromEnumerable();
                    variableController.SetVariable(varName, valueObj.GetType(), valueObj);
                    break;
                case List<int> listInt:
                    var valueInt = listInt.GetRandomValueFromEnumerable();
                    variableController.SetVariable(varName, valueInt.GetType(), valueInt);
                    break;
                case List<bool> listBool:
                    var valueBool = listBool.GetRandomValueFromEnumerable();
                    variableController.SetVariable(varName, valueBool.GetType(), valueBool);
                    break;
                case List<double> listDouble:
                    var valueDouble = listDouble.GetRandomValueFromEnumerable();
                    variableController.SetVariable(varName, valueDouble.GetType(), valueDouble);
                    break;
                case List<string> listString:
                    var valueString = listString.GetRandomValueFromEnumerable();
                    variableController.SetVariable(varName, valueString.GetType(), valueString);
                    break;
                case List<float> listFloat:
                    var valueFloat = listFloat.GetRandomValueFromEnumerable();
                    variableController.SetVariable(varName, valueFloat.GetType(), valueFloat);
                    break;
                case List<long> listLong:
                    var valueLong = listLong.GetRandomValueFromEnumerable();
                    variableController.SetVariable(varName, valueLong.GetType(), valueLong);
                    break;
            }
        }

        /// <summary>
        /// Шаг для сохранения значения из коллекции в переменную.
        /// </summary>
        /// <param name="collectionName">Идентификатор коллекции.</param>
        /// <param name="number">Номер переменной.</param>
        /// <param name="varName">Идентификатор переменной.</param>

        [StepDefinition(@"я выбираю значение из коллекции ""(.+)"" с номером ""(.+)"" и записываю его в переменную ""(.+)""")]
        public void StoreVariableFromEnumerable(string collectionName, string number, string varName)
        {
            collectionName.Should().NotBeNull("Значение \"collectionName\" не задано");
            varName.Should().NotBeNull("Значение \"varName\" не задано");
            number.Should().NotBeNull("Значение \"number\" не задано");
            if (!int.TryParse(number, out var numberValue)) throw new NotValidNumberException($"Значение \"{number}\" не является числом.");
            var collection = this.variableController.GetVariableValue(collectionName);
            (collection is IEnumerable).Should().BeTrue($"\"{collectionName}\" не является коллекцией");
            switch (collection)
            {
                case List<object> listObj:
                    listObj.Count.Should().BeGreaterThan(numberValue,
                        $"Номер значения - \"{number}\" не содержится в коллекции \"{collectionName}\" с размером {listObj.Count}");
                    var valueObj = listObj.GetValueFromEnumerable(numberValue);
                    this.variableController.SetVariable(varName, valueObj.GetType(), valueObj);
                    break;
                case List<int> listInt:
                    listInt.Count.Should().BeGreaterThan(numberValue,
                        $"Номер значения - \"{number}\" не содержится в коллекции \"{collectionName}\" с размером {listInt.Count}");
                    var valueInt = listInt.GetValueFromEnumerable(numberValue);
                    this.variableController.SetVariable(varName, valueInt.GetType(), valueInt);
                    break;
                case List<bool> listBool:
                    listBool.Count.Should().BeGreaterThan(numberValue,
                        $"Номер значения - \"{number}\" не содержится в коллекции \"{collectionName}\" с размером {listBool.Count}");
                    var valueBool = listBool.GetValueFromEnumerable(numberValue);
                    this.variableController.SetVariable(varName, valueBool.GetType(), valueBool);
                    break;
                case List<double> listDouble:
                    listDouble.Count.Should().BeGreaterThan(numberValue,
                        $"Номер значения - \"{number}\" не содержится в коллекции \"{collectionName}\" с размером {listDouble.Count}");
                    var valueDouble = listDouble.GetValueFromEnumerable(numberValue);
                    this.variableController.SetVariable(varName, valueDouble.GetType(), valueDouble);
                    break;
                case List<string> listString:
                    listString.Count.Should().BeGreaterThan(numberValue,
                        $"Номер значения - \"{number}\" не содержится в коллекции \"{collectionName}\" с размером {listString.Count}");
                    var valueString = listString.GetValueFromEnumerable(numberValue);
                    this.variableController.SetVariable(varName, valueString.GetType(), valueString);
                    break;
                case List<float> listFloat:
                    listFloat.Count.Should().BeGreaterThan(numberValue,
                        $"Номер значения - \"{number}\" не содержится в коллекции \"{collectionName}\" с размером {listFloat.Count}");
                    var valueFloat = listFloat.GetValueFromEnumerable(numberValue);
                    this.variableController.SetVariable(varName, valueFloat.GetType(), valueFloat);
                    break;
                case List<long> listLong:
                    listLong.Count.Should().BeGreaterThan(numberValue,
                        $"Номер значения - \"{number}\" не содержится в коллекции \"{collectionName}\" с размером {listLong.Count}");
                    var valueLong = listLong.GetValueFromEnumerable(numberValue);
                    this.variableController.SetVariable(varName, valueLong.GetType(), valueLong);
                    break;
            }
        }

        /// <summary>
        /// Шаг для сохранения словаря в переменную без указания типа.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="dictionary">Словарь.</param>
        [StepDefinition(@"я сохраняю словарь в переменную ""(.+)"":")]
        public void StoreDictionaryAsVariableNoType(string varName, Dictionary<string,object> dictionary)
        {
            varName.Should().NotBeNull("Значение \"varname\" не задано");
            this.variableController.SetVariable(varName, dictionary.GetType(), dictionary);
        }

        /// <summary>
        /// Шаг для сохранения произвольного значения из словаря в переменную.
        /// </summary>
        /// <param name="dictionaryName">Идентификатор словаря.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я выбираю произвольное значение из словаря ""(.+)"" и записываю его в переменную ""(.+)""")]
        public void StoreRandomVariableFromDictionary(string dictionaryName, string varName)
        {
            varName.Should().NotBeNull("Значение \"varName\" не задано");
            dictionaryName.Should().NotBeNull("Значение \"dictionaryName\" не задано");
            var value = ((Dictionary<string, object>)this.variableController.GetVariableValue(dictionaryName)).GetRandomValueFromDictionary();
            this.variableController.SetVariable(varName, value.GetType(), value);
        }

        /// <summary>
        /// Шаг для сохранения значения из коллекции в переменную.
        /// </summary>
        /// <param name="dictionaryName">Идентификатор словаря.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я выбираю значение из словаря ""(.+)"" с ключом ""(.+)"" и записываю его в переменную ""(.+)""")]
        public void StoreVariableFromDictionary(string dictionaryName, string key, string varName)
        {
            dictionaryName.Should().NotBeNull("Значение \"dictionaryName\" не задано");
            key.Should().NotBeNull("Значение \"key\" не задано");
            varName.Should().NotBeNull("Значение \"varName\" не задано");
            key = variableController.ReplaceVariables(key) ?? key;
            var value = ((Dictionary<string, object>)this.variableController.GetVariableValue(dictionaryName)).GetValueFromDictionary(key);
            value.Should().NotBeNull($"В словаре {dictionaryName} нет записи с ключом {key}");
            this.variableController.SetVariable(varName, value.GetType(), value);
        }
    }
}