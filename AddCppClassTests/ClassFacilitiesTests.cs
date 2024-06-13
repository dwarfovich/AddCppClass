using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dwarfovich.AddCppClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dwarfovich.AddCppClass.Tests
{
    [TestClass()]
    public class ClassFacilitiesTests
    {
        [TestMethod()]
        public void GenerateSnakeCaseHeaderFilenameTestValidness()
        {
            AddCppClass.ClassFacilities classGenerator = new AddCppClass.ClassFacilities();

            AddCppClass.Settings classSettings = new AddCppClass.Settings(className: "", style: FilenameStyle.SnakeCase, headerExtension: ".h");
            Assert.AreEqual("", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "1";
            Assert.AreEqual("1", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "a";
            Assert.AreEqual("a", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "a1";
            Assert.AreEqual("a1", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "A";
            Assert.AreEqual("a", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "AB";
            Assert.AreEqual("ab", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "AB1";
            Assert.AreEqual("ab1", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "AB1DC";
            Assert.AreEqual("ab1_dc", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "Hello1";
            Assert.AreEqual("hello1", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "Hello1hello";
            Assert.AreEqual("hello1_hello", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MyClass";
            Assert.AreEqual("my_class", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MyClass2";
            Assert.AreEqual("my_class2", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MyClassAdditions";
            Assert.AreEqual("my_class_additions", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MClassAdditions";
            Assert.AreEqual("mclass_additions", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "M2classAdditions";
            Assert.AreEqual("m2_class_additions", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "M2class";
            Assert.AreEqual("m2_class", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "myClass";
            Assert.AreEqual("my_class", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "myClass1Additions";
            Assert.AreEqual("my_class1_additions", classGenerator.GenerateFilename(classSettings));
        }

        [TestMethod()]
        public void GenerateCamelCaseFilenameTestValidness()
        {
            AddCppClass.ClassFacilities classGenerator = new AddCppClass.ClassFacilities();

            AddCppClass.Settings classSettings = new AddCppClass.Settings(className: "", style: FilenameStyle.CamelCase, headerExtension: ".hpp");
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "1";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "a";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "a1";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "A";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "AB";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "AB1";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "AB1DC";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "Hello1";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "Hello1hello";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MyClass";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MyClass2";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MyClassAdditions";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MClassAdditions";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "M2classAdditions";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "M2class";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "myClass";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
            classSettings.className = "myClass1Additions";
            Assert.AreEqual(classSettings.className, classGenerator.GenerateFilename(classSettings));
        }

        [TestMethod()]
        public void GenerateLowerCaseFilenameTestValidness()
        {
            AddCppClass.ClassFacilities classGenerator = new AddCppClass.ClassFacilities();

            AddCppClass.Settings classSettings = new AddCppClass.Settings(className: "", style: FilenameStyle.LowerCase, headerExtension: ".hpp");
            Assert.AreEqual("", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "1";
            Assert.AreEqual("1", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "a";
            Assert.AreEqual("a", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "a1";
            Assert.AreEqual("a1", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "A";
            Assert.AreEqual("a", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "AB";
            Assert.AreEqual("ab", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "AB1";
            Assert.AreEqual("ab1", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "AB1DC";
            Assert.AreEqual("ab1dc", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "Hello1";
            Assert.AreEqual("hello1", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "Hello1hello";
            Assert.AreEqual("hello1hello", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MyClass";
            Assert.AreEqual("myclass", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MyClass2";
            Assert.AreEqual("myclass2", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MyClassAdditions";
            Assert.AreEqual("myclassadditions", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "MClassAdditions";
            Assert.AreEqual("mclassadditions", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "M2classAdditions";
            Assert.AreEqual("m2classadditions", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "M2class";
            Assert.AreEqual("m2class", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "myClass";
            Assert.AreEqual("myclass", classGenerator.GenerateFilename(classSettings));
            classSettings.className = "myClass1Additions";
            Assert.AreEqual("myclass1additions", classGenerator.GenerateFilename(classSettings));
        }

        [TestMethod()]
        public void IsValidСlassNameTest()
        {
            Assert.IsTrue(ClassFacilities.IsValidClassName("a"));
            Assert.IsTrue(ClassFacilities.IsValidClassName("SuperClass"));
            Assert.IsTrue(ClassFacilities.IsValidClassName("a1"));
            Assert.IsTrue(ClassFacilities.IsValidClassName("a1a"));
            Assert.IsTrue(ClassFacilities.IsValidClassName("a11a22"));
            Assert.IsTrue(ClassFacilities.IsValidClassName("ABRA3K122"));
            Assert.IsTrue(ClassFacilities.IsValidClassName("_123_"));
            Assert.IsTrue(ClassFacilities.IsValidClassName("__ABRA3K122"));
            Assert.IsTrue(ClassFacilities.IsValidClassName("_1_B_RA3K122__"));
        }

        [TestMethod()]
        public void IsInvalidСlassNameTest()
        {
            Assert.IsFalse(ClassFacilities.IsValidClassName(""));
            Assert.IsFalse(ClassFacilities.IsValidClassName(" "));
            Assert.IsFalse(ClassFacilities.IsValidClassName("1"));
            Assert.IsFalse(ClassFacilities.IsValidClassName("0abc"));
            Assert.IsFalse(ClassFacilities.IsValidClassName("a a"));
            Assert.IsFalse(ClassFacilities.IsValidClassName(" asbgt"));
            Assert.IsFalse(ClassFacilities.IsValidClassName(" 1"));
            Assert.IsFalse(ClassFacilities.IsValidClassName("_ _"));
            Assert.IsFalse(ClassFacilities.IsValidClassName("a "));
            Assert.IsFalse(ClassFacilities.IsValidClassName("abra746 "));
        }

        [TestMethod()]
        public void IsValidNamespaceTest()
        {
            Assert.IsTrue(ClassFacilities.IsValidNamespace(""));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("a"));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("::a"));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("a1_"));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("_9p_"));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("::_9p_"));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("_qwe::q"));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("::_qwe::q"));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("_q4_::_q::p091_::as_"));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("::_q4_::_q::p091_::as_"));
        }

        [TestMethod()]
        public void IsInvalidNamespaceTest()
        {
            Assert.IsFalse(ClassFacilities.IsValidNamespace(" "));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("::"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace(":a"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("a::"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("a:"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("::a1_::"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("::_:"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_9p_::a::"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_qwe:::q"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_::_q::p0 91_::as_"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_::::p0"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_:: ::p0"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_:p0"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace(":_q4_::p0"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_::p0:"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_:: :p0"));
        }
    }
}