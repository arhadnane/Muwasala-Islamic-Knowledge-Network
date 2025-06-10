using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Muwasala.Agents;

/// <summary>
/// Circuit breaker pattern implementation for external service calls
/// Prevents cascade failures and improves response times by failing fast
/// </summary>
public interface ICircuitBreakerService
{
    Task<T> ExecuteAsync<T>(string serviceKey, Func<Task<T>> operation, T fallbackValue);
    bool IsServiceAvailable(string serviceKey);
    void ResetCircuit(string serviceKey);
}

public class CircuitBreakerService : ICircuitBreakerService
{
    private readonly ConcurrentDictionary<string, CircuitState> _circuits = new();
    private readonly ILogger<CircuitBreakerService> _logger;

    // Circuit breaker configuration
    private readonly int _failureThreshold = 3;           // Open circuit after 3 failures
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30); // 30-second timeout
    private readonly TimeSpan _retryDelay = TimeSpan.FromMinutes(1); // Wait 1 minute before retry

    public CircuitBreakerService(ILogger<CircuitBreakerService> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(string serviceKey, Func<Task<T>> operation, T fallbackValue)
    {
        var circuit = _circuits.GetOrAdd(serviceKey, _ => new CircuitState());

        // Check if circuit is open
        if (circuit.State == CircuitBreakerState.Open)
        {
            if (DateTime.UtcNow >= circuit.NextRetryTime)
            {
                // Try to close circuit (half-open state)
                circuit.State = CircuitBreakerState.HalfOpen;
                _logger.LogInformation("🔄 Circuit breaker for {ServiceKey} moving to half-open state", serviceKey);
            }
            else
            {
                // Circuit is still open, return fallback immediately
                _logger.LogWarning("🚫 Circuit breaker for {ServiceKey} is OPEN - returning fallback value", serviceKey);
                return fallbackValue;
            }
        }

        try
        {
            // Execute with timeout
            using var cts = new CancellationTokenSource(_timeout);
            var result = await operation().WaitAsync(cts.Token);

            // Success - reset failure count and close circuit
            if (circuit.State == CircuitBreakerState.HalfOpen)
            {
                circuit.State = CircuitBreakerState.Closed;
                circuit.FailureCount = 0;
                _logger.LogInformation("✅ Circuit breaker for {ServiceKey} closed successfully", serviceKey);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            // Timeout occurred
            _logger.LogWarning("⏰ Timeout in circuit breaker for {ServiceKey}", serviceKey);
            return await HandleFailure(serviceKey, circuit, fallbackValue);
        }
        catch (Exception ex)
        {
            // Any other failure
            _logger.LogWarning(ex, "❌ Exception in circuit breaker for {ServiceKey}: {Message}", serviceKey, ex.Message);
            return await HandleFailure(serviceKey, circuit, fallbackValue);
        }
    }

    public bool IsServiceAvailable(string serviceKey)
    {
        var circuit = _circuits.GetOrAdd(serviceKey, _ => new CircuitState());
        return circuit.State != CircuitBreakerState.Open || DateTime.UtcNow >= circuit.NextRetryTime;
    }

    public void ResetCircuit(string serviceKey)
    {
        if (_circuits.TryGetValue(serviceKey, out var circuit))
        {
            circuit.State = CircuitBreakerState.Closed;
            circuit.FailureCount = 0;
            _logger.LogInformation("🔄 Circuit breaker for {ServiceKey} manually reset", serviceKey);
        }
    }

    private async Task<T> HandleFailure<T>(string serviceKey, CircuitState circuit, T fallbackValue)
    {
        circuit.FailureCount++;
        circuit.LastFailureTime = DateTime.UtcNow;

        if (circuit.FailureCount >= _failureThreshold)
        {
            // Open the circuit
            circuit.State = CircuitBreakerState.Open;
            circuit.NextRetryTime = DateTime.UtcNow.Add(_retryDelay);
            _logger.LogError("🚫 Circuit breaker for {ServiceKey} OPENED after {FailureCount} failures", 
                serviceKey, circuit.FailureCount);
        }

        return fallbackValue;
    }

    private class CircuitState
    {
        public CircuitBreakerState State { get; set; } = CircuitBreakerState.Closed;
        public int FailureCount { get; set; } = 0;
        public DateTime LastFailureTime { get; set; } = DateTime.MinValue;
        public DateTime NextRetryTime { get; set; } = DateTime.MinValue;
    }

    private enum CircuitBreakerState
    {
        Closed,     // Normal operation
        Open,       // Circuit is open, calls fail immediately
        HalfOpen    // Testing if service is back online
    }
}
