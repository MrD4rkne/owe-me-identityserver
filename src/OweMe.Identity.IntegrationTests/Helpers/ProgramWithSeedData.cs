namespace OweMe.Identity.IntegrationTests.Helpers;

public sealed class ProgramWithSeedData : ProgramFixture
{
    public ProgramWithSeedData()
    {
        WithMigrations();
        WithSeeding(TestSeedData.Data);
        WithTestUser(TestSeedData.TestUser);
    }
}
