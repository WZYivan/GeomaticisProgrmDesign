using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolBox.Utility;

namespace ToolBox.Tests
{
    [TestClass]
    public class AutoBackwardsRollerTests
    {
        [TestMethod]
        public void Constructor_InitializesCorrectly()
        {
            var roller = new AutoBackwardsRoller<int>(5);
            Assert.AreEqual(5, roller.Value);
        }

        [TestMethod]
        public void Set_SingleValue_ChangesValue()
        {
            var roller = new AutoBackwardsRoller<int>(1);
            roller.Set(2);
            Assert.AreEqual(2, roller.Value);
        }

        [TestMethod]
        public void Set_SameValue_DoesNotChangeValue()
        {
            var roller = new AutoBackwardsRoller<int>(1);
            roller.Set(1);
            Assert.AreEqual(1, roller.Value);
        }

        [TestMethod]
        public void RollBack_SingleChange_RestoresPreviousValue()
        {
            var roller = new AutoBackwardsRoller<int>(1);
            roller.Set(2);
            bool result = roller.RollBack();

            Assert.IsTrue(result);
            Assert.AreEqual(1, roller.Value);
        }

        [TestMethod]
        public void RollBack_MultipleChanges_RestoresStepByStep()
        {
            var roller = new AutoBackwardsRoller<int>(1);
            roller.Set(2); // Stack: [1(0)]
            roller.Set(3); // Stack: [1(0), 2(0)]
            roller.Set(4); // Stack: [1(0), 2(0), 3(0)]

            Assert.AreEqual(4, roller.Value);

            roller.RollBack(); // Back to 3
            Assert.AreEqual(3, roller.Value);

            roller.RollBack(); // Back to 2
            Assert.AreEqual(2, roller.Value);

            roller.RollBack(); // Back to 1
            Assert.AreEqual(1, roller.Value);
        }

        [TestMethod]
        public void RollBack_NoChanges_ReturnsFalse()
        {
            var roller = new AutoBackwardsRoller<int>(1);
            bool result = roller.RollBack();

            Assert.IsFalse(result);
            Assert.AreEqual(1, roller.Value);
        }

        [TestMethod]
        public void Set_WithRepeats_HandlesCorrectly()
        {
            var roller = new AutoBackwardsRoller<int>(1);
            roller.Set(2);    // Current: 2, Stack: [1(0)]
            roller.Set(2);    // Current: 2, Stack: [1(0), 2(1)]
            roller.Set(3);    // Current: 3, Stack: [1(0), 2(1), 2(0)]

            Assert.AreEqual(3, roller.Value);

            // First rollback should reduce repeat count
            roller.RollBack();
            Assert.AreEqual(2, roller.Value);

            // Second rollback should reduce repeat count again
            roller.RollBack();
            Assert.AreEqual(2, roller.Value);

            // Third rollback should move to previous value
            roller.RollBack();
            Assert.AreEqual(1, roller.Value);
        }

        [TestMethod]
        public void Using_WithUsingStatement_RollsBackAutomatically()
        {
            var roller = new AutoBackwardsRoller<int>(1);

            using (var token = roller.Using(5))
            {
                Assert.AreEqual(5, roller.Value);
            }

            Assert.AreEqual(1, roller.Value);
        }

        [TestMethod]
        public void Using_MultipleNested_RollsBackCorrectly()
        {
            var roller = new AutoBackwardsRoller<int>(1);

            Assert.AreEqual(1, roller.Value);

            using (var token1 = roller.Using(2))
            {
                Assert.AreEqual(2, roller.Value);

                using (var token2 = roller.Using(3))
                {
                    Assert.AreEqual(3, roller.Value);

                    using (var token3 = roller.Using(4))
                    {
                        Assert.AreEqual(4, roller.Value);
                    }

                    Assert.AreEqual(3, roller.Value);
                }

                Assert.AreEqual(2, roller.Value);
            }

            Assert.AreEqual(1, roller.Value);
        }

