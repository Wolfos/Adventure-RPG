using System.Collections;
using Models;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests.World
{
    public class TimeManagerTests
    {
        private void Setup()
        {
            TimeManager.Time = 10.5f; // Expecting 10:30am
        }
        [Test]
        public void IsBetween_NotBetween_ReturnsFalse()
        {
            Setup();
            var actual = TimeManager.IsBetween(new TimeStamp(10, 31), new TimeStamp(10, 29));
            Assert.IsFalse(actual);
        }

        [Test]
        public void IsBetween_Between_ReturnsTrue()
        {
            Setup();
            var actual = TimeManager.IsBetween(new TimeStamp(23, 0), new TimeStamp(11, 0));
            Assert.IsTrue(actual);
        }

        [Test]
        public void RealTime_ReturnsExpected()
        {
            Setup();
            var actual = TimeManager.RealTime();
            Assert.AreEqual(10.5f, actual);
        }
    }
}