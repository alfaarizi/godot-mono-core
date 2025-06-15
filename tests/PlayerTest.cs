using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class PlayerTest
{
    [TestCase]
    public void TestPlayerCreation()
    {
        var player = new Player();
        AssertThat(player).IsNotNull();
    }
}