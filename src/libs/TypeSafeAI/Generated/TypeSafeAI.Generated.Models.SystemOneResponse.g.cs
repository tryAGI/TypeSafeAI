
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// Answers grouped by question name, with the model used and token usage.
    /// </summary>
    public sealed partial class SystemOneResponse
    {
        /// <summary>
        /// Name of the model that answered the questions. May differ from the alias supplied in the request.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("model")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required string Model { get; set; }

        /// <summary>
        /// Answers keyed by the question names supplied in the request. Each answer's type matches its question's type.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("answers")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required global::System.Collections.Generic.Dictionary<string, global::TypeSafeAI.Generated.Answer> Answers { get; set; }

        /// <summary>
        /// Input and output token counts for this evaluation.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("usage")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required global::TypeSafeAI.Generated.Usage Usage { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemOneResponse" /> class.
        /// </summary>
        /// <param name="model">
        /// Name of the model that answered the questions. May differ from the alias supplied in the request.
        /// </param>
        /// <param name="answers">
        /// Answers keyed by the question names supplied in the request. Each answer's type matches its question's type.
        /// </param>
        /// <param name="usage">
        /// Input and output token counts for this evaluation.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public SystemOneResponse(
            string model,
            global::System.Collections.Generic.Dictionary<string, global::TypeSafeAI.Generated.Answer> answers,
            global::TypeSafeAI.Generated.Usage usage)
        {
            this.Model = model ?? throw new global::System.ArgumentNullException(nameof(model));
            this.Answers = answers ?? throw new global::System.ArgumentNullException(nameof(answers));
            this.Usage = usage ?? throw new global::System.ArgumentNullException(nameof(usage));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemOneResponse" /> class.
        /// </summary>
        public SystemOneResponse()
        {
        }

    }
}