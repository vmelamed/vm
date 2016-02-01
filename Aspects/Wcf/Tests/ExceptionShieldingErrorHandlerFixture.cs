//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Exception Handling Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using System.Web.Services.Protocols;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.WCF;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.Behaviors;
using ExceptionShieldingErrorHandler = vm.Aspects.Wcf.Behaviors.ExceptionShieldingErrorHandler;

namespace vm.Aspects.Wcf.Tests
{
    [TestClass]
    public class ExceptionShieldingErrorHandlerFixture
    {
        [TestInitialize]
        public void Initialize()
        {
            DIContainer
                .Initialize()
                .Register(Facility.Registrar)
                ;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // DO NOT FORGET TO DEFINE THE CONDITIONAL SYMBOL TEST!
            Facility.Registrar.Reset();
            DIContainer.Reset();
        }

        class NoContext : IWcfContextUtilities
        {
            public bool HasOperationContext => false;

            public bool HasWebOperationContext => false;

            public string GetFaultedAction(Type faultContractType) => null;

            public string GetOperationAction() => null;

            public MethodInfo GetOperationMethod() => null;

            public WebHttpBehavior GetWebHttpBehavior() => null;
        }

        [TestMethod]
        public void CanCreateInstance()
        {
            var context = new NoContext();
            var instance = new ExceptionShieldingErrorHandler(context);

            Assert.IsNotNull(instance);
            Assert.AreEqual(ExceptionShielding.DefaultExceptionPolicy, instance.ExceptionPolicyName);
        }

        [TestMethod]
        public void CanCreateInstanceWithPolicyName()
        {
            var context = new NoContext();
            var instance = new ExceptionShieldingErrorHandler(context, "Policy");

            Assert.AreEqual("Policy", instance.ExceptionPolicyName);
        }

        [TestMethod]
        public void CanHandleErrorWithMessageFault()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context);

