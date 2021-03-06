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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Development.RhinoMocks.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Generics;
using Remotion.TypePipe.StrongNaming;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class StrongNameCheckingEmittableOperandProviderDecoratorTest
  {
    private IEmittableOperandProvider _innerMock;
    private ITypeAnalyzer _typeAnalyzerMock;

    private StrongNameCheckingEmittableOperandProviderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _typeAnalyzerMock = MockRepository.GenerateStrictMock<ITypeAnalyzer>();
      _innerMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();

      _decorator = new StrongNameCheckingEmittableOperandProviderDecorator (_innerMock, _typeAnalyzerMock);
    }

    [Test]
    public void GetEmittableType ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var emittableType = ReflectionObjectMother.GetSomeType();

      CheckGetEmittable ((p, t) => p.GetEmittableType (t), type, emittableType, emittableType);
    }

    [Test]
    public void GetEmittableField ()
    {
      var field = ReflectionObjectMother.GetSomeField();
      var emittableField = ReflectionObjectMother.GetSomeField();

      CheckGetEmittable ((p, f) => p.GetEmittableField (f), field, emittableField, emittableField.DeclaringType);
    }

    [Test]
    public void GetEmittableConstructor ()
    {
      var constructor = ReflectionObjectMother.GetSomeConstructor();
      var emittableConstructor = ReflectionObjectMother.GetSomeConstructor();

      CheckGetEmittable ((p, f) => p.GetEmittableConstructor (f), constructor, emittableConstructor, emittableConstructor.DeclaringType);
    }

    [Test]
    public void GetEmittableMethod ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      var emittableMethod = ReflectionObjectMother.GetSomeNonGenericMethod();

      CheckGetEmittable ((p, f) => p.GetEmittableMethod (f), method, emittableMethod, emittableMethod.DeclaringType);
    }

    [Test]
    public void GetEmittableMethod_GenericArguments ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      var emittableMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => GenericMethod<int>());
      _innerMock.Stub (stub => stub.GetEmittableMethod (method)).Return (emittableMethod);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (emittableMethod.DeclaringType)).Return (true);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (int))).Return (true);

      var result = _decorator.GetEmittableMethod (method);

      _typeAnalyzerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (emittableMethod));
    }

    [Test]
    public void GetEmittableMethod_MethodBuilder ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      var emittableMethod = ReflectionEmitObjectMother.CreateMethodBuilder();
      Assert.That (emittableMethod.GetGenericArguments(), Is.Null);
      _innerMock.Stub (stub => stub.GetEmittableMethod (method)).Return (emittableMethod);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (emittableMethod.DeclaringType)).Return (true);

      Assert.That (() => _decorator.GetEmittableMethod (method), Throws.Nothing);
    }

    [Test]
    public void DelegatingMembers ()
    {
      var mappedType = MutableTypeObjectMother.Create();
      var mappedGenericParameter = MutableGenericParameterObjectMother.Create();
      var mappedField = MutableFieldInfoObjectMother.Create();
      var mappedConstructor = MutableConstructorInfoObjectMother.Create();
      var mappedMethod = MutableMethodInfoObjectMother.Create();

      var emittableType = ReflectionObjectMother.GetSomeType();
      var emittableGenericParameter = ReflectionObjectMother.GetSomeGenericParameter();
      var emittableField = ReflectionObjectMother.GetSomeField();
      var emittableConstructor = ReflectionObjectMother.GetSomeConstructor();
      var emittableMethod = ReflectionObjectMother.GetSomeMethod();

      var helper = new DecoratorTestHelper<IEmittableOperandProvider> (_decorator, _innerMock);

      helper.CheckDelegation (d => d.AddMapping (mappedType, emittableType));
      helper.CheckDelegation (d => d.AddMapping (mappedGenericParameter, emittableGenericParameter));
      helper.CheckDelegation (d => d.AddMapping (mappedField, emittableField));
      helper.CheckDelegation (d => d.AddMapping (mappedConstructor, emittableConstructor));
      helper.CheckDelegation (d => d.AddMapping (mappedMethod, emittableMethod));
    }

    private void CheckGetEmittable<T> (Func<IEmittableOperandProvider, T, T> getEmittableOperandFunc, T operand, T emittableOperand, Type checkedType)
    {
      _innerMock.Expect (mock => getEmittableOperandFunc (mock, operand)).Return (emittableOperand);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (checkedType)).Return (true);

      var result = getEmittableOperandFunc (_decorator, operand);

      _innerMock.VerifyAllExpectations();
      _typeAnalyzerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (emittableOperand));

      _innerMock.BackToRecord();
      _typeAnalyzerMock.BackToRecord();
      _innerMock.Expect (mock => getEmittableOperandFunc (mock, operand)).Return (emittableOperand);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (checkedType)).Return (false);
      _innerMock.Replay();
      _typeAnalyzerMock.Replay();

      var message = "Strong-naming is enabled but a participant used the type '" + checkedType.FullName + "' which comes from the unsigned assembly '"
                    + checkedType.Assembly.GetName().Name + "'.";
      Assert.That (() => getEmittableOperandFunc (_decorator, operand), Throws.InvalidOperationException.With.Message.EqualTo (message));
      _innerMock.VerifyAllExpectations();
      _typeAnalyzerMock.VerifyAllExpectations();
    }

    void GenericMethod<[UsedImplicitly] T> () { }
  }
}