using Xunit;
public class VersionTests {
 [Fact] public void BindingVersionMatchesTheAuditedSdk() => Assert.Equal(0x040801u, SteamAudio.Interop.Api.Version);
}
