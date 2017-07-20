using System.Threading;
using NUnit.Framework;
using Moq;

public interface ICreditDecisionService
{
    string GetDecision(int creditScore);
}

public class CreditDecisionService : ICreditDecisionService
{
    public string GetDecision(int creditScore)
    {
        // Simulate a long (2500ms) call to a remote web service:
        Thread.Sleep(2500);
        if (creditScore < 550)
            return "Declined";
        if (creditScore <= 675)
            return "Maybe";
        return "We look forward to doing business with you!";
    }
}

public class CreditDecision
{
    readonly ICreditDecisionService creditDecisionService;

    public CreditDecision(ICreditDecisionService creditDecisionService)
    {
        this.creditDecisionService = creditDecisionService;
    }

    public string MakeCreditDecision(int creditScore)
    {
        return creditDecisionService.GetDecision(creditScore);
    }
}

[TestFixture]
public class CreditDecisionTests
{
    Mock<ICreditDecisionService> mockCreditDecisionService;

    CreditDecision systemUnderTest;

    [TestCase(677, "Declined")] //Will pass although incorrect
    [TestCase(100, "Declined")]
    [TestCase(549, "Declined")]
    [TestCase(550, "Maybe")]
    [TestCase(675, "Maybe")]
    [TestCase(676, "We look forward to doing business with you!")]
    public void MockedMakeCreditDecision_Always_ReturnsExpectedResult(int creditScore, string expectedResult)
    {
        mockCreditDecisionService = new Mock<ICreditDecisionService>(MockBehavior.Strict);
        mockCreditDecisionService.Setup(p => p.GetDecision(creditScore)).Returns(expectedResult);

        systemUnderTest = new CreditDecision(mockCreditDecisionService.Object);
        var result = systemUnderTest.MakeCreditDecision(creditScore);

        Assert.That(result, Is.EqualTo(expectedResult));

        //Using Verify allow to verify correctness of dependencies i.e.
        //   - Actually called into CreditDecisionService
        //   - Passed the correct parameters to CreditDecisionService
        //   - Returned the actual value from CreditDecisionService without modifying it.
        //Can also user VerifyAll() but not as explicit;
        mockCreditDecisionService.Verify(m => m.GetDecision(It.IsAny<int>()), Times.Once);
    }
}

public class BadCreditDecisionTests
{
    readonly CreditDecisionService creditDecisionService = new CreditDecisionService();

    CreditDecision systemUnderTest;

    [TestCase(678, "Declined")] //Will fail as expected
    [TestCase(100, "Declined")]
    [TestCase(549, "Declined")]
    [TestCase(550, "Maybe")]
    [TestCase(675, "Maybe")]
    [TestCase(676, "We look forward to doing business with you!")]
    public void MakeCreditDecision_Always_ReturnsExpectedResult(int creditScore, string expectedResult)
    {
        //Call to real service adds 2.5 secs per test. Adds 12.5 secs to test run
        systemUnderTest = new CreditDecision(creditDecisionService);

        var result = systemUnderTest.MakeCreditDecision(creditScore);

        // Fortunately assertion works as normal...
        Assert.That(result, Is.EqualTo(expectedResult));

        // Because we used "real" service rather than a mock can no
        // verify the correctness of theinteraction of MakeCreditDecision
        // method and CreditDecisionService
        // creditDecisionService.VerifyAll();
    }
}