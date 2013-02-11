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
using System.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  public class EventOnTypeInstantiation
      : CustomEventInfo
  {
    public EventOnTypeInstantiation (
        TypeInstantiation declaringType,
        EventInfo event_,
        MethodOnTypeInstantiation addMethod,
        MethodOnTypeInstantiation removeMethod,
        MethodOnTypeInstantiation raiseMethod)
        : base (declaringType, event_.Name, event_.Attributes, addMethod, removeMethod, raiseMethod) {}

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      throw new NotImplementedException();
    }
  }
}