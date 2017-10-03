using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Tests
{
    /// <summary>
    /// Class ClassIdentityTest tests the identity rules of the reference type <typeparamref name="T"/>.
    /// It uses four separate instances of the class, where the first three are considered to be equal and the fourth is not equal to the first three.
    /// </summary>
    /// <typeparam name="T">The reference type whose identity rules implementation are to be tested.</typeparam>
    [TestClass]
    public abstract class IdentityTester<T> where T : IEquatable<T>
    {
        protected static T _obj1;
        protected static T _obj2;
        protected static T _obj3;
        protected static T _obj4;
        protected Action<T>[] _modifyingMethods;

        public void AssertIsInitialized()
        {
            Assert.IsTrue(!ReferenceEquals(_obj1, null) &&
                          !ReferenceEquals(_obj2, null) &&
                          !ReferenceEquals(_obj3, null) &&
                          !ReferenceEquals(_obj4, null));
        }

        /// <summary>
        /// Initializes the specified class with 4 objects of type T and a method which modifies its argumnt of type T.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <param name="obj3">The obj3.</param>
        /// <param name="obj4">The obj4.</param>
        /// <param name="modifyingMethods">The modifying method.</param>
        public void Initialize(
            T obj1,
            T obj2,
            T obj3,
            T obj4,
            params Action<T>[] modifyingMethods)
        {
            if (ReferenceEquals(obj1, null))
                throw new ArgumentNullException(nameof(obj1));
            if (ReferenceEquals(obj2, null))
                throw new ArgumentNullException(nameof(obj2));
            if (ReferenceEquals(obj3, null))
                throw new ArgumentNullException(nameof(obj3));
            if (ReferenceEquals(obj4, null))
                throw new ArgumentNullException(nameof(obj4));

            if (modifyingMethods == null)
                throw new ArgumentNullException(nameof(modifyingMethods));
            if (modifyingMethods.Length == 0)
                throw new ArgumentNullException("modifyingMethods must not be empty.");
            if (modifyingMethods.Any(m => m == null))
                throw new ArgumentException("modifyingMethods cannot contain null delegates.");

            if (ReferenceEquals(obj1, obj2))
                throw new ArgumentException("The arguments obj1 and obj2 must be different instances considered to be equal.");
            if (ReferenceEquals(obj2, obj3))
                throw new ArgumentException("The arguments obj2 and obj3 must be different instances considered to be equal.");
            if (ReferenceEquals(obj3, obj4))
                throw new ArgumentException("The arguments obj3 and obj4 must be different instances considered to be equal.");

            if (!obj1.Equals(obj2) || !obj2.Equals(obj3))
                throw new ArgumentException("The arguments obj1, obj2 and obj3 should be considered equal.");
            if (obj1.Equals(obj4))
                throw new ArgumentException("The arguments obj1 and obj4 should be considered not equal.");

            _obj1 = obj1;
            _obj2 = obj2;
            _obj3 = obj3;
            _obj4 = obj4;

            _modifyingMethods = (Action<T>[])modifyingMethods.Clone();
        }

        #region Generic method Equals tests
        [TestMethod]
        public void GenericNotEqualToNull()
        {
            AssertIsInitialized();

            Assert.IsTrue(!_obj1.Equals((IEquatable<T>)null), "obj1 must not be null.");
            Assert.IsTrue(!_obj2.Equals((IEquatable<T>)null), "obj2 must not be null.");
            Assert.IsTrue(!_obj3.Equals((IEquatable<T>)null), "obj3 must not be null.");
            Assert.IsTrue(!_obj4.Equals((IEquatable<T>)null), "obj4 must not be null.");
        }

        [TestMethod]
        public void GenericReflexiveTest()
        {
            AssertIsInitialized();

            Assert.IsTrue(_obj1.Equals(_obj1), "The method Equals must be reflexive, i.e. a==a.");
            Assert.IsTrue(_obj3.Equals(_obj3), "The method Equals must be reflexive, i.e. a==a.");
            Assert.IsTrue(_obj4.Equals(_obj4), "The method Equals must be reflexive, i.e. a==a.");
            Assert.IsTrue(_obj2.Equals(_obj2), "The method Equals must be reflexive, i.e. a==a.");
        }

        [TestMethod]
        public void GenericSymmetricEqualityTest()
        {
            AssertIsInitialized();

            Assert.AreEqual(_obj1.Equals(_obj2), _obj2.Equals(_obj1), "The method Equals must be symmetric, i.e. (a==b) == (b==a).");
            Assert.AreEqual(_obj2.Equals(_obj3), _obj3.Equals(_obj2), "The method Equals must be symmetric, i.e. (a==b) == (b==a).");
            Assert.AreEqual(_obj3.Equals(_obj4), _obj4.Equals(_obj3), "The method Equals must be symmetric, i.e. (a==b) == (b==a).");
            Assert.AreEqual(_obj4.Equals(_obj1), _obj1.Equals(_obj4), "The method Equals must be symmetric, i.e. (a==b) == (b==a).");
        }

        [TestMethod]
        public void GenericTransitiveTest()
        {
            AssertIsInitialized();

            Assert.IsTrue(_obj1.Equals(_obj2) &&
                          _obj2.Equals(_obj3) &&
                          _obj1.Equals(_obj3), "The method Equals must be transitive, i.e. (a==b && b==c) -> (a==c).");
            Assert.IsTrue(!_obj1.Equals(_obj4) &&
                          !_obj2.Equals(_obj4) &&
                          !_obj3.Equals(_obj4), "The not-equals is not consistent with the method equals' transitivity, i.e. (a==b && b==c && a!=d) -> (b!=d && c!=d).");
        }
        #endregion

        #region Non-generic method Equals tests
        [TestMethod]
        public void NotEqualToNull()
        {
            AssertIsInitialized();

            Assert.IsTrue(!_obj1.Equals((object)null), "obj1.Equals(null) must not return true.");
            Assert.IsTrue(!_obj2.Equals((object)null), "obj2.Equals(null) must not return true.");
            Assert.IsTrue(!_obj3.Equals((object)null), "obj3.Equals(null) must not return true.");
            Assert.IsTrue(!_obj4.Equals((object)null), "obj4.Equals(null) must not return true.");
        }

        [TestMethod]
        public void ReflexiveTest()
        {
            AssertIsInitialized();

            Assert.IsTrue(_obj1.Equals((object)_obj1), "The method Equals must be reflexive, i.e. obj1.Equals(obj1) == true.");
            Assert.IsTrue(_obj2.Equals((object)_obj2), "The method Equals must be reflexive, i.e. obj2.Equals(obj1) == true.");
            Assert.IsTrue(_obj3.Equals((object)_obj3), "The method Equals must be reflexive, i.e. obj3.Equals(obj1) == true.");
            Assert.IsTrue(_obj4.Equals((object)_obj4), "The method Equals must be reflexive, i.e. obj4.Equals(obj1) == true.");
        }

        [TestMethod]
        public void SymmetricEqualityTest()
        {
            AssertIsInitialized();

            Assert.AreEqual(_obj1.Equals((object)_obj2), _obj2.Equals((object)_obj1), "The method Equals must be symmetric, i.e. obj1.Equals(obj2)==obj2.Equals(obj1).");
            Assert.AreEqual(_obj2.Equals((object)_obj3), _obj3.Equals((object)_obj2), "The method Equals must be symmetric, i.e. obj2.Equals(obj3)==obj3.Equals(obj2).");
            Assert.AreEqual(_obj3.Equals((object)_obj4), _obj4.Equals((object)_obj3), "The method Equals must be symmetric, i.e. obj3.Equals(obj4)==obj4.Equals(obj3).");
            Assert.AreEqual(_obj4.Equals((object)_obj1), _obj1.Equals((object)_obj4), "The method Equals must be symmetric, i.e. obj4.Equals(obj1)==obj1.Equals(obj4).");
        }

        [TestMethod]
        public void TransitiveTest()
        {
            AssertIsInitialized();

            Assert.IsTrue(_obj1.Equals((object)_obj2) &&
                          _obj2.Equals((object)_obj3) &&
                          _obj1.Equals((object)_obj3),
                          "The method Equals must be transitive, i.e. (obj1.Equals(obj2) && obj2.Equals(obj3) -> obj1.Equals(obj3).");
            Assert.IsTrue(!_obj1.Equals((object)_obj4) &&
                          !_obj2.Equals((object)_obj4) &&
                          !_obj3.Equals((object)_obj4),
                          "The not-equals is not consistent with the method equals' transitivity, i.e. (obj1.Equals(obj2) && obj2.Equals(obj3) && !obj1.Equals(obj4) && !obj2.Equals(obj4) -> !obj3.Equals(obj3).");
        }
        #endregion

        #region Operator equals (and not equals) tests
        [TestMethod]
        public void OperatorEqualsToNull()
        {
            AssertIsInitialized();

            Assert.IsTrue(!(_obj1==null), "obj1 must not be equal to null.");
            Assert.IsTrue(!(_obj2==null), "obj2 must not be equal to null.");
            Assert.IsTrue(!(_obj3==null), "obj3 must not be equal to null.");
            Assert.IsTrue(!(_obj4==null), "obj4 must not be equal to null.");

            Assert.IsTrue(!(null==_obj1), "null must not be equal to obj1.");
            Assert.IsTrue(!(null==_obj2), "null must not be equal to obj2.");
            Assert.IsTrue(!(null==_obj3), "null must not be equal to obj3.");
            Assert.IsTrue(!(null==_obj4), "null must not be equal to obj4.");
        }

        [TestMethod]
        public void OperatorNotEqualsToNull()
        {
            AssertIsInitialized();

            Assert.IsTrue(_obj1!=null, "Objects must not be equal to null.");
            Assert.IsTrue(_obj2!=null, "Objects must not be equal to null.");
            Assert.IsTrue(_obj3!=null, "Objects must not be equal to null.");
            Assert.IsTrue(_obj4!=null, "Objects must not be equal to null.");

            Assert.IsTrue(null!=_obj1, "Objects must not be equal to null.");
            Assert.IsTrue(null!=_obj2, "Objects must not be equal to null.");
            Assert.IsTrue(null!=_obj3, "Objects must not be equal to null.");
            Assert.IsTrue(null!=_obj4, "Objects must not be equal to null.");
        }

        //[TestMethod]
        //public void OperatorEqualsTests()
        //{
        //    var target1 = new T(typeof("abc");
        //    var target2 = new T(typeof("abc");
        //    var target3 = new T(typeof("abc");
        //    var target4 = new T(typeof("xyz");

        //    Assert.IsTrue(!(target1==(T)null), "target1 must not be equal to null.");
        //    Assert.IsTrue(!((T)null==target1), "target1 must not be equal to obj1.");

        //    // reflexitivity
        //    var t = target1;

        //    Assert.IsTrue(target1==t, "The operator == must be reflexive.");
        //    Assert.IsFalse(target1!=t, "The operator == must be reflexive.");

        //    // symmetricity
        //    Assert.AreEqual(target1==target2, target2==target1, "The operator == must be symmetric.");
        //    Assert.AreEqual(target1!=target4, target4!=target1, "The operator != must be symmetric.");

        //    // transityvity
        //    Assert.IsTrue(target1==target2 && target2==target3 && target3==target1, "The operator == must be transitive.");

        //    Assert.IsTrue(target1==target2 && target1!=target4 && target2!=target4, "The operator != must be transitive.");
        //}
        #endregion

        #region GetHashCode rules
        [TestMethod]
        public void GetHashCodeConsistency()
        {
            AssertIsInitialized();

            var hash = _obj1.GetHashCode();

            foreach (var m in _modifyingMethods)
            {
                m(_obj1);
                Assert.AreEqual(hash, _obj1.GetHashCode(), "The method GetHashCode must be consistent regardless of its modifications.");
            }
        }

        [TestMethod]
        public void GetHashCodeRelationships()
        {
            AssertIsInitialized();

            var hash1 = _obj1.GetHashCode();
            var hash2 = _obj2.GetHashCode();
            var hash3 = _obj3.GetHashCode();
            var hash4 = _obj4.GetHashCode();

            Assert.IsTrue(hash1 == hash2 && hash2 == hash3, "The hash codes of objects considered to be equal should be equal.");

            if (hash1 == hash4)
                Assert.Inconclusive("Different objects with the same hash code is possible but should be rare. "+
                                    "Try to change the obj4's values or change the hash function to give better distribution.");
        }
        #endregion
    }
}
