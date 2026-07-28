#region

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue.Mfa;

#endregion

namespace GotrueTests.Mfa;

/// <summary>
///     End-to-end service-role administration of a user's MFA factors against the live stack: listing factors
///     (with their verification status) and deleting a factor on the user's behalf.
/// </summary>
[TestClass]
[TestCategory("E2E")]
public class MfaAdminTests : MfaFixture
{
    [TestMethod]
    public async Task ListFactors_ShouldReflectFactorVerificationStatus()
    {
        var session = await this.SignUpNewUser();
        var enrollment = await this.EnrollTotp();
        var unverified = await this.Admin.ListFactors(new MfaAdminListFactorsParams { UserId = session.User!.Id });
        unverified.Factors.Should().ContainSingle();
        unverified.Factors[0].Id.Should().Be(enrollment.Id);
        unverified.Factors[0].Status.Should().Be("unverified");
        await this.Client.ChallengeAndVerify(new MfaChallengeAndVerifyParams { FactorId = enrollment.Id, Code = TotpCode(enrollment) });
        var verified = await this.Admin.ListFactors(new MfaAdminListFactorsParams { UserId = session.User.Id });
        verified.Factors.Should().ContainSingle();
        verified.Factors[0].Status.Should().Be("verified");
    }

    [TestMethod]
    public async Task DeleteFactor_ShouldRemoveTheUsersFactor()
    {
        var session = await this.SignUpNewUser();
        var enrollment = await this.EnrollTotp();
        (await this.Admin.ListFactors(new MfaAdminListFactorsParams { UserId = session.User!.Id })).Factors.Should().ContainSingle();
        var deleted = await this.Admin.DeleteFactor(new MfaAdminDeleteFactorParams { Id = enrollment.Id, UserId = session.User.Id });
        deleted.Id.Should().Be(enrollment.Id);
        (await this.Admin.ListFactors(new MfaAdminListFactorsParams { UserId = session.User.Id })).Factors.Should().BeEmpty();
    }
}
