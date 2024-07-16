using System;
using System.Collections.Generic;
using Items;
using Sirenix.OdinInspector;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Data
{
	public class ItemDatabase: MonoBehaviour
	{
		[SerializeField] private ItemData[] itemData;

		private Dictionary<Guid, ItemData> _dictionary;
		private static ItemDatabase _instance;

		private void Awake()
		{
			_instance = this;
			_dictionary = new();
			foreach (var item in itemData)
			{
				_dictionary.Add(item.Guid, item);
			}
		}

		public static ItemData GetItem(Guid guid)
		{
			return _instance._dictionary[guid];
		}
		
#if UNITY_EDITOR
		[Button("Autofill")]
		private void Autofill()
		{
			var guids = AssetDatabase.FindAssets("t:ItemData");
			var items = new List<ItemData>();
			foreach (var guid in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadAssetAtPath<ItemData>(path);
				items.Add(asset);
			}

			itemData = items.ToArray();
		}
#endif
	}
}