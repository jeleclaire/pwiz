/*
 * Original author: Brendan MacLean <brendanx .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2009-2010 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Globalization;
using System.IO;
using Ionic.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace pwiz.SkylineTestUtil
{
    public static class ExtensionTestContext
    {
        public static string GetTestPath(this TestContext testContext, string relativePath)
        {
            return Path.Combine(testContext.TestDir, relativePath);
        }

        public static String GetProjectDirectory(this TestContext testContext, string relativePath)
        {
            for (String directory = testContext.TestDir; directory.Length > 10; directory = Path.GetDirectoryName(directory))
            {
                if (Equals(Path.GetFileName(directory), "TestResults"))
                    return Path.Combine(Path.GetDirectoryName(directory), relativePath);
            }
            return null;
        }

        public static void ExtractTestFiles(this TestContext testContext, string relativePathZip)
        {
            testContext.ExtractTestFiles(relativePathZip, testContext.TestDir);
        }

        public static void ExtractTestFiles(this TestContext testContext, string relativePathZip, string destDir)
        {
            string pathZip = testContext.GetProjectDirectory(relativePathZip);
            using (ZipFile zipFile = ZipFile.Read(pathZip))
            {
                foreach (ZipEntry zipEntry in zipFile)
                    zipEntry.Extract(destDir, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public static bool CanImportThermoRaw
        {
            get { return Equals(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator); }
        }

        public static string ExtThermoRaw
        {
            get { return CanImportThermoRaw ? ".RAW" : ".mzML"; }
        }
    }
}