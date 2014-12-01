using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Apex.Retry.Tests
{
    [TestFixture]
    public class RetryPolicyTest
    {
        [Test]
        public void Process_should_retry_when_exception_is_in_list_of_retry_exceptions()
        {
            int actual = 0;
            Action testMethod = () =>
            {
                actual++; 
                throw new ArgumentNullException();
            };

            var sut = new RetryPolicy(testMethod)
                .WithRetryOnExceptions(typeof (ArgumentNullException))
                .WithWaitStrategy(new TimeSpan(0,0,1))
                .WithStopStrategy(2);

            const int expected = 2;            
            
            Assert.Throws<ArgumentNullException>(() => sut.Process());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Process_should_not_retry_when_exception_is_not_in_list_of_retry_exceptions()
        {
            int actual = 0;
            Action testMethod = () =>
            {
                actual++;
                throw new NotImplementedException();
            };

            var sut = new RetryPolicy(testMethod)
                .WithRetryOnExceptions(typeof(ArgumentNullException))
                .WithWaitStrategy(new TimeSpan(0, 0, 1))
                .WithStopStrategy(2);

            const int expected = 1;

            Assert.Throws<NotImplementedException>(() => sut.Process());
            Assert.That(actual, Is.EqualTo(expected));            
        }
    }
}
