using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace NPlugins.Sqlite
{
    public partial class Repository<T> where T : Storable
    {
        /// <summary>
        ///     Inserts a new instance of a class to the proper table in Data Base.
        /// </summary>
        /// <param name="data">The instance of class to be stored.</param>
        public void Insert(T data)
        {
            InsertData.Instance.Act(data);
        }

        /// <summary>
        ///     Inserts a list of new instances of a class to the proper table in Data Base.
        /// </summary>
        /// <param name="data">The list of class to be stored.</param>
        public void Insert(List<T> data)
        {
            foreach (T entry in data)
                InsertData.Instance.Act(entry);
        }

        /// <summary>
        ///     Insert data into Repository table in a custom way.
        /// </summary>
        /// <param name="query">Queries to be run to insert.</param>
        public void InsertCustom(params string[] query)
        {
            foreach (string q in query)
                RunQuery(q, null);
        }

        /// <summary>
        ///     Updates entry of existing insertion based on Id.
        /// </summary>
        /// <param name="data">The instance of class to be updated.</param>
        public void Update(T data)
        {
            InsertData.Instance.Act(data);
        }

        /// <summary>
        ///     Updates entry of existing inserton based on custom parameter.
        /// </summary>
        /// <param name="data">The instance of class to be updated.</param>
        /// <param name="param">Parameter to be used as update criteria.</param>
        /// <param name="value">Value to be matched with parameter.</param>
        public void Update(T data, string param, object value)
        {
            InsertData.Instance.Update(data, false, param, value);
        }

        private static int GetCurrentTableIndex(Type propertyType)
        {
            string query = String.Format("Select * from sqlite_sequence where name = '{0}'", GetTableName(propertyType));

            int id = 0;
            RunQuery(query, reader =>
            {
                string idFormat = reader[1].ToString();
                idFormat = idFormat.Trim();

                if (idFormat.Length > 0)
                    id = int.Parse(reader[1].ToString());
            });

            return id;
        }

        private class InsertData
        {
            private object objectToInsert;

            public static InsertData Instance
            {
                get { return instance ?? (instance = new InsertData()); }
            }

            private static InsertData instance;

            private InsertData()
            {

            }

            public void Act<TU>(TU data)
            {
                this.objectToInsert = data;

                PropertyInfo property = typeof (TU).GetProperty("Id");

                int id = 0;
                if (property != null)
                    id = (int) property.GetValue(data, null);

                if (id == 0)
                    Insert(data);
                else
                    Update(data, false, null, null);
            }

            public void Insert<TU>(TU data)
            {
                string query = "INSERT INTO " + GetTableName(typeof (TU)) + " (";

                List<string> columns = GetColumnsNames<TU>();
                List<PropertyDescriptor> properties = GetProperties(typeof (TU));

                columns.Sort();
                properties = properties.OrderBy(_ => _.Name).ToList();

                foreach (string column in columns)
                {
                    if (column == "Id")
                        continue;

                    query += column + ", ";
                }

                query = query.Remove(query.Length - 2) + ")" + " VALUES (";
                foreach (PropertyDescriptor property in properties)
                {
                    if (property.Name == "Id")
                        continue;

                    string propertyQuery = ConvertObjectValueToProperStringFormat(property,
                        typeof (TU).GetProperty(property.Name).GetValue(data, null), typeof (TU));

                    if (propertyQuery == null)
                        continue;

                    query += propertyQuery + ", ";
                }

                query = query.Remove(query.Length - 2) + ");";

                RunQuery(query, null);
            }

            public void Update<TU>(TU data, bool isNested, string parameter, object value)
            {
                string query = "update " + GetTableName(typeof (TU)) + " set ";

                List<string> columns = GetColumnsNames<TU>();
                List<PropertyDescriptor> properties = GetProperties(typeof (TU));
                                
                columns.Sort();
                properties = properties.OrderBy(_ => _.Name).ToList();

                for (int i = 0; i < columns.Count; i++)
                {
                    if (columns[i] == "Id")
                        continue;

                    string propertyQuery = ConvertObjectValueToProperStringFormat(properties[i],
                        data.GetType().GetProperty(properties[i].Name).GetValue(data, null), typeof (TU));

                    if (propertyQuery == null)
                        continue;

                    query += columns[i] + "=" + propertyQuery + ", ";
                }
                query = query.Remove(query.Length - 2);
                Debug.Log(1 + " " + isNested);
                
                int id = (int) typeof (TU).GetProperty("Id").GetValue(data, null);// isNested
                    //? (int) typeof (T).GetProperty(typeof(TU).Name + "Id").GetValue(this.objectToInsert as T, null)
                    //: (int) typeof (TU).GetProperty("Id").GetValue(data, null);
                
                Debug.Log(2 + " " + isNested);
                
                query += !string.IsNullOrEmpty(parameter)
                    ? " where " + parameter + "=" + value + ";"
                    : " where Id=" + id + ";";

                Debug.Log(query);

                RunQuery(query, null);
            }

            public string ConvertObjectValueToProperStringFormat(PropertyDescriptor propertyDescriptor, object value,
                Type tableType)
            {
                Type propertyType = propertyDescriptor.PropertyType;

                if (propertyType.IsEnum ||
                    propertyType == typeof (string) ||
                    propertyType == typeof (bool)
                    )
                    return "'" + value + "'";

                if (propertyType == typeof (DateTime))
                    return "'" + ((DateTime) value).ToString("yyyy-MM-dd HH:mm:ss") + "'";

                if (IsUnityEngineObject(propertyType))
                    return ConvertUnityEngineObjectToProperStringFormat(propertyType, value);

                if (propertyType == typeof (float) ||
                    propertyType == typeof (double) ||
                    propertyType == typeof (int) ||
                    propertyType == typeof (float?) ||
                    propertyType == typeof (double?) ||
                    propertyType == typeof (int?) ||
                    propertyType == typeof (byte)
                    )
                    return value.ToString();

                if (PropertyIsList(propertyDescriptor))
                    return ConvertCollectionToProperTable(propertyType, value, propertyDescriptor.Name, tableType);

                if (propertyType.IsClass && (value as Storable) != null)
                    return ConvertCustomClassObjectToProperStringFormat(propertyType, value);

                if (propertyType.IsClass && (value as Storable) == null)
                    return null;

                throw new DataMisalignedException();
            }

            private string ConvertCollectionToProperTable(Type propertyType, object value, string propertyName,
                Type tableType)
            {
                Type genericType = propertyType.GetGenericArguments()[0];

                if (genericType == null || value == null || IsCollectionEmpty(value))
                    return null;

                Type customClass = CreateCustomClass(genericType, propertyName, tableType);

                MethodInfo insertNewInstanceMethod = MakeGenericMethod(typeof (InsertData), customClass, "Insert");

                int tableTypeId = GetCurrentTableIndex(tableType) + 1;

                foreach (var val in (value.GetType().IsArray ? (object[]) value : (IList) value))
                {
                    var customClassInstance = Activator.CreateInstance(customClass);

                    if (val.GetType().IsClass && val.GetType() != typeof (string))
                    {
                        List<PropertyDescriptor> customClassProperties = GetProperties(customClass);
                        List<PropertyDescriptor> propertyTypeProperties = GetProperties(genericType);

                        IEnumerable<string> commonProperties = customClassProperties.Select(_ => _.Name).
                            Intersect(propertyTypeProperties.Select(_ => _.Name));

                        foreach (string property in commonProperties)
                        {
                            var propertyValue = genericType.GetProperty(property).GetValue(val, null);

                            customClass.GetProperty(property).
                                SetValue(customClassInstance, propertyValue, null);
                        }
                    }
                    else
                        customClass.GetProperty(propertyName).
                            SetValue(customClassInstance, val, null);

                    customClass.GetProperty(tableType.Name + "Index").
                        SetValue(customClassInstance, tableTypeId, null);

                    insertNewInstanceMethod.Invoke(this, new[] {customClassInstance});
                }

                return null;
            }

            private string ConvertCustomClassObjectToProperStringFormat(Type propertyType, object value)
            {
                int id = (int) propertyType.GetProperty("Id").GetValue(value, null);

                if (propertyType == typeof (T))
                    return id.ToString(CultureInfo.InvariantCulture);

                string methodName = ((T)this.objectToInsert).Id == 0 ? "Insert" : "Update";
                object[] parameters = methodName == "Insert" ? new[] { value } : new[] { value, true, null, null };

                MakeGenericMethod(typeof(InsertData), propertyType, methodName).Invoke(this, parameters);

                id = GetCurrentTableIndex(propertyType);

                return id.ToString(CultureInfo.InvariantCulture);
            }

            private string ConvertUnityEngineObjectToProperStringFormat(Type propertyType, object value)
            {
                List<string> properties = new List<string>();

                if (propertyType == typeof (Color))
                    properties = new List<string> {"r", "g", "b", "a"};

                else if (propertyType == typeof (Rect))
                    properties = new List<string> {"x", "y", "width", "height"};

                else if (propertyType == typeof (Vector2))
                    properties = new List<string> {"x", "y"};

                else if (propertyType == typeof (Vector3))
                    properties = new List<string> {"x", "y", "z"};

                else if (propertyType == typeof (Vector4) || propertyType == typeof (Quaternion))
                    properties = new List<string> {"x", "y", "z", "w"};

                string entry = "'";
                foreach (string property in properties)
                    entry += property + ":" + propertyType.GetField(property).GetValue(value) + "/";

                return entry.Remove(entry.Length - 1) + "'";
            }
        }
    }
}