            bool result = shielding.HandleError(null);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanProvideFaultWithNullException()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context);
            var message = GetDefaultMessage();

            shielding.ProvideFault(null, MessageVersion.Default, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsTrue(fault.Code.IsReceiverFault);
            CheckHandlingInstanceId("DefaultLogs.txt", fault.Reason.ToString());
        }

        [TestMethod]
        public void CanProvideFaultWithNullVersion()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context);
            var message = GetDefaultMessage();
            shielding.ProvideFault(new ArithmeticException(), null, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsTrue(fault.Code.IsSenderFault);
        }

        [TestMethod]
        public void CanProvideFaultWithNullMessage()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context);
            Message message = null;

            shielding.ProvideFault(new ArithmeticException(), MessageVersion.Default, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsTrue(fault.Code.IsSenderFault);
        }

        [TestMethod]
        public void CanProvideFaultWithMockHandler()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context);
            var exception = new ArithmeticException("My Exception");
            var message = GetDefaultMessage();

            shielding.ProvideFault(exception, MessageVersion.Default, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsTrue(fault.Code.IsSenderFault);
            MockFaultContract details = fault.GetDetail<MockFaultContract>();
            Assert.IsNotNull(details);
            Assert.AreEqual(exception.Message, details.Message);
        }

        [TestMethod]
        public void CanProvideFaultOnInvalidPolicyName()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context, "Invalid Policy");
            var message = GetDefaultMessage();

            shielding.ProvideFault(new Exception(), MessageVersion.Default, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsFalse(fault.HasDetail);
            Assert.IsTrue(fault.Code.IsReceiverFault);
            CheckHandlingInstanceId("DefaultLogs.txt", fault.Reason.ToString());
        }

        [TestMethod]
        public void CanProvideFaultOnCustomPolicyName()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context, "CustomPolicy");
            var message = GetDefaultMessage();

            shielding.ProvideFault(new ArgumentException("Arg"), MessageVersion.Default, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsTrue(fault.HasDetail);
            Assert.IsTrue(fault.Code.IsSenderFault);
            var details = fault.GetDetail<MockFaultContract>();
            Assert.IsNotNull(details);
            Assert.AreEqual("Arg", details.Message);
        }

        [TestMethod]
        public void CanProvideFaultOnUnhandledLoggedExceptions()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context, "UnhandledLoggedExceptions");
            var message = GetDefaultMessage();

            shielding.ProvideFault(new Exception(), MessageVersion.Default, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsFalse(fault.HasDetail);
            Assert.IsTrue(fault.Code.IsReceiverFault);
            CheckHandlingInstanceId("UnhandledLogs.txt", fault.Reason.ToString());
        }

        [TestMethod]
        public void CanProvideFaultOnHandledLoggedExceptions()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context, "HandledLoggedExceptions");
            var message = GetDefaultMessage();

            shielding.ProvideFault(new ArithmeticException(), MessageVersion.Default, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsTrue(fault.HasDetail);
            Assert.IsTrue(fault.Code.IsSenderFault);
            CheckLoggedMessage("HandledLogs.txt", fault.Reason.ToString());
        }

        [TestMethod]
        public void CanProvideFaultOnExceptionTypeNotFound()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context, "ExceptionTypeNotFound");
            var message = GetDefaultMessage();

            shielding.ProvideFault(new Exception(), MessageVersion.Default, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsFalse(fault.HasDetail);
            Assert.IsTrue(fault.Code.IsReceiverFault);
            CheckHandlingInstanceId("DefaultLogs.txt", fault.Reason.ToString());
        }

        [TestMethod]
        public void ShouldReturnReceiverFault()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context, "CustomPolicy");
            var message = GetDefaultMessage();
            var exception = new NotSupportedException("NotSupportedException");

            shielding.ProvideFault(exception, MessageVersion.Default, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsTrue(fault.Code.IsReceiverFault);
            Assert.IsFalse(string.IsNullOrEmpty(fault.Reason.ToString()));
            Assert.IsFalse(fault.HasDetail);
            CheckHandlingInstanceId("DefaultLogs.txt", fault.Reason.ToString());
        }

        [TestMethod]
        public void ShouldReturnSenderFault()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context);
            var message = GetDefaultMessage();
            var exception = new ArithmeticException("Message");

            shielding.ProvideFault(exception, MessageVersion.Default, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsTrue(fault.Code.IsSenderFault);
            Assert.IsFalse(string.IsNullOrEmpty(fault.Reason.ToString()));
            Assert.IsTrue(fault.HasDetail);
            MockFaultContract details = fault.GetDetail<MockFaultContract>();
            Assert.IsNotNull(details);
            Assert.AreEqual(exception.Message, details.Message);
        }

        [TestMethod]
        public void CanPopulateFaultContractFromExceptionProperties()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context);
            var message = GetDefaultMessage();
            var exception = new ArgumentNullException("MyMessage");

            shielding.ProvideFault(exception, MessageVersion.Default, ref message);

            var fault = GetFaultFromMessage(message);
            Assert.IsTrue(fault.HasDetail);
            Assert.IsTrue(fault.Code.IsSenderFault);
            MockFaultContract details = fault.GetDetail<MockFaultContract>();
            Assert.IsNotNull(details);
            Assert.AreEqual(exception.Message, details.Message);
            Assert.IsTrue(details.Id != Guid.Empty);
        }

        [TestMethod]
        public void ShouldGetFaultExceptionWithPolicy()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context, "FaultException");
            var faultException = GetFaultException("test", SoapException.ServerFaultCode.Name);
            var message = Message.CreateMessage(MessageVersion.Default, faultException.CreateMessageFault(), "");

            shielding.ProvideFault(faultException, MessageVersion.Default, ref message);

            var actualFault = GetFaultFromMessage(message);
            var expectedFault = faultException.CreateMessageFault();
            Assert.AreEqual(expectedFault.Reason.ToString(), actualFault.Reason.ToString());
            Assert.AreEqual(expectedFault.HasDetail, actualFault.HasDetail);
            Assert.AreEqual(expectedFault.Code.IsReceiverFault, actualFault.Code.IsReceiverFault);
        }

        [TestMethod]
        public void ShouldGetFaultExceptionWithoutPolicy()
        {
            var context = new NoContext();
            var shielding = new ExceptionShieldingErrorHandler(context);
            var faultException = GetFaultException("test", SoapException.ServerFaultCode.Name);
            var message = Message.CreateMessage(MessageVersion.Default, faultException.CreateMessageFault(), "");

            shielding.ProvideFault(faultException, MessageVersion.Default, ref message);

            var actualFault = GetFaultFromMessage(message);
            var expectedFault = faultException.CreateMessageFault();
            Assert.AreEqual(expectedFault.Reason.ToString(), actualFault.Reason.ToString());
            Assert.AreEqual(expectedFault.HasDetail, actualFault.HasDetail);
            Assert.AreEqual(expectedFault.Code.IsReceiverFault, actualFault.Code.IsReceiverFault);
        }

        void CheckLoggedMessage(
            string logFileName,
            string message)
        {
            // close the current logger
            Logger.Writer.Dispose();
            string logFileContent = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName));
            Assert.IsTrue(logFileContent.Contains(message));
        }

        void CheckHandlingInstanceId(
            string logFileName,
            string message)
        {
            // close the current logger
            Logger.Writer.Dispose();
            string logFileContent = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName));
            Match match = RegularExpression.Guid.Match(message);

            Assert.IsFalse(string.IsNullOrEmpty(match.Value));
            Assert.IsTrue(logFileContent.Contains(match.Value));
        }

        FaultException GetFaultException(
            string faultReason,
            string faultCode)
        {
            return new FaultException(
                    new FaultReason(faultReason),
                    FaultCode.CreateReceiverFaultCode(
                        faultCode,
                        SoapException.ServerFaultCode.Namespace));
        }

        Message GetDefaultMessage()
        {
            return Message.CreateMessage(MessageVersion.Default, "testing");
        }

        MessageFault GetFaultFromMessage(Message message)
        {
            Assert.IsNotNull(message);
            Assert.IsTrue(message.IsFault);

            return MessageFault.CreateFault(message, 0x1000);
        }
    }
}
