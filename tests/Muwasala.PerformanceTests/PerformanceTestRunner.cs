using Xunit;

namespace Muwasala.PerformanceTests;

public class PerformanceTestRunner
{
    [Fact(Skip = "Manual performance test - run only when needed")]
    public void RunSearchPerformanceTests()
    {
        SearchPerformanceTests.RunSearchPerformanceTest();
    }

    [Fact(Skip = "Manual performance test - run only when needed")]
    public void RunConcurrentSearchTests()
    {
        SearchPerformanceTests.RunConcurrentSearchTest();
    }

    [Fact(Skip = "Manual performance test - run only when needed")]
    public void RunPaginationTests()
    {
        SearchPerformanceTests.RunPaginationTest();
    }
}
