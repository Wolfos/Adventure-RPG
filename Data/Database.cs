using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public class Database : MonoBehaviour
	{
		[SerializeField] private MonoBehaviour[] databases;
		private Dictionary<Type, object> _databases;

		private static Database _instance;
		private static Database Instance
		{
			get
			{
				if (_instance == null)
				{
					var prefab = Resources.Load("Database");
					var go = (GameObject)Instantiate(prefab);
					_instance = go.GetComponent<Database>();
				}

				return _instance;
			}
		}

		private void Awake()
		{
			_databases = new Dictionary<Type, object>();
			foreach (var db in databases)
			{
				_databases.Add(db.GetType(), db);
			}
		}

		public static T GetDatabase<T>()
		{
			object db;
			Instance._databases.TryGetValue(typeof(T), out db);
			if (db == null) Debug.LogError("System of type " + typeof(T) + " was not found");
			return (T)db;
		}
	}
}