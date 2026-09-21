#nullable enable

namespace TypeSafeAI.Generated
{
    public partial interface IRawTypeSafeClient
    {
        /// <summary>
        /// Models V1<br/>
        /// List the models and aliases available to the authenticated account.<br/>
        /// Pass a returned model name as `model` in a POST /v1/systemone request.
        /// </summary>
        /// <param name="requestOptions">Per-request overrides such as headers, query parameters, timeout, retries, and response buffering.</param>
        /// <param name="cancellationToken">The token to cancel the operation with</param>
        /// <exception cref="global::TypeSafeAI.Generated.ApiException"></exception>
        global::System.Threading.Tasks.Task<global::TypeSafeAI.Generated.ModelMetadataList> ModelsV1V1ModelsGetAsync(
            global::TypeSafeAI.Generated.AutoSDKRequestOptions? requestOptions = default,
            global::System.Threading.CancellationToken cancellationToken = default);
        /// <summary>
        /// Models V1<br/>
        /// List the models and aliases available to the authenticated account.<br/>
        /// Pass a returned model name as `model` in a POST /v1/systemone request.
        /// </summary>
        /// <param name="requestOptions">Per-request overrides such as headers, query parameters, timeout, retries, and response buffering.</param>
        /// <param name="cancellationToken">The token to cancel the operation with</param>
        /// <exception cref="global::TypeSafeAI.Generated.ApiException"></exception>
        global::System.Threading.Tasks.Task<global::TypeSafeAI.Generated.AutoSDKHttpResponse<global::TypeSafeAI.Generated.ModelMetadataList>> ModelsV1V1ModelsGetAsResponseAsync(
            global::TypeSafeAI.Generated.AutoSDKRequestOptions? requestOptions = default,
            global::System.Threading.CancellationToken cancellationToken = default);
    }
}