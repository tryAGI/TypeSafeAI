#nullable enable

namespace TypeSafeAI.Generated
{
    public partial interface IRawTypeSafeClient
    {
        /// <summary>
        /// Systemone<br/>
        /// Answer one or more questions about the content supplied in `state`.<br/>
        /// You can mix question types in one request. Answers use the same names as the<br/>
        /// questions, so you can match each result to its question. The response also includes<br/>
        /// the model used and token usage.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestOptions">Per-request overrides such as headers, query parameters, timeout, retries, and response buffering.</param>
        /// <param name="cancellationToken">The token to cancel the operation with</param>
        /// <exception cref="global::TypeSafeAI.Generated.ApiException"></exception>
        global::System.Threading.Tasks.Task<global::TypeSafeAI.Generated.SystemOneResponse> SystemoneV1SystemonePostAsync(

            global::TypeSafeAI.Generated.SystemOneRequest request,
            global::TypeSafeAI.Generated.AutoSDKRequestOptions? requestOptions = default,
            global::System.Threading.CancellationToken cancellationToken = default);
        /// <summary>
        /// Systemone<br/>
        /// Answer one or more questions about the content supplied in `state`.<br/>
        /// You can mix question types in one request. Answers use the same names as the<br/>
        /// questions, so you can match each result to its question. The response also includes<br/>
        /// the model used and token usage.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestOptions">Per-request overrides such as headers, query parameters, timeout, retries, and response buffering.</param>
        /// <param name="cancellationToken">The token to cancel the operation with</param>
        /// <exception cref="global::TypeSafeAI.Generated.ApiException"></exception>
        global::System.Threading.Tasks.Task<global::TypeSafeAI.Generated.AutoSDKHttpResponse<global::TypeSafeAI.Generated.SystemOneResponse>> SystemoneV1SystemonePostAsResponseAsync(

            global::TypeSafeAI.Generated.SystemOneRequest request,
            global::TypeSafeAI.Generated.AutoSDKRequestOptions? requestOptions = default,
            global::System.Threading.CancellationToken cancellationToken = default);
        /// <summary>
        /// Systemone<br/>
        /// Answer one or more questions about the content supplied in `state`.<br/>
        /// You can mix question types in one request. Answers use the same names as the<br/>
        /// questions, so you can match each result to its question. The response also includes<br/>
        /// the model used and token usage.
        /// </summary>
        /// <param name="state">
        /// The content all questions in this request refer to.
        /// </param>
        /// <param name="model">
        /// Name or alias of the model to use. Available names are returned by GET /v1/models.
        /// </param>
        /// <param name="questions">
        /// Questions to ask about the content, each with a name you choose. The response uses those names to identify the answers.
        /// </param>
        /// <param name="requestOptions">Per-request overrides such as headers, query parameters, timeout, retries, and response buffering.</param>
        /// <param name="cancellationToken">The token to cancel the operation with</param>
        /// <exception cref="global::System.InvalidOperationException"></exception>
        global::System.Threading.Tasks.Task<global::TypeSafeAI.Generated.SystemOneResponse> SystemoneV1SystemonePostAsync(
            global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>> state,
            string model,
            object questions,
            global::TypeSafeAI.Generated.AutoSDKRequestOptions? requestOptions = default,
            global::System.Threading.CancellationToken cancellationToken = default);
    }
}