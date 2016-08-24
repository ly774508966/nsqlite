using System;
using System.Collections.Generic;

namespace NPlugins.Sqlite
{
    public partial class Repository<T> where T : Storable
	{
        /// <summary>
        ///     Delete given instance of class.
        /// </summary>
        /// <param name="data">Instance of class.</param>
        public void Delete(T data)
        {
            DeleteData.Instance.Delete(data);
        }

        /// <summary>
        ///     Delete table's entry for given Id.
        /// </summary>
        /// <param name="id">Id of entry to be deleted.</param>
        public void Delete(int id)
        {
            DeleteData.Instance.Delete(id);
        }

        /// <summary>
        ///     Delete every entry from table.
        /// </summary>
	    public void DeleteAll()
	    {
	        DeleteData.Instance.DeleteAll();
	    }

        /// <summary>
        ///     Delete entries that fit searching criteria.
        /// </summary>
        public void DeleteWhere(Func<T, bool> selector)
	    {
            DeleteData.Instance.DeleteWhere(selector);
	    }

		private class DeleteData
		{
			public static DeleteData Instance { get { return instance ?? (instance = new DeleteData()); } }
			private static DeleteData instance;
			
			private DeleteData()
			{
				
			}

			public void Delete (T data)
			{
                Delete(data.Id);
                /*
                PropertyInfo idProperty = typeof(T).GetProperty("Id");

                string propertyName = "Id";
                string propertyValue;
                if (idProperty == null)
                {
                    PropertyDescriptor otherProperty = GetProperties(typeof(T)).FirstOrDefault();

                    propertyName = (otherProperty.PropertyType.IsClass && otherProperty.PropertyType != typeof(string)) ?
                                        otherProperty.Name + "Id" : otherProperty.Name;
                    propertyValue = InsertData.Instance.ConvertObjectValueToProperStringFormat(otherProperty, typeof(T).GetProperty(propertyName).GetValue(data, null), typeof(T));
                }
                else
                    propertyValue = idProperty.GetValue(data, null).ToString();

				string query = "delete from " + typeof(T).Name + " WHERE " + propertyName + "=" + propertyValue + ";";
				
				RunQuery(query, null);
                */
			}

			public void Delete (int id)
			{
				if (id == 0)
					return;

                string query = "delete from " + GetTableName(typeof(T)) + " WHERE Id=" + id + ";";
				
				RunQuery(query, null);
			}

		    public void DeleteAll()
		    {
                string query = "delete from " + GetTableName(typeof(T)) + ";";

                RunQuery(query, null);
		    }

            public void DeleteWhere(Func<T, bool> selector)
            {
                List<T> entries = FindData.Instance.FindWhere(selector);

                foreach (T entry in entries)
                    Delete(entry.Id);
            }
        }
	}
}