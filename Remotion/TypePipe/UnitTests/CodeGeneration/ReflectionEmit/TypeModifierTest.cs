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
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModifierTest
  {
    private IModuleBuilder _moduleBuilderMock;
    private ISubclassProxyNameProvider _subclassProxyNameProviderStub;
    private DebugInfoGenerator _debugInfoGeneratorStub;

    private TypeModifier _typeModifier;

    [SetUp]
    public void SetUp ()
    {
      _moduleBuilderMock = MockRepository.GenerateStrictMock<IModuleBuilder> ();
      _subclassProxyNameProviderStub = MockRepository.GenerateStub<ISubclassProxyNameProvider>();
      _debugInfoGeneratorStub = MockRepository.GenerateStub<DebugInfoGenerator>();

      _typeModifier = new TypeModifier (_moduleBuilderMock, _subclassProxyNameProviderStub, _debugInfoGeneratorStub);
    }

    [Test]
    public void Initialization_NullDebugInfoGenerator ()
    {
      var typeModifier = new TypeModifier (_moduleBuilderMock, _subclassProxyNameProviderStub, null);
      Assert.That (typeModifier.DebugInfoGenerator, Is.Null);
    }

    [Test]
    public void ApplyModifications ()
    {
      var fakeUnderlyingSystemType = ReflectionObjectMother.GetSomeType ();
      var underlyingStrategyStub = MockRepository.GenerateStub<IUnderlyingTypeStrategy> ();
      underlyingStrategyStub.Stub (stub => stub.GetConstructors (Arg<BindingFlags>.Is.Anything)).Return (new ConstructorInfo[0]);
      underlyingStrategyStub.Stub (stub => stub.GetUnderlyingSystemType()).Return (fakeUnderlyingSystemType);
      
      var mutableTypeMock = MutableTypeObjectMother.CreatePartialMock (underlyingTypeStrategy: underlyingStrategyStub);
      
      var typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder> ();
      var fakeResultType = ReflectionObjectMother.GetSomeType ();
      bool acceptCalled = false;

      _subclassProxyNameProviderStub.Stub (stub => stub.GetSubclassProxyName (mutableTypeMock)).Return ("foofoo");
      _moduleBuilderMock
          .Expect (mock => mock.DefineType ("foofoo", TypeAttributes.Public | TypeAttributes.BeforeFieldInit, fakeUnderlyingSystemType))
          .Return (typeBuilderMock);
      mutableTypeMock
          .Expect (mock => mock.Accept (Arg<ITypeModificationHandler>.Is.Anything))
          .WhenCalled (mi =>
          {
            acceptCalled = true;
            Assert.That (mi.Arguments[0], Is.TypeOf<TypeModificationHandler>());
            var handler = (TypeModificationHandler) mi.Arguments[0];
            Assert.That (handler.SubclassProxyBuilder, Is.SameAs (typeBuilderMock));
            Assert.That (handler.ExpressionPreparer, Is.TypeOf<ExpandingExpressionPreparer> ());
            Assert.That (handler.ReflectionToBuilderMap.GetBuilder (mutableTypeMock), Is.SameAs (typeBuilderMock));
            Assert.That (handler.ILGeneratorFactory, Is.TypeOf <ILGeneratorDecoratorFactory>());
            var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) handler.ILGeneratorFactory;
            Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.TypeOf<OffsetTrackingILGeneratorFactory>());
            Assert.That (handler.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorStub));
          });
      typeBuilderMock
          .Expect (mock => mock.CreateType ()).Return (fakeResultType)
          .WhenCalled (mi => Assert.That (acceptCalled, Is.True));

      var result = _typeModifier.ApplyModifications (mutableTypeMock);

      _moduleBuilderMock.VerifyAllExpectations ();
      typeBuilderMock.VerifyAllExpectations ();
      mutableTypeMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResultType));
    }

    [Test]
    public void ApplyModifications_ClonesConstructorsReturnedByMutableType ()
    {
      var typeBuilderMock = MockRepository.GenerateMock<ITypeBuilder> ();
      var constructorBuilderMock1 = MockRepository.GenerateStrictMock<IConstructorBuilder> ();
      var constructorBuilderMock2 = MockRepository.GenerateStrictMock<IConstructorBuilder> ();

      var constructor1 = ReflectionObjectMother.GetConstructor (() => new ClassWithConstructors ("string"));
      var constructor2 = ReflectionObjectMother.GetConstructor (() => new ClassWithConstructors ());

      var mutableType = CreateMutableTypeWithConstructors (constructor1, constructor2);

      _moduleBuilderMock
          .Stub (mock => mock.DefineType (Arg<string>.Is.Anything, Arg<TypeAttributes>.Is.Anything, Arg<Type>.Is.Anything))
          .Return (typeBuilderMock);

      SetupCtorExpectations (mutableType, constructor1, typeBuilderMock, constructorBuilderMock1, MethodAttributes.Public);
      SetupCtorExpectations (mutableType, constructor2, typeBuilderMock, constructorBuilderMock2, MethodAttributes.Assembly);

      _typeModifier.ApplyModifications (mutableType);

      typeBuilderMock.VerifyAllExpectations ();
      constructorBuilderMock1.VerifyAllExpectations();
      constructorBuilderMock2.VerifyAllExpectations ();
    }

    [Test]
    public void ApplyModifications_ClonesConstructors_ChangesVisibilityFromProtectedOrInternalToProtected ()
    {
      var typeBuilderMock = MockRepository.GenerateMock<ITypeBuilder> ();
      var constructorBuilderMock = MockRepository.GenerateStrictMock<IConstructorBuilder> ();

      var constructor = ReflectionObjectMother.GetConstructor (() => new ClassWithConstructors (7));
      var mutableType = CreateMutableTypeWithConstructors (constructor);

      _moduleBuilderMock
          .Stub (mock => mock.DefineType (Arg<string>.Is.Anything, Arg<TypeAttributes>.Is.Anything, Arg<Type>.Is.Anything))
          .Return (typeBuilderMock);

      SetupCtorExpectations (mutableType, constructor, typeBuilderMock, constructorBuilderMock, MethodAttributes.Family);

      _typeModifier.ApplyModifications (mutableType);

      typeBuilderMock.VerifyAllExpectations ();
      constructorBuilderMock.VerifyAllExpectations ();
    }

    private void SetupCtorExpectations (
        MutableType mutableType,
        ConstructorInfo clonedCtor,
        ITypeBuilder typeBuilderMock,
        IConstructorBuilder constructorBuilderMock,
        MethodAttributes expectedCtorVisibility)
    {
      var expectedCtorAttributes = clonedCtor.Attributes & ~MethodAttributes.MemberAccessMask | expectedCtorVisibility;

      var clonedCtorParameterInfos = clonedCtor.GetParameters();
      var clonedCtorParameterTypes = clonedCtorParameterInfos.Select (pi => pi.ParameterType).ToArray();
      typeBuilderMock
          .Expect (mock => mock.DefineConstructor (expectedCtorAttributes, CallingConventions.HasThis, clonedCtorParameterTypes))
          .Return (constructorBuilderMock);
      foreach (var clonedCtorParameterInfo in clonedCtorParameterInfos)
      {
        ParameterInfo parameterInfo = clonedCtorParameterInfo;
        constructorBuilderMock.Expect (mock => mock.DefineParameter (parameterInfo.Position + 1, parameterInfo.Attributes, parameterInfo.Name));
      }
      constructorBuilderMock
          .Expect (
              mock =>
              mock.SetBody (
                  Arg<LambdaExpression>.Is.Anything,
                  Arg<ILGeneratorDecoratorFactory>.Matches (p => p.InnerFactory is OffsetTrackingILGeneratorFactory),
                  Arg.Is (_debugInfoGeneratorStub)))
          .WhenCalled (mi =>
          {
            CheckBaseCtorCallExpression ((LambdaExpression) mi.Arguments[0], clonedCtor, mutableType);
            var mutableReflectionObjectMap = ((ILGeneratorDecoratorFactory) mi.Arguments[1]).ReflectionToBuilderMap;
            Assert.That (mutableReflectionObjectMap.GetBuilder (mutableType), Is.Not.Null);
            Assert.That (mutableReflectionObjectMap.GetBuilder (mutableType.GetMutableConstructor (clonedCtor)), Is.SameAs (constructorBuilderMock));
          });
    }

    private void CheckBaseCtorCallExpression (LambdaExpression lambdaExpression, ConstructorInfo baseConstructor, Type expectedThisType)
    {
      var expectedParameterData = baseConstructor.GetParameters().Select (pi => new { pi.ParameterType, pi.Name });
      var actualParameterData = lambdaExpression.Parameters.Select (pe => new { ParameterType = pe.Type, pe.Name });
      Assert.That (expectedParameterData, Is.EqualTo (actualParameterData));

      Assert.That (lambdaExpression.Body, Is.AssignableTo<MethodCallExpression> ());
      Assert.That (lambdaExpression.Body.Type, Is.SameAs (typeof (void)));
      var methodCallExpression = (MethodCallExpression) lambdaExpression.Body;
      Assert.That (methodCallExpression.Method, Is.TypeOf<ConstructorAsMethodInfoAdapter>().With.Property ("ConstructorInfo").EqualTo (baseConstructor));
      Assert.That (methodCallExpression.Object, Is.TypeOf<TypeAsUnderlyingSystemTypeExpression>());
      var typeAsUnderlyingSystemTypeExpression = (TypeAsUnderlyingSystemTypeExpression) methodCallExpression.Object;
      Assert.That (typeAsUnderlyingSystemTypeExpression.InnerExpression, Is.TypeOf<ThisExpression>().With.Property ("Type").SameAs (expectedThisType));
      Assert.That (methodCallExpression.Arguments, Is.EqualTo (lambdaExpression.Parameters));
    }

    private MutableType CreateMutableTypeWithConstructors (params ConstructorInfo[] constructors)
    {
      var underlyingTypeStrategy = MockRepository.GenerateStub<IUnderlyingTypeStrategy> ();
      underlyingTypeStrategy.Stub (stub => stub.GetConstructors (Arg<BindingFlags>.Is.Anything)).Return (constructors);
      underlyingTypeStrategy.Stub (stub => stub.GetUnderlyingSystemType ()).Return (typeof (ClassWithConstructors));

      return MutableTypeObjectMother.Create (underlyingTypeStrategy: underlyingTypeStrategy);
    }


    public class ClassWithConstructors
    {
      public ClassWithConstructors (string s)
      {
        Dev.Null = s;
      }

      internal ClassWithConstructors ()
      {
      }

      // Family OR Assembly
      protected internal ClassWithConstructors (int i)
      {
        Dev.Null = i;
      }
    }
  }
}