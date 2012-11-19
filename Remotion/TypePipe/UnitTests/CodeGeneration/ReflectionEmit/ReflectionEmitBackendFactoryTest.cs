// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.IO;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ReflectionEmitBackendFactoryTest
  {
    private const string c_assemblyName = "TestAssembly";
    private const string c_assemblyFileName = c_assemblyName + ".dll";
    private const string c_pdbFileName = c_assemblyName + ".dll";

    [TearDown]
    public void TearDown ()
    {
      var directory = Path.GetTempPath();
      FileUtility.DeleteAndWaitForCompletion (Path.Combine (directory, c_assemblyFileName));
      FileUtility.DeleteAndWaitForCompletion (Path.Combine (directory, c_pdbFileName));
    }

    [Test]
    public void CreateModuleBuilder ()
    {
      var directory = Path.GetTempPath();
      var tuple = ReflectionEmitBackendFactory.CreateModuleBuilder (c_assemblyName, directory);

      var moduleBuilder = tuple.Item1;
      var assemblyBuilder = tuple.Item2;

      Assert.That (assemblyBuilder.GetName().Name, Is.EqualTo (c_assemblyName));

      Assert.That (moduleBuilder, Is.TypeOf<UniqueNamingModuleBuilderDecorator>());
      var uniqueNamingModuleBuilderDecorator = (UniqueNamingModuleBuilderDecorator) moduleBuilder;
      Assert.That (uniqueNamingModuleBuilderDecorator.InnerModuleBuilder, Is.TypeOf<ModuleBuilderAdapter>());
      var moduleBuilderAdapter = (ModuleBuilderAdapter) uniqueNamingModuleBuilderDecorator.InnerModuleBuilder;
      Assert.That (moduleBuilderAdapter.ScopeName, Is.EqualTo (c_assemblyFileName));

      var assemblyPath = Path.Combine (directory, c_assemblyFileName);
      var pdbPath = Path.Combine (directory, c_pdbFileName);
      Assert.That (File.Exists (assemblyPath), Is.False);
      Assert.That (File.Exists (pdbPath), Is.False);

      assemblyBuilder.Save (c_assemblyFileName);

      Assert.That (File.Exists (assemblyPath), Is.True);
      Assert.That (File.Exists (pdbPath), Is.True);
    }
  }
}