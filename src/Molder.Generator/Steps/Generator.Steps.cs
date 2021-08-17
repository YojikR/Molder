﻿using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Molder.Controllers;
using Molder.Helpers;
using Molder.Infrastructures;
using TechTalk.SpecFlow;
using Molder.Generator.Extensions;
using Molder.Generator.Models.Generators;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Molder.Extensions;
using Microsoft.Extensions.Logging;
using Molder.Models.Directory;
using Molder.Models.File;
using TechTalk.SpecFlow.Assist;

namespace Molder.Generator.Steps
{
    /// <summary>
    /// Общие шаги для генерации данных.
    /// </summary>
    [Binding]
    public class GeneratorSteps 
    {
        AsyncLocal<List<string>> _paths = new() { Value = new List<string>() };  
        
        private string _locale = string.Empty;
        public IFakerGenerator fakerGenerator = null;

        private readonly VariableController variableController;
        private readonly FeatureContext featureContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="Generator"/> class.
        /// Привязка общих шагов к работе с переменным через контекст.
        /// </summary>
        /// <param name="variableController">Контекст для работы с переменными.</param>
        /// <param name="featureContext">Контекст feature файла.</param>
        public GeneratorSteps(VariableController variableController, FeatureContext featureContext)
        {
            this.variableController = variableController;
            this.featureContext = featureContext;
            fakerGenerator = new FakerGenerator();
            //{
            //    var userDir = new UserDirectory().Get();
            //    var dir = $"{userDir}{Path.DirectorySeparatorChar}{featureContext.FeatureInfo.Title}";
            //    variableController.SetVariable(Infrastructures.Constants.USER_DIR, dir.GetType(), dir);
            //}
            //{
            //    var binDir = new BinDirectory().Get();
            //    variableController.SetVariable(Infrastructures.Constants.BIN_DIR, binDir.GetType(), binDir);
            //}
            //if(featureContext.ContainsKey(Infrastructures.Constants.PATHS)) return;
            //featureContext.Add(Infrastructures.Constants.PATHS, new List<string>());
        }

        [ExcludeFromCodeCoverage]
        [BeforeScenario(Order = -20000)]
        public void BeforeScenario()
        {
            _locale = featureContext.Locale();
            fakerGenerator = new FakerGenerator
            {
                Locale = _locale
            };
            ((FakerGenerator) fakerGenerator).ReloadLocale();
        }

        [StepArgumentTransformation]
        public IEnumerable<Models.DTO.FileInfo> GetFilesInfo(Table table)
        {
            return table.ReplaceWith(variableController).CreateSet<Models.DTO.FileInfo>();
        }
        
        #region Store DateTime
        /// <summary>
        /// Шаг для сохранения даты в переменную.
        /// </summary>
        /// <param name="day">День.</param>
        /// <param name="month">Месяц.</param>
        /// <param name="year">Год.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю дату ([0-9]{1,2})\.([0-9]{2})\.([0-9]+) в переменную ""(.+)""")]
        public void StoreAsVariableDate(int day, int month, int year, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            var dt = fakerGenerator.GetDate(day, month, year);
            dt.Should().NotBeNull($"проверьте корректность создания даты day:{day},month:{month},year:{year}");
            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{dt}");
            variableController.SetVariable(varName, dt.GetType(), dt);
        }

        /// <summary>
        /// Шаг для сохранения даты в переменную, используя конкретный формат.
        /// </summary>
        /// <param name="day">День.</param>
        /// <param name="month">Месяц.</param>
        /// <param name="year">Год.</param>
        /// <param name="format">Формат представления даты.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю дату ([0-9]{1,2})\.([0-9]{2})\.([0-9]+) в формате ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableDateWithFormat(int day, int month, int year, string format, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            var dt = fakerGenerator.GetDate(day, month, year);

            dt.Should().NotBeNull($"проверьте корректность создания даты day:{day},month:{month},year:{year}");
            var strDate = dt?.ToString(format);

            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{strDate}");
            variableController.SetVariable(varName, strDate.GetType(), strDate);
        }

