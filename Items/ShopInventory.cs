using System.Linq;
using Data;
using Models;
using UnityEngine;

namespace Items
{
    public class ShopInventory : SaveableObject
    {
        [SerializeField] private Item[] items;
        [SerializeField] private Container container;
        private int _lastRefreshDay;

        protected override void Start()
        {
            base.Start();
            if (SaveGameManager.NewGame)
            {
                RefreshStock();
            }
        }

        private void RefreshStock()
        {
            container.Clear();
            foreach (var item in items)
            {
                container.AddItem(item.id);
            }

            _lastRefreshDay = TimeManager.Day;
        }

        private void Update()
        {
            if (TimeManager.Day > _lastRefreshDay)
            {
                RefreshStock();
            }
        }

        public override string Save()
        {
            var data = new ContainerData();
            foreach (var item in container.items.Where(item => item != null))
            {
                data.itemIds.Add(item.id);
                data.itemQuantities.Add(item.Quantity);
            }

            data.lastRefreshDay = _lastRefreshDay;
            var json = JsonUtility.ToJson(data);
            return json;
        }

        public override void Load(string json)
        {
            container.Clear();
            var data = JsonUtility.FromJson<ContainerData>(json);
            for(var i = 0; i < data.itemIds.Count; i++)
            {
                for (var ii = 0; ii < data.itemQuantities[i]; ii++)
                {
                    container.AddItem(data.itemIds[i]);
                }
            }

            _lastRefreshDay = data.lastRefreshDay;
        }
    }
}