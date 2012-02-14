/// Copyright 2011 Timothy James
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
namespace IndexLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using IndexLibrary;

    [Experimental("Not full tested")]
    [System.CLSCompliant(true)]
    public static class IndexWriterRelfectionExtensions
    {
        #region Fields

        private const string NO_VALUE = "{null}";

        #endregion Fields

        #region Methods

        public static void WriteDefinition(this IndexWriter writer, Assembly assembly)
        {
            WriteDefinition(writer, assembly, ReflectionTypes.All);
        }

        public static void WriteDefinition(this IndexWriter writer, Assembly assembly, ReflectionTypes dataToExport)
        {
            if (assembly == null)
                return;
            Type[] assemblyTypes = assembly.GetTypes();
            if (assemblyTypes == null)
                return;
            int totalTypes = assemblyTypes.Length;

            //Type writerClass = typeof(IndexWriterRelfectionExtensions);
            //MethodInfo writerMethod = writerClass.GetMethod("WriteDefinition", new Type[] { typeof(IndexWriter),  typeof(FieldStorage), typeof(DefinitionData) });
            //if (writerMethod == null) return;

            for (int i = 0; i < totalTypes; i++) {
                // we can't output generic parameter types
                //if (assemblyTypes[i].ContainsGenericParameters) continue;

                //MethodInfo typeMethod = writerMethod.MakeGenericMethod(assemblyTypes[i]);
                //typeMethod.Invoke(writer, new object[] { writer, new FieldStorage(true, FieldSearchableRule.Analyzed, FieldVectorRule.No), DefinitionData.All });
                WriteDefinition(writer, assemblyTypes[i], dataToExport);
            }
        }

        public static void WriteDefinition(this IndexWriter writer, Type typeToOutput)
        {
            WriteDefinition(writer, typeToOutput, ReflectionTypes.All);
        }

        public static void WriteDefinition(this IndexWriter writer, Type typeToOutput, ReflectionTypes dataToExport)
        {
            if (typeToOutput == null)
                throw new ArgumentNullException("typeToOutput", "typeToOutput cannot be null");
            if (writer == null)
                throw new ArgumentNullException("writer", "writer cannot be null");
            if (!writer.IsOpen)
                throw new ObjectDisposedException("writer", "You cannot access this method from a disposed IndexWriter");

            string assemblyName = typeToOutput.Assembly.FullName;
            string codeTypeFieldName = "CodeType";
            string assemblyFieldName = "AssemblyName";
            string declaringTypeFieldName = "DeclaringType";
            string declarationModifiersFieldName = "DeclarationModifiers";
            string declarationFieldName = "Declaration";
            string parametersFieldName = "Parameters";

            bool exportAll = ((dataToExport & ReflectionTypes.All) == ReflectionTypes.All);
            int i = 0;

            if (exportAll || (dataToExport & ReflectionTypes.Constructors) == ReflectionTypes.Constructors) {
                ConstructorInfo[] constructors = typeToOutput.GetConstructors();
                if (constructors != null) {
                    int totalConstructors = constructors.Length;
                    for (i = 0; i < totalConstructors; i++) {
                        IndexDocument constructorDocument = new IndexDocument();
                        constructorDocument.Add(new FieldNormal(codeTypeFieldName, "Constructor", true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        constructorDocument.Add(new FieldNormal(assemblyFieldName, assemblyName, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        constructorDocument.Add(new FieldNormal(declaringTypeFieldName, constructors[i].DeclaringType == null ? NO_VALUE : constructors[i].DeclaringType.Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        constructorDocument.Add(new FieldNormal(declarationModifiersFieldName, GetAccessParameters(constructors[i]), true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        constructorDocument.Add(new FieldNormal(declarationFieldName, constructors[i].Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        constructorDocument.Add(new FieldNormal(parametersFieldName, GetParameterString(constructors[i]), true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        writer.Write(constructorDocument);
                    }
                }
            }
            if (exportAll || (dataToExport & ReflectionTypes.Properties) == ReflectionTypes.Properties) {
                PropertyInfo[] properties = typeToOutput.GetProperties();
                if (properties != null) {
                    int totalProperties = properties.Length;
                    for (i = 0; i < totalProperties; i++) {
                        IndexDocument propertyDocument = new IndexDocument();
                        propertyDocument.Add(new FieldNormal(codeTypeFieldName, "Property", true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        propertyDocument.Add(new FieldNormal(assemblyFieldName, assemblyName, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        propertyDocument.Add(new FieldNormal(declaringTypeFieldName, properties[i].DeclaringType == null ? NO_VALUE : properties[i].DeclaringType.Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        propertyDocument.Add(new FieldNormal(declarationModifiersFieldName, GetAccessParameters(properties[i]), true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        propertyDocument.Add(new FieldNormal(declarationFieldName, properties[i].Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        propertyDocument.Add(new FieldNormal(parametersFieldName, NO_VALUE, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        writer.Write(propertyDocument);
                    }
                }
            }
            if (exportAll || (dataToExport & ReflectionTypes.NestedTypes) == ReflectionTypes.NestedTypes) {
                Type[] nestedTypes = typeToOutput.GetNestedTypes();
                if (nestedTypes != null) {
                    int totalNestedTypes = nestedTypes.Length;
                    for (i = 0; i < totalNestedTypes; i++) {
                        IndexDocument nestedDocument = new IndexDocument();
                        nestedDocument.Add(new FieldNormal(codeTypeFieldName, "NestedType", true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        nestedDocument.Add(new FieldNormal(assemblyFieldName, assemblyName, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        nestedDocument.Add(new FieldNormal(declaringTypeFieldName, nestedTypes[i].DeclaringType == null ? NO_VALUE : nestedTypes[i].DeclaringType.Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        nestedDocument.Add(new FieldNormal(declarationModifiersFieldName, GetAccessParameters(nestedTypes[i]), true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        nestedDocument.Add(new FieldNormal(declarationFieldName, nestedTypes[i].Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        nestedDocument.Add(new FieldNormal(parametersFieldName, NO_VALUE, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        writer.Write(nestedDocument);
                    }
                }
            }
            if (exportAll || (dataToExport & ReflectionTypes.Methods) == ReflectionTypes.Methods) {
                MethodInfo[] methods = typeToOutput.GetMethods();
                if (methods != null) {
                    int totalMethods = methods.Length;
                    for (i = 0; i < totalMethods; i++) {
                        IndexDocument methodDocument = new IndexDocument();
                        methodDocument.Add(new FieldNormal(codeTypeFieldName, "Method", true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        methodDocument.Add(new FieldNormal(assemblyFieldName, assemblyName, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        methodDocument.Add(new FieldNormal(declaringTypeFieldName, methods[i].DeclaringType == null ? NO_VALUE : methods[i].DeclaringType.Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        methodDocument.Add(new FieldNormal(declarationModifiersFieldName, GetAccessParameters(methods[i]), true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        methodDocument.Add(new FieldNormal(declarationFieldName, methods[i].Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        methodDocument.Add(new FieldNormal(parametersFieldName, GetParameterString(methods[i]), true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        writer.Write(methodDocument);
                    }
                }
            }
            if (exportAll || (dataToExport & ReflectionTypes.Members) == ReflectionTypes.Members) {
                MemberInfo[] members = typeToOutput.GetMembers();
                if (members != null) {
                    int totalMembers = members.Length;
                    for (i = 0; i < totalMembers; i++) {
                        IndexDocument memberDocument = new IndexDocument();
                        memberDocument.Add(new FieldNormal(codeTypeFieldName, "Member", true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        memberDocument.Add(new FieldNormal(assemblyFieldName, assemblyName, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        memberDocument.Add(new FieldNormal(declaringTypeFieldName, members[i].DeclaringType == null ? NO_VALUE : members[i].DeclaringType.Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        memberDocument.Add(new FieldNormal(declarationModifiersFieldName, NO_VALUE, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        memberDocument.Add(new FieldNormal(declarationFieldName, members[i].Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        memberDocument.Add(new FieldNormal(parametersFieldName, NO_VALUE, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        writer.Write(memberDocument);
                    }
                }
            }
            if (exportAll || (dataToExport & ReflectionTypes.Interfaces) == ReflectionTypes.Interfaces) {
                Type[] interfaces = typeToOutput.GetInterfaces();
                if (interfaces != null) {
                    int totalInterfaces = interfaces.Length;
                    for (i = 0; i < totalInterfaces; i++) {
                        IndexDocument interfaceDocument = new IndexDocument();
                        interfaceDocument.Add(new FieldNormal(codeTypeFieldName, "Interface", true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        interfaceDocument.Add(new FieldNormal(assemblyFieldName, assemblyName, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        interfaceDocument.Add(new FieldNormal(declaringTypeFieldName, interfaces[i].DeclaringType == null ? NO_VALUE : interfaces[i].DeclaringType.Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        interfaceDocument.Add(new FieldNormal(declarationModifiersFieldName, GetAccessParameters(interfaces[i]), true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        interfaceDocument.Add(new FieldNormal(declarationFieldName, interfaces[i].Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        interfaceDocument.Add(new FieldNormal(parametersFieldName, NO_VALUE, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        writer.Write(interfaceDocument);
                    }
                }
            }
            if (exportAll || (dataToExport & ReflectionTypes.Fields) == ReflectionTypes.Fields) {
                FieldInfo[] fields = typeToOutput.GetFields();
                if (fields != null) {
                    int totalFields = fields.Length;
                    for (i = 0; i < totalFields; i++) {
                        IndexDocument fieldDocument = new IndexDocument();
                        fieldDocument.Add(new FieldNormal(codeTypeFieldName, "Field", true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        fieldDocument.Add(new FieldNormal(assemblyFieldName, assemblyName, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        fieldDocument.Add(new FieldNormal(declaringTypeFieldName, fields[i].DeclaringType == null ? NO_VALUE : fields[i].DeclaringType.Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        fieldDocument.Add(new FieldNormal(declarationModifiersFieldName, NO_VALUE, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        fieldDocument.Add(new FieldNormal(declarationFieldName, fields[i].Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        fieldDocument.Add(new FieldNormal(parametersFieldName, GetAccessParameters(fields[i]), true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        writer.Write(fieldDocument);
                    }
                }
            }
            if (exportAll || (dataToExport & ReflectionTypes.Events) == ReflectionTypes.Events) {
                EventInfo[] events = typeToOutput.GetEvents();
                if (events != null) {
                    int totalEvents = events.Length;
                    for (i = 0; i < totalEvents; i++) {
                        IndexDocument eventDocument = new IndexDocument();
                        eventDocument.Add(new FieldNormal(codeTypeFieldName, "Event", true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        eventDocument.Add(new FieldNormal(assemblyFieldName, assemblyName, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        eventDocument.Add(new FieldNormal(declaringTypeFieldName, events[i].DeclaringType == null ? NO_VALUE : events[i].DeclaringType.Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        eventDocument.Add(new FieldNormal(declarationModifiersFieldName, NO_VALUE, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        eventDocument.Add(new FieldNormal(declarationFieldName, events[i].Name, true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        eventDocument.Add(new FieldNormal(parametersFieldName, GetAccessParameters(events[i].GetRaiseMethod()), true, FieldSearchableRule.Analyzed, FieldVectorRule.No));
                        writer.Write(eventDocument);
                    }
                }
            }
        }

        public static void WriteInstance<TKey>(this IndexWriter writer, TKey objectToWrite, FieldStorage storage)
            where TKey : class
        {
            // field name
            // each property has a fieldName (name) and it's value (value)
            if (writer == null)
                throw new ArgumentNullException("writer", "writer cannot be null");
            if (!writer.IsOpen)
                throw new ObjectDisposedException("writer", "You cannot access this method from a disposed IndexWriter");
            if (objectToWrite == null)
                throw new ArgumentNullException("objectToWrite", "objectToWrite cannot be null");
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");
            List<PropertyInfo> properties = new List<PropertyInfo>(objectToWrite.GetType().GetProperties());
            properties.RemoveAll(x => !x.CanRead);
            if (properties.Count == 0)
                throw new InvalidOperationException("You cannot index a class that doesn't have any publicly accessible (get) properties");
            int totalProperties = properties.Count;
            IndexDocument document = new IndexDocument();
            int addedProperties = 0;
            for (int i = 0; i < totalProperties; i++) {
                PropertyInfo info = properties[i];
                object propertyValue = info.GetValue(objectToWrite, null);
                if (propertyValue == null)
                    continue;
                document.Add(new FieldNormal(info.Name, propertyValue.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
                ++addedProperties;
            }

            if (addedProperties > 0)
                writer.Write(document);
        }

        public static void WriteInstance<TKey>(this IndexWriter writer, IEnumerable<TKey> values, FieldStorage storage)
            where TKey : class
        {
            // write all classes to index, but use the same storage rule for all of them
            if (writer == null)
                throw new ArgumentNullException("writer", "writer cannot be null");
            if (!writer.IsOpen)
                throw new ObjectDisposedException("writer", "You cannot access this method from a disposed IndexWriter");
            if (values == null)
                throw new ArgumentNullException("values", "values cannot be null");
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");
            foreach (TKey value in values)
                WriteInstance<TKey>(writer, value, storage);
        }

        public static void WriteInstance<TKey>(this IndexWriter writer, IDictionary<TKey, FieldStorage> values)
            where TKey : class
        {
            // write all classes to index, but use the specified storage rule for each one
            if (writer == null)
                throw new ArgumentNullException("writer", "writer cannot be null");
            if (!writer.IsOpen)
                throw new ObjectDisposedException("writer", "You cannot access this method from a disposed IndexWriter");

            if (values == null)
                throw new ArgumentNullException("values", "values cannot be null");
            foreach (KeyValuePair<TKey, FieldStorage> pair in values) {
                if (pair.Key == null || pair.Value == null)
                    continue;
                WriteInstance<TKey>(writer, pair.Key, pair.Value);
            }
        }

        public static void WriteInstance<TKey>(this IndexWriter writer, TKey objectToWrite, IndexWriterRuleCollection ruleCollection)
            where TKey : class
        {
            if (writer == null)
                throw new ArgumentNullException("writer", "writer cannot be null");
            if (!writer.IsOpen)
                throw new ObjectDisposedException("writer", "You cannot access this method from a disposed IndexWriter");
            if (objectToWrite == null)
                throw new ArgumentNullException("objectToWrite", "objectToWrite cannot be null");
            if (ruleCollection == null)
                throw new ArgumentNullException("ruleCollection", "storage cannot be null");
            List<PropertyInfo> properties = new List<PropertyInfo>(objectToWrite.GetType().GetProperties());
            properties.RemoveAll(x => !x.CanRead);
            if (properties.Count == 0)
                throw new InvalidOperationException("You cannot index a class that doesn't have any publicly accessible (get) properties");
            int totalProperties = properties.Count;
            IndexDocument document = new IndexDocument();
            int addedProperties = 0;
            for (int i = 0; i < totalProperties; i++) {
                PropertyInfo info = properties[i];
                IndexWriterRule rule = ruleCollection.GetRuleFromType(info.PropertyType);
                object propertyValue = info.GetValue(objectToWrite, null);
                if (rule.SkipColumnIfNull && propertyValue == null)
                    continue;
                else if (!rule.SkipColumnIfNull && string.IsNullOrEmpty(rule.DefaultValueIfNull) && propertyValue == null)
                    continue;

                string stringValue = string.Empty;
                if (propertyValue == null)
                    stringValue = rule.DefaultValueIfNull;
                else
                    stringValue = propertyValue.ToString();
                propertyValue = null;

                FieldStorage storage = (rule.AppliedColumns.ContainsKey(info.Name)) ? rule.AppliedColumns[info.Name] : rule.DefaultStorageRule;
                document.Add(new FieldNormal(info.Name, stringValue, storage.Store, storage.SearchRule, storage.VectorRule));
                ++addedProperties;
            }

            if (addedProperties > 0)
                writer.Write(document);
        }

        public static void WriteInstance<TKey>(this IndexWriter writer, IEnumerable<TKey> values, IndexWriterRuleCollection ruleCollection)
            where TKey : class
        {
            if (writer == null)
                throw new ArgumentNullException("writer", "writer cannot be null");
            if (!writer.IsOpen)
                throw new ObjectDisposedException("writer", "You cannot access this method from a disposed IndexWriter");
            if (values == null)
                throw new ArgumentNullException("values", "values cannot be null");
            foreach (TKey value in values)
                WriteInstance<TKey>(writer, value, ruleCollection);
        }

        public static void WriteInstance<TKey>(this IndexWriter writer, TKey objectToWrite, IEnumerable<string> propertiesToIndex, FieldStorage storage)
            where TKey : class
        {
            if (writer == null)
                throw new ArgumentNullException("writer", "writer cannot be null");
            if (!writer.IsOpen)
                throw new ObjectDisposedException("writer", "You cannot access this method from a disposed IndexWriter");
            if (objectToWrite == null)
                throw new ArgumentNullException("objectToWrite", "objectToWrite cannot be null");
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");
            List<PropertyInfo> properties = new List<PropertyInfo>(objectToWrite.GetType().GetProperties());
            properties.RemoveAll(x => !x.CanRead || !propertiesToIndex.Contains(x.Name));
            if (properties.Count == 0)
                throw new InvalidOperationException("You cannot index a class that doesn't have any publicly accessible (get) properties");
            //for (int i = (totalProperties -1); i >= 0; i--) if (!propertiesToIndex.Contains(properties[i].Name)) properties.RemoveAt(i);

            // now all properties can be indexed
            int totalProperties = properties.Count;
            IndexDocument document = new IndexDocument();
            int addedProperties = 0;
            for (int i = 0; i < totalProperties; i++) {
                PropertyInfo info = properties[i];
                object propertyValue = info.GetValue(objectToWrite, null);
                if (propertyValue == null)
                    continue;
                document.Add(new FieldNormal(info.Name, propertyValue.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
                ++addedProperties;
            }

            if (addedProperties > 0)
                writer.Write(document);
        }

        public static void WriteInstance<TKey>(this IndexWriter writer, IEnumerable<TKey> values, IEnumerable<string> propertiesToIndex, FieldStorage storage)
            where TKey : class
        {
            if (writer == null)
                throw new ArgumentNullException("writer", "writer cannot be null");
            if (!writer.IsOpen)
                throw new ObjectDisposedException("writer", "You cannot access this method from a disposed IndexWriter");
            if (values == null)
                throw new ArgumentNullException("values", "values cannot be null");
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");
            foreach (TKey value in values)
                WriteInstance<TKey>(writer, value, propertiesToIndex, storage);
        }

        private static string GetAccessParameters(MethodBase methodBase)
        {
            if (methodBase == null)
                return NO_VALUE;
            StringBuilder builder = new StringBuilder();
            if (methodBase.IsPrivate)
                builder.Append("Private ");
            else if (methodBase.IsPublic)
                builder.Append("Public ");
            if (methodBase.IsStatic)
                builder.Append("Static ");
            if (methodBase.IsVirtual) {
                if (!methodBase.IsFinal) {
                    builder.Append("Overridable ");
                }
                else {
                    builder.Append("Virtual ");
                }
            }
            if (methodBase.IsAbstract)
                builder.Append("Abstract ");
            if (methodBase.IsGenericMethod)
                builder.Append("<T> ");
            if (builder.Length == 0)
                return NO_VALUE;
            return builder.ToString();
        }

        private static string GetAccessParameters(PropertyInfo info)
        {
            if (info == null)
                return NO_VALUE;
            StringBuilder builder = new StringBuilder();

            if (info.CanRead) {
                builder.Append(GetAccessParameters(info.GetGetMethod()) + " ");
                builder.Append("Get; ");
            }
            if (info.CanWrite) {
                builder.Append(GetAccessParameters(info.GetSetMethod()) + " ");
                builder.Append("Set;");
            }

            if (builder.Length > 0)
                return builder.ToString();
            return NO_VALUE;
        }

        private static string GetAccessParameters(Type classType)
        {
            if (classType == null)
                return NO_VALUE;
            StringBuilder builder = new StringBuilder();
            if (classType.IsPublic)
                builder.Append("Public ");
            else
                builder.Append("Private ");

            if (classType.IsAbstract)
                builder.Append("Abstract ");
            if (classType.IsGenericType)
                builder.Append("<T> ");
            if (builder.Length == 0)
                return NO_VALUE;
            return builder.ToString();
        }

        private static string GetAccessParameters(FieldInfo info)
        {
            if (info == null)
                return NO_VALUE;
            StringBuilder builder = new StringBuilder();
            if (info.IsPrivate)
                builder.Append("Private ");
            else if (info.IsPublic)
                builder.Append("Public ");
            if (info.IsStatic)
                builder.Append("Static ");
            if (builder.Length == 0)
                return NO_VALUE;
            return builder.ToString();
        }

        private static string GetParameterString(MethodBase methodBase)
        {
            if (methodBase == null)
                return NO_VALUE;
            ParameterInfo[] parameters = methodBase.GetParameters();
            StringBuilder builder = new StringBuilder();
            if (parameters != null) {
                int totalParameters = parameters.Length;
                for (int i = 0; i < totalParameters; i++) {
                    builder.Append(parameters[i].ParameterType.Name + " ");
                    builder.Append(parameters[i].Name + " , ");
                }
            }
            if (builder.Length == 0)
                return NO_VALUE;
            return builder.ToString();
        }

        #endregion Methods
    }
}