        /// <summary>
        /// Шаг для сохранения точного времени в переменную (с миллисекундами).
        /// </summary>
        /// <param name="hours">Часы.</param>
        /// <param name="minutes">Минуты.</param>
        /// <param name="seconds">Секунды.</param>
        /// <param name="milliseconds">Миллисекунды.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю время ([0-9]{1,2}):([0-9]{2}):([0-9]{2})\.([0-9]+) в переменную ""(.+)""")]
        public void StoreAsVariableTimeLong(int hours, int minutes, int seconds, int milliseconds, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            var dt = fakerGenerator.GetDateTime(1, 1, 1, hours, minutes, seconds, milliseconds);
            dt.Should().NotBeNull($"проверьте корректность создания времени hours:{hours},minutes:{minutes},seconds:{seconds},milliseconds:{milliseconds}");

            Log.Logger().LogInformation($"Result time is equal to {Environment.NewLine}{dt}");
            variableController.SetVariable(varName, dt.GetType(), dt);
        }

        /// <summary>
        /// Шаг для сохранения точного времени (с миллисекундами) в переменную, используя конкретный формат.
        /// </summary>
        /// <param name="hours">Часы.</param>
        /// <param name="minutes">Минуты.</param>
        /// <param name="seconds">Секунды.</param>
        /// <param name="milliseconds">Миллисекунды.</param>
        /// <param name="format">Формат представления времени.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю время ([0-9]{1,2}):([0-9]{2}):([0-9]{2})\.([0-9]+) в формате ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableTimeLongWithFormat(int hours, int minutes, int seconds, int milliseconds, string format, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.GetDateTime(1, 1, 1, hours, minutes, seconds, milliseconds);
            dt.Should().NotBeNull($"проверьте корректность создания времени hours:{hours},minutes:{minutes},seconds:{seconds},milliseconds:{milliseconds}");
            var time = dt?.ToString(format);

