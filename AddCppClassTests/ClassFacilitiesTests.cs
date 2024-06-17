using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dwarfovich.AddCppClass.Tests
{
    [TestClass()]
    public class ClassFacilitiesTests
    {
        [TestMethod()]
        public void GenerateSnakeCaseHeaderFilenameTestValidness()
        {
            ClassFacilities classGenerator = new AddCppClass.ClassFacilities();

            Settings classSettings = new AddCppClass.Settings(className: "", style: FilenameStyle.SnakeCase, headerExtension: ".h");
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
            ClassFacilities classGenerator = new AddCppClass.ClassFacilities();

            Settings classSettings = new AddCppClass.Settings(className: "", style: FilenameStyle.CamelCase, headerExtension: ".hpp");
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
            ClassFacilities classGenerator = new AddCppClass.ClassFacilities();

            Settings classSettings = new AddCppClass.Settings(className: "", style: FilenameStyle.LowerCase, headerExtension: ".hpp");
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
            Assert.IsTrue(ClassFacilities.IsValidNamespace("a1_"));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("_9p_"));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("_qwe::q"));
            Assert.IsTrue(ClassFacilities.IsValidNamespace("_q4_::_q::p091_::as_"));
        }

        [TestMethod()]
        public void IsInvalidNamespaceTest()
        {
            Assert.IsFalse(ClassFacilities.IsValidNamespace(" "));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("::"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace(":a"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("::a"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("a::"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("a:"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("::a1_::"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("::_:"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_9p_::a::"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_qwe:::q"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("::_qwe::q"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_::_q::p0 91_::as_"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_::::p0"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_:: ::p0"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_:p0"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace(":_q4_::p0"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_::p0:"));
            Assert.IsFalse(ClassFacilities.IsValidNamespace("_q4_:: :p0"));
        }

        [TestMethod()]
        public void IsValidSubfolderTest()
        {
            Assert.IsTrue(ClassFacilities.IsValidSubfolder(string.Empty));
            Assert.IsTrue(ClassFacilities.IsValidSubfolder("a"));
            Assert.IsTrue(ClassFacilities.IsValidSubfolder("a\\b"));
            Assert.IsTrue(ClassFacilities.IsValidSubfolder("a\\_3_\\t5"));
            Assert.IsTrue(ClassFacilities.IsValidSubfolder("a\\_3_\\t5kf\\za\\_1"));
        }

        [TestMethod()]
        public void IsInvalidSubfolderTest()
        {
            Assert.IsFalse(ClassFacilities.IsValidSubfolder(" "));
            Assert.IsFalse(ClassFacilities.IsValidSubfolder(" a"));
            Assert.IsFalse(ClassFacilities.IsValidSubfolder("a "));
            Assert.IsFalse(ClassFacilities.IsValidSubfolder("a a"));
            Assert.IsFalse(ClassFacilities.IsValidSubfolder(" "));
            Assert.IsFalse(ClassFacilities.IsValidSubfolder(" a/a"));
            Assert.IsFalse(ClassFacilities.IsValidSubfolder("a /fe"));
            Assert.IsFalse(ClassFacilities.IsValidSubfolder("a /a"));
        }

        [TestMethod()]
        public void ConformSubfolderTest()
        {
            Assert.AreEqual("", ClassFacilities.ConformSubfolder(String.Empty));
            Assert.AreEqual("", ClassFacilities.ConformSubfolder(""));
            Assert.AreEqual("a",ClassFacilities.ConformSubfolder("a"));
            Assert.AreEqual("a",ClassFacilities.ConformSubfolder("/a"));
            Assert.AreEqual("a",ClassFacilities.ConformSubfolder("a/"));
            Assert.AreEqual("a", ClassFacilities.ConformSubfolder("/a/"));
            Assert.AreEqual("a", ClassFacilities.ConformSubfolder("\\a"));
            Assert.AreEqual("a", ClassFacilities.ConformSubfolder("a\\"));
            Assert.AreEqual("a", ClassFacilities.ConformSubfolder("\\a\\"));
            Assert.AreEqual("a\\b",ClassFacilities.ConformSubfolder("/a/b/"));
            Assert.AreEqual("a\\b\\c",ClassFacilities.ConformSubfolder("/a/b/c/"));
            Assert.AreEqual("a\\b\\c", ClassFacilities.ConformSubfolder("\\a\\b\\c\\"));
        }

        [TestMethod()]
        public void IsValidPrecompiledHeaderPathTest()
        {
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath(""));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath("a"));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath(".a"));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath("q/.a"));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath("q\\.a"));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath("pch.hpp"));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath("a2/pch.hpp"));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath("//a"));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath("a2\\pch.hpp"));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath("\\\\a"));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath("\\a2\\pch.hpp"));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath("/a2\\pch.hpp"));
            Assert.IsTrue(ClassFacilities.IsValidPrecompiledHeaderPath("/_\\a/2\\_pch.h"));
        }

        [TestMethod()]
        public void IsInvalidPrecompiledHeaderPathTest()
        {
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("/"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("\\"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("a/"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("a//"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("a\\\\"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("/a/"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("\\a/"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("\\a\\"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("/a\\"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("a."));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("/a."));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("\\a."));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("\\a.\\"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("\\a./"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath(" "));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath(" a"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("a "));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("a a"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("a a/q"));
            Assert.IsFalse(ClassFacilities.IsValidPrecompiledHeaderPath("a a/q q"));
        }

        public void ConformPrecompiledHeaderPathTest()
        {
            Assert.AreEqual("", ClassFacilities.ConformPrecompiledHeaderPath(String.Empty));
            Assert.AreEqual("a", ClassFacilities.ConformPrecompiledHeaderPath("a"));
            Assert.AreEqual("a.h", ClassFacilities.ConformPrecompiledHeaderPath("a.h"));
            Assert.AreEqual("a", ClassFacilities.ConformPrecompiledHeaderPath("/a"));
            Assert.AreEqual("a", ClassFacilities.ConformPrecompiledHeaderPath("\\a"));
            Assert.AreEqual("a/a.h", ClassFacilities.ConformPrecompiledHeaderPath("\\a\\a.h"));
            Assert.AreEqual("_/_a/a.h", ClassFacilities.ConformPrecompiledHeaderPath("_\\_a/a.h"));
        }

        [TestMethod()]
        public void IsValidFileExtensionTest()
        {
            Assert.IsTrue(ClassFacilities.IsValidHeaderExtension(".a"));
            Assert.IsTrue(ClassFacilities.IsValidHeaderExtension(".ab3_"));
            Assert.IsTrue(ClassFacilities.IsValidHeaderExtension(".0ab3_Z"));
            Assert.IsTrue(ClassFacilities.IsValidHeaderExtension(".a.b"));
            Assert.IsTrue(ClassFacilities.IsValidHeaderExtension(".a.b_._d.4"));
        }

        [TestMethod()]
        public void IsInvalidFileExtensionTest()
        {
            Assert.IsFalse(ClassFacilities.IsValidHeaderExtension(string.Empty));
            Assert.IsFalse(ClassFacilities.IsValidHeaderExtension(" "));
            Assert.IsFalse(ClassFacilities.IsValidHeaderExtension("."));
            Assert.IsFalse(ClassFacilities.IsValidHeaderExtension(".."));
            Assert.IsFalse(ClassFacilities.IsValidHeaderExtension(".d."));
            Assert.IsFalse(ClassFacilities.IsValidHeaderExtension(".d.a."));
            Assert.IsFalse(ClassFacilities.IsValidHeaderExtension("d a"));
            Assert.IsFalse(ClassFacilities.IsValidHeaderExtension(".d a"));
            Assert.IsFalse(ClassFacilities.IsValidHeaderExtension(".d.a "));
            Assert.IsFalse(ClassFacilities.IsValidHeaderExtension(" .pch"));
        }
    }
}