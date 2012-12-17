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
using Remotion.ServiceLocation;
using Remotion.TypePipe.Serialization.Implementation;

namespace Remotion.TypePipe.Serialization
{
  /// <summary>
  /// Allows the registration of an <see cref="IObjectFactory"/> under a given factory identifier. The object factory is used during the .NET 
  /// deserialization process of objects which were created from another factory with the same identifier. Therefore the two object
  /// factory instances should use the same participants and participant configurations, in order to generate equivalent types for the requested
  /// type. This allows the serialization of object instances without saving the generated assemblies to disk.
  /// </summary>
  /// 
  [ConcreteImplementation (typeof (ObjectFactoryRegistry), Lifetime = LifetimeKind.Singleton)]
  public interface IObjectFactoryRegistry
  {
    /// <summary>
    /// Registers an <see cref="IObjectFactory"/> under the specified identifier.
    /// </summary>
    /// <exception cref="InvalidOperationException">If a factory is already registered under the specified identifier.</exception>
    /// <param name="factoryIdentifier">The factory identifier.</param>
    /// <param name="objectFactory">The object factory to register.</param>
    void Register (string factoryIdentifier, IObjectFactory objectFactory);

    /// <summary>
    /// Unregisters the <see cref="IObjectFactory"/> instance that is currently registered under the specified identifier.
    /// No exception is thrown if no factory is registered under the given identifier.
    /// </summary>
    /// <param name="factoryIdentifier">The factory identifier.</param>
    void Unregister (string factoryIdentifier);

    /// <summary>
    /// Retrieves the <see cref="IObjectFactory"/> instance that is registered under the specified identifier.
    /// </summary>
    /// <exception cref="InvalidOperationException">If no factory is registered under the specified identifier.</exception>
    /// <param name="factoryIdentifier">The factory identifier.</param>
    /// <returns>The registered object factory.</returns>
    IObjectFactory Get (string factoryIdentifier);
  }
}