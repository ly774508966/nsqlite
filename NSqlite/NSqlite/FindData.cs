using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using Mono.Data.Sqlite;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace NPlugins.Sqlite
{
    public partial class Repository<T> where T : Storable
	{
        /// <summary>
        ///     Find object by it's Id.
        /// </summary>
        /// <param name="id">Object's Id</param>
        /// <returns>An instance of the object with the given Id.</returns>
        public T Find(int id)
        {
            return FindData.Instance.Find<T>(id);
        }

        /// <summary>
        ///     Finds all objects recorded in table.
        /// </summary>
        /// <returns>List with every single object recorded in the type's table.</returns>
        public List<T> FindAll()
        {
            return FindData.Instance.FindAll<T>();
        }

        /// <summary>
        ///    Finds every object within a specific condition. 
        /// </summary>
        /// <param name="expression">Lambda expression for searching required condition.</param>
        /// <returns>List of every entry within specified condition.</returns>
	    public List<T> FindWhere(Func<T, bool> expression)
	    {
            return FindData.Instance.FindWhere(expression);
        }

        public List<T> FindWhere(string propertyName, object value)
        {
            return FindData.Instance.FindWhereProperty<T>(propertyName, value);
        }

        /// <summary>
        ///     Finds the first n entries in a table, given a property to sort the table by.
        /// </summary>
        /// <param name="propertyName">Property to sort table by (Id, Score, ...)</param>
        /// <param name="amountOfEntries">The number of list members.</param>
        /// <returns>List with N entries sorted descendingly by specified property</returns>
        /// <remarks>
        ///     It's important to note that:
        ///     a) The propertyName parameter must match a model property name, including in case.
        ///     b) This can be used to sort for high score, for instance.
        /// </remarks>
	    public List<T> FindDescending(string propertyName, int amountOfEntries)
	    {
            return FindData.Instance.FindInOrder<T>(propertyName, amountOfEntries, "desc");
	    }

        /// <summary>
        ///     Finds the last n entries in a table, given a property to sort the table by.
        /// </summary>
        /// <param name="propertyName">Property to sort table by (Id, Score, ...)</param>
        /// <param name="amountOfEntries">The number of list members.</param>
        /// <returns>List with N entries sorted ascendingly by specified property</returns>
        /// <remarks>
        ///     It's important to note that:
        ///     a) The propertyName parameter must match a model property name, including in case.
        ///     b) This can be used to sort for high score, for instance.
        /// </remarks>
	    public List<T> FindAscending(string propertyName, int amountOfEntries)
	    {
            return FindData.Instance.FindInOrder<T>(propertyName, amountOfEntries, "asc");
	    }

        /// <summary>
        ///     Finds entries matching developer written query criteria. 
        /// </summary>
        /// <param name="query">Query defined by developer.</param>
        /// <returns>List with all entries matching criteria</returns>
        public List<T> FindCustom(string query)
        {
            return FindData.Instance.FindCustom<T>(query);
        }

	    private class FindData
		{
            public static FindData Instance { get { return instance ?? (instance = new FindData()); } }
			private static FindData instance;
			
			private FindData() { }

	        private TU ReadDataFromReader<TU>(SqliteDataReader reader)
	        {
                TU item = (TU)Activator.CreateInstance(typeof(TU), null);

                var properties = GetProperties(typeof(TU));

	            for (int i = 0; i < reader.FieldCount; i++)
	            {
	                string columnName = reader.GetName(i);
	                columnName = columnName.EndsWith("Id") && columnName.Length > 2
	                    ? columnName.Remove(reader.GetName(i).Length - 2)
	                    : columnName;

	                var property = properties.Find(_ => columnName.Contains(_.Name));

	                if (property != null)
	                    property.SetValue(item, ConvertToProperObjectFormat(property, reader[i]));
	            }

	            int index = int.Parse(reader[0].ToString()); 
	            properties.Where(PropertyIsList).ToList().ForEach(property =>
                    property.SetValue(item, ConvertArrayObjectToProperObjectFormat(property, index, typeof(TU))));
                    
                return item;
	        }

			public TU Find<TU>(int id)
			{
				if(id == 0)
					return default(TU);

                string query = "select * from " + GetTableName(typeof(TU)) + " WHERE Id = " + id + ";";
                
                TU item = (TU)Activator.CreateInstance(typeof(TU), null);

                RunQuery(query, reader =>
				{
					while(reader.Read())
					{
                        item = ReadDataFromReader<TU>(reader);
					}
				});

				return item;
			}

			public List<TU> FindAll<TU>()
			{
                string query = "select * from " + GetTableName(typeof(TU)) + ";";

                return FindList<TU>(query);
			}

	        public List<TU> FindWhereProperty<TU>(string propertyName, object value)
            {
                Type propertyType = value.GetType();
                if (propertyType.IsClass && propertyType != typeof (string))
                {
                    value = value.GetType().GetProperty("Id").GetValue(value, null);
                    propertyName += "Id";
                }
                else
                {
                    List<PropertyDescriptor> templateProperties = GetProperties(typeof(TU));

                    PropertyDescriptor propertyDescriptor = templateProperties.Find(_ => _.Name == propertyName);

                    value = InsertData.Instance.ConvertObjectValueToProperStringFormat(propertyDescriptor, value, typeof(TU));
                }

                string query = "select * from " + GetTableName(typeof(TU)) + " where " + propertyName + "=" + value + ";";

                return FindList<TU>(query);
            }

            public List<TU> FindCustom<TU>(string query)
            {
                return FindList<TU>(query);
            }

            public List<TU> FindWhere<TU>(Func<TU, bool> expression)
	        {
                if (expression == null)
                    return null;

                /*
                dynamic operation = expression.Body;
                dynamic left = operation.Left;
                dynamic right = operation.Right;

                Dictionary<ExpressionType, String> ops = new Dictionary<ExpressionType, String>
                {
                    {ExpressionType.Equal, "="},
                    {ExpressionType.GreaterThan, ">"},
                    {ExpressionType.LessThan, "<"},
                    {ExpressionType.NotEqual, "!="}
                };
                // add all required operations here            

                // Instead of SELECT *, select all required fields, since you know the type
                var q = String.Format("SELECT * FROM {0} WHERE {1} {2} {3}", GetTableName(typeof(TU)), left.Member.Name, ops[operation.NodeType], right.Value);
                */
                List<TU> entries = FindAll<TU>();

                return entries.Where(expression).ToList();
	        }

            public List<TU> FindInOrder<TU>(string propertyName, int amountOfEntries, string order)
	        {
                string query = "select * from " + GetTableName(typeof(TU)) + " order by " + propertyName + " " + order + " limit " + amountOfEntries;

                return FindList<TU>(query);
	        }

	        private List<TU> FindList<TU>(string query)
	        {
                List<TU> list = new List<TU>();

                RunQuery(query, reader =>
                {
                    while (reader.Read())
                    {
                        TU item = ReadDataFromReader<TU>(reader);

                        list.Add(item);
                    }
                });

                return list;
	        }

			private object ConvertToProperObjectFormat(PropertyDescriptor property, object value)
			{
                Type propertyType = property.PropertyType;

				if (propertyType == typeof(string))
					return value.ToString();

				if (propertyType == typeof(DateTime))
					return DateTime.Parse(value.ToString());

                if (propertyType == typeof(float) || propertyType == typeof(float?))
					return float.Parse(value.ToString());
				
				if (propertyType == typeof(double) || propertyType == typeof(float?))
					return double.Parse(value.ToString());

                if (propertyType == typeof(int) || propertyType == typeof(int?) || propertyType == typeof(byte))
					return int.Parse(value.ToString());

			    if (propertyType.IsEnum)
                    return Enum.Parse(propertyType, value.ToString());

			    if (propertyType == typeof (bool))
			        return bool.Parse(value.ToString());

			    if (IsUnityEngineObject(propertyType))
					return ConvertUnityEngineObjectToProperObjectFormat(propertyType, value);

				if (propertyType.IsClass)
					return ConvertCustomClassToProperObjectFormat(propertyType, value);
				
				return value;
			}

	        private object ConvertArrayObjectToProperObjectFormat(PropertyDescriptor property, int index, Type tableType)
	        {
	            Type genericPropertyArgumentType = property.PropertyType.GetGenericArguments()[0];

	            if (genericPropertyArgumentType == null || index == 0)
	                return null;

	            string propertyName = property.Name;

	            Type customClassType = CreateCustomClass(genericPropertyArgumentType, propertyName, tableType);

	            MethodInfo findCustomClassMethod = MakeGenericMethod(typeof (FindData), customClassType,
	                "FindWhereProperty");

	            string queryPropertyName = tableType.Name + "Index";

	            var listOfCustomClass = findCustomClassMethod.Invoke(this, new object[] {queryPropertyName, index});

	            var constructedListType = typeof (List<>).MakeGenericType(genericPropertyArgumentType);

	            IList listInstance = (IList) Activator.CreateInstance(constructedListType);

	            foreach (var customClassInstance in (IList) listOfCustomClass)
	            {
	                object value = null;

	                if (genericPropertyArgumentType.IsClass && genericPropertyArgumentType != typeof (string))
	                {
	                    value = Activator.CreateInstance(genericPropertyArgumentType);

	                    List<PropertyDescriptor> customClassProperties = GetProperties(customClassType);
	                    List<PropertyDescriptor> propertyTypeProperties = GetProperties(genericPropertyArgumentType);

	                    IEnumerable<string> commonProperties = customClassProperties.Select(_ => _.Name).
	                        Intersect(propertyTypeProperties.Select(_ => _.Name));

	                    foreach (string customClassProperty in commonProperties)
	                    {
	                        var propertyValue =
	                            customClassType.GetProperty(customClassProperty).GetValue(customClassInstance, null);

	                        genericPropertyArgumentType.GetProperty(customClassProperty).
	                            SetValue(value, propertyValue, null);
	                    }
	                }
	                else
	                {
	                    PropertyDescriptor prop =
	                        GetProperties(customClassType)
	                            .FirstOrDefault(_ => !_.Name.Contains("Id") && !_.Name.Contains("Index"));

	                    if (prop != null)
	                        value = customClassType.GetProperty(prop.Name).GetValue(customClassInstance, null);
	                }
	                listInstance.Add(value);
	            }

	            if (property.PropertyType.IsArray)
	            {
	                object[] array = new object[listInstance.Count];

	                for (int i = 0; i < listInstance.Count; i++)
	                {
	                    array[i] = listInstance[i];
	                }

	                return array;
	            }

	            return listInstance;
	        }

	        private object ConvertCustomClassToProperObjectFormat (Type propertyType, object value)
			{
                return MakeGenericMethod(typeof(FindData), propertyType, "Find")
                            .Invoke(this, new object[] { int.Parse(value.ToString()) });
			}

			private object ConvertUnityEngineObjectToProperObjectFormat(Type propertyType, object value)
			{
				var dic = ParseUnityEngineValueAsDictionary((string)value);

				if (propertyType == typeof(Color))
                    return new Color(dic["r"], dic["g"], dic["b"], dic["a"]);
				
				if (propertyType == typeof(Rect))
                    return new Rect(dic["x"], dic["y"], dic["width"], dic["height"]);
				
				if (propertyType == typeof(Vector3))
                    return new Vector3(dic["x"], dic["y"], dic["z"]);
				
				if (propertyType == typeof(Vector2))
                    return new Vector2(dic["x"], dic["y"]);
				
				if (propertyType == typeof(Vector4))
				    return new Vector4(dic["x"], dic["y"], dic["z"], dic["w"]);
				
				if (propertyType == typeof(Quaternion))
                    return new Quaternion(dic["x"], dic["y"], dic["z"], dic["w"]);

				return value;
			}

			private Dictionary<string, float> ParseUnityEngineValueAsDictionary(string value)
			{
                string[] values = value.Split('/');

				Dictionary<string, float> dict = new Dictionary<string, float>();
				foreach(var pair in values)
				{
					string[] keyVal = pair.Split(':');

                    dict.Add(keyVal[0], Single.Parse(keyVal[1]));
				}

				return dict;
			}
        }
	}
}