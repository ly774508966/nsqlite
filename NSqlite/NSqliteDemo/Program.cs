using System;
using System.Collections.Generic;
using NPlugins.Sqlite;
using UnityEngine;

namespace NSqliteDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseClass = new BaseClass
            {
                Name = "Nicolas",
                Strings = new List<string> { "String1", "String2", "String3", "String4" },
                Models = new List<OtherModel>
                {
                    new OtherModel
                    {
                        CanWrite = false,
                        Date = DateTime.Now,
                        Position = new Vector3(100, 10, 1)
                    },
                    new OtherModel
                    {
                        CanWrite = true,
                        Date = DateTime.UtcNow,
                        Position = new Vector3(1, 10, 100)
                    }
                }
            };

            Repository<BaseClass> repository = new Repository<BaseClass>("URI=file:SqliteTest.db");

            //repository.Insert(baseClass);

            repository.FindWhere(_ => _.Name == "Nicolas");
            
            Console.ReadKey();
        }

        public class BaseClass : Storable
        {
            public string Name { get; set; }

            public List<OtherModel> Models { get; set; } 

            public List<string> Strings { get; set; }
        }
    }
}
