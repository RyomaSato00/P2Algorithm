/// <summary>
/// P2アルゴリズムによる分位数のオンライン推定器クラス。
/// サンプル値を逐次追加しながら、指定の分位数を効率的に推定する。
/// </summary>
/// <remarks>
/// 参考: https://aakinshin.net/posts/p2-quantile-estimator-rounding-issue/
/// </remarks>
public class P2QuantileEstimator
{
    /// <summary>
    /// マーカーの数。P2アルゴリズムではマーカーの数は5つと規定される。
    /// </summary>
    private const int MARKER_COUNT = 5;

    /// <summary>
    /// 求めたい分位数の全体に対する割合。中央値であれば、0.5を指定する。
    /// </summary>
    private readonly double _probability;

    /// <summary>
    /// 各マーカーが下から数えて何番目に位置するか。
    /// </summary>
    private readonly int[] _realIndices = new int[MARKER_COUNT];

    /// <summary>
    /// 各マーカーが理想的には下から数えて何番目に位置すべきか。
    /// </summary>
    private readonly double[] _desiredIndices = new double[MARKER_COUNT];

    /// <summary>
    /// 各マーカーの値。
    /// </summary>
    private readonly double[] _markers = new double[MARKER_COUNT];

    /// <summary>
    /// これまでに追加されたサンプル数。
    /// </summary>
    public int NCount { get; private set; } = 0;

    /// <summary>
    /// 現在の分位数推定値。
    /// </summary>
    public double Quantile => _markers[2];

    public int[] N => _realIndices;
    public double[] Q => _markers;
    public double[] Ns => _desiredIndices;

    public P2QuantileEstimator(double probability)
    {
        _probability = probability;
        ResetAlgorithm();
    }

    /// <summary>
    /// アルゴリズムの状態を初期化する。
    /// </summary>
    public void ResetAlgorithm()
    {
        NCount = 0;

        for (int i = 0; MARKER_COUNT > i; i++)
        {
            _markers[i] = 0.0;
        }

        for (int i = 0; MARKER_COUNT > i; i++)
        {
            _realIndices[i] = i;
        }

        UpdateDesiredIndices(MARKER_COUNT);
    }

    /// <summary>
    /// サンプル値を追加し、分位数推定値を更新する。
    /// </summary>
    /// <param name="x">追加する値</param>
    public void AddValue(double x)
    {
        // サンプル数が5つに満たない場合
        if (MARKER_COUNT > NCount)
        {
            // マーカー登録
            _markers[NCount] = x;

            // サンプル数カウントアップ
            NCount++;

            // 5つの値が揃ったら、マーカーを小さい順にソートしておく。
            if (MARKER_COUNT <= NCount)
            {
                BubbleSort(_markers, MARKER_COUNT);
            }
        }
        else
        {
            // サンプル数カウントアップ
            NCount++;

            // 最小値、最大値の更新
            if (x < _markers[0])
            {
                _markers[0] = x;
            }
            else if (x > _markers[MARKER_COUNT - 1])
            {
                _markers[MARKER_COUNT - 1] = x;
            }

            // マーカーのインデックスを更新
            UpdateRealIndices(x);

            // 理想的なインデックスを更新
            UpdateDesiredIndices(NCount);

            // 必要に応じてマーカーの値を更新
            UpdateMarker();
        }
    }

    /// <summary>
    /// 新しい値に応じて実際のインデックスを更新する。
    /// </summary>
    /// <param name="x">追加された値</param>
    private void UpdateRealIndices(double x)
    {
        // 最小値と最大値のインデックスは固定。
        _realIndices[0] = 0;
        _realIndices[MARKER_COUNT - 1] = NCount - 1;

        // 新しい値がマーカーの値より小さい場合、マーカーのインデックスを1つ増やす。
        for (int i = 1; MARKER_COUNT - 1 > i; i++)
        {
            if (x < _markers[i])
            {
                _realIndices[i]++;
            }
        }
    }

