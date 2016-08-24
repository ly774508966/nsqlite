using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Linq;
using UnityEngine;

namespace NPlugins.Sqlite
{
    public partial class Repository<T> where T : Storable
	{
        private class SetupTable
		{
			public static SetupTable Instance { get { return instance ?? (instance = new SetupTable()); } }
			private static SetupTable instance;

			private SetupTable()
			{
			}

			public void SetTable ()
			{
				CreateTable(typeof(T));
			}

	        private void CreateTable(Type type)
			{
                string tableName = GetTableName (type);
				
				string query = "create table if not exists " + tableName + " (Id INTEGER PRIMARY KEY AUTOINCREMENT";

                List<PropertyDescriptor> properties = GetProperties(type);

				List<string> foreignKeys = new List<string>();

				foreach(var property in properties)
				{
					string propName = property.Name;
					
					if(propName == "Id")
						continue;

					string propertyQuery;

					try 
					{
                        propertyQuery = HandleValue(property, propName, type);
					}
					catch(Exception)
					{
						continue;
					}

                    if(propertyQuery == null)
                        continue;

					string[] queries = propertyQuery.Split(',');

					query += ",\n" + queries[0];

					if(queries.Length > 1)
						foreignKeys.Add(queries[1]);
				}

	            query = foreignKeys.Aggregate(query, (current, foreignKey) => current + (",\n" + foreignKey));

	            query += "\n);";

                RunQuery(query, null);
			}

            private string HandleValue(PropertyDescriptor propertyDesciptor, string propertyName, Type tableType)
			{
                Type propertyType = propertyDesciptor.PropertyType;

				if (propertyType == typeof(DateTime) ||
				    propertyType.IsEnum ||
				    propertyType == typeof(string) ||
				    IsUnityEngineObject(propertyType) 
				    )
					return propertyName + " TEXT";

                if (propertyType == typeof(float) || propertyType == typeof(float?))
					return propertyName + " Double";

                if (propertyType == typeof(double) || propertyType == typeof(double?))
					return propertyName + " Float";

                if (propertyType == typeof(int) || propertyType == typeof(int?))
					return propertyName + " INTEGER";

				if (propertyType == typeof(bool))
					return propertyName + " TEXT";
				
				if (PropertyIsList(propertyDesciptor))
                    return HandleCollection(propertyType, propertyName, tableType);

				if (propertyType.IsClass)
					return HandleCustomClass(propertyType, propertyName);

				throw new DataMisalignedException();
			}
			
			private string HandleCustomClass(Type propertyType, string propertyName)
			{
				if(propertyType != typeof(T))
				{
					CreateTable(propertyType);
				}

                return propertyName + "Id INTEGER" + ", FOREIGN KEY (" + propertyName + "Id) REFERENCES " + propertyType.Name + "(Id)";
			}

            private string HandleCollection(Type propertyType, string propertyName, Type tableType)
            {
                Type type = propertyType.IsArray ? propertyType.GetElementType() : propertyType.GetGenericArguments()[0];

                if (type == null)
                    return null;

                Type customClass = CreateCustomClass(type, propertyName, tableType);

                CreateTable(customClass);

                return null;
            }
		}
	}
}