            Log.Logger().LogInformation($"Result time is equal to {Environment.NewLine}{time}");
            variableController.SetVariable(varName, time.GetType(), time);
        }

        /// <summary>
        /// Шаг для сохранения даты и времени в переменную.
        /// </summary>
        /// <param name="day">День.</param>
        /// <param name="month">Месяц.</param>
        /// <param name="year">Год.</param>
        /// <param name="hours">Часы.</param>
        /// <param name="minutes">Минуты.</param>
        /// <param name="seconds">Секунды.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю дату и время ([0-9]{1,2})\.([0-9]{2})\.([0-9]+) ([0-9]{1,2}):([0-9]{2}):([0-9]{2}) в переменную ""(.+)""")]
        public void StoreAsVariableDateTime(int day, int month, int year, int hours, int minutes, int seconds, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.GetDateTime(day, month, year, hours, minutes, seconds);
            dt.Should().NotBeNull($"проверьте корректность создания даты и времени day:{day},month:{month},year:{year},hours:{hours},minutes:{minutes},seconds:{seconds}");

            Log.Logger().LogInformation($"Result dateTime is equal to {Environment.NewLine}{dt}");
            variableController.SetVariable(varName, dt.GetType(), dt);
        }

        /// <summary>
        /// Шаг для сохранения даты и времены в переменную, используя конкретный формат.
        /// </summary>
        /// <param name="day">День.</param>
        /// <param name="month">Месяц.</param>
        /// <param name="year">Год.</param>
        /// <param name="hours">Часы.</param>
        /// <param name="minutes">Минуты.</param>
        /// <param name="seconds">Секунды.</param>
        /// <param name="format">Формат представления даты и времени.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю дату и время ([0-9]{1,2})\.([0-9]{2})\.([0-9]+) ([0-9]{1,2}):([0-9]{2}):([0-9]{2}) в формате ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableDateTimeWithFormat(int day, int month, int year, int hours, int minutes, int seconds, string format, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.GetDateTime(day, month, year, hours, minutes, seconds);
            dt.Should().NotBeNull($"проверьте корректность создания даты и времени day:{day},month:{month},year:{year},hours:{hours},minutes:{minutes},seconds:{seconds}");

            var dateTime = dt?.ToString(format);

            Log.Logger().LogInformation($"Result dateTime is equal to {Environment.NewLine}{dateTime}");
            variableController.SetVariable(varName, dateTime.GetType(), dateTime);
        }

        #endregion
        #region Current DateTime
        /// <summary>
        /// Шаг для сохранения текущей даты в переменную.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю текущую дату в переменную ""(.+)""")]
        public void StoreAsVariableCurrentDate(string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var now = fakerGenerator.Current();

            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{now}");
            variableController.SetVariable(varName, now.GetType(), now);
        }

        /// <summary>
        /// Шаг для сохранения текущей даты в переменную, используя конкретный формат.
        /// </summary>
        /// <param name="format">Формат представления даты.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю текущую дату в формате ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableCurrentDateWithFormat(string format, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var now = fakerGenerator.Current().ToString(format);

            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{now}");
            variableController.SetVariable(varName, now.GetType(), now);
        }
        #endregion
        #region Random DateTime
        [StepDefinition(@"я сохраняю рандомную дату в переменную ""(.+)""")]
        public void StoreAsVariableRandomDateTime(string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.Between();
            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{dt}");
            variableController.SetVariable(varName, dt.GetType(), dt);
        }

        [StepDefinition(@"я сохраняю рандомную дату в формате ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableRandomDateTime(string format, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.Between();
            var randomDateTime = dt.ToString(format);
            variableController.SetVariable(varName, randomDateTime.GetType(), randomDateTime);
        }
        #endregion
        #region Past DateTime

        /// <summary>
        /// Шаг для сохранения прошедшей даты, которая отличается от текущей на определенный срок в переменную.
        /// </summary>
        /// <param name="year">Количество лет от текущей даты.</param>
        /// <param name="month">Количество месяцев от текущей даты.</param>
        /// <param name="day">Количество дней от текущей даты.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю прошедшую дату, которая отличается от текущей на ""([0-9]+)"" (?:лет|год[а]?) ""([0-9]+)"" (?:месяц|месяц(?:а|ев)) ""([0-9]+)"" (?:день|дн(?:я|ей)) в переменную ""(.+)""")]
        public void StoreAsVariablePastDateTimeWithDifference(int year, int month, int day, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.GetDate(day, month, year, false);
            dt.Should().NotBeNull($"проверьте корректность создания даты day:{day},month:{month},year:{year}");

            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{dt}");
            variableController.SetVariable(varName, dt.GetType(), dt);
        }

        /// <summary>
        /// Шаг для сохранения прошедшей даты, которая отличается от текущей на определенный срок в переменную, используя конкретный формат.
        /// </summary>
        /// <param name="year">Количество лет от текущей даты.</param>
        /// <param name="month">Количество месяцев от текущей даты.</param>
        /// <param name="day">Количество дней от текущей даты.</param>
        /// <param name="format">Формат представления даты.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю прошедшую дату, которая отличается от текущей на ""([0-9]+)"" (?:лет|год[а]?) ""([0-9]+)"" (?:месяц|месяц(?:а|ев)) ""([0-9]+)"" (?:день|дн(?:я|ей)) в формате ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariablePastDateTimeWithDifference(int year, int month, int day, string format, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.GetDate(day, month, year, false);
            dt.Should().NotBeNull($"проверьте корректность создания даты day:{day},month:{month},year:{year}");
            var pastDateTime = dt?.ToString(format);

            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{pastDateTime}");
            variableController.SetVariable(varName, pastDateTime.GetType(), pastDateTime);
        }

        /// <summary>
        /// Шаг для сохранения прошедшей даты, которая отличается от текущей на определенный срок в переменную, используя конкретный формат.
        /// </summary>
        /// <param name="year">Количество лет от текущей даты.</param>
        /// <param name="month">Количество месяцев от текущей даты.</param>
        /// <param name="day">Количество дней от текущей даты.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="fYear"></param>
        /// <param name="fMonth"></param>
        /// <param name="fDay"></param>
        [StepDefinition(@"я сохраняю прошедшую дату, которая отличается от ([0-9]{1,2})\.([0-9]{2})\.([0-9]+) на ""([0-9]+)"" (?:лет|год[а]?) ""([0-9]+)"" (?:месяц|месяц(?:а|ев)) ""([0-9]+)"" (?:день|дн(?:я|ей)) в переменную ""(.+)""")]
        public void StoreAsVariablePastDateTime(int fYear, int fMonth, int fDay, int year, int month, int day, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.GetDate(fDay, fMonth, fYear);
            dt.Should().NotBeNull($"проверьте корректность создания даты day:{fDay},month:{fMonth},year:{fYear}");

            var pdt = fakerGenerator.GetDate(day, month, year, false, dt);
            pdt.Should().NotBeNull($"проверьте корректность создания даты day:{day},month:{month},year:{year}");

            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{pdt}");
            variableController.SetVariable(varName, pdt.GetType(), pdt);
        }

        /// <summary>
        /// Шаг для сохранения прошедшей даты, которая отличается от текущей на определенный срок в переменную, используя конкретный формат.
        /// </summary>
        /// <param name="year">Количество лет от текущей даты.</param>
        /// <param name="month">Количество месяцев от текущей даты.</param>
        /// <param name="day">Количество дней от текущей даты.</param>
        /// <param name="format">Формат представления даты.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="fYear"></param>
        /// <param name="fMonth"></param>
        /// <param name="fDay"></param>
        [StepDefinition(@"я сохраняю прошедшую дату, которая отличается от ([0-9]{1,2})\.([0-9]{2})\.([0-9]+) на ""([0-9]+)"" (?:лет|год[а]?) ""([0-9]+)"" (?:месяц|месяц(?:а|ев)) ""([0-9]+)"" (?:день|дн(?:я|ей)) в формате ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariablePastDateTime(int fYear, int fMonth, int fDay, int year, int month, int day, string format, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.GetDate(fDay, fMonth, fYear);
            dt.Should().NotBeNull($"проверьте корректность создания даты day:{fDay},month:{fMonth},year:{fYear}");

            var pdt = fakerGenerator.GetDate(day, month, year, false, dt);
            pdt.Should().NotBeNull($"проверьте корректность создания даты day:{day},month:{month},year:{year}");
            var pastDateTime = pdt?.ToString(format);

            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{pastDateTime}");
            variableController.SetVariable(varName, pastDateTime.GetType(), pastDateTime);
        }
        #endregion
        #region Future DateTime
        /// <summary>
        /// Шаг для сохранения будущей даты, которая отличается от текущей на определенный срок в переменную.
        /// </summary>
        /// <param name="year">Количество лет от текущей даты.</param>
        /// <param name="month">Количество месяцев от текущей даты.</param>
        /// <param name="day">Количество дней от текущей даты.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю будущую дату, которая отличается от текущей на ""([0-9]+)"" (?:лет|год[а]?) ""([0-9]+)"" (?:месяц|месяц(?:а|ев)) ""([0-9]+)"" (?:день|дн(?:я|ей)) в переменную ""(.+)""")]
        public void StoreAsVariableFutureDateTimeWithDifference(int year, int month, int day, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.GetDate(day, month, year, true);
            dt.Should().NotBeNull($"Проверьте корректность создания даты day:{day},month:{month},year:{year}.");

            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{dt}");
            variableController.SetVariable(varName, dt.GetType(), dt);
        }


        /// <summary>
        /// Шаг для сохранения будущей даты, которая отличается от текущей на определенный срок в переменную, используя конкретный формат.
        /// </summary>
        /// <param name="year">Количество лет от текущей даты.</param>
        /// <param name="month">Количество месяцев от текущей даты.</param>
        /// <param name="day">Количество дней от текущей даты.</param>
        /// <param name="format">Формат представления даты.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю будущую дату, которая отличается от текущей на ""([0-9]+)"" (?:лет|год[а]?) ""([0-9]+)"" (?:месяц|месяц(?:а|ев)) ""([0-9]+)"" (?:день|дн(?:я|ей)) в формате ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableFutureDateTimeWithDifference(int year, int month, int day, string format, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.GetDate(day, month, year, true);
            dt.Should().NotBeNull($"Проверьте корректность создания даты day:{day},month:{month},year:{year}.");
            var futureDateTime = dt?.ToString(format);

            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{futureDateTime}");
            variableController.SetVariable(varName, futureDateTime.GetType(), futureDateTime);
        }

        /// <summary>
        /// Шаг для сохранения будущей даты, которая отличается от даты на определенный срок в переменную.
        /// </summary>
        /// <param name="year">Количество лет от текущей даты.</param>
        /// <param name="month">Количество месяцев от текущей даты.</param>
        /// <param name="day">Количество дней от текущей даты.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="fYear"></param>
        /// <param name="fMonth"></param>
        /// <param name="fDay"></param>
        [StepDefinition(@"я сохраняю будущую дату, которая отличается от ([0-9]{1,2})\.([0-9]{2})\.([0-9]+) на ""([0-9]+)"" (?:лет|год[а]?) ""([0-9]+)"" (?:месяц|месяц(?:а|ев)) ""([0-9]+)"" (?:день|дн(?:я|ей)) в переменную ""(.+)""")]
        public void StoreAsVariableFutureDateTime(int fYear, int fMonth, int fDay, int year, int month, int day, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.GetDate(fDay, fMonth, fYear);
            dt.Should().NotBeNull($"Проверьте корректность создания даты day:{fDay},month:{fMonth},year:{fYear}.");

            var fdt = fakerGenerator.GetDate(day, month, year, true, dt);
            fdt.Should().NotBeNull($"Проверьте корректность создания даты day:{day},month:{month},year:{year}.");

            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{fdt}");
            variableController.SetVariable(varName, fdt.GetType(), fdt);
        }

        /// <summary>
        /// Шаг для сохранения будущей даты, которая отличается от даты на определенный срок в переменную, используя конкретный формат.
        /// </summary>
        /// <param name="year">Количество лет от текущей даты.</param>
        /// <param name="month">Количество месяцев от текущей даты.</param>
        /// <param name="day">Количество дней от текущей даты.</param>
        /// <param name="format">Формат представления даты.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        /// <param name="fYear"></param>
        /// <param name="fMonth"></param>
        /// <param name="fDay"></param>
        [StepDefinition(@"я сохраняю будущую дату, которая отличается от ([0-9]{1,2})\.([0-9]{2})\.([0-9]+) на ""([0-9]+)"" (?:лет|год[а]?) ""([0-9]+)"" (?:месяц|месяц(?:а|ев)) ""([0-9]+)"" (?:день|дн(?:я|ей)) в формате ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableFutureDateTime(int fYear, int fMonth, int fDay, int year, int month, int day, string format, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var dt = fakerGenerator.GetDate(fDay, fMonth, fYear);
            dt.Should().NotBeNull($"Проверьте корректность создания даты day:{fDay},month:{fMonth},year:{fYear}.");

            var fdt = fakerGenerator.GetDate(day, month, year, true, dt);
            fdt.Should().NotBeNull($"Проверьте корректность создания даты day:{day},month:{month},year:{year}.");
            var futureDateTime = fdt?.ToString(format);

            Log.Logger().LogInformation($"Result date is equal to {Environment.NewLine}{futureDateTime}");
            variableController.SetVariable(varName, futureDateTime.GetType(), futureDateTime);
        }
        #endregion
        #region Random string with prefix
        /// <summary>
        /// Шаг для сохранения случанойго набора букв и цифр в переменную, используя конкретный префикс.
        /// </summary>
        /// <param name="len">Длина строки.</param>
        /// <param name="prefix">Префикс.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю случайный набор букв и цифр длиной ([0-9]+) знаков с префиксом ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableRandomStringWithPrefix(int len, string prefix, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            len.Check(prefix, string.Empty);

            var str = prefix + fakerGenerator.String(len - prefix.Length);
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        /// <summary>
        /// Шаг для добавления случайного набора букв в переменную, используя конкретный префикс.
        /// </summary>
        /// <param name="len">Длина строки.</param>
        /// <param name="prefix">Префикс.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю случайный набор букв длиной ([0-9]+) знаков с префиксом ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableRandomCharWithPrefix(int len, string prefix, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            len.Check(prefix, string.Empty);

            var str = prefix + fakerGenerator.Chars(len - prefix.Length);
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        /// <summary>
        /// Шаг для добавления случайного набора цифр в переменную, ипользуя конкретный префикс.
        /// </summary>
        /// <param name="len">Длина строки.</param>
        /// <param name="prefix">Префикс.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю случайный набор цифр длиной ([0-9]+) знаков с префиксом ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableRandomNumberWithPrefix(int len, string prefix, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            len.Check(prefix, string.Empty);

            var str = prefix + fakerGenerator.Numbers(len - prefix.Length);
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }
        #endregion
        #region Random string with postfix
        /// <summary>
        /// Шаг для сохранения случанойго набора букв и цифр в переменную, используя конкретный постфикс.
        /// </summary>
        /// <param name="len">Длина строки.</param>
        /// <param name="postfix">Постфикс.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю случайный набор букв и цифр длиной ([0-9]+) знаков с постфиксом ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableRandomStringWithPostFix(int len, string postfix, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            len.Check(string.Empty, postfix);

            var str = fakerGenerator.String(len - postfix.Length) + postfix;
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        /// <summary>
        /// Шаг для добавления случайного набора букв в переменную, используя конкретный префикс.
        /// </summary>
        /// <param name="len">Длина строки.</param>
        /// <param name="postfix">Постфикс.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю случайный набор букв длиной ([0-9]+) знаков с постфиксом ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableRandomCharWithPostfix(int len, string postfix, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            len.Check(string.Empty, postfix);

            var str = fakerGenerator.Chars(len - postfix.Length) + postfix;
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        /// <summary>
        /// Шаг для добавления случайного набора цифр в переменную, ипользуя конкретный префикс.
        /// </summary>
        /// <param name="len">Длина строки.</param>
        /// <param name="postfix">Постфикс.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю случайный набор цифр длиной ([0-9]+) знаков с постфиксом ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableRandomNumberWithPostfix(int len, string postfix, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            len.Check(string.Empty, postfix);

            var str = fakerGenerator.Numbers(len - postfix.Length) + postfix;
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }
        #endregion
        #region Random string
        /// <summary>
        /// Шаг для добавления случайного набора букв и цифр в переменную.
        /// </summary>
        /// <param name="len">Длина строки.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю случайный набор букв и цифр длиной ([0-9]+) знаков в переменную ""(.+)""")]
        public void StoreAsVariableRandomString(int len, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            len.Check();
            var str = fakerGenerator.String(len);

            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        /// <summary>
        /// Шаг для добавления случайного набора букв в переменную.
        /// </summary>
        /// <param name="len">Длина строки.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю случайный набор букв длиной ([0-9]+) знаков в переменную ""(.+)""")]
        public void StoreAsVariableRandomChar(int len, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            len.Check();
            var str = fakerGenerator.Chars(len);

            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        /// <summary>
        /// Шаг для добавления случайного набора цифр в переменную.
        /// </summary>
        /// <param name="len">Длина строки.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю случайный набор цифр длиной ([0-9]+) знаков в переменную ""(.+)""")]
        public void StoreAsVariableRandomNumber(int len, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            len.Check();
            var str = fakerGenerator.Numbers(len);

            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }
        #endregion

        /// <summary>
        /// Шаг для сохранения случайного номера телефона в переменную, используя конкретный формат.
        /// Пример формата: 7##########.
        /// </summary>
        /// <param name="mask">Маска для телефона.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю случайный номер телефона в формате ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableRandomPhone(string mask, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            var str = fakerGenerator.Phone(mask);
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        /// <summary>
        /// Шаг для сохранения UUID в переменную.
        /// </summary>
        /// <param name="varName">Идентификатор переменной.</param>
        [StepDefinition(@"я сохраняю новый (?:универсальный уникальный идентификатор|UUID) в переменную ""(.+)""")]
        public void StoreAsVariableUuid(string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            var str = fakerGenerator.Guid();
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        [StepDefinition(@"я сохраняю случайный месяц в переменную ""(.+)""")]
        public void StoreAsVariableMonth(string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            var str = fakerGenerator.Month();
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        [StepDefinition(@"я сохраняю случайный день недели в переменную ""(.+)""")]
        public void StoreAsVariableWeekday(string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            var str = fakerGenerator.Weekday();
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        [StepDefinition(@"я сохраняю случайный email с провайдером ""(.+)"" в переменную ""(.+)""")]
        public void StoreAsVariableEmail(string provider, string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            var str = fakerGenerator.Email(provider);
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        [StepDefinition(@"я сохраняю случайный Ip адрес в переменную ""(.+)""")]
        public void StoreAsVariableIp(string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            var str = fakerGenerator.Ip();
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        [StepDefinition(@"я сохраняю случайный Url в переменную ""(.+)""")]
        public void StoreAsVariableUrl(string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            var str = fakerGenerator.Url();
            Log.Logger().LogInformation($"Result string is equal to {Environment.NewLine}{str}");
            variableController.SetVariable(varName, str.GetType(), str);
        }

        /// <summary>
        /// Шаг для сохранения Credentials в переменную.
        /// </summary>
        /// <param name="host">Хост.</param>
        /// <param name="authType">Тип авторизации.</param>
        /// <param name="domain">Домен.</param>
        /// <param name="username">Логин.</param>
        /// <param name="password">Зашифрованный пароль.</param>
        /// <param name="varName">Идентификатор переменной.</param>
        [ExcludeFromCodeCoverage]
        [StepDefinition(@"я создаю полномочия для хоста ""(.+)"" c типом ""(.+)"" для пользователя с доменом ""(.+)"", логином ""(.+)"", паролем ""(.+)"" и сохраняю в переменную ""(.+)""")]
        public void StoreCredentialsForHostToVariable(string host, AuthType authType, string domain, string username, string password, string varName)
        {
            var _host = variableController.ReplaceVariables(host) ?? host;
            var _domain = variableController.ReplaceVariables(domain) ?? domain;
            var _username = variableController.ReplaceVariables(username) ?? username;
            var _password = variableController.ReplaceVariables(password) ?? password;

            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var credentialCache = new CredentialCache();
            var networkCredential = new NetworkCredential(_username, _password, _domain);
            credentialCache.Add(new Uri(_host), authType.ToString(), networkCredential);

            Log.Logger().LogInformation($"Create NetworkCredential for {authType.ToString()} with host:{_host}, domain:{_domain} and username:{_username}.");
            variableController.SetVariable(varName, credentialCache.GetType(), credentialCache);
        }

        /// <summary>
        /// Шаг для преобразования значения одной переменной в массив.
        /// </summary>
        /// <param name="varName">Исходная переменная.</param>
        /// <param name="chars">Массив символов-разделителей.</param>
        /// <param name="newVarName">Переменная-результат.</param>
        [StepDefinition(@"я преобразую значение переменной ""(.+)"" в массив, используя символы ""(.+)"" и сохраняю в переменную ""(.+)""")]
        public void StoreVariableValueToArrayVariable(string varName, string chars, string newVarName)
        {
            variableController.Variables.Should().ContainKey(varName, $"переменная \"{varName}\" не существует");
            variableController.Variables.Should().NotContainKey(newVarName, $"переменная \"{newVarName}\" уже существует");

            var str = variableController.GetVariableValueText(varName);
            str.Should().NotBeNull($"Значения в переменной \"{varName}\" нет");

            var enumerable = Converter.CreateEnumerable(str, chars);
            Log.Logger().LogInformation($"Result array is equal to {Environment.NewLine}{string.Join(',', enumerable as string[])}");
            variableController.SetVariable(newVarName, enumerable.GetType(), enumerable);
        }
        
        [StepDefinition(@"я создаю файл:")]
        [StepDefinition(@"я создаю файлы:")]
        public void CreateFiles(IEnumerable<Models.DTO.FileInfo> filesInfo)
        {
            var userDir = variableController.GetVariableValueText(Infrastructures.Constants.USER_DIR);
            foreach (var fileInfo in filesInfo)
            {
                new TextFile().Create(fileInfo.Name, fileInfo.Path ?? userDir, fileInfo.Content).Should().BeTrue($"A file named \"{fileInfo.Name}\" in \"{fileInfo.Path ?? userDir}\" was not created. Detailed information in the logs.");
                var fullpath = $"{fileInfo.Path ?? userDir}{Path.DirectorySeparatorChar}{fileInfo.Name}";
                variableController.SetVariable(fileInfo.Name, typeof(string), fullpath);
            }
        }

        [StepDefinition(@"я проверяю наличие файла:")]
        [StepDefinition(@"я проверяю наличие файлов:")]
        public void ExistsFiles(IEnumerable<Models.DTO.FileInfo> filesInfo)
        {
            var userDir = variableController.GetVariableValueText(Infrastructures.Constants.USER_DIR);
            foreach (var fileInfo in filesInfo)
            {
                new TextFile().IsExist(fileInfo.Name, fileInfo.Path ?? userDir).Should().BeTrue($"A file named \"{fileInfo.Name}\" in \"{fileInfo.Path ?? userDir}\" was not exist. Detailed information in the logs.");
            }
        }
    }
}