    /// <summary>
    /// サンプル数に応じて理想インデックスを計算・更新する。
    /// </summary>
    /// <param name="nCount">サンプル数</param>
    private void UpdateDesiredIndices(int nCount)
    {
        // インデックスの最大値
        double maxIndex = nCount - 1;
        double mediumIndex = maxIndex / 2;

        _desiredIndices[0] = 0;
        _desiredIndices[1] = mediumIndex * _probability;
        _desiredIndices[2] = maxIndex * _probability;
        _desiredIndices[3] = mediumIndex + mediumIndex * _probability;
        _desiredIndices[4] = maxIndex;
    }

    /// <summary>
    /// マーカー値を補正する（放物線補間または線形補間）。
    /// </summary>
    private void UpdateMarker()
    {
        // ※0番目のマーカーと4番目のマーカーはそれぞれ最小値と最大値なので更新不要。
        for (int i = 1; MARKER_COUNT - 1 > i; i++)
        {
            // 理想的なインデックスと実際のインデックスの誤差を計算
            double error = _desiredIndices[i] - _realIndices[i];

            // 誤差が1以上かつ、インデックスを1増減しても隣のマーカーと重ならない？
            if (error >= 1 && _realIndices[i + 1] - _realIndices[i] > 1
            || error <= -1 && _realIndices[i - 1] - _realIndices[i] < -1)
            {
                // 誤差の符号を取得
                int sign = Sign(error);

                // 放物線補間から新しいマーカーの値を計算
                double newMarker = Parabolic(i, sign);

                // 新しいマーカーの値が隣のマーカーの値の間に収まる？
                if (_markers[i - 1] < newMarker && newMarker < _markers[i + 1])
                {
                    _markers[i] = newMarker;
                }
                // 収まらない場合は線形補間で新しいマーカーの値を計算
                else
                {
                    _markers[i] = Linear(i, sign);
                }

                // インデックスを1増減
                _realIndices[i] += sign;
            }
        }
    }

    /// <summary>
    /// 値の符号を返す。
    /// </summary>
    /// <param name="x">誤差値</param>
    /// <returns>正なら1、負なら-1、ゼロなら0</returns>
    private static int Sign(double x)
    {
        int result = 0;

        if (x > 0) result = 1;
        else if (x < 0) result = -1;

        return result;
    }

    /// <summary>
    /// 放物線補間で新しいマーカー値を計算する。
    /// </summary>
    /// <param name="memoryIndex">今回のマーカーに対応するメモリのインデックス</param>
    /// <param name="sign">補正方向（±1）</param>
    /// <returns>新しいマーカー値</returns>
    private double Parabolic(int memoryIndex, double sign)
    {
        return _markers[memoryIndex] + sign / (_realIndices[memoryIndex + 1] - _realIndices[memoryIndex - 1]) * (
            (_realIndices[memoryIndex] - _realIndices[memoryIndex - 1] + sign) * (_markers[memoryIndex + 1] - _markers[memoryIndex]) / (_realIndices[memoryIndex + 1] - _realIndices[memoryIndex]) +
            (_realIndices[memoryIndex + 1] - _realIndices[memoryIndex] - sign) * (_markers[memoryIndex] - _markers[memoryIndex - 1]) / (_realIndices[memoryIndex] - _realIndices[memoryIndex - 1])
        );
    }

    /// <summary>
    /// 線形補間で新しいマーカー値を計算する。
    /// </summary>
    /// <param name="memoryIndex">今回のマーカーに対応するメモリのインデックス</param>
    /// <param name="sign">補正方向（±1）</param>
    /// <returns>新しいマーカー値</returns>
    private double Linear(int memoryIndex, int sign)
    {
        return _markers[memoryIndex] + sign * (_markers[memoryIndex + sign] - _markers[memoryIndex]) / (_realIndices[memoryIndex + sign] - _realIndices[memoryIndex]);
    }

    /// <summary>
    /// 配列をバブルソートで小さい順に並び替える。
    /// </summary>
    private static void BubbleSort(double[] array, int length)
    {
        for (int i = 0; i < length - 1; i++)
        {
            for (int j = 0; j < length - i - 1; j++)
            {
                if (array[j] > array[j + 1])
                {
                    double temp = array[j];
                    array[j] = array[j + 1];
                    array[j + 1] = temp;
                }
            }
        }
    }
}
