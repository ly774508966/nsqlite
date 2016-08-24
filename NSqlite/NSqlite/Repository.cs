using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace NPlugins.Sqlite
{
    /// <summary>
    ///     Class responsible for handling the database, storing and retrieving data in the 
    ///     template provided's format.
    /// </summary>
    /// <typeparam name="T">The type that the repository will store.</typeparam>
    public partial class Repository<T> : IDisposable where T : Storable 
    {
		private static string DbPath;

        private static Dictionary<Type, List<PropertyDescriptor>> Properties;

        private static Dictionary<Type, List<string>> Columns; 

	    public Repository(string path)
		{
            DbPath = path;

            Properties = new Dictionary<Type, List<PropertyDescriptor>>();

            Columns = new Dictionary<Type, List<string>>();

			SetupTable.Instance.SetTable();
		}

        private static bool PropertyIsList(PropertyDescriptor property)
        {
            try
            {
                var instance = Activator.CreateInstance(property.PropertyType, null);

                return instance is ICollection;
            }
            catch (Exception)
            {
                return property.PropertyType.IsArray;

                return false;
            }
        }

        private static bool IsCollectionEmpty(object collection)
        {
            if (collection.GetType().IsArray)
                return ((object[])collection).Length == 0;

            if (collection.GetType() is ICollection)
                return ((IList)collection).Count == 0;

            return true;
        }

        private static void RunQuery(string query, Action<SqliteDataReader> read)
		{
            using (SqliteConnection connection = new SqliteConnection(DbPath))
            {
                connection.Open();
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (read != null)
                            read(reader);
                    }
                }
            }
		}

		private static List<PropertyDescriptor> GetProperties(Type type)
		{
            if (Properties.ContainsKey(type))
                return Properties[type];

            var info = TypeDescriptor.GetProperties(type);

		    var p = info.Cast<PropertyDescriptor>().ToList();

            Properties.Add(type, p);

            return p;
		}
		
		private static List<string> GetColumnsNames<TU>()
		{
            if (Columns.ContainsKey(typeof(TU)))
                return Columns[typeof(TU)];

			string query = "Select * from " + GetTableName(typeof(TU));
			
			List<string> columns = new List<string>();
			RunQuery(query, reader => 
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					columns.Add(reader.GetName(i));
				}
			});

            Columns.Add(typeof(TU), columns);

			return columns;
		}

        private static string GetTableName(Type type)
        {
            string name = type.Name;

            if (type.IsGenericType)
            {
                name = name.Remove(name.IndexOf('`'));

                name = type.GetGenericArguments().Aggregate(name, (current, t) => current + GetTableName(t));
            }

            return name;
        }

        private static MethodInfo MakeGenericMethod(Type classWithMethod, Type genericType, string methodName)
        {
            return classWithMethod.GetMethod(methodName).MakeGenericMethod(new[] { genericType });
        }

        private static Type CreateCustomClass(Type propertyType, string propertyName, Type tableType)
        {
            Dictionary<string, Type> propertiesForCustomClass = new Dictionary<string, Type>();

            if (propertyType.IsClass && propertyType != typeof(string))
            {
                foreach (var typeProperty in GetProperties(propertyType))
                    propertiesForCustomClass.Add(typeProperty.Name, typeProperty.PropertyType);
            }
            else
                propertiesForCustomClass.Add(propertyName, propertyType);

            propertiesForCustomClass.Add(tableType.Name + "Index", typeof(int));

            return ClassBuilder.MakeCustomClass(tableType.Name + propertyName, propertiesForCustomClass);
        }

        private static bool IsUnityEngineObject(Type propertyType)
        {
            if (propertyType == typeof (Color) ||
                propertyType == typeof (Rect) ||
                propertyType == typeof (Vector2) ||
                propertyType == typeof (Vector3) ||
                propertyType == typeof (Vector4) ||
                propertyType == typeof (Quaternion)
                )
                return true;

            return false;
        }

        public void Dispose()
        {
            
        }
    }
}