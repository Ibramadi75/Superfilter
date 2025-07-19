using System.Reflection;
using BenchmarkDotNet.Running;

Console.WriteLine("🔍 Test du point de basculement performance...");

BenchmarkSwitcher
    .FromAssembly(Assembly.GetExecutingAssembly())
    .Run(args);
    
    