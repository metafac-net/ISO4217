using BenchmarkDotNet.Running;
using System.Reflection;

namespace MetaFac.Standard.RFC4648.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run();
        }
    }
}