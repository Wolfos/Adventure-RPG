using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Items;


namespace Tests
{
	public class ItemTests
	{
		private Container TestContainer(int slots)
		{
			GameObject g = new GameObject("Test");
			var container = g.AddComponent<Container>();
			container.SetSlots(slots);
			return container;
		}
		
		[Test]
		public void AddItem()
		{
			var container = TestContainer(1);
			Assert.True(container.AddItem(0));
			Assert.False(container.AddItem(1));
			Assert.AreEqual(container.GetItemCount(), 1);
		}

		[Test]
		public void GetItem()
		{
			var container = TestContainer(1);
			container.AddItem(0);
			Assert.NotNull(container.GetItemBySlot(0));
			Assert.NotNull(container.GetItemByID(0));
		}

		[Test]
		public void SwapItem()
		{
			var container1 = TestContainer(2);
			var container2 = TestContainer(2);
			
			// Put item in different slot
			container1.AddItem(0);
			container1.SwapItem(0, 1);
			Assert.IsNull(container1.GetItemBySlot(0));
			Assert.NotNull(container1.GetItemBySlot(1));
			
			// Move item to different container
			container1.SwapItem(1, 0, container2);
			Assert.IsNull(container1.GetItemBySlot(1));
			Assert.NotNull(container2.GetItemBySlot(0));
			Assert.AreEqual(container1.GetItemCount(), 0);
			Assert.AreEqual(container2.GetItemCount(), 1);
		}
	}
}
