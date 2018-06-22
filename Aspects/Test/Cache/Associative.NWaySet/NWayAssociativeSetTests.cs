
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using vm.Aspects.Cache.Associative.NWaySet;

namespace vm.Aspects.Tests.Cache.Associative.NWaySet
{
    /// <summary>
    /// Summary description for NWayAssociativeSetTests
    /// </summary>
    [TestClass]
    public class NWaySetAssociativeCacheTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestConstructor_2setsOf4Lru()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 4);

            Assert.AreEqual(2, target.NumberOfSets);
            Assert.AreEqual(4, target.SetSize);
            Assert.AreEqual(8, target.Size);

            Assert.IsInstanceOfType(target.Entries, typeof(Entry<string, string>[]));
            Assert.AreEqual(8, target.Entries.Length);

            Assert.IsInstanceOfType(target.Sets, typeof(EntrySet<string, string>[]));
            Assert.AreEqual(2, target.Sets.Length);

            var beginIndex = 0;

            foreach (var set in target.Sets)
            {
                var wrappedSet = new PrivateObject(set);

                Assert.IsNotNull(wrappedSet.GetProperty("Lock"));
                Assert.IsInstanceOfType(wrappedSet.GetProperty("Lock"), typeof(ReaderWriterLockSlim));

                Assert.AreEqual(target, wrappedSet.GetField("_cache"));

                Assert.AreEqual(beginIndex, (int)wrappedSet.GetField("_entriesBegin"));
                Assert.AreEqual(beginIndex+target.SetSize, (int)wrappedSet.GetField("_entriesEnd"));

                Assert.AreEqual(0L, (long)wrappedSet.GetField("_nextUsage"));

                beginIndex += target.SetSize;
            }
        }

        [TestMethod]
        public void TestConstructor_4setsOf2Mru()
        {
            var target = new NWaySetAssociativeCache<string, string>(4, 2);

            Assert.AreEqual(4, target.NumberOfSets);
            Assert.AreEqual(2, target.SetSize);
            Assert.AreEqual(8, target.Size);

            Assert.IsInstanceOfType(target.Entries, typeof(Entry<string, string>[]));
            Assert.AreEqual(8, target.Entries.Length);

            Assert.IsInstanceOfType(target.Sets, typeof(EntrySet<string, string>[]));
            Assert.AreEqual(4, target.Sets.Length);

            var beginIndex = 0;

            foreach (var set in target.Sets)
            {
                var wrappedSet = new PrivateObject(set);

                Assert.IsNotNull(wrappedSet.GetProperty("Lock"));
                Assert.IsInstanceOfType(wrappedSet.GetProperty("Lock"), typeof(ReaderWriterLockSlim));

                Assert.AreEqual(target, wrappedSet.GetField("_cache"));

                Assert.AreEqual(beginIndex, (int)wrappedSet.GetField("_entriesBegin"));
                Assert.AreEqual(beginIndex+target.SetSize, (int)wrappedSet.GetField("_entriesEnd"));

                Assert.AreEqual(0L, (long)wrappedSet.GetField("_nextUsage"));

                beginIndex += target.SetSize;
            }
        }

        [TestMethod]
        public void Test_AddFirst()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
            };

            Assert.AreEqual(3, target.Entries.Where(e => !e.IsUsed).Count());

            var entry = target.Entries.Single(e => e.IsUsed);

            Assert.AreEqual("data1", entry.Value);
            Assert.AreEqual("key1", entry.Key);
            Assert.AreEqual("key1".GetHashCode(), entry.KeyHash);
            Assert.AreEqual(1, entry.UsageStamp);

            Assert.AreEqual(1, target.Count());
        }

        [TestMethod]
        public void Test_AddFirstSecond()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
                ["key2"] = "data2",
            };

            Assert.AreEqual(2, target.Entries.Where(e => !e.IsUsed).Count());

            var entries = target.Entries.Where(e => e.IsUsed).OrderBy(e => e.Key).ToArray();

            Assert.AreEqual(2, entries.Length);

            Assert.AreEqual("data1", entries[0].Value);
            Assert.AreEqual("key1", entries[0].Key);
            Assert.AreEqual("key1".GetHashCode(), entries[0].KeyHash);
            Assert.IsTrue(entries[0].UsageStamp == 1);

            Assert.AreEqual("data2", entries[1].Value);
            Assert.AreEqual("key2", entries[1].Key);
            Assert.AreEqual("key2".GetHashCode(), entries[1].KeyHash);
            Assert.IsTrue(entries[1].UsageStamp == 2);

            Assert.AreEqual(2, target.Count());
        }

        [TestMethod]
        public void Test_GetFirst()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
            };

            Assert.IsTrue(target.TryGetValue("key1", out var value));
            Assert.AreEqual("data1", value);
            Assert.AreEqual(1, target.Count());
        }

        [TestMethod]
        public void Test_GetFirstSecond()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
                ["key2"] = "data2",
            };

            Assert.IsTrue(target.TryGetValue("key1", out var value));
            Assert.AreEqual("data1", value);

            Assert.IsTrue(target.TryGetValue("key2", out value));
            Assert.AreEqual("data2", value);
            Assert.AreEqual(2, target.Count());
        }

        [TestMethod]
        public void Test_IndexFirst()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
            };

            Assert.AreEqual("data1", target["key1"]);
            Assert.AreEqual(1, target.Count());
        }
        [TestMethod]
        public void Test_IndexFirstSecond()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
                ["key2"] = "data2",
            };

            Assert.AreEqual("data1", target["key1"]);
            Assert.AreEqual("data2", target["key2"]);
            Assert.AreEqual(2, target.Count());
        }

        [TestMethod]
        public void Test_GetEmpty()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
                ["key2"] = "data2",
            };

            Assert.IsFalse(target.TryGetValue("key3", out var value));
            Assert.IsNull(value);
            Assert.AreEqual(2, target.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void Test_IndexEmpty()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
                ["key2"] = "data2",
            };

            Assert.AreEqual(2, target.Count());
            var s = target["key3"];
        }

        [TestMethod]
        public void Test_ContainsKey()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
                ["key2"] = "data2",
            };

            Assert.AreEqual(2, target.Count());
            Assert.IsTrue(target.ContainsKey("key1"));
            Assert.IsTrue(target.ContainsKey("key2"));
            Assert.IsFalse(target.ContainsKey("key3"));
        }

        [TestMethod]
        public void Test_ContainsKeyValue()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
                ["key2"] = "data2",
            };

            Assert.AreEqual(2, target.Count());
            Assert.IsTrue(target.Contains(new KeyValuePair<string, string>("key1", "data1")));
            Assert.IsTrue(target.Contains(new KeyValuePair<string, string>("key2", "data2")));
            Assert.IsFalse(target.Contains(new KeyValuePair<string, string>("key0", "data1")));
            Assert.IsFalse(target.Contains(new KeyValuePair<string, string>("key1", "data0")));
        }

        [TestMethod]
        public void Test_ReplaceFirstSecond()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
                ["key2"] = "data2",
            };

            Assert.IsTrue(target.TryGetValue("key1", out var value));
            Assert.AreEqual("data1", value);

            Assert.IsTrue(target.TryGetValue("key2", out value));
            Assert.AreEqual("data2", value);

            target.Add("key1", "data1.1");
            Assert.IsTrue(target.TryGetValue("key1", out value));
            Assert.AreEqual("data1.1", value);

            target.Add("key2", "data2.1");
            Assert.IsTrue(target.TryGetValue("key2", out value));
            Assert.AreEqual("data2.1", value);
            Assert.AreEqual(2, target.Count());
        }


        [TestMethod]
        public void Test_ReplaceIndexFirstSecond()
        {
            var target = new NWaySetAssociativeCache<string, string>(2, 2)
            {
                ["key1"] = "data1",
                ["key2"] = "data2",
            };

            Assert.AreEqual("data1", target["key1"]);
            Assert.AreEqual("data2", target["key2"]);

            target["key1"] = "data1.1";
            target["key2"] = "data2.1";

            Assert.AreEqual("data1.1", target["key1"]);
            Assert.AreEqual("data2.1", target["key2"]);
            Assert.AreEqual(2, target.Count());
        }

        [TestMethod]
        public void Test_EvictOne()
        {
            var target = new NWaySetAssociativeCache<int, string>(2, 2)
            {
                [1] = "data1",
                [2] = "data2",
                [3] = "data3",
                [4] = "data4",
                [5] = "data5",
            };

            Assert.AreEqual("data2", target[2]);
            Assert.AreEqual("data3", target[3]);
            Assert.AreEqual("data4", target[4]);
            Assert.AreEqual("data5", target[5]);

            Assert.IsFalse(target.TryGetValue(1, out var value));
            Assert.IsNull(value);
            Assert.AreEqual(4, target.Count());
        }

        [TestMethod]
        public void Test_RemoveKey()
        {
            var target = new NWaySetAssociativeCache<int, string>(2, 2)
            {
                [2] = "data2",
                [3] = "data3",
                [4] = "data4",
                [5] = "data5",
            };

            Assert.AreEqual("data2", target[2]);
            Assert.AreEqual("data3", target[3]);
            Assert.AreEqual("data4", target[4]);
            Assert.AreEqual("data5", target[5]);
            Assert.AreEqual(4, target.Count());

            Assert.IsFalse(target.Remove(1));
            Assert.IsTrue(target.Remove(4));
            Assert.IsFalse(target.TryGetValue(4, out var s));

            Assert.AreEqual(3, target.Count());
        }

        [TestMethod]
        public void Test_RemoveKeyValue()
        {
            var target = new NWaySetAssociativeCache<int, string>(2, 2)
            {
                [2] = "data2",
                [3] = "data3",
                [4] = "data4",
                [5] = "data5",
            };

            Assert.AreEqual("data2", target[2]);
            Assert.AreEqual("data3", target[3]);
            Assert.AreEqual("data4", target[4]);
            Assert.AreEqual("data5", target[5]);
            Assert.AreEqual(4, target.Count());

            Assert.IsFalse(target.Remove(new KeyValuePair<int, string>(0, "data4")));
            Assert.IsFalse(target.Remove(new KeyValuePair<int, string>(4, "data0")));
            Assert.IsTrue(target.Remove(new KeyValuePair<int, string>(4, "data4")));
            Assert.IsFalse(target.TryGetValue(4, out var s));

            Assert.AreEqual(3, target.Count());
        }

        [TestMethod]
        public void Test_Keys()
        {
            var target = new NWaySetAssociativeCache<int, string>(2, 2)
            {
                [2] = "data2",
                [3] = "data3",
                [4] = "data4",
                [5] = "data5",
            };

            var keys = target.Keys;

            Assert.AreEqual(4, keys.Count());
            Assert.IsTrue(keys.Contains(2));
            Assert.IsTrue(keys.Contains(3));
            Assert.IsTrue(keys.Contains(4));
            Assert.IsTrue(keys.Contains(5));
        }

        [TestMethod]
        public void Test_Values()
        {
            var target = new NWaySetAssociativeCache<int, string>(2, 2)
            {
                [2] = "data2",
                [3] = "data3",
                [4] = "data4",
                [5] = "data5",
            };

            var values = target.Values;

            Assert.AreEqual(4, values.Count());
            Assert.IsTrue(values.Contains("data2"));
            Assert.IsTrue(values.Contains("data3"));
            Assert.IsTrue(values.Contains("data4"));
            Assert.IsTrue(values.Contains("data5"));
        }

        [TestMethod]
        public void Test_Reset()
        {
            var target = new NWaySetAssociativeCache<int, string>(2, 2)
            {
                [2] = "data2",
                [3] = "data3",
                [4] = "data4",
                [5] = "data5",
            };

            Assert.AreEqual(4, target.Count());

            target.Clear();
            Assert.AreEqual(0, target.Count());
        }

        [TestMethod]
        public void Test_Enumerator()
        {
            var target = new NWaySetAssociativeCache<int, string>(2, 2)
            {
                [2] = "data2",
                [3] = "data3",
                [4] = "data4",
                [5] = "data5",
            };

            var keys = new HashSet<int> { 2, 3, 4, 5, };
            var values = new HashSet<string> { "data2", "data3", "data4", "data5", };

            foreach (var kv in target)
            {
                Assert.IsTrue(keys.Contains(kv.Key));
                Assert.IsTrue(values.Contains(kv.Value));

                keys.Remove(kv.Key);
                values.Remove(kv.Value);
            }

            Assert.IsTrue(!keys.Any());
            Assert.IsTrue(!values.Any());
        }

        [TestMethod]
        public void Test_CopyTo()
        {
            var target = new NWaySetAssociativeCache<int, string>(2, 2)
            {
                [2] = "data2",
                [3] = "data3",
                [4] = "data4",
                [5] = "data5",
            };

            var array = new KeyValuePair<int, string>[4];

            target.CopyTo(array, 0);

            var keys = new HashSet<int> { 2, 3, 4, 5, };
            var values = new HashSet<string> { "data2", "data3", "data4", "data5", };

            foreach (var kv in array)
            {
                Assert.IsTrue(keys.Contains(kv.Key));
                Assert.IsTrue(values.Contains(kv.Value));

                keys.Remove(kv.Key);
                values.Remove(kv.Value);
            }

            Assert.IsTrue(!keys.Any());
            Assert.IsTrue(!values.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_ParametersValidationCopyTo1()
        {
            var target = new NWaySetAssociativeCache<int, string>(2, 2)
            {
                [2] = "data2",
                [3] = "data3",
                [4] = "data4",
                [5] = "data5",
            };

            var array = new KeyValuePair<int, string>[4];

            target.CopyTo(null, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ParametersValidationCopyTo2()
        {
            var target = new NWaySetAssociativeCache<int, string>(2, 2)
            {
                [2] = "data2",
                [3] = "data3",
                [4] = "data4",
                [5] = "data5",
            };

            var array = new KeyValuePair<int, string>[4];

            target.CopyTo(array, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ParametersValidationCopyTo3()
        {
            var target = new NWaySetAssociativeCache<int, string>(2, 2)
            {
                [2] = "data2",
                [3] = "data3",
                [4] = "data4",
                [5] = "data5",
            };

            var array = new KeyValuePair<int, string>[4];

            target.CopyTo(array, 1);
        }
    }
}
