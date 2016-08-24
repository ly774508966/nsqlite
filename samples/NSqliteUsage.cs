using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;
using System.Data;
using NPlugins.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System;

public class NSqliteUsage : MonoBehaviour 
{
	private Repository<TestClass> repository;

	void Start () 
	{
		const string databaseName = "db.rdb";

		string dbPath = "URI=file:" + Application.dataPath + "/" +  databaseName;
		
		repository = new Repository<TestClass> (dbPath);

		RunNSqliteMethods ();
	}

	private void RunNSqliteMethods ()
	{
		Insert ();

		InsertList ();

		UpdateEntry ();

		Find ();

		FindAll ();

		FindWhere ();

//		Delete ();
//
//		DeleteFromInstance ();
//
//		DeleteWhere ();
//
//		DeleteAll ();
	}

	private void Insert ()
	{
		var test = new TestClass
		{
			Color = new Color(0.5f, 0.3f, 0.2f, 1),
			Vector3 = new Vector3(10, 20, 30),
			Name = "NSqlite",
			Boolean = false,
			DateTime = DateTime.Now
		};
		
		repository.Insert(test);
	}

	private void InsertList ()
	{
		List<TestClass> tests = new List<TestClass>
		{
			new TestClass
			{
				Color = new Color(0, 0, 1, 1),
				Vector3 = Vector3.left,
				Name = "Entry 1",
				Boolean = true,
				DateTime = DateTime.UtcNow
			},
			new TestClass
			{
				Color = new Color(0, 1, 0, 1),
				Vector3 = Vector3.right,
				Name = "Entry 2",
				Boolean = true,
				DateTime = DateTime.Now
			}
		};

		repository.Insert(tests);
	}

	private void UpdateEntry ()
	{
		TestClass updateObject = repository.FindWhere(_ => _.Name == "NSqlite").FirstOrDefault();

		if(updateObject == null) 
			return;

		updateObject.Name = "NSqlite Plugin";
		
		repository.Update(updateObject);
	}

	private void Find ()
	{
		const int id = 1;

		TestClass foundObject = repository.Find(id);

		if(foundObject == null) 
			return;

		Debug.Log(foundObject.Name + foundObject.DateTime);
	}

	private void FindAll ()
	{
		List<TestClass> findAllObjects = repository.FindAll();
		
		findAllObjects.ForEach(f => Debug.Log(f.Boolean));
	}

	private void FindWhere ()
	{
		List<TestClass> findWhereObject = repository.FindWhere(_ => _.Vector3 == new Vector3(10, 20, 30));
		
		Debug.Log(findWhereObject.Count);
	}

	private void Delete ()
	{
		const int id = 1;

		repository.Delete(id);
	}

	private void DeleteFromInstance ()
	{
		TestClass findWhereObject = repository.FindWhere(_ => _.Vector3 == new Vector3(10, 20, 30)).FirstOrDefault();

		if(findWhereObject == null) 
			return;

		repository.Delete(findWhereObject);
	}

	private void DeleteAll ()
	{
		repository.DeleteAll();
	}

	private void DeleteWhere ()
	{
		repository.DeleteWhere(_ => _.Name == "Entry 2");
	}
}