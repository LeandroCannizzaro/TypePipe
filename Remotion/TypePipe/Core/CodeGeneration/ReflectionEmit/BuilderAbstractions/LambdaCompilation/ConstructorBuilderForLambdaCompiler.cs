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
using System.Reflection;
using System.Reflection.Emit;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions.LambdaCompilation
{
  /// <summary>
  /// Adapts <see cref="ConstructorBuilder"/> for the <see cref="IMethodBuilderForLambdaCompiler"/> interface.
  /// </summary>
  /// <remarks>
  /// This class is internal because it should only be used from <see cref="TypeModifier"/> and <see cref="TypeModificationHandler"/>.
  /// </remarks>
  internal class ConstructorBuilderForLambdaCompiler : IMethodBuilderForLambdaCompiler
  {
    private readonly ConstructorBuilder _constructorBuilder;

    public ConstructorBuilderForLambdaCompiler (ConstructorBuilder constructorBuilder)
    {
      ArgumentUtility.CheckNotNull ("constructorBuilder", constructorBuilder);

      _constructorBuilder = constructorBuilder;
    }

    public Type DeclaringType
    {
      get { return _constructorBuilder.DeclaringType; }
    }

    public void SetReturnType (Type returnType)
    {
      Assertion.IsTrue (returnType == _constructorBuilder.ReturnType);
    }

    public void SetParameters (Type[] parameterType)
    {
      // Ignore because parameters should have been correctly set prior to this call.
      // We cannot assert correctness because _constructorBuilder.GetParameters() throws.
    }

    public ParameterBuilder DefineParameter (int position, ParameterAttributes parameterAttributes, string parameterName)
    {
      return _constructorBuilder.DefineParameter (position, parameterAttributes, parameterName);
    }

    public ILGenerator GetILGenerator ()
    {
      return _constructorBuilder.GetILGenerator();
    }

    public MethodBase AsMethodBase ()
    {
      return _constructorBuilder;
    }
  }
}