/*
*
*	DataBasePath.cs
*
*	Created by Nícolas Reichert.
*
*/

using UnityEngine;

namespace NPlugins.Sqlite
{
    public static class DataBasePath
    {

        /// <summary>
        /// 	Returns the proper path for the database, given a name for the database. must contain extension (sqdb, rdb, ...)
        /// </summary>
        /// <remarks>
        /// 	If you already have a database created on your build (it must be in StreamingAssets folder), 
        /// 	this method will copy the existing database to the proper path on any mobile device that installs the app.
        /// </remarks>
        public static string DbPath(string dbName)
        {
            string dbPath = "";

            if (Application.platform == RuntimePlatform.Android)
            {
                string filePath = Application.persistentDataPath + "/" + dbName;
                if (!System.IO.File.Exists(filePath))
                {
                    string oriPath = "jar:file://" + Application.dataPath + "!/assets/" + dbName;

                    WWW reader = new WWW(oriPath);
                    while (!reader.isDone)
                        Debug.Log(Time.time);

                    System.IO.File.WriteAllBytes(filePath, reader.bytes);
                }

                dbPath = "URI=file:" + Application.persistentDataPath + "/" + dbName;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                dbPath = "URI=file:" + Application.persistentDataPath + "/" + dbName;

                if (!System.IO.File.Exists(Application.persistentDataPath + "/" + dbName))
                {
                    System.IO.File.Copy(Application.dataPath + "/Raw/" + dbName, Application.persistentDataPath + "/" + dbName, true);
                }
            }
            else
            {
                if (!System.IO.Directory.Exists(Application.dataPath + "/StreamingAssets/"))
                {
                    System.IO.Directory.CreateDirectory(Application.dataPath + "/StreamingAssets/");
                }

                dbPath = "URI=file:" + Application.dataPath + "/StreamingAssets/" + dbName;
            }

            return dbPath;
        }
    }
}