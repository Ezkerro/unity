﻿//===============================================================================
// Microsoft patterns & practices
// Unity Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity.TestSupport;

namespace Microsoft.Practices.Unity.Tests
{
    /// <summary>
    /// Tests for issues reported on Codeplex
    /// </summary>
    [TestClass]
    public class CodeplexIssuesFixture
    {
        // http://www.codeplex.com/unity/WorkItem/View.aspx?WorkItemId=1307
        [TestMethod]
        public void InjectionConstructorWorksIfItIsFirstConstructor()
        {
            UnityContainer container = new UnityContainer();
            container.RegisterType<IBasicInterface, ClassWithDoubleConstructor>();
            IBasicInterface result = container.Resolve<IBasicInterface>();
        }

        // https://www.codeplex.com/Thread/View.aspx?ProjectName=unity&ThreadId=25301
        [TestMethod]
        public void CanUseNonDefaultLifetimeManagerWithOpenGenericRegistration()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterType(typeof(IFoo<>),
                typeof(MyFoo<>),
                new ContainerControlledLifetimeManager());
            IFoo<int> intFoo = container.Resolve<IFoo<int>>();
            IFoo<string> stringFoo1 = container.Resolve<IFoo<string>>();
            IFoo<string> stringFoo2 = container.Resolve<IFoo<string>>();

            Assert.AreSame(stringFoo1, stringFoo2);
        }

        // https://www.codeplex.com/Thread/View.aspx?ProjectName=unity&ThreadId=25301
        [TestMethod]
        public void CanOverrideGenericLifetimeManagerWithSpecificOne()
        {
            IUnityContainer container = new UnityContainer()
                .RegisterType(typeof(IFoo<>),
                    typeof(MyFoo<>),
                    new ContainerControlledLifetimeManager())
                .RegisterType(typeof(MyFoo<double>), new TransientLifetimeManager());

            IFoo<string> string1 = container.Resolve<IFoo<string>>();
            IFoo<string> string2 = container.Resolve<IFoo<string>>();

            IFoo<double> double1 = container.Resolve<IFoo<double>>();
            IFoo<double> double2 = container.Resolve<IFoo<double>>();

            Assert.AreSame(string1, string2);
            Assert.AreNotSame(double1, double2);
        }

        // https://www.codeplex.com/Thread/View.aspx?ProjectName=unity&ThreadId=26318
        [TestMethod]
        public void RegisteringInstanceInChildOverridesRegisterTypeInParent()
        {
            IUnityContainer container = new UnityContainer()
                .RegisterType<IBasicInterface, ClassWithDoubleConstructor>(new ContainerControlledLifetimeManager());

            IUnityContainer child = container.CreateChildContainer()
                .RegisterInstance<IBasicInterface>(new MockBasic());

            IBasicInterface result = child.Resolve<IBasicInterface>();

            Assert.IsInstanceOfType(result, typeof(MockBasic));
        }

        // http://www.codeplex.com/unity/Thread/View.aspx?ThreadId=30292
        [TestMethod]
        public void CanConfigureGenericDictionaryForInjectionUsingRegisterType()
        {
            IUnityContainer container = new UnityContainer()
                .RegisterType(typeof(IDictionary<,>), typeof(Dictionary<,>),
                    new InjectionConstructor());

            IDictionary<string, string> result = container.Resolve<IDictionary<string, string>>();
        }

        // http://unity.codeplex.com/WorkItem/View.aspx?WorkItemId=6431
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AccessViolationExceptionOnx64()
        {
            var container1 = new UnityContainer();
            container1.RegisterType<InnerX64Class>();
            // FooProperty is static, this should throw here
            container1.RegisterType<OuterX64Class>(new InjectionProperty("FooProperty"));
        }

        // http://unity.codeplex.com/WorkItem/View.aspx?WorkItemId=6491
        [TestMethod]
        public void CanResolveTimespan()
        {
            var container = new UnityContainer()
                .RegisterType<TimeSpan>(new ExternallyControlledLifetimeManager(),
                new InjectionConstructor(0L));
            var expected = new TimeSpan();
            var result = container.Resolve<TimeSpan>();

            Assert.AreEqual(expected, result);
        }

        // http://unity.codeplex.com/WorkItem/View.aspx?WorkItemId=6053
        [TestMethod]
        public void ResolveAllWithChildDoesNotRepeatOverriddenRegistrations()
        {
            var parent = new UnityContainer()
                .RegisterInstance("str1", "string1")
                .RegisterInstance("str2", "string2");

            var child = parent.CreateChildContainer()
                .RegisterInstance("str2", "string20")
                .RegisterInstance("str3", "string30");

            var result = child.ResolveAll<string>();

            result.AssertContainsInAnyOrder("string1", "string20", "string30");
        }

        public interface IBasicInterface
        { 
        }

        public class ClassWithDoubleConstructor : IBasicInterface
        {
            private string myString = "";

            [InjectionConstructor]
            public ClassWithDoubleConstructor()
                : this(string.Empty)
            {
            }

            public ClassWithDoubleConstructor(string myString)
            {
                this.myString = myString;
            }
        }

        public interface IFoo<T>
        {
            
        }

        public class MyFoo<T> : IFoo<T>
        {
            
        }

        public class MockBasic : IBasicInterface
        {
            
        }

        public class InnerX64Class
        {
            
        }

        public class OuterX64Class
        {
            public static InnerX64Class FooProperty { get; set; }
        }
    }
}