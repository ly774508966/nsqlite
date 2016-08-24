using System;
using System.Linq;
using System.Reflection;
using NPlugins.Sqlite;
using UnityEditor;
using UnityEngine;

namespace NSqliteEditor
{
    public class NSqliteEditor : EditorWindow
    {
        private string assemblyName = "Assembly-CSharp";
        private string dbName = "MyDatabase";

        [MenuItem("NSqlite/GenerateDatabase")]
        public static void GenerateDatabase()
        {
            GetWindow(typeof(NSqliteEditor));
        }

        public void OnGUI()
        {
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            this.assemblyName = EditorGUILayout.TextField("Assembly Name", this.assemblyName);

            this.dbName = EditorGUILayout.TextField("Database Name", this.dbName);

            if (GUILayout.Button("Generate", EditorStyles.miniButtonRight))
            {
                Generate();
            }
        }

        private void Generate()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains(this.assemblyName));

            foreach (Assembly a in assembly)
            {
                foreach (var type in a.GetTypes())
                {
                    if (type.IsSubclassOf(typeof (Storable)))
                    {
                        var instance = Activator.CreateInstance(type);

                        string dbPath = DataBasePath.DbPath(this.dbName + ".db");

                        Type repository = typeof(Repository<>);

                        Type genericRepository = repository.MakeGenericType(type);

                        var r = Activator.CreateInstance(genericRepository, dbPath);
                    }
                }
            }
        }
    }
}
