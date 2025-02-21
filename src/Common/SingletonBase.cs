//SingletonBaseクラス
using System.Diagnostics;

namespace Common;

public abstract class SingletonBase<T>
    where T : class
{
    private static Lazy<T>? s_instance = null;

    protected static bool HasInstance => s_instance != null;

    public static T Instance
    {
        get
        {
            return s_instance == null
                ? throw new InvalidOperationException("Singleton instance is not initialized.")
                : s_instance.Value;
        }
    }

    /// <summary>
    /// デバッグ用のインスタンス初期化メソッド
    /// </summary>
    [Conditional("DEBUG")]
    protected static void InitializeInstance()
    {
        s_instance = null; // nullを代入することで、Lazy<T>をリセット
    }

    protected static void InitializeInstance(Func<T> factory)
    {
        s_instance = new Lazy<T>(factory);
    }

    protected SingletonBase() { }
}
