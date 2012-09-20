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
using System.Collections.ObjectModel;
using System.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public class TestableUnderlyingInfoDescriptorBase<T> : UnderlyingInfoDescriptorBase<T>
    where T : class 
  {
    public static Func<ReadOnlyCollection<ICustomAttributeData>> GetEmptyProviderField ()
    {
      return EmptyCustomAttributeDataProvider;
    }

    public new static Func<ReadOnlyCollection<ICustomAttributeData>> GetCustomAttributeProvider (MemberInfo member)
    {
      return UnderlyingInfoDescriptorBase<T>.GetCustomAttributeProvider (member);
    }

    public new static Func<ReadOnlyCollection<ICustomAttributeData>> GetCustomAttributeProvider (ParameterInfo parameter)
    {
      return UnderlyingInfoDescriptorBase<T>.GetCustomAttributeProvider (parameter);
    }

    public TestableUnderlyingInfoDescriptorBase (
        T underlyingSystemMember, string name, Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider)
        : base (underlyingSystemMember, name, customAttributeDataProvider)
    {
    }
  }
}