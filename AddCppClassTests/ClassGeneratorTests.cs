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
            AddCppClass.ClassGenerator classGenerator = new AddCppClass.ClassGenerator();

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
            AddCppClass.ClassGenerator classGenerator = new AddCppClass.ClassGenerator();

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
        public void IsValidclassNameTest()
        {
            Assert.IsTrue(ClassGenerator.IsValidclassName("a"));
            Assert.IsTrue(ClassGenerator.IsValidclassName("SuperClass"));
            Assert.IsTrue(ClassGenerator.IsValidclassName("a1"));
            Assert.IsTrue(ClassGenerator.IsValidclassName("a1a"));
            Assert.IsTrue(ClassGenerator.IsValidclassName("a11a22"));
            Assert.IsTrue(ClassGenerator.IsValidclassName("ABRA3K122"));
            Assert.IsTrue(ClassGenerator.IsValidclassName("_123_"));
            Assert.IsTrue(ClassGenerator.IsValidclassName("_ABRA3K122"));
            Assert.IsTrue(ClassGenerator.IsValidclassName("_1_B_RA3K122__"));
        }

        [TestMethod()]
        public void IsInvalidclassNameTest()
        {
            Assert.IsFalse(ClassGenerator.IsValidclassName(""));
            Assert.IsFalse(ClassGenerator.IsValidclassName(" "));
            Assert.IsFalse(ClassGenerator.IsValidclassName("1"));
            Assert.IsFalse(ClassGenerator.IsValidclassName("0abc"));
            Assert.IsFalse(ClassGenerator.IsValidclassName("a a"));
            Assert.IsFalse(ClassGenerator.IsValidclassName(" asbgt"));
            Assert.IsFalse(ClassGenerator.IsValidclassName(" 1"));
            Assert.IsFalse(ClassGenerator.IsValidclassName("_ _"));
            Assert.IsFalse(ClassGenerator.IsValidclassName("a "));
            Assert.IsFalse(ClassGenerator.IsValidclassName("abra746 "));
        }
    }
}