﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast.Compiler;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  /// <summary>
  /// Acts as a placeholder for delegate <see cref="Type"/>s in the <see cref="LambdaCompiler"/> and must be replaced during code generation.
  /// </summary>
  public class DelegateTypePlaceholder : CustomType
  {
    private readonly Type _returnType;
    private readonly ReadOnlyCollection<Type> _parameterTypes;

    public DelegateTypePlaceholder (Type returnType, IEnumerable<Type> parameterTypes)
        : base (
            new MemberSelector (new BindingFlagsEvaluator()),
            "DelegateTypePlaceholder",
            null,
            TypeAttributes.Public | TypeAttributes.Sealed,
            null,
            EmptyTypes)
    {
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      _returnType = returnType;
      _parameterTypes = parameterTypes.ToList().AsReadOnly();

      SetBaseType (typeof (MulticastDelegate));
    }

    public Type ReturnType
    {
      get { return _returnType; }
    }

    public ReadOnlyCollection<Type> ParameterTypes
    {
      get { return _parameterTypes; }
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      throw new NotSupportedException ("Method GetCustomAttributeData is not supported.");
    }

    protected override IEnumerable<Type> GetAllInterfaces ()
    {
      throw new NotSupportedException ("Method GetAllInterfaces is not supported.");
    }

    protected override IEnumerable<FieldInfo> GetAllFields ()
    {
      throw new NotSupportedException ("Method GetAllFields is not supported.");
    }

    protected override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      throw new NotSupportedException ("Method GetAllConstructors is not supported.");
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      throw new NotSupportedException ("OIdaXXXXX");

      //var parameters = _parameterTypes.Take (_parameterTypes.Count - 1).Select (t => new ParameterDeclaration (t));
      //yield return new MethodOnCustomType (this, "Invoke", MethodAttributes.Public, EmptyTypes, _parameterTypes.Last(), parameters);
    }

    protected override IEnumerable<PropertyInfo> GetAllProperties ()
    {
      throw new NotSupportedException ("Method GetAllProperties is not supported.");
    }

    protected override IEnumerable<EventInfo> GetAllEvents ()
    {
      throw new NotSupportedException ("Method GetAllEvents is not supported.");
    }
  }
}