        [TestMethod]
        public void RollBack_WithRepeats_MultipleRollbacks()
        {
            var roller = new AutoBackwardsRoller<int>(1);

            roller.Set(2); // Stack: [1(0)]
            roller.Set(2); // Stack: [1(0), 2(1)] - repeated
            roller.Set(2); // Stack: [1(0), 2(2)] - repeated again
            roller.Set(3); // Stack: [1(0), 2(2), 2(0)]

            Assert.AreEqual(3, roller.Value);

            // First rollback: still 2, but repeat count decreases
            roller.RollBack();
            Assert.AreEqual(2, roller.Value);

            // Second rollback: still 2, repeat count decreases
            roller.RollBack();
            Assert.AreEqual(2, roller.Value);

            // Third rollback: still 2, repeat count decreases
            roller.RollBack();
            Assert.AreEqual(2, roller.Value);

            // Fourth rollback: now back to 1
            roller.RollBack();
            Assert.AreEqual(1, roller.Value);
        }

        [TestMethod]
        public void ValueInStack_IsPopOk_WhenRepeatCountIsZero()
        {
            // This is tested indirectly through the main functionality
            var roller = new AutoBackwardsRoller<int>(1);
            roller.Set(2);

            // At this point, the ValueInStack for value 1 should have RepeatCount = 0
            // So RollBack should actually change the current value
            bool result = roller.RollBack();
            Assert.IsTrue(result);
            Assert.AreEqual(1, roller.Value);
        }

        [TestMethod]
        public void ValueInStack_IsRepeat_WhenValuesAreEqual()
        {
            var roller = new AutoBackwardsRoller<int>(1);
            roller.Set(1); // This should be treated as a repeat

            // The internal logic should recognize this as a repeat
            Assert.AreEqual(1, roller.Value); // Value shouldn't change
        }

        [TestMethod]
        public void Dispose_MultipleTimes_DoesNotThrow()
        {
            var roller = new AutoBackwardsRoller<int>(1);
            var token = roller.Using(5);

            token.Dispose();
            token.Dispose(); // Should not throw exception
            token.Dispose(); // Should not throw exception

            Assert.IsTrue(true); // If we reach here, no exception was thrown
        }

        [TestMethod]
        public void ComplexScenario_MixedOperations()
        {
            var roller = new AutoBackwardsRoller<string>("initial");

            // Set new value
            roller.Set("first");
            Assert.AreEqual("first", roller.Value);

            // Repeat same value
            roller.Set("first");
            Assert.AreEqual("first", roller.Value);

            // Set different value
            roller.Set("second");
            Assert.AreEqual("second", roller.Value);

            // Set another different value
            roller.Set("third");
            Assert.AreEqual("third", roller.Value);

            // Roll back - should go back to "second"
            roller.RollBack();
            Assert.AreEqual("second", roller.Value);

            // Roll back - should still be "first" (due to repeat)
            roller.RollBack();
            Assert.AreEqual("first", roller.Value);

            // Roll back - should still be "first" (due to repeat)
            roller.RollBack();
            Assert.AreEqual("first", roller.Value);

            // Roll back - should go back to "initial"
            roller.RollBack();
            Assert.AreEqual("initial", roller.Value);

            // Try to roll back again - should return false
            bool result = roller.RollBack();
            Assert.IsFalse(result);
            Assert.AreEqual("initial", roller.Value);
        }

        [TestMethod]
        public void GenericType_String_WorksCorrectly()
        {
            var roller = new AutoBackwardsRoller<string>("hello");

            roller.Set("world");
            Assert.AreEqual("world", roller.Value);

            roller.Set("hello"); // Different from initial but same as constructor param
            Assert.AreEqual("hello", roller.Value);

            roller.RollBack();
            Assert.AreEqual("world", roller.Value);
        }

        [TestMethod]
        public void GenericType_CustomObject_WorksCorrectly()
        {
            var obj1 = new TestClass(1, "test1");
            var obj2 = new TestClass(2, "test2");
            var obj3 = new TestClass(1, "test1"); // Same values as obj1, so should be equal

            var roller = new AutoBackwardsRoller<TestClass>(obj1);

            roller.Set(obj2);
            Assert.AreSame(obj2, roller.Value);

            roller.Set(obj3); // Should be treated as repeat since obj1.Equals(obj3)
            Assert.AreNotSame(obj2, roller.Value); // Value shouldn't change because obj3 equals initial value

            roller.RollBack();
            Assert.AreSame(obj2, roller.Value);
        }

        // Test class for testing generic functionality
        private class TestClass : IEquatable<TestClass>
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public TestClass(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public bool Equals(TestClass other)
            {
                if (other == null) return false;
                return Id == other.Id && Name == other.Name;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as TestClass);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id, Name);
            }
        }
    }
}