using System;
using System.Collections.Generic;

namespace NPlugins.Sqlite
{
    public class DbAccess
    {
        public static DbAccess Instance { get { return instance ?? (instance = new DbAccess()); } }
        private static DbAccess instance;

        private string connection;

        private DbAccess()
        {
            this.connection = "URI=file:SqliteTest.db";
        }

        public void SetDatabaseConnection(string connection)
        {
            this.connection = connection;
        }

        #region Insert

        /// <summary>
        ///     Inserts a new instance of a class to the proper table in Data Base.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <param name="data">The instance of class to be stored.</param>
        public void Insert<T>(T data) where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                rep.Insert(data);
            }
        }

        /// <summary>
        ///     Inserts a list of new instances of a class to the proper table in Data Base.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <param name="data">The list of class to be stored.</param>
        public void Insert<T>(List<T> data) where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                rep.Insert(data);
            }
        }

        /// <summary>
        ///     Updates entry of existing insertion.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <param name="data">The instance of class to be updated.</param>
        public void Update<T>(T data) where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                rep.Update(data);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        ///     Delete given instance of class.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <param name="data">Instance of class.</param>
        public void Delete<T>(T data) where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                rep.Delete(data);
            }
        }

        /// <summary>
        ///     Delete table's entry for given Id.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <param name="id">Id of entry to be deleted.</param>
        public void Delete<T>(int id) where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                rep.Delete(id);
            }
        }

        /// <summary>
        ///     Delete every entry from table.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        public void DeleteAll<T>() where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                rep.DeleteAll();
            }
        }

        /// <summary>
        ///     Delete entries that fit searching criteria.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <param name="selector">Searching criteria.</param>
        public void DeleteWhere<T>(Func<T, bool> selector) where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                rep.DeleteWhere(selector);
            }
        }

        #endregion

        #region Find

        /// <summary>
        ///     Find object by it's Id.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <param name="id">Object's Id</param>
        /// <returns>An instance of the object with the given Id.</returns>
        public T Find<T>(int id) where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                return rep.Find(id);
            }
        }

        /// <summary>
        ///     Finds all objects recorded in table.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <returns>List with every single object recorded in the type's table.</returns>
        public List<T> FindAll<T>() where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                return rep.FindAll();
            }
        }

        /// <summary>
        ///    Finds every object within a specific condition. 
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <param name="expression">Lambda expression for searching required condition.</param>
        /// <returns>List of every entry within specified condition.</returns>
        public List<T> FindWhere<T>(Func<T, bool> expression) where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                return rep.FindWhere(expression);
            }
        }

        /// <summary>
        ///     Finds every object which provided property name has same value as provided expected value.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <param name="propertyName">The name of the property to be compared to.</param>
        /// <param name="expectedValue">The value that the property is expected to store</param>
        /// <returns>List of every entry within specified condition.</returns>
        public List<T> FindWhere<T>(string propertyName, object expectedValue) where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                return rep.FindWhere(propertyName, expectedValue);
            }
        }

        /// <summary>
        ///     Finds the first n entries in a table, given a property to sort the table by.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <param name="propertyName">Property to sort table by (Id, Score, ...)</param>
        /// <param name="amountOfEntries">The number of list members.</param>
        /// <returns>List with N entries sorted descendingly by specified property</returns>
        /// <remarks>
        ///     It's important to note that:
        ///     a) The propertyName parameter must match a model property name, including in case.
        ///     b) This can be used to sort for high score, for instance.
        /// </remarks>
        public List<T> FindDescending<T>(string propertyName, int amountOfEntries) where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                return rep.FindDescending(propertyName, amountOfEntries);
            }
        }

        /// <summary>
        ///     Finds the last n entries in a table, given a property to sort the table by.
        /// </summary>
        /// <typeparam name="T">The type that the repository will store.</typeparam>
        /// <param name="propertyName">Property to sort table by (Id, Score, ...)</param>
        /// <param name="amountOfEntries">The number of list members.</param>
        /// <returns>List with N entries sorted ascendingly by specified property</returns>
        /// <remarks>
        ///     It's important to note that:
        ///     a) The propertyName parameter must match a model's property name, including the case.
        ///     b) This can be used to sort for lower timings, for instance.
        /// </remarks>        
        public List<T> FindAscending<T>(string propertyName, int amountOfEntries) where T : Storable
        {
            using (Repository<T> rep = new Repository<T>(this.connection))
            {
                return rep.FindAscending(propertyName, amountOfEntries);
            }
        }

        #endregion
    }
}