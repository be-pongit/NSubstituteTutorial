﻿using System;
using Moq;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NSubstituteTutorial
{
    [TestFixture]
    public class MoqComparison
    {
        ICalculator nsub = Substitute.For<ICalculator>();
        Mock<ICalculator> moq = new Mock<ICalculator>();

        [Test]
        public void BasicExample()
        {
            // Methods
            moq.Setup(calc => calc.Add(1, 1)).Returns(2);
            Assert.AreEqual(2, moq.Object.Add(1, 1));

            nsub.Add(1, 1).Returns(2);
            Assert.AreEqual(2, nsub.Add(1, 1));

            // Properties
            moq.Setup(calc => calc.Mode).Returns("DEC");
            Assert.AreEqual("DEC", moq.Object.Mode);

            nsub.Mode = "DEC";
            // Can also use the same syntax as for methods:
            // nsub.Mode.Returns("DEC");
            Assert.AreEqual("DEC", nsub.Mode);
        }

        [Test]
        public void OutAndRef()
        {
            {
                float remainder = 0.4F;
                moq.Setup(calc => calc.Divide(12, 5, out remainder)).Returns(2);
                remainder = 0;

                Assert.AreEqual(2, moq.Object.Divide(12, 5, out remainder));
                Assert.AreEqual(0.4F, remainder);
            }

            {
                nsub.Divide(12, 5, out float remainder).Returns(x =>
                {
                    x[2] = 0.4F; // [2] = 3th parameter remainder
                    return 2;
                });
                remainder = 0;

                Assert.AreEqual(2, nsub.Divide(12, 5, out remainder));
                Assert.AreEqual(0.4F, remainder);
            }
        }

        [Test]
        public void ManipulateResult()
        {
            moq.Setup(calc => calc.Add(1, 1)).Returns((int a, int b) => a + b + 1);
            Assert.AreEqual(3, moq.Object.Add(1, 1));

            nsub.Add(1, 1).Returns(r => (int) r[0] + (int) r[1] + 1);
            Assert.AreEqual(3, nsub.Add(1, 1));
        }

        [Test]
        public void ThrowExceptions()
        {
            var moq2 = new Mock<ICalculator>();
            moq2.Setup(calc => calc.Add(1, 1)).Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => moq2.Object.Add(1, 1));

            moq2.Setup(calc => calc.SetMode("HEX")).Throws(new ArgumentException());
            Assert.Throws<ArgumentException>(() => moq2.Object.SetMode("HEX"));

            // using NSubstitute.ExceptionExtensions;
            nsub.Add(1, 1).Throws(new InvalidOperationException());
            Assert.Throws<InvalidOperationException>(() => nsub.Add(1, 1));

            nsub.When(x => x.SetMode("HEX")).Throw<ArgumentException>();
            Assert.Throws<ArgumentException>(() => nsub.SetMode("HEX"));

            // The extension method syntax is much cleaner
            // Without it, the setup becomes the following
            nsub.Add(1, 1).Returns(x => { throw new InvalidOperationException(); });
            nsub.When(x => x.SetMode("HEX")).Do(x => { throw new ArgumentException(); });
        }

        [Test]
        public void MatchingArguments()
        {
            // It.
            moq.Setup(calc => calc.Add(It.IsAny<int>(), It.Is<int>(b => b % 2 == 0))).Returns(3);
            
            //Mock also has
            //- It.IsRegex("", , RegexOptions.IgnoreCase)
            //- It.IsInRange(0, 1, Range.Inclusive)

            // Arg.
            nsub.Add(Arg.Any<int>(), Arg.Is<int>(b => b % 2 == 0)).Returns(3);
        }

        [Test]
        public void Verification()
        {
            // clear calls from other tests
            var moq = new Mock<ICalculator>();
            moq.Object.Add(1, 1);
            moq.Object.Add(1, 1);
            moq.Verify(calc => calc.Add(1, It.IsAny<int>()), Times.Exactly(2));

            // Properties
            moq.VerifyGet(calc => calc.Mode, Times.Never);


            var nsub = Substitute.For<ICalculator>();
            nsub.Add(1, 1);
            nsub.Add(1, 1);
            nsub.Received(2).Add(1, Arg.Any<int>());

            // Properties
            var requiredAssignmentForCompiler = nsub.DidNotReceive().Mode;
        }
    }
}
