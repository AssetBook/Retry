using System;
using NSubstitute.Routing.Handlers;
using NUnit.Framework;

namespace Apex.Retry.Test
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

	    [Test]
	    public void Process_should_retry_when_expression_evalutes_to_true()
	    {
			int actual = 0;
			Action testMethod = () =>
			{
				actual++;
			};

		    Func<bool> expression = () => actual <= 1;

			var sut = new RetryPolicy(testMethod)
				.WithRetryOn(expression)				
				.WithStopStrategy(3);

			const int expected = 2;

			sut.Process();
			Assert.That(actual, Is.EqualTo(expected));            
	    }

	    [Test]
	    public void Process_should_call_WhenMaxAttemptsReached_deleage_when_max_attempts_have_been_reached()
	    {
		    var actual = false;
		    var sut = new RetryPolicy(() => { var a = 1 + 1; })
			    .WithRetryOn(() => 1 == 2)
			    .WithStopStrategy(1)
			    .WhenMaxAttemptsReached(() => actual = true);

			sut.Process();

			Assert.That(actual, Is.True);
	    }
    }
}
