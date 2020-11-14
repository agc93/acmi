using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace AceCore.Tests.Infrastructure {
    public class PathFileDataAttribute : DataAttribute {
        private readonly string _filePath;
        private readonly string _propertyName;

        /// <summary>
        /// Load data from a JSON file as the data source for a theory
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the JSON file to load</param>
        public PathFileDataAttribute(string filePath)
            : this(filePath, null) { }

        /// <summary>
        /// Load data from a JSON file as the data source for a theory
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the JSON file to load</param>
        /// <param name="propertyName">The name of the property on the JSON file that contains the data for the test</param>
        public PathFileDataAttribute(string filePath, string propertyName) {
            _filePath = filePath;
            _propertyName = propertyName;
        }

        /// <inheritDoc />
        public override IEnumerable<object[]> GetData(MethodInfo testMethod) {
            if (testMethod == null) { throw new ArgumentNullException(nameof(testMethod)); }

            // Get the absolute path to the JSON file
            var path = Path.IsPathRooted(_filePath)
                ? _filePath
                : Path.GetRelativePath(Directory.GetCurrentDirectory(), _filePath);

            if (!File.Exists(path)) {
                throw new ArgumentException($"Could not find file at path: {path}");
            }

            // Load the file
            var fileData = File.ReadAllLines(_filePath);

            if (string.IsNullOrEmpty(_propertyName)) {
                //whole file is the data
                return fileData.Select(line => new[] { line }).Cast<object[]>();
            }

            // Only use the specified property as the data
            return fileData.Where(line => line.StartsWith(_propertyName)).Select(line => new[] { line }).Cast<object[]>();
            /* var allData = JObject.Parse(fileData);
            var data = allData[_propertyName];
            return data.ToObject<List<object[]>>(); */
        }
    }
}