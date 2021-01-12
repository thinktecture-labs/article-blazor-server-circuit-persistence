using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CircuitPersistence.Services
{
	public class SessionPersistenceCircuitHandler : CircuitHandler
	{
		private readonly Guid _instanceId = Guid.NewGuid();
		private readonly ILogger<SessionPersistenceCircuitHandler> _logger;
		private readonly IJSRuntime _jsRuntime;
		private readonly ISessionStore _store;

		public string CircuitId { get; private set; }
		public string SessionId { get; private set; }

		public SessionPersistenceCircuitHandler(ILogger<SessionPersistenceCircuitHandler> logger, IJSRuntime jsRuntime, ISessionStore store)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
			_store = store ?? throw new ArgumentNullException(nameof(store));

			_logger.LogInformation("Circuit Handler created with Id {CircuitHandlerId}", _instanceId);
		}

		public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
		{
			_logger.LogInformation("CircuitHandler {CircuitHandlerId} got {CircuitEvent} for Circuit {CircuitId}", _instanceId, "OPENED", circuit.Id);

			CircuitId = circuit.Id;
			SessionId = await GetRemoteSessionIdAsync(cancellationToken);
			if (!String.IsNullOrEmpty(SessionId))
			{
				_logger.LogInformation("CircuitHandler {CircuitHandlerId} FOUND remote session {RemoteSessionId} for Circuit {CircuitId}", _instanceId, SessionId, circuit.Id);
			}
			else
			{
				SessionId = Guid.NewGuid().ToString();
				await SetRemoteSessionIdAsync(SessionId, cancellationToken);
				_logger.LogInformation("CircuitHandler {CircuitHandlerId} CREATED {RemoteSessionId} for Circuit {CircuitId}", _instanceId, SessionId, circuit.Id);
			}

			await _store.SaveValueAsync($"{SessionId}_LastAccessed", DateTime.UtcNow.ToString("g"));

			await base.OnCircuitOpenedAsync(circuit, cancellationToken);
		}

		public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
		{
			_logger.LogInformation("CircuitHandler {CircuitHandlerId} got {CircuitEvent} for Circuit {CircuitId}", _instanceId, "connection down", circuit.Id);

			return base.OnConnectionDownAsync(circuit, cancellationToken);
		}

		public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
		{
			_logger.LogInformation("CircuitHandler {CircuitHandlerId} got {CircuitEvent} for Circuit {CircuitId}", _instanceId, "connection up", circuit.Id);

			return base.OnConnectionUpAsync(circuit, cancellationToken);
		}

		public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
		{
			_logger.LogInformation("CircuitHandler {CircuitHandlerId} got {CircuitEvent} for Circuit {CircuitId}", _instanceId, "CLOSED", circuit.Id);

			await _store.SaveValueAsync($"{SessionId}_LastAccessed", DateTime.UtcNow.ToString("g"));
			await base.OnCircuitClosedAsync(circuit, cancellationToken);
		}

		#region JS Interop

		private const string RemoteSessionIdKey = "sessionId";

		private async Task<string> GetRemoteSessionIdAsync(CancellationToken cancellationToken = default)
		{
			return await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", cancellationToken, RemoteSessionIdKey);
		}

		private async Task SetRemoteSessionIdAsync(string value, CancellationToken cancellationToken = default)
		{
			await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", cancellationToken, RemoteSessionIdKey, value);
		}

		#endregion
	}
}
