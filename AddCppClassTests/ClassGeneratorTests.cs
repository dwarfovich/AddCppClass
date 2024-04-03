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
    public class ClassGeneratorTests
    {
        [TestMethod()]
        public void GenerateSnakeCaseHeaderFilenameTestValidness()
        {
            AddCppClass.ClassGenerator classGenerator = new AddCppClass.ClassGenerator();

            AddCppClass.ClassSettings classSettings = new AddCppClass.ClassSettings(className: "", style: FilenameStyle.SnakeCase, headerExtension: ".h");
            Assert.AreEqual("", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "1";
            Assert.AreEqual("1", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "a";
            Assert.AreEqual("a", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "a1";
            Assert.AreEqual("a1", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "A";
            Assert.AreEqual("a", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "AB";
            Assert.AreEqual("ab", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "AB1";
            Assert.AreEqual("ab1", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "AB1DC";
            Assert.AreEqual("ab1_dc", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "Hello1";
            Assert.AreEqual("hello1", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "Hello1hello";
            Assert.AreEqual("hello1_hello", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MyClass";
            Assert.AreEqual("my_class", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MyClass2";
            Assert.AreEqual("my_class2", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MyClassAdditions";
            Assert.AreEqual("my_class_additions", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MClassAdditions";
            Assert.AreEqual("mclass_additions", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "M2classAdditions";
            Assert.AreEqual("m2_class_additions", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "M2class";
            Assert.AreEqual("m2_class", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "myClass";
            Assert.AreEqual("my_class", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "myClass1Additions";
            Assert.AreEqual("my_class1_additions", classGenerator.GenerateFilename(classSettings));
        }

        [TestMethod()]
        public void GenerateCamelCaseFilenameTestValidness()
        {
            AddCppClass.ClassGenerator classGenerator = new AddCppClass.ClassGenerator();

            AddCppClass.ClassSettings classSettings = new AddCppClass.ClassSettings(className: "", style: FilenameStyle.CamelCase, headerExtension: ".hpp");
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "1";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "a";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "a1";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "A";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "AB";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "AB1";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "AB1DC";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "Hello1";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "Hello1hello";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MyClass";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MyClass2";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MyClassAdditions";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MClassAdditions";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "M2classAdditions";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "M2class";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "myClass";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "myClass1Additions";
            Assert.AreEqual(classSettings.ClassName, classGenerator.GenerateFilename(classSettings));
        }

        [TestMethod()]
        public void GenerateLowerCaseFilenameTestValidness()
        {
            AddCppClass.ClassGenerator classGenerator = new AddCppClass.ClassGenerator();

            AddCppClass.ClassSettings classSettings = new AddCppClass.ClassSettings(className: "", style: FilenameStyle.LowerCase, headerExtension: ".hpp");
            Assert.AreEqual("", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "1";
            Assert.AreEqual("1", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "a";
            Assert.AreEqual("a", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "a1";
            Assert.AreEqual("a1", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "A";
            Assert.AreEqual("a", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "AB";
            Assert.AreEqual("ab", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "AB1";
            Assert.AreEqual("ab1", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "AB1DC";
            Assert.AreEqual("ab1dc", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "Hello1";
            Assert.AreEqual("hello1", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "Hello1hello";
            Assert.AreEqual("hello1hello", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MyClass";
            Assert.AreEqual("myclass", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MyClass2";
            Assert.AreEqual("myclass2", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MyClassAdditions";
            Assert.AreEqual("myclassadditions", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "MClassAdditions";
            Assert.AreEqual("mclassadditions", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "M2classAdditions";
            Assert.AreEqual("m2classadditions", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "M2class";
            Assert.AreEqual("m2class", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "myClass";
            Assert.AreEqual("myclass", classGenerator.GenerateFilename(classSettings));
            classSettings.ClassName = "myClass1Additions";
            Assert.AreEqual("myclass1additions", classGenerator.GenerateFilename(classSettings));
        }

        [TestMethod()]
        public void IsValidClassNameTest()
        {
            Assert.IsTrue(ClassGenerator.IsValidClassName("a"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("SuperClass"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("a1"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("a1a"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("a11a22"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("ABRA3K122"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("_123_"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("_ABRA3K122"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("_1_B_RA3K122__"));
        }

        [TestMethod()]
        public void IsValidFullyQualifiedClassNameTest()
        {
            Assert.IsTrue(ClassGenerator.IsValidClassName("::a"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("::Super::Class"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("a::a"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("::a::a"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("::a::a::a::a"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("a1a::qwerty::_12_::a123_"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("_::a::p11::a22::_a"));
            Assert.IsTrue(ClassGenerator.IsValidClassName("::_::a::p11::a22::_a"));
        }

        [TestMethod()]
        public void IsInvalidClassNameTest()
        {
            Assert.IsFalse(ClassGenerator.IsValidClassName(""));
            Assert.IsFalse(ClassGenerator.IsValidClassName(" "));
            Assert.IsFalse(ClassGenerator.IsValidClassName("1"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("0abc"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("a a"));
            Assert.IsFalse(ClassGenerator.IsValidClassName(" asbgt"));
            Assert.IsFalse(ClassGenerator.IsValidClassName(" 1"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("_ _"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("a "));
            Assert.IsFalse(ClassGenerator.IsValidClassName("abra746 "));
        }

        [TestMethod()]
        public void IsInvalidFullyWualifiedClassNameTest()
        {
            Assert.IsFalse(ClassGenerator.IsValidClassName("::"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("a::"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("1::"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("_::"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("_::_::"));
            Assert.IsFalse(ClassGenerator.IsValidClassName(":::a"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("::::a"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("abra:::tabra"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("abra::746 "));
            Assert.IsFalse(ClassGenerator.IsValidClassName("abra::a ::a"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("1abra::a::a"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("abra::1a::a"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("abra::a::1a"));
            Assert.IsFalse(ClassGenerator.IsValidClassName(":a"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("::a:a"));
            Assert.IsFalse(ClassGenerator.IsValidClassName("::a::a:"));
        }
    }
}