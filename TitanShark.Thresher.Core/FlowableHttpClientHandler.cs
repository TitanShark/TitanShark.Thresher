﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public abstract class FlowableHttpClientHandler : HttpClientHandler
    {
        public Transmitter Transmitter { get; }
        
        protected FlowableHttpClientHandler(Transmitter transmitter = null)
        {
            Transmitter = transmitter ?? CreateDefaultTransmitter();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var callId = CallId.CreateFromGuid();

            await OnPreparing(callId, request, cancellationToken);

            try
            {
                var response = await OnSending(callId, request, cancellationToken);

                await OnDone(callId, request, response, cancellationToken);

                return response;
            }
            catch(Exception exception)
            {
                await OnError(callId, request, exception, cancellationToken);

                throw;
            }
        }

        protected virtual Transmitter CreateDefaultTransmitter()
        {
            return new Transmitter
            (
                (callId, request, cancellationToken) => base.SendAsync(request, cancellationToken)
            );
        }

        protected virtual Task<HttpResponseMessage> OnSending(CallId callId, HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            var sendFunction = Transmitter?.SendFunction;

            if (sendFunction != null)
            {
                return sendFunction(callId, request, cancellationToken);
            }

            return Task.FromResult(default(HttpResponseMessage));
        }

        protected abstract Task OnPreparing(CallId callId, HttpRequestMessage request, CancellationToken cancellationToken = default);

        protected abstract Task OnDone(CallId callId, HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken = default);

        protected abstract Task OnError(CallId callId, HttpRequestMessage request, Exception exception, CancellationToken cancellationToken = default);
    }
}
