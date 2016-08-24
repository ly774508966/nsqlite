using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using NPlugins.Sqlite;

public class NSqliteUsage2 : MonoBehaviour
{

	void Start()
	{
		const string databaseName = "db2.rdb";
		
		string dbPath = DataBasePath.DbPath(databaseName);//"URI=file:" + Application.dataPath + "/" +  databaseName;

		var baseClass = new BaseClass
		{
			Name = "TEST",
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
		
		Repository<BaseClass> repository = new Repository<BaseClass>(dbPath);

		repository.Insert(baseClass);

		List<BaseClass> baseClassList = repository.FindWhere(b => b.Name == "TEST");

		Debug.Log(baseClassList.Count);
	}

	/// <summary>
	/// 	Model classes MUST inherit from <see cref="Storable"/>. 
	/// </summary>
	public class BaseClass : Storable
	{
		public string Name { get; set; }
		
		public List<string> Strings { get; set; }
		
		public List<OtherModel> Models { get; set; } 
	}

	/// <summary>
	/// 	Model classes MUST inherit from <see cref="Storable"/>. 
	/// </summary>
	public class OtherModel : Storable
	{
		public DateTime Date { get; set; }
		
		public bool CanWrite { get; set; }
		
		public Vector3 Position { get; set; }
	}
}
