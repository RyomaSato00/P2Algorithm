public class LogicTester
{
    // private readonly double[] _testData =
    // [
    //     2.1, 2.3, 2.0, 1.9, 2.2, 2.4, 2.1, 2.5, 2.0, 2.3,
    //     7.8, 8.0, 7.9, 8.1, 8.2, 7.7, 8.3, 8.0, 7.9, 8.1
    // ];
    private readonly double[] _testData = [
        0.5273, 0.1839, 0.9471, 0.6245, 0.3082, 0.7516, 0.0924, 0.4698, 0.8372, 0.2157,
        0.6810, 0.3946, 0.0593, 0.9981, 0.1227, 0.7462, 0.3089, 0.5734, 0.8895, 0.4316,
        0.2648, 0.6159, 0.0382, 0.7821, 0.9347, 0.1573, 0.5012, 0.3468, 0.7093, 0.2894,
        0.6205, 0.1742, 0.9638, 0.0871, 0.3982, 0.8427, 0.2561, 0.7124, 0.1346, 0.5539,
        0.4793, 0.6601, 0.0217, 0.9054, 0.3702, 0.7983, 0.2438, 0.6897, 0.1184, 0.5310,
        0.6029, 0.1937, 0.9732, 0.0675, 0.4286, 0.8619, 0.3051, 0.7482, 0.0913, 0.4675,
        0.8294, 0.2128, 0.6751, 0.3892, 0.0537, 0.9924, 0.1165, 0.7398, 0.3026, 0.5671,
        0.8823, 0.4264, 0.2593, 0.6102, 0.0314, 0.7756, 0.9281, 0.1519, 0.4957, 0.3412,
        0.7036, 0.2841, 0.6147, 0.1689, 0.9572, 0.0816, 0.3927, 0.8371, 0.2503, 0.7069,
        0.1289, 0.5483, 0.4736, 0.6547, 0.0162, 0.8998, 0.3647, 0.7926, 0.2382, 0.6841
    ];

    public void Run()
    {
        var modelQ1 = new P2QuantileEstimator(0.25);
        var modelQ2 = new P2QuantileEstimator(0.50);
        var modelQ3 = new P2QuantileEstimator(0.75);
        // var modelQ1 = new P2Original(0.25);
        // var modelQ2 = new P2Original(0.50);
        // var modelQ3 = new P2Original(0.75);

        var buffer = new List<double>();

        foreach (var item in _testData)
        {
            modelQ1.AddValue(item);
            modelQ2.AddValue(item);
            modelQ3.AddValue(item);
            buffer.Add(item);
            buffer.Sort();

            Console.WriteLine($"Value: {item:F2}, Q1: {modelQ1.Quantile:F2}, Q2: {modelQ2.Quantile:F2}, Q3: {modelQ3.Quantile:F2} | Q1: {buffer[(int)(buffer.Count / 4)]}, Q2: {buffer[(int)(buffer.Count / 2)]}, Q3: {buffer[(int)(buffer.Count * 3 / 4)]}  {modelQ2.NCount},{modelQ2.N[0]},{modelQ2.N[1]},{modelQ2.N[2]},{modelQ2.N[3]},{modelQ2.N[4]},{modelQ2.Ns[0]:F2},{modelQ2.Ns[1]:F2},{modelQ2.Ns[2]:F2},{modelQ2.Ns[3]:F2},{modelQ2.Ns[4]:F2}");
        }
    }
}
