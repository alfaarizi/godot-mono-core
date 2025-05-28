using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class PlayerTest
{
    [TestCase]
    public static void TestPlayerCreation()
    {
        var player = new Player();
        AssertThat(player).IsNotNull();
    }

    [TestCase]
    public static void TestPlayerSpeed()
    {
        _ = AssertThat(Player.Speed).IsEqual(500.0f);
    }
}