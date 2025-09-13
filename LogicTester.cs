public class LogicTester
{
    private readonly double[] _testData =
    [
        2.1, 2.3, 2.0, 1.9, 2.2, 2.4, 2.1, 2.5, 2.0, 2.3,
        7.8, 8.0, 7.9, 8.1, 8.2, 7.7, 8.3, 8.0, 7.9, 8.1
    ];

    public void Run()
    {
        var modelQ1 = new P2QuantileEstimator(0.25);
        var modelQ2 = new P2QuantileEstimator(0.50);
        var modelQ3 = new P2QuantileEstimator(0.75);
        // var modelQ1 = new P2Original(0.25);
        // var modelQ2 = new P2Original(0.50);
        // var modelQ3 = new P2Original(0.75);

        foreach (var item in _testData)
        {
            modelQ1.AddValue(item);
            modelQ2.AddValue(item);
            modelQ3.AddValue(item);

            Console.WriteLine($"Value: {item:F2}, Q1: {modelQ1.GetQuantile():F2}, Q2: {modelQ2.GetQuantile():F2}, Q3: {modelQ3.GetQuantile():F2},  {modelQ2.Count},{modelQ2.N[0]},{modelQ2.N[1]},{modelQ2.N[2]},{modelQ2.N[3]},{modelQ2.N[4]},{modelQ2.Ns[0]:F2},{modelQ2.Ns[1]:F2},{modelQ2.Ns[2]:F2},{modelQ2.Ns[3]:F2},{modelQ2.Ns[4]:F2}");
        }
